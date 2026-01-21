'use client';

import { useState, useEffect, useRef, useCallback } from 'react';
import type { Hero as HeroType, Enemy as EnemyType, ActiveSkill, Projectile as ProjectileType } from '@/lib/game-types';
import Hero from '@/components/game/Hero';
import Enemy from '@/components/game/Enemy';
import Tavern from '@/components/game/Tavern';
import GameUI from '@/components/game/GameUI';
import SkillBar from '@/components/game/SkillBar';
import Projectile from '@/components/game/Projectile';


interface GameScreenProps {
  heroes: HeroType[];
  day: number;
  gold: number;
  tavernHealth: number;
  setTavernHealth: React.Dispatch<React.SetStateAction<number>>;
  onWaveComplete: (goldEarned: number, heroes: HeroType[], killsInWave: number) => void;
  onGameOver: (finalHeroes: HeroType[]) => void;
  gameSpeed: number;
  setGameSpeed: React.Dispatch<React.SetStateAction<number>>;
  totalKills: number;
}

const GAME_AREA_WIDTH = 1280;
const GAME_AREA_HEIGHT = 720;
const MELEE_RANGE_DIVISOR = 4; // Warrior's attack range is sight range / this

const calculateDistance = (pos1: {x:number, y:number}, pos2: {x:number, y:number}) => {
  return Math.sqrt(Math.pow(pos1.x - pos2.x, 2) + Math.pow(pos1.y - pos2.y, 2));
}

export default function GameScreen({ 
  heroes: initialHeroes, 
  day, 
  gold,
  tavernHealth,
  setTavernHealth, 
  onWaveComplete, 
  onGameOver,
  gameSpeed,
  setGameSpeed,
  totalKills
}: GameScreenProps) {
  const [heroes, setHeroes] = useState<(HeroType & { patrolTarget?: { x: number; y: number }, chargeTarget?: { x: number, y: number } })[]>(initialHeroes.map(h => ({...h, position: {...h.basePosition}, attackCooldown: 0, targetEnemyId: null})));
  const [enemies, setEnemies] = useState<EnemyType[]>([]);
  const [projectiles, setProjectiles] = useState<ProjectileType[]>([]);
  const [activeSkills, setActiveSkills] = useState<ActiveSkill[]>([]);
  const [goldEarned, setGoldEarned] = useState(0);
  const [killsInWave, setKillsInWave] = useState(0);
  const [draggedHero, setDraggedHero] = useState<string | null>(null);

  const gameAreaRef = useRef<HTMLDivElement>(null);
  const enemiesToSpawn = day * 5 + 5;
  const spawnedEnemiesCount = useRef(0);
  const heroStatsRef = useRef<Map<string, { kills: number; damageDealt: number }>>(new Map());

  useEffect(() => {
    initialHeroes.forEach(hero => {
        if (!heroStatsRef.current.has(hero.id)) {
            heroStatsRef.current.set(hero.id, { 
                kills: hero.kills || 0, 
                damageDealt: hero.damageDealt || 0 
            });
        }
    });
  }, [initialHeroes]);

  const spawnEnemy = useCallback(() => {
    const side = Math.floor(Math.random() * 3); // 0: top, 1: left, 2: right
    let position: { x: number; y: number };

    if (side === 0) { // Top
      position = { x: Math.random() * 100, y: 0 };
    } else if (side === 1) { // Left
      position = { x: 0, y: Math.random() * 80 }; // Not too low
    } else { // Right
      position = { x: 100, y: Math.random() * 80 };
    }
    
    const enemyId = `enemy-${Date.now()}-${Math.random()}`;
    const health = 20 + day * 5;
    
    setEnemies(prev => [...prev, {
      id: enemyId,
      position,
      health,
      maxHealth: health,
      speed: 0.1 + day * 0.02,
      damage: 5 + day * 2,
      attackRange: 4, // Percent of game area width
      attackSpeed: 0.5, // Attacks per second
      aggroRange: 20, // Percent of game area width
    }]);
    spawnedEnemiesCount.current++;
  }, [day]);

  useEffect(() => {
    spawnedEnemiesCount.current = 0;
    const spawnInterval = setInterval(() => {
      if (spawnedEnemiesCount.current < enemiesToSpawn) {
        spawnEnemy();
      } else {
        clearInterval(spawnInterval);
      }
    }, (2000 / (1 + day * 0.1)) / gameSpeed);

    return () => clearInterval(spawnInterval);
  }, [day, enemiesToSpawn, spawnEnemy, gameSpeed]);
  
  useEffect(() => {
    if (spawnedEnemiesCount.current >= enemiesToSpawn && enemies.length === 0 && heroes.some(h => !h.isDead)) {
      const finalHeroes = heroes.map(h => {
        const stats = heroStatsRef.current.get(h.id);
        return { ...h, ...stats, damageDealt: Math.round(stats?.damageDealt || 0) };
      });
      onWaveComplete(goldEarned, finalHeroes, killsInWave);
    }
    if ((heroes.length > 0 && heroes.every(h => h.isDead) && day > 0) || tavernHealth <= 0) {
        const finalHeroesWithStats = initialHeroes.map(hero => {
            const stats = heroStatsRef.current.get(hero.id) || { kills: 0, damageDealt: 0 };
            return {
                ...hero,
                ...stats,
                damageDealt: Math.round(stats.damageDealt),
                isDead: heroes.find(h => h.id === hero.id)?.isDead || hero.isDead,
                health: heroes.find(h => h.id === hero.id)?.health || 0,
            };
        });
        onGameOver(finalHeroesWithStats);
    }
  }, [enemies, heroes, spawnedEnemiesCount.current, enemiesToSpawn, onWaveComplete, goldEarned, day, onGameOver, killsInWave, tavernHealth, initialHeroes]);

  const gameLoop = useCallback(() => {
    if (tavernHealth <= 0) {
        return;
    }

    // --- This is a "snapshot" of the state at the beginning of the frame ---
    const currentHeroes = heroes;
    const currentEnemies = enemies;
    const currentProjectiles = projectiles;
    
    // --- Calculate buffs from active skills ---
    const totalDamageBuff = activeSkills
      .filter(s => s.name === 'Battle Cry' && s.buff?.type === 'damage')
      .reduce((sum, s) => sum + (s.buff?.value || 0), 0);

    // --- Damage and event buffers for this frame ---
    const heroDamageBuffer = new Map<string, number>();
    const enemyDamageBuffer = new Map<string, { damage: number; casterId: string }[]>();
    let tavernDamageThisFrame = 0;
    const newProjectiles: ProjectileType[] = [];

    // --- STAGE 1: Process movements and attacks, populating buffers ---

    // 1a. Process Enemies: Movement and Attacks
    const nextEnemyStates = currentEnemies.map(enemy => {
        let targetPosition: { x: number; y: number } = { x: 50, y: 95 }; // Tavern position
        let isAttacking = false;
        
        let nextEnemyState = {...enemy};

        let targetHero: (typeof currentHeroes[0]) | undefined = undefined;
        let potentialTarget = currentHeroes.find(h => h.id === nextEnemyState.targetHeroId && !h.isDead && h.id !== draggedHero);

        if (potentialTarget && calculateDistance(nextEnemyState.position, potentialTarget.position) <= nextEnemyState.aggroRange) {
            targetHero = potentialTarget;
        } else {
            let closestDist = Infinity;
            currentHeroes.filter(h => !h.isDead && h.id !== draggedHero).forEach(h => {
                const dist = calculateDistance(nextEnemyState.position, h.position);
                if (dist < nextEnemyState.aggroRange && dist < closestDist) {
                    closestDist = dist;
                    targetHero = h;
                }
            });
        }
        
        nextEnemyState.targetHeroId = targetHero ? targetHero.id : null;

        if (targetHero) {
            const distToHero = calculateDistance(nextEnemyState.position, targetHero.position);
            if (distToHero <= nextEnemyState.attackRange) {
                isAttacking = true;
                const damagePerFrame = (nextEnemyState.damage * nextEnemyState.attackSpeed) / 30; // 30 FPS
                const currentDamage = heroDamageBuffer.get(targetHero.id) || 0;
                heroDamageBuffer.set(targetHero.id, currentDamage + damagePerFrame);
            } else {
                targetPosition = targetHero.position;
            }
        }

        if (!isAttacking) {
            const dx = targetPosition.x - nextEnemyState.position.x;
            const dy = targetPosition.y - nextEnemyState.position.y;
            const dist = Math.sqrt(dx * dx + dy * dy);
            if (dist > 1) {
                nextEnemyState.position.x += (dx / dist) * nextEnemyState.speed;
                nextEnemyState.position.y += (dy / dist) * nextEnemyState.speed;
            }
        }
        
        if (calculateDistance(nextEnemyState.position, {x: 50, y: 95}) < 5) {
            tavernDamageThisFrame += nextEnemyState.damage;
            nextEnemyState.health = 0; // Mark for removal
        }
        
        return nextEnemyState;
    });


    // 1b. Process Heroes: AI, Attacks, and Movement (Refactored)
    const heroesAfterLogic = currentHeroes.map(hero => {
      let heroToUpdate = { ...hero, attackCooldown: Math.max(0, (hero.attackCooldown || 0) - 1) };
      if (heroToUpdate.isDead || heroToUpdate.id === draggedHero) {
          return heroToUpdate;
      }

      const heroSightRange = (heroToUpdate.range / GAME_AREA_WIDTH) * 100;

      // --- TARGET ACQUISITION (same for all) ---
      let currentTarget = currentEnemies.find(e => e.id === heroToUpdate.targetEnemyId);
      if (!currentTarget || calculateDistance(heroToUpdate.position, currentTarget.position) > heroSightRange) {
          let closestDist = Infinity;
          let newTarget: EnemyType | null = null;
          currentEnemies.forEach(enemy => {
              const dist = calculateDistance(heroToUpdate.position, enemy.position);
              if (dist < heroSightRange && dist < closestDist) {
                  closestDist = dist;
                  newTarget = enemy;
              }
          });
          heroToUpdate = { ...heroToUpdate, targetEnemyId: newTarget ? newTarget.id : null };
          currentTarget = newTarget;
      }

      // --- CLASS-SPECIFIC ACTION (ATTACK & MOVEMENT) ---
      if (heroToUpdate.heroClass === 'Warrior') {
          // --- WARRIOR LOGIC ---
          const meleeRange = heroSightRange / MELEE_RANGE_DIVISOR;

          // a. Movement
          if (heroToUpdate.chargeTarget) {
              const chargeSpeed = 2;
              const distToChargeTarget = calculateDistance(heroToUpdate.position, heroToUpdate.chargeTarget);
              if (distToChargeTarget < chargeSpeed) {
                  heroToUpdate.position = heroToUpdate.chargeTarget;
                  heroToUpdate = { ...heroToUpdate, chargeTarget: undefined };
              } else {
                  const dx = heroToUpdate.chargeTarget.x - heroToUpdate.position.x;
                  const dy = heroToUpdate.chargeTarget.y - heroToUpdate.position.y;
                  heroToUpdate.position.x += (dx / distToChargeTarget) * chargeSpeed;
                  heroToUpdate.position.y += (dy / distToChargeTarget) * chargeSpeed;
              }
          } else if (currentTarget) {
              // Move into melee range if not there
              const distToTarget = calculateDistance(heroToUpdate.position, currentTarget.position);
              if (distToTarget > meleeRange) {
                  const speed = 0.4;
                  const dx = currentTarget.position.x - heroToUpdate.position.x;
                  const dy = currentTarget.position.y - heroToUpdate.position.y;
                  heroToUpdate.position.x += (dx / distToTarget) * speed;
                  heroToUpdate.position.y += (dy / distToTarget) * speed;
              }
          } else {
              // Patrol when idle
              const patrolRadius = 5;
              const speed = 0.1;
              if (!heroToUpdate.patrolTarget || calculateDistance(heroToUpdate.position, heroToUpdate.patrolTarget) < speed * 2) {
                   const newPatrolTarget = {
                      x: heroToUpdate.basePosition.x + (Math.random() - 0.5) * 2 * patrolRadius,
                      y: heroToUpdate.basePosition.y + (Math.random() - 0.5) * 2 * patrolRadius,
                  };
                  heroToUpdate = { ...heroToUpdate, patrolTarget: {
                      x: Math.max(0, Math.min(100, newPatrolTarget.x)),
                      y: Math.max(0, Math.min(100, newPatrolTarget.y)),
                  }};
              }
              if (heroToUpdate.patrolTarget) {
                  const dx = heroToUpdate.patrolTarget.x - heroToUpdate.position.x;
                  const dy = heroToUpdate.patrolTarget.y - heroToUpdate.position.y;
                  const distToPatrol = Math.sqrt(dx*dx + dy*dy);
                  if (distToPatrol > speed) {
                      heroToUpdate.position.x += (dx / distToPatrol) * speed;
                      heroToUpdate.position.y += (dy / distToPatrol) * speed;
                  }
              }
          }
          
          // b. Attack
          if (currentTarget && heroToUpdate.attackCooldown <= 0) {
              const distToTarget = calculateDistance(heroToUpdate.position, currentTarget.position);
              if (distToTarget <= meleeRange) {
                  const damageInstances = enemyDamageBuffer.get(currentTarget.id) || [];
                  damageInstances.push({ casterId: hero.id, damage: heroToUpdate.attackDamage + totalDamageBuff });
                  enemyDamageBuffer.set(currentTarget.id, damageInstances);
                  heroToUpdate = { ...heroToUpdate, attackCooldown: (1 / heroToUpdate.attackSpeed) * 30 };
              }
          }
      } else { 
          // --- RANGED LOGIC (Mage or Archer) ---
          // a. Movement: NONE. They do not move on their own.
          
          // b. Attack
          if (currentTarget && heroToUpdate.attackCooldown <= 0) {
              const distToTarget = calculateDistance(heroToUpdate.position, currentTarget.position);
              if (distToTarget <= heroSightRange) {
                  newProjectiles.push({
                      id: `proj-${Date.now()}-${Math.random()}`,
                      casterId: hero.id,
                      targetId: currentTarget.id,
                      position: { ...heroToUpdate.position },
                      speed: 3,
                      damage: heroToUpdate.attackDamage + totalDamageBuff,
                      visual: hero.heroClass === 'Archer' ? '→' : '🔥'
                  });
                 heroToUpdate = { ...heroToUpdate, attackCooldown: (1 / heroToUpdate.attackSpeed) * 30 };
              }
          }
      }
      return heroToUpdate;
    });

    // 1c. Process Projectiles
    const remainingProjectiles = currentProjectiles.filter(proj => {
        const target = currentEnemies.find(e => e.id === proj.targetId);
        if (!target) return false; // Target is gone

        const dx = target.position.x - proj.position.x;
        const dy = target.position.y - proj.position.y;
        const dist = Math.sqrt(dx * dx + dy * dy);

        if (dist < proj.speed) {
            const damageInstances = enemyDamageBuffer.get(proj.targetId) || [];
            damageInstances.push({ casterId: proj.casterId, damage: proj.damage });
            enemyDamageBuffer.set(proj.targetId, damageInstances);
            return false; // Projectile hits
        } else {
            proj.position.x += (dx / dist) * proj.speed;
            proj.position.y += (dy / dist) * proj.speed;
            return true; // Projectile continues
        }
    });
    
    // 1d. Process AoE skills
    const nextActiveSkills = activeSkills.map(skill => ({ ...skill, duration: (skill.duration || 0) - (1/30) })).filter(skill => (skill.duration || 0) > 0);
    
    nextActiveSkills.forEach(skill => {
        if (skill.damage && skill.duration) {
            const skillRange = (skill.areaOfEffect / GAME_AREA_WIDTH) * 100;
            const damagePerFrame = skill.damage / (skill.duration * 30);
            currentEnemies.forEach(enemy => {
                if (calculateDistance(enemy.position, skill.position) < skillRange) {
                    const damageInstances = enemyDamageBuffer.get(enemy.id) || [];
                    damageInstances.push({ casterId: skill.casterId, damage: damagePerFrame });
                    enemyDamageBuffer.set(enemy.id, damageInstances);
                }
            });
        }
    });
    

    // --- STAGE 2: APPLY DAMAGES AND HEALING, and filter out the dead ---
    
    const nextEnemies = nextEnemyStates.map(enemy => {
        let newHealth = enemy.health;
        const damageToApply = enemyDamageBuffer.get(enemy.id);
        if (damageToApply) {
            damageToApply.forEach(instance => {
                newHealth -= instance.damage;
                const stats = heroStatsRef.current.get(instance.casterId);
                if (stats) {
                  stats.damageDealt = (stats.damageDealt || 0) + instance.damage;
                }
            });
        }
        
        if (enemy.health > 0 && newHealth <= 0) {
            setGoldEarned(g => g + 5);
            setKillsInWave(k => k + 1);
            const killer = damageToApply?.sort((a,b) => b.damage - a.damage)[0];
            if (killer) {
              const stats = heroStatsRef.current.get(killer.casterId);
              if (stats) stats.kills += 1;
            }
        }
        
        return { ...enemy, health: newHealth };
    }).filter(e => e.health > 0);

    const nextHeroes = heroesAfterLogic.map(hero => {
        if (hero.isDead) return hero;

        let newHealth = hero.health;
        const damageToTake = heroDamageBuffer.get(hero.id);
        if (damageToTake) {
            newHealth -= damageToTake;
        }

        nextActiveSkills.forEach(skill => {
            if (skill.healing && skill.duration) {
                const skillRange = (skill.areaOfEffect / GAME_AREA_WIDTH) * 100;
                if (calculateDistance(hero.position, skill.position) < skillRange) {
                    const healingPerFrame = skill.healing / (skill.duration * 30);
                    newHealth = Math.min(hero.maxHealth, newHealth + healingPerFrame);
                }
            }
        });
        
        if (newHealth <= 0) {
            return { ...hero, health: 0, isDead: true, targetEnemyId: null };
        }
        
        return { ...hero, health: newHealth };
    });

    // --- STAGE 3: COMMIT THE NEW STATE ---
    setEnemies(nextEnemies);
    setHeroes(nextHeroes);
    setProjectiles([...remainingProjectiles, ...newProjectiles]);
    setActiveSkills(nextActiveSkills);
    if (tavernDamageThisFrame > 0) {
        setTavernHealth(th => Math.max(0, th - tavernDamageThisFrame));
    }

  }, [heroes, enemies, projectiles, activeSkills, onGameOver, tavernHealth, draggedHero, onWaveComplete, goldEarned, killsInWave, day, setTavernHealth]);

  useEffect(() => {
    const loopId = setInterval(gameLoop, (1000 / 30) / gameSpeed);
    return () => clearInterval(loopId);
  }, [gameLoop, gameSpeed]);

  const handleUseSkill = useCallback((skill: ActiveSkill) => {
    console.log(`Player action: Use skill '${skill.name}'.`);
    
    let skillInstance = {...skill};

    if (skill.name === 'Whirlwind') {
      const caster = heroes.find(h => h.id === skill.casterId);
      if (caster) {
        skillInstance.position = caster.position;
      }
    }

    if (skill.name === 'Charge') {
      setHeroes(currentHeroes => currentHeroes.map(h => {
        if (h.id === skill.casterId) {
          console.log(`Hero ${h.heroClass} (${h.id.slice(0,5)}) is charging to (${skill.position.x.toFixed(2)}, ${skill.position.y.toFixed(2)}).`);
          return { ...h, chargeTarget: skill.position };
        }
        return h;
      }));
    }

    if (skillInstance.healing && !skillInstance.duration) {
      console.log(`Applying instant healing for skill: ${skillInstance.name}`);
      
      setHeroes(currentHeroes => {
        const skillRange = (skillInstance.areaOfEffect / GAME_AREA_WIDTH) * 100;
        
        const newHeroes = currentHeroes.map(hero => {
          if (hero.isDead) return hero;
          const distance = calculateDistance(hero.position, skillInstance.position);
          if (distance < skillRange) {
            const newHealth = Math.min(hero.maxHealth, hero.health + skillInstance.healing!);
            console.log(`Healing Log: Skill '${skillInstance.name}' instantly healed ${hero.heroClass} (${hero.id.slice(0,5)}) for ${skillInstance.healing} HP.`);
            return { ...hero, health: newHealth };
          }
          return hero;
        });
        
        return newHeroes;
      });
    } else if (skillInstance.duration && (skillInstance.healing || skillInstance.damage || skillInstance.buff)) {
        setActiveSkills(prev => [...prev, skillInstance]);
    }


    if (skillInstance.damage && !skillInstance.duration) {
      console.log(`Applying instant damage for skill: ${skillInstance.name}`);
      
      setEnemies(currentEnemies => {
        const skillRange = (skillInstance.areaOfEffect / GAME_AREA_WIDTH) * 100;
        
        const potentialTargets = currentEnemies
          .map(enemy => ({
            enemy,
            distance: Math.sqrt(Math.pow(enemy.position.x - skillInstance.position.x, 2) + Math.pow(enemy.position.y - skillInstance.position.y, 2))
          }))
          .filter(({ distance }) => distance < (skillInstance.areaOfEffect === 0 ? 10 : skillRange))
          .sort((a, b) => a.distance - b.distance);

        const maxTargets = skillInstance.maxTargets || (skillInstance.areaOfEffect === 0 ? 1 : potentialTargets.length);
        const finalTargetIds = potentialTargets.slice(0, maxTargets).map(t => t.enemy.id);

        let goldFromSkill = 0;
        const newEnemies = currentEnemies.map(enemy => {
          if (finalTargetIds.includes(enemy.id)) {
            const newHealth = enemy.health - skillInstance.damage!;
            const stats = heroStatsRef.current.get(skillInstance.casterId);
            if (stats) {
                stats.damageDealt = Number(stats.damageDealt || 0) + skillInstance.damage!;
            }
            console.log(`Damage Log: Skill '${skillInstance.name}' dealt ${skillInstance.damage} damage to Enemy (${enemy.id.slice(0,5)}). New health: ${newHealth.toFixed(0)}`);
            if (enemy.health > 0 && newHealth <= 0) {
              goldFromSkill += 5;
              if (stats) {
                  stats.kills += 1;
              }
              setKillsInWave(k => k + 1);
              console.log(`Enemy killed by skill '${skillInstance.name}'.`);
            }
            return { ...enemy, health: newHealth };
          }
          return enemy;
        });
        
        if (goldFromSkill > 0) {
          setGoldEarned(g => g + goldFromSkill);
          console.log(`Earned ${goldFromSkill} gold from skill.`);
        }
        
        return newEnemies.filter((e): e is EnemyType => e.health > 0);
      });
    }
  }, [heroes, setEnemies, setGoldEarned, setHeroes, setKillsInWave]);

  const handleMouseDownOnHero = (heroId: string, e: React.MouseEvent) => {
    e.stopPropagation();
    console.log(`Player action: Start dragging hero ${heroId}.`);
    setDraggedHero(heroId);
  };

  const handleMouseUp = () => {
    if (draggedHero) {
      console.log(`Player action: Finished dragging hero ${draggedHero}.`);
      setHeroes(currentHeroes => currentHeroes.map(h =>
        h.id === draggedHero ? { ...h, basePosition: h.position, targetEnemyId: null } : h
      ));
      setDraggedHero(null);
    }
  };

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!draggedHero || !gameAreaRef.current) return;

    const rect = gameAreaRef.current.getBoundingClientRect();
    const x = ((e.clientX - rect.left) / rect.width) * 100;
    const y = ((e.clientY - rect.top) / rect.height) * 100;

    const clampedX = Math.max(0, Math.min(100, x));
    const clampedY = Math.max(0, Math.min(100, y));

    setHeroes(currentHeroes => currentHeroes.map(h =>
      h.id === draggedHero ? { ...h, position: { x: clampedX, y: clampedY } } : h
    ));
  };


  return (
    <div
      ref={gameAreaRef}
      onMouseMove={handleMouseMove}
      onMouseUp={handleMouseUp}
      onMouseLeave={handleMouseUp}
      className="w-full h-full bg-emerald-900/20 border-4 border-yellow-900/50 rounded-lg shadow-2xl relative overflow-hidden font-headline cursor-default"
    >
      <div className="absolute inset-0 bg-[radial-gradient(circle,transparent_60%,hsl(var(--background)/0.3))]"></div>
      <GameUI 
        day={day} 
        gold={gold} 
        tavernHealth={tavernHealth} 
        maxTavernHealth={1000}
        gameSpeed={gameSpeed}
        setGameSpeed={setGameSpeed}
        totalKills={totalKills + killsInWave}
      />
      
      <Tavern />

      {heroes.map(hero => !hero.isDead && (
        <Hero 
          key={hero.id} 
          hero={hero} 
          onMouseDown={handleMouseDownOnHero}
          isBeingDragged={draggedHero === hero.id}
        />
      ))}
      {enemies.map(enemy => <Enemy key={enemy.id} enemy={enemy} />)}
      {projectiles.map(proj => <Projectile key={proj.id} projectile={proj} />)}

      {activeSkills.map(skill => (
        <div 
          key={`${skill.id}-${skill.timestamp}`}
          className="absolute bg-primary/20 border-2 border-primary rounded-full animate-pulse"
          style={{
            left: `${skill.position.x}%`,
            top: `${skill.position.y}%`,
            width: `${(skill.areaOfEffect / GAME_AREA_WIDTH) * 100 * 2}%`,
            height: `${(skill.areaOfEffect / GAME_AREA_HEIGHT) * 100 * 2}%`,
            transform: 'translate(-50%, -50%)',
            pointerEvents: 'none',
          }}
        />
      ))}

      <SkillBar heroes={heroes} onUseSkill={handleUseSkill} gameAreaRef={gameAreaRef} />
    </div>
  );
}
