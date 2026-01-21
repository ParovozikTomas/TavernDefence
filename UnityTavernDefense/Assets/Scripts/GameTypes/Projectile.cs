using UnityEngine;

namespace TavernDefense
{
    [System.Serializable]
    public class Projectile
    {
        public string id;
        public string casterId;
        public string targetId;
        public Vector2 position;
        public float speed;
        public float damage;
        public string visual;
    }
}