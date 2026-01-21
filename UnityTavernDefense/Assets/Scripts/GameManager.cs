using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TavernDefense
{
    public class GameManager : MonoBehaviour
    {
        // Constants
        private const float GAME_AREA_WIDTH = 1280f;
        private const float GAME_AREA_HEIGHT = 720f;
        private const int MELEE_RANGE_DIVISOR = 4; // Warrior's attack range is sight range / this
        
        // Game state
        public List<Hero> heroes;
        public List<Enemy> enemies;
        public List<Projectile> projectiles;
        public List<ActiveSkill> activeSkills;
        
        // Game properties
        public int day = 1;
        public int gold = 0;
        public float tavernHealth = 1000f;
        public float maxTavernHealth = 1000f;
        public float gameSpeed = 1f;
        public int totalKills = 0;
        public int goldEarned = 0;
        public int killsInWave = 0;
        
        // Event callbacks
        public Action<int, List<Hero>, int> OnWaveComplete;
        public Action<List<Hero>> OnGameOver;
        
        // Internal state
        private int enemiesToSpawn;
        private int spawnedEnemiesCount;
        private string draggedHeroId;
        private Dictionary<string, HeroStats> heroStatsMap = new Dictionary<string, HeroStats>();
        
        // Helper classes
        [Serializable]
        private class HeroStats
        {
            public int kills;
            public float damageDealt;
        }
        
        void Start()
        {
            InitializeGame();
        }

        void Update()
        {
            RunGameLoop();
        }

        private void InitializeGame()
        {
            // Initialize lists
            heroes = new List<Hero>();
            enemies = new List<Enemy>();
            projectiles = new List<Projectile>();
            activeSkills = new List<ActiveSkill>();
            
            // Initialize hero stats
            foreach (var hero in heroes)
            {
                if (!heroStatsMap.ContainsKey(hero.id))
                {
                    heroStatsMap[hero.id] = new HeroStats
                    {
                        kills = hero.kills,
                        damageDealt = hero.damageDealt
                    };
                }
            }
        }

        public void SpawnEnemy()
        {
            // Determine spawn side (0: top, 1: left, 2: right)
            int side = UnityEngine.Random.Range(0, 3);
            Vector2 position;

            switch (side)
            {
                case 0: // Top
                    position = new Vector2(UnityEngine.Random.Range(0f, 100f), 0f);
                    break;
                case 1: // Left
                    position = new Vector2(0f, UnityEngine.Random.Range(0f, 80f)); // Not too low
                    break;
                default: // Right
                    position = new Vector2(100f, UnityEngine.Random.Range(0f, 80f));
                    break;
            }

            string enemyId = $"enemy-{DateTime.Now.Ticks}-{UnityEngine.Random.value}";
            float health = 20 + day * 5;

            var enemy = new Enemy
            {
                id = enemyId,
                position = position,
                health = health,
                maxHealth = health,
                speed = 0.1f + day * 0.02f,
                damage = 5 + day * 2,
                attackRange = 4f, // Percent of game area width
                attackSpeed = 0.5f, // Attacks per second
                aggroRange = 20f // Percent of game area width
            };

            enemies.Add(enemy);
            spawnedEnemiesCount++;
        }

        public void StartWave()
        {
            enemiesToSpawn = day * 5 + 5;
            spawnedEnemiesCount = 0;
            
            // Spawn enemies over time
            InvokeRepeating("SpawnEnemy", 0f, (2000f / (1 + day * 0.1f)) / gameSpeed);
        }

        private void RunGameLoop()
        {
            if (tavernHealth <= 0)
            {
                return;
            }

            // Calculate buffs from active skills
            float totalDamageBuff = activeSkills
                .Where(s => s.name == "Battle Cry" && s.buff != null && s.buff.type == "damage")
                .Sum(s => s.buff.value);

            // Damage and event buffers for this frame
            Dictionary<string, float> heroDamageBuffer = new Dictionary<string, float>();
            Dictionary<string, List<DamageInstance>> enemyDamageBuffer = new Dictionary<string, List<DamageInstance>>();
            float tavernDamageThisFrame = 0f;
            List<Projectile> newProjectiles = new List<Projectile>();

            // Process Enemies: Movement and Attacks
            var nextEnemyStates = new List<Enemy>();
            foreach (var enemy in enemies)
            {
                Vector2 targetPosition = new Vector2(50f, 95f); // Tavern position
                bool isAttacking = false;

                Enemy nextEnemyState = new Enemy
                {
                    id = enemy.id,
                    position = enemy.position,
                    health = enemy.health,
                    maxHealth = enemy.maxHealth,
                    speed = enemy.speed,
                    damage = enemy.damage,
                    attackRange = enemy.attackRange,
                    attackSpeed = enemy.attackSpeed,
                    aggroRange = enemy.aggroRange,
                    targetHeroId = enemy.targetHeroId
                };

                Hero targetHero = null;
                Hero potentialTarget = heroes.FirstOrDefault(h => h.id == nextEnemyState.targetHeroId && !h.isDead && h.id != draggedHeroId);

                if (potentialTarget != null && CalculateDistance(nextEnemyState.position, potentialTarget.position) <= nextEnemyState.aggroRange)
                {
                    targetHero = potentialTarget;
                }
                else
                {
                    float closestDist = float.MaxValue;
                    foreach (var hero in heroes.Where(h => !h.isDead && h.id != draggedHeroId))
                    {
                        float dist = CalculateDistance(nextEnemyState.position, hero.position);
                        if (dist < nextEnemyState.aggroRange && dist < closestDist)
                        {
                            closestDist = dist;
                            targetHero = hero;
                        }
                    }
                }

                nextEnemyState.targetHeroId = targetHero != null ? targetHero.id : null;

                if (targetHero != null)
                {
                    float distToHero = CalculateDistance(nextEnemyState.position, targetHero.position);
                    if (distToHero <= nextEnemyState.attackRange)
                    {
                        isAttacking = true;
                        float damagePerFrame = (nextEnemyState.damage * nextEnemyState.attackSpeed) / 30f; // 30 FPS
                        float currentDamage = heroDamageBuffer.ContainsKey(targetHero.id) ? heroDamageBuffer[targetHero.id] : 0f;
                        heroDamageBuffer[targetHero.id] = currentDamage + damagePerFrame;
                    }
                    else
                    {
                        targetPosition = targetHero.position;
                    }
                }

                if (!isAttacking)
                {
                    float dx = targetPosition.x - nextEnemyState.position.x;
                    float dy = targetPosition.y - nextEnemyState.position.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (dist > 1)
                    {
                        nextEnemyState.position.x += (dx / dist) * nextEnemyState.speed;
                        nextEnemyState.position.y += (dy / dist) * nextEnemyState.speed;
                    }
                }

                if (CalculateDistance(nextEnemyState.position, new Vector2(50f, 95f)) < 5)
                {
                    tavernDamageThisFrame += nextEnemyState.damage;
                    nextEnemyState.health = 0; // Mark for removal
                }

                nextEnemyStates.Add(nextEnemyState);
            }

            // Process Heroes: AI, Attacks, and Movement
            var heroesAfterLogic = new List<Hero>();
            foreach (var hero in heroes)
            {
                Hero heroToUpdate = new Hero
                {
                    id = hero.id,
                    heroClass = hero.heroClass,
                    level = hero.level,
                    skill = hero.skill,
                    position = hero.position,
                    basePosition = hero.basePosition,
                    health = hero.health,
                    maxHealth = hero.maxHealth,
                    attackDamage = hero.attackDamage,
                    attackSpeed = hero.attackSpeed,
                    range = hero.range,
                    upgrades = hero.upgrades,
                    damageDealt = hero.damageDealt,
                    kills = hero.kills,
                    isDead = hero.isDead,
                    targetEnemyId = hero.targetEnemyId,
                    attackCooldown = Mathf.Max(0, hero.attackCooldown - 1),
                    patrolTarget = hero.patrolTarget,
                    chargeTarget = hero.chargeTarget
                };

                if (heroToUpdate.isDead || heroToUpdate.id == draggedHeroId)
                {
                    heroesAfterLogic.Add(heroToUpdate);
                    continue;
                }

                float heroSightRange = (heroToUpdate.range / GAME_AREA_WIDTH) * 100f;

                // Target Acquisition
                Enemy currentTarget = nextEnemyStates.FirstOrDefault(e => e.id == heroToUpdate.targetEnemyId);
                if (currentTarget == null || CalculateDistance(heroToUpdate.position, currentTarget.position) > heroSightRange)
                {
                    float closestDist = float.MaxValue;
                    Enemy newTarget = null;
                    foreach (var enemy in nextEnemyStates)
                    {
                        float dist = CalculateDistance(heroToUpdate.position, enemy.position);
                        if (dist < heroSightRange && dist < closestDist)
                        {
                            closestDist = dist;
                            newTarget = enemy;
                        }
                    }
                    heroToUpdate.targetEnemyId = newTarget != null ? newTarget.id : null;
                    currentTarget = newTarget;
                }

                // Class-specific actions
                if (heroToUpdate.heroClass == HeroClass.Warrior)
                {
                    // Warrior Logic
                    float meleeRange = heroSightRange / MELEE_RANGE_DIVISOR;

                    // Movement
                    if (heroToUpdate.chargeTarget.HasValue)
                    {
                        float chargeSpeed = 2f;
                        float distToChargeTarget = CalculateDistance(heroToUpdate.position, heroToUpdate.chargeTarget.Value);
                        
                        if (distToChargeTarget < chargeSpeed)
                        {
                            heroToUpdate.position = heroToUpdate.chargeTarget.Value;
                            heroToUpdate.chargeTarget = null;
                        }
                        else
                        {
                            float dx = heroToUpdate.chargeTarget.Value.x - heroToUpdate.position.x;
                            float dy = heroToUpdate.chargeTarget.Value.y - heroToUpdate.position.y;
                            heroToUpdate.position.x += (dx / distToChargeTarget) * chargeSpeed;
                            heroToUpdate.position.y += (dy / distToChargeTarget) * chargeSpeed;
                        }
                    }
                    else if (currentTarget != null)
                    {
                        // Move into melee range if not there
                        float distToTarget = CalculateDistance(heroToUpdate.position, currentTarget.position);
                        if (distToTarget > meleeRange)
                        {
                            float speed = 0.4f;
                            float dx = currentTarget.position.x - heroToUpdate.position.x;
                            float dy = currentTarget.position.y - heroToUpdate.position.y;
                            heroToUpdate.position.x += (dx / distToTarget) * speed;
                            heroToUpdate.position.y += (dy / distToTarget) * speed;
                        }
                    }
                    else
                    {
                        // Patrol when idle
                        float patrolRadius = 5f;
                        float speed = 0.1f;
                        
                        if (!heroToUpdate.patrolTarget.HasValue || CalculateDistance(heroToUpdate.position, heroToUpdate.patrolTarget.Value) < speed * 2)
                        {
                            Vector2 newPatrolTarget = new Vector2(
                                heroToUpdate.basePosition.x + (UnityEngine.Random.value - 0.5f) * 2 * patrolRadius,
                                heroToUpdate.basePosition.y + (UnityEngine.Random.value - 0.5f) * 2 * patrolRadius
                            );
                            
                            heroToUpdate.patrolTarget = new Vector2(
                                Mathf.Clamp(newPatrolTarget.x, 0, 100),
                                Mathf.Clamp(newPatrolTarget.y, 0, 100)
                            );
                        }
                        
                        if (heroToUpdate.patrolTarget.HasValue)
                        {
                            float dx = heroToUpdate.patrolTarget.Value.x - heroToUpdate.position.x;
                            float dy = heroToUpdate.patrolTarget.Value.y - heroToUpdate.position.y;
                            float distToPatrol = Mathf.Sqrt(dx * dx + dy * dy);
                            
                            if (distToPatrol > speed)
                            {
                                heroToUpdate.position.x += (dx / distToPatrol) * speed;
                                heroToUpdate.position.y += (dy / distToPatrol) * speed;
                            }
                        }
                    }

                    // Attack
                    if (currentTarget != null && heroToUpdate.attackCooldown <= 0)
                    {
                        float distToTarget = CalculateDistance(heroToUpdate.position, currentTarget.position);
                        if (distToTarget <= meleeRange)
                        {
                            if (!enemyDamageBuffer.ContainsKey(currentTarget.id))
                                enemyDamageBuffer[currentTarget.id] = new List<DamageInstance>();
                            
                            enemyDamageBuffer[currentTarget.id].Add(new DamageInstance
                            {
                                casterId = hero.id,
                                damage = heroToUpdate.attackDamage + totalDamageBuff
                            });
                            
                            heroToUpdate.attackCooldown = (1 / heroToUpdate.attackSpeed) * 30f;
                        }
                    }
                }
                else
                {
                    // Ranged Logic (Mage or Archer)
                    // No movement for ranged heroes
                    
                    // Attack
                    if (currentTarget != null && heroToUpdate.attackCooldown <= 0)
                    {
                        float distToTarget = CalculateDistance(heroToUpdate.position, currentTarget.position);
                        if (distToTarget <= heroSightRange)
                        {
                            string visual = hero.heroClass == HeroClass.Archer ? "→" : "🔥";
                            
                            newProjectiles.Add(new Projectile
                            {
                                id = $"proj-{DateTime.Now.Ticks}-{UnityEngine.Random.value}",
                                casterId = hero.id,
                                targetId = currentTarget.id,
                                position = heroToUpdate.position,
                                speed = 3f,
                                damage = heroToUpdate.attackDamage + totalDamageBuff,
                                visual = visual
                            });
                            
                            heroToUpdate.attackCooldown = (1 / heroToUpdate.attackSpeed) * 30f;
                        }
                    }
                }

                heroesAfterLogic.Add(heroToUpdate);
            }

            // Process Projectiles
            var remainingProjectiles = new List<Projectile>();
            foreach (var proj in projectiles)
            {
                Enemy target = nextEnemyStates.FirstOrDefault(e => e.id == proj.targetId);
                if (target == null) continue; // Target is gone

                float dx = target.position.x - proj.position.x;
                float dy = target.position.y - proj.position.y;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist < proj.speed)
                {
                    // Projectile hits target
                    if (!enemyDamageBuffer.ContainsKey(proj.targetId))
                        enemyDamageBuffer[proj.targetId] = new List<DamageInstance>();
                    
                    enemyDamageBuffer[proj.targetId].Add(new DamageInstance
                    {
                        casterId = proj.casterId,
                        damage = proj.damage
                    });
                }
                else
                {
                    // Projectile continues
                    Projectile updatedProj = new Projectile
                    {
                        id = proj.id,
                        casterId = proj.casterId,
                        targetId = proj.targetId,
                        position = new Vector2(
                            proj.position.x + (dx / dist) * proj.speed,
                            proj.position.y + (dy / dist) * proj.speed
                        ),
                        speed = proj.speed,
                        damage = proj.damage,
                        visual = proj.visual
                    };
                    remainingProjectiles.Add(updatedProj);
                }
            }

            // Process AoE skills
            var nextActiveSkills = new List<ActiveSkill>();
            foreach (var skill in activeSkills)
            {
                ActiveSkill updatedSkill = new ActiveSkill
                {
                    id = skill.id,
                    name = skill.name,
                    description = skill.description,
                    icon = skill.icon,
                    cooldown = skill.cooldown,
                    areaOfEffect = skill.areaOfEffect,
                    damage = skill.damage,
                    healing = skill.healing,
                    duration = skill.duration - (1f/30f),
                    maxTargets = skill.maxTargets,
                    buff = skill.buff,
                    casterId = skill.casterId,
                    position = skill.position,
                    timestamp = skill.timestamp
                };
                
                if (updatedSkill.duration > 0)
                {
                    nextActiveSkills.Add(updatedSkill);
                    
                    // Apply AoE effects
                    if (skill.damage > 0 && skill.duration > 0)
                    {
                        float skillRange = (skill.areaOfEffect / GAME_AREA_WIDTH) * 100f;
                        float damagePerFrame = skill.damage / (skill.duration * 30f);
                        
                        foreach (var enemy in nextEnemyStates)
                        {
                            if (CalculateDistance(enemy.position, skill.position) < skillRange)
                            {
                                if (!enemyDamageBuffer.ContainsKey(enemy.id))
                                    enemyDamageBuffer[enemy.id] = new List<DamageInstance>();
                                
                                enemyDamageBuffer[enemy.id].Add(new DamageInstance
                                {
                                    casterId = skill.casterId,
                                    damage = damagePerFrame
                                });
                            }
                        }
                    }
                }
            }

            // Apply damages and healing, filter out the dead
            var nextEnemies = new List<Enemy>();
            foreach (var enemy in nextEnemyStates)
            {
                float newHealth = enemy.health;
                if (enemyDamageBuffer.ContainsKey(enemy.id))
                {
                    foreach (var instance in enemyDamageBuffer[enemy.id])
                    {
                        newHealth -= instance.damage;
                        
                        if (heroStatsMap.ContainsKey(instance.casterId))
                        {
                            heroStatsMap[instance.casterId].damageDealt += instance.damage;
                        }
                    }
                }

                if (enemy.health > 0 && newHealth <= 0)
                {
                    goldEarned += 5;
                    killsInWave++;
                    
                    // Award kill to the highest damage dealer
                    if (enemyDamageBuffer.ContainsKey(enemy.id) && enemyDamageBuffer[enemy.id].Count > 0)
                    {
                        var killer = enemyDamageBuffer[enemy.id].OrderByDescending(d => d.damage).FirstOrDefault();
                        if (heroStatsMap.ContainsKey(killer.casterId))
                        {
                            heroStatsMap[killer.casterId].kills++;
                        }
                    }
                }

                if (newHealth > 0)
                {
                    Enemy updatedEnemy = new Enemy
                    {
                        id = enemy.id,
                        position = enemy.position,
                        health = newHealth,
                        maxHealth = enemy.maxHealth,
                        speed = enemy.speed,
                        damage = enemy.damage,
                        attackRange = enemy.attackRange,
                        attackSpeed = enemy.attackSpeed,
                        aggroRange = enemy.aggroRange,
                        targetHeroId = enemy.targetHeroId
                    };
                    nextEnemies.Add(updatedEnemy);
                }
            }

            var nextHeroes = new List<Hero>();
            foreach (var hero in heroesAfterLogic)
            {
                if (hero.isDead)
                {
                    nextHeroes.Add(hero);
                    continue;
                }

                float newHealth = hero.health;
                if (heroDamageBuffer.ContainsKey(hero.id))
                {
                    newHealth -= heroDamageBuffer[hero.id];
                }

                // Apply healing from active skills
                foreach (var skill in nextActiveSkills)
                {
                    if (skill.healing > 0 && skill.duration > 0)
                    {
                        float skillRange = (skill.areaOfEffect / GAME_AREA_WIDTH) * 100f;
                        if (CalculateDistance(hero.position, skill.position) < skillRange)
                        {
                            float healingPerFrame = skill.healing / (skill.duration * 30f);
                            newHealth = Mathf.Min(hero.maxHealth, newHealth + healingPerFrame);
                        }
                    }
                }

                Hero updatedHero = new Hero
                {
                    id = hero.id,
                    heroClass = hero.heroClass,
                    level = hero.level,
                    skill = hero.skill,
                    position = hero.position,
                    basePosition = hero.basePosition,
                    health = newHealth,
                    maxHealth = hero.maxHealth,
                    attackDamage = hero.attackDamage,
                    attackSpeed = hero.attackSpeed,
                    range = hero.range,
                    upgrades = hero.upgrades,
                    damageDealt = hero.damageDealt,
                    kills = hero.kills,
                    isDead = newHealth <= 0,
                    targetEnemyId = newHealth <= 0 ? null : hero.targetEnemyId,
                    attackCooldown = hero.attackCooldown,
                    patrolTarget = hero.patrolTarget,
                    chargeTarget = hero.chargeTarget
                };

                if (newHealth <= 0)
                {
                    updatedHero.health = 0;
                    updatedHero.isDead = true;
                    updatedHero.targetEnemyId = null;
                }

                nextHeroes.Add(updatedHero);
            }

            // Update state
            enemies = nextEnemies;
            heroes = nextHeroes;
            projectiles = remainingProjectiles.Concat(newProjectiles).ToList();
            activeSkills = nextActiveSkills;

            if (tavernDamageThisFrame > 0)
            {
                tavernHealth = Mathf.Max(0, tavernHealth - tavernDamageThisFrame);
            }

            // Check win/lose conditions
            if (spawnedEnemiesCount >= enemiesToSpawn && enemies.Count == 0 && heroes.Any(h => !h.isDead))
            {
                var finalHeroes = heroes.Select(h => {
                    if (heroStatsMap.ContainsKey(h.id))
                    {
                        var stats = heroStatsMap[h.id];
                        return new Hero
                        {
                            id = h.id,
                            heroClass = h.heroClass,
                            level = h.level,
                            skill = h.skill,
                            position = h.position,
                            basePosition = h.basePosition,
                            health = h.health,
                            maxHealth = h.maxHealth,
                            attackDamage = h.attackDamage,
                            attackSpeed = h.attackSpeed,
                            range = h.range,
                            upgrades = h.upgrades,
                            damageDealt = Mathf.RoundToInt(stats.damageDealt),
                            kills = stats.kills,
                            isDead = h.isDead,
                            targetEnemyId = h.targetEnemyId,
                            attackCooldown = h.attackCooldown,
                            patrolTarget = h.patrolTarget,
                            chargeTarget = h.chargeTarget
                        };
                    }
                    return h;
                }).ToList();

                OnWaveComplete?.Invoke(goldEarned, finalHeroes, killsInWave);
            }
            
            if ((heroes.Count > 0 && heroes.All(h => h.isDead) && day > 0) || tavernHealth <= 0)
            {
                var finalHeroesWithStats = heroes.Select(hero => {
                    var stats = heroStatsMap.ContainsKey(hero.id) ? heroStatsMap[hero.id] : new HeroStats { kills = 0, damageDealt = 0 };
                    var originalHero = heroes.FirstOrDefault(h => h.id == hero.id);
                    
                    return new Hero
                    {
                        id = hero.id,
                        heroClass = hero.heroClass,
                        level = hero.level,
                        skill = hero.skill,
                        position = hero.position,
                        basePosition = hero.basePosition,
                        health = originalHero?.health ?? 0,
                        maxHealth = hero.maxHealth,
                        attackDamage = hero.attackDamage,
                        attackSpeed = hero.attackSpeed,
                        range = hero.range,
                        upgrades = hero.upgrades,
                        damageDealt = Mathf.RoundToInt(stats.damageDealt),
                        kills = stats.kills,
                        isDead = originalHero?.isDead ?? hero.isDead,
                        targetEnemyId = hero.targetEnemyId,
                        attackCooldown = hero.attackCooldown,
                        patrolTarget = hero.patrolTarget,
                        chargeTarget = hero.chargeTarget
                    };
                }).ToList();
                
                OnGameOver?.Invoke(finalHeroesWithStats);
            }
        }

        private struct DamageInstance
        {
            public string casterId;
            public float damage;
        }

        private float CalculateDistance(Vector2 pos1, Vector2 pos2)
        {
            return Mathf.Sqrt(Mathf.Pow(pos1.x - pos2.x, 2) + Mathf.Pow(pos1.y - pos2.y, 2));
        }

        public void UseSkill(ActiveSkill skill)
        {
            Debug.Log($"Player action: Use skill '{skill.name}'.");

            ActiveSkill skillInstance = new ActiveSkill
            {
                id = skill.id,
                name = skill.name,
                description = skill.description,
                icon = skill.icon,
                cooldown = skill.cooldown,
                areaOfEffect = skill.areaOfEffect,
                damage = skill.damage,
                healing = skill.healing,
                duration = skill.duration,
                maxTargets = skill.maxTargets,
                buff = skill.buff,
                casterId = skill.casterId,
                position = skill.position,
                timestamp = skill.timestamp
            };

            // Handle specific skill behaviors
            if (skill.name == "Whirlwind")
            {
                var caster = heroes.FirstOrDefault(h => h.id == skill.casterId);
                if (caster != null)
                {
                    skillInstance.position = caster.position;
                }
            }

            if (skill.name == "Charge")
            {
                for (int i = 0; i < heroes.Count; i++)
                {
                    if (heroes[i].id == skill.casterId)
                    {
                        Debug.Log($"Hero {heroes[i].heroClass} ({heroes[i].id.Substring(0, 5)}) is charging to ({skill.position.x:F2}, {skill.position.y:F2}).");
                        var updatedHero = new Hero
                        {
                            id = heroes[i].id,
                            heroClass = heroes[i].heroClass,
                            level = heroes[i].level,
                            skill = heroes[i].skill,
                            position = heroes[i].position,
                            basePosition = heroes[i].basePosition,
                            health = heroes[i].health,
                            maxHealth = heroes[i].maxHealth,
                            attackDamage = heroes[i].attackDamage,
                            attackSpeed = heroes[i].attackSpeed,
                            range = heroes[i].range,
                            upgrades = heroes[i].upgrades,
                            damageDealt = heroes[i].damageDealt,
                            kills = heroes[i].kills,
                            isDead = heroes[i].isDead,
                            targetEnemyId = heroes[i].targetEnemyId,
                            attackCooldown = heroes[i].attackCooldown,
                            patrolTarget = heroes[i].patrolTarget,
                            chargeTarget = skill.position
                        };
                        heroes[i] = updatedHero;
                        break;
                    }
                }
            }

            if (skillInstance.healing > 0 && skillInstance.duration <= 0)
            {
                Debug.Log($"Applying instant healing for skill: {skillInstance.name}");

                for (int i = 0; i < heroes.Count; i++)
                {
                    if (heroes[i].isDead) continue;
                    
                    float distance = CalculateDistance(heroes[i].position, skillInstance.position);
                    float skillRange = (skillInstance.areaOfEffect / GAME_AREA_WIDTH) * 100f;
                    
                    if (distance < skillRange)
                    {
                        float newHealth = Mathf.Min(heroes[i].maxHealth, heroes[i].health + skillInstance.healing);
                        var updatedHero = new Hero
                        {
                            id = heroes[i].id,
                            heroClass = heroes[i].heroClass,
                            level = heroes[i].level,
                            skill = heroes[i].skill,
                            position = heroes[i].position,
                            basePosition = heroes[i].basePosition,
                            health = newHealth,
                            maxHealth = heroes[i].maxHealth,
                            attackDamage = heroes[i].attackDamage,
                            attackSpeed = heroes[i].attackSpeed,
                            range = heroes[i].range,
                            upgrades = heroes[i].upgrades,
                            damageDealt = heroes[i].damageDealt,
                            kills = heroes[i].kills,
                            isDead = heroes[i].isDead,
                            targetEnemyId = heroes[i].targetEnemyId,
                            attackCooldown = heroes[i].attackCooldown,
                            patrolTarget = heroes[i].patrolTarget,
                            chargeTarget = heroes[i].chargeTarget
                        };
                        heroes[i] = updatedHero;
                        
                        Debug.Log($"Healing Log: Skill '{skillInstance.name}' instantly healed {heroes[i].heroClass} ({heroes[i].id.Substring(0, 5)}) for {skillInstance.healing} HP.");
                    }
                }
            }
            else if (skillInstance.duration > 0 && (skillInstance.healing > 0 || skillInstance.damage > 0 || skillInstance.buff != null))
            {
                activeSkills.Add(skillInstance);
            }

            if (skillInstance.damage > 0 && skillInstance.duration <= 0)
            {
                Debug.Log($"Applying instant damage for skill: {skillInstance.name}");

                var potentialTargets = new List<(Enemy enemy, float distance)>();
                foreach (var enemy in enemies)
                {
                    float distance = Mathf.Sqrt(Mathf.Pow(enemy.position.x - skillInstance.position.x, 2) + Mathf.Pow(enemy.position.y - skillInstance.position.y, 2));
                    potentialTargets.Add((enemy, distance));
                }

                potentialTargets = potentialTargets
                    .Where(t => t.distance < (skillInstance.areaOfEffect == 0 ? 10 : (skillInstance.areaOfEffect / GAME_AREA_WIDTH) * 100f))
                    .OrderBy(t => t.distance)
                    .ToList();

                int maxTargets = skillInstance.maxTargets > 0 ? skillInstance.maxTargets : 
                                (skillInstance.areaOfEffect == 0 ? 1 : potentialTargets.Count);
                
                var finalTargetIds = potentialTargets.Take(maxTargets).Select(t => t.enemy.id).ToList();

                int goldFromSkill = 0;
                var updatedEnemies = new List<Enemy>();
                
                foreach (var enemy in enemies)
                {
                    if (finalTargetIds.Contains(enemy.id))
                    {
                        float newHealth = enemy.health - skillInstance.damage;
                        
                        if (heroStatsMap.ContainsKey(skillInstance.casterId))
                        {
                            heroStatsMap[skillInstance.casterId].damageDealt += skillInstance.damage;
                        }

                        Debug.Log($"Damage Log: Skill '{skillInstance.name}' dealt {skillInstance.damage} damage to Enemy ({enemy.id.Substring(0, 5)}). New health: {newHealth:F0}");

                        if (enemy.health > 0 && newHealth <= 0)
                        {
                            goldFromSkill += 5;
                            
                            if (heroStatsMap.ContainsKey(skillInstance.casterId))
                            {
                                heroStatsMap[skillInstance.casterId].kills++;
                            }
                            
                            killsInWave++;
                            Debug.Log($"Enemy killed by skill '{skillInstance.name}'.");
                        }

                        updatedEnemies.Add(new Enemy
                        {
                            id = enemy.id,
                            position = enemy.position,
                            health = newHealth,
                            maxHealth = enemy.maxHealth,
                            speed = enemy.speed,
                            damage = enemy.damage,
                            attackRange = enemy.attackRange,
                            attackSpeed = enemy.attackSpeed,
                            aggroRange = enemy.aggroRange,
                            targetHeroId = enemy.targetHeroId
                        });
                    }
                    else
                    {
                        updatedEnemies.Add(enemy);
                    }
                }

                enemies = updatedEnemies.Where(e => e.health > 0).ToList();

                if (goldFromSkill > 0)
                {
                    goldEarned += goldFromSkill;
                    Debug.Log($"Earned {goldFromSkill} gold from skill.");
                }
            }
        }

        public void StartDraggingHero(string heroId)
        {
            Debug.Log($"Player action: Start dragging hero {heroId}.");
            draggedHeroId = heroId;
        }

        public void FinishDraggingHero()
        {
            if (!string.IsNullOrEmpty(draggedHeroId))
            {
                Debug.Log($"Player action: Finished dragging hero {draggedHeroId}.");
                
                for (int i = 0; i < heroes.Count; i++)
                {
                    if (heroes[i].id == draggedHeroId)
                    {
                        var updatedHero = new Hero
                        {
                            id = heroes[i].id,
                            heroClass = heroes[i].heroClass,
                            level = heroes[i].level,
                            skill = heroes[i].skill,
                            position = heroes[i].position,
                            basePosition = heroes[i].position, // Set base position to current position
                            health = heroes[i].health,
                            maxHealth = heroes[i].maxHealth,
                            attackDamage = heroes[i].attackDamage,
                            attackSpeed = heroes[i].attackSpeed,
                            range = heroes[i].range,
                            upgrades = heroes[i].upgrades,
                            damageDealt = heroes[i].damageDealt,
                            kills = heroes[i].kills,
                            isDead = heroes[i].isDead,
                            targetEnemyId = null, // Clear target when moved
                            attackCooldown = heroes[i].attackCooldown,
                            patrolTarget = heroes[i].patrolTarget,
                            chargeTarget = heroes[i].chargeTarget
                        };
                        heroes[i] = updatedHero;
                        break;
                    }
                }
                
                draggedHeroId = null;
            }
        }

        public void UpdateHeroPosition(string heroId, Vector2 newPosition)
        {
            if (draggedHeroId == heroId)
            {
                for (int i = 0; i < heroes.Count; i++)
                {
                    if (heroes[i].id == heroId)
                    {
                        var updatedHero = new Hero
                        {
                            id = heroes[i].id,
                            heroClass = heroes[i].heroClass,
                            level = heroes[i].level,
                            skill = heroes[i].skill,
                            position = newPosition,
                            basePosition = heroes[i].basePosition,
                            health = heroes[i].health,
                            maxHealth = heroes[i].maxHealth,
                            attackDamage = heroes[i].attackDamage,
                            attackSpeed = heroes[i].attackSpeed,
                            range = heroes[i].range,
                            upgrades = heroes[i].upgrades,
                            damageDealt = heroes[i].damageDealt,
                            kills = heroes[i].kills,
                            isDead = heroes[i].isDead,
                            targetEnemyId = heroes[i].targetEnemyId,
                            attackCooldown = heroes[i].attackCooldown,
                            patrolTarget = heroes[i].patrolTarget,
                            chargeTarget = heroes[i].chargeTarget
                        };
                        heroes[i] = updatedHero;
                        break;
                    }
                }
            }
        }
    }
}