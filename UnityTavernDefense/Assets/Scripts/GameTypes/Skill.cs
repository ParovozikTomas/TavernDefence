using UnityEngine;

namespace TavernDefense
{
    [System.Serializable]
    public class Skill
    {
        public string id;
        public string name;
        public string description;
        public string icon;
        public float cooldown;
        public float areaOfEffect; // radius, 0 for single target
        public float damage;
        public float healing;
        public float duration; // duration of the skill effect in seconds
        public int maxTargets; // for skills like chain lightning
        public Buff buff;
    }

    [System.Serializable]
    public class Buff
    {
        public string type; // "damage"
        public float value;
    }
}