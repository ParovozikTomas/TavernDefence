using UnityEngine;

namespace TavernDefense
{
    [System.Serializable]
    public class Enemy
    {
        public string id;
        public Vector2 position;
        public float health;
        public float maxHealth;
        public float speed;
        public float damage;
        public float attackRange;
        public float attackSpeed;
        public float aggroRange;
        public string targetHeroId;

        public Enemy()
        {
            targetHeroId = null;
        }
    }
}