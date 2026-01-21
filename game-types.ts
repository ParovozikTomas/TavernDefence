export type HeroClass = 'Archer' | 'Warrior' | 'Mage';

export interface Skill {
  id: string;
  name: string;
  description: string;
  icon: string;
  cooldown: number;
  areaOfEffect: number; // radius, 0 for single target
  damage?: number;
  healing?: number;
  duration?: number; // duration of the skill effect in seconds
  maxTargets?: number; // for skills like chain lightning
  buff?: { type: 'damage'; value: number };
}

export interface Hero {
  id: string;
  heroClass: HeroClass;
  level: number;
  skill: Skill;
  position: { x: number; y: number };
  basePosition: { x: number; y: number };
  health: number;
  maxHealth: number;
  attackDamage: number;
  attackSpeed: number;
  range: number;
  upgrades: {
    damage: number;
    health: number;
    speed: number;
  };
  damageDealt: number;
  kills: number;
  isDead?: boolean;
  targetEnemyId?: string | null;
  attackCooldown?: number;
}

export interface Enemy {
  id:string;
  position: { x: number; y: number };
  health: number;
  maxHealth: number;
  speed: number;
  damage: number;
  attackRange: number;
  attackSpeed: number;
  aggroRange: number;
  targetHeroId?: string | null;
}

export interface ActiveSkill extends Skill {
  casterId: string;
  position: { x: number, y: number };
  timestamp: number;
}

export interface Projectile {
  id: string;
  casterId: string;
  targetId: string;
  position: { x: number; y: number };
  speed: number;
  damage: number;
  visual: string;
}
