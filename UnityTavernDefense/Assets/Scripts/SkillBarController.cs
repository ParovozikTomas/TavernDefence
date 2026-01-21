using System;
using System.Collections.Generic;
using UnityEngine;

namespace TavernDefense
{
    public class SkillBarController : MonoBehaviour
    {
        // References
        private GameManager gameManager;
        
        // Event callback
        public Action<ActiveSkill> OnUseSkill;

        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        public void UseHeroSkill(int heroIndex)
        {
            if (gameManager != null && heroIndex >= 0 && heroIndex < gameManager.heroes.Count)
            {
                var hero = gameManager.heroes[heroIndex];
                
                if (hero != null && !hero.isDead)
                {
                    // Create an ActiveSkill instance from the hero's skill
                    var activeSkill = new ActiveSkill
                    {
                        id = hero.skill.id,
                        name = hero.skill.name,
                        description = hero.skill.description,
                        icon = hero.skill.icon,
                        cooldown = hero.skill.cooldown,
                        areaOfEffect = hero.skill.areaOfEffect,
                        damage = hero.skill.damage,
                        healing = hero.skill.healing,
                        duration = hero.skill.duration,
                        maxTargets = hero.skill.maxTargets,
                        buff = hero.skill.buff,
                        casterId = hero.id,
                        position = hero.position,
                        timestamp = Time.time
                    };
                    
                    OnUseSkill?.Invoke(activeSkill);
                }
            }
        }
        
        public void UseSkillAtPosition(ActiveSkill skill, Vector2 position)
        {
            if (gameManager != null)
            {
                var activeSkill = new ActiveSkill
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
                    position = position,
                    timestamp = Time.time
                };
                
                OnUseSkill?.Invoke(activeSkill);
            }
        }
        
        public void UseSkillWithTarget(ActiveSkill skill, string targetId)
        {
            // This method could be used for targeted skills
            // Implementation would depend on specific game requirements
        }
    }
}