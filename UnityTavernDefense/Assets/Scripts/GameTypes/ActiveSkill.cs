using UnityEngine;

namespace TavernDefense
{
    [System.Serializable]
    public class ActiveSkill : Skill
    {
        public string casterId;
        public Vector2 position;
        public float timestamp;
    }
}