using UnityEngine;

namespace TavernDefense
{
    [System.Serializable]
    public class Hero
    {
        public string id;
        public HeroClass heroClass;
        public int level;
        public Skill skill;
        public Vector2 position;
        public Vector2 basePosition;
        public float health;
        public float maxHealth;
        public float attackDamage;
        public float attackSpeed;
        public float range;
        public HeroUpgrades upgrades;
        public float damageDealt;
        public int kills;
        public bool isDead;
        public string targetEnemyId;
        public float attackCooldown;

        // Additional fields for Unity implementation
        public Vector2? patrolTarget;
        public Vector2? chargeTarget;

        public Hero()
        {
            upgrades = new HeroUpgrades();
            isDead = false;
            targetEnemyId = null;
            attackCooldown = 0f;
        }
    }

    [System.Serializable]
    public class HeroUpgrades
    {
        public int damage;
        public int health;
        public int speed;
    }
}