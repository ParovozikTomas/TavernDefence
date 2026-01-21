using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TavernDefense
{
    public class HeroSetupManager : MonoBehaviour
    {
        // Game state
        private List<HeroClass> team;
        private List<Hero> finalHeroes;
        private int currentHeroIndex;
        private SetupStep currentStep;

        // Event callback
        public Action<List<Hero>> OnSetupComplete;

        // Enums
        public enum SetupStep
        {
            HeroSelection,
            SkillSelection
        }

        void Start()
        {
            InitializeSetup();
        }

        private void InitializeSetup()
        {
            team = new List<HeroClass>();
            finalHeroes = new List<Hero>();
            currentHeroIndex = 0;
            currentStep = SetupStep.HeroSelection;
        }

        public void AddHero(HeroClass heroClass)
        {
            if (team.Count < 5)
            {
                Debug.Log($"Player action: Add hero '{heroClass}' to team.");
                team.Add(heroClass);
            }
        }

        public void RemoveHero(int index)
        {
            if (index >= 0 && index < team.Count)
            {
                Debug.Log($"Player action: Remove hero '{team[index]}' from team.");
                team.RemoveAt(index);
            }
        }

        public void ConfirmHeroSelection()
        {
            if (team.Count == 5)
            {
                Debug.Log($"Player action: Confirm hero selection. Team: {string.Join(", ", team)}");
                currentStep = SetupStep.SkillSelection;
            }
        }

        public void SelectSkill(Skill skill)
        {
            HeroClass heroClass = team[currentHeroIndex];
            Debug.Log($"Player action: Select skill '{skill.name}' for {heroClass}.");

            // Get hero properties based on class
            float range = GetRangeForClass(heroClass);
            float attackSpeed = GetAttackSpeedForClass(heroClass);

            // Position heroes in formation
            Vector2 position = new Vector2(20 + currentHeroIndex * 15, 75);

            var newHero = new Hero
            {
                id = $"hero-{currentHeroIndex}-{Guid.NewGuid()}",
                heroClass = heroClass,
                skill = skill,
                level = 1,
                position = position,
                basePosition = position,
                health = 100,
                maxHealth = 100,
                attackDamage = 10,
                attackSpeed = attackSpeed,
                range = range,
                damageDealt = 0,
                kills = 0,
                upgrades = new HeroUpgrades(),
                targetEnemyId = null,
                attackCooldown = 0
            };

            finalHeroes.Add(newHero);

            if (currentHeroIndex < 4)
            {
                currentHeroIndex++;
            }
            else
            {
                OnSetupComplete?.Invoke(finalHeroes);
            }
        }

        private float GetRangeForClass(HeroClass heroClass)
        {
            switch (heroClass)
            {
                case HeroClass.Warrior: return 150; // Sight range
                case HeroClass.Mage: return 300;
                case HeroClass.Archer: return 400;
                default: return 100;
            }
        }

        private float GetAttackSpeedForClass(HeroClass heroClass)
        {
            switch (heroClass)
            {
                case HeroClass.Mage: return 0.5f;
                case HeroClass.Archer: return 1.0f;
                case HeroClass.Warrior: return 0.8f;
                default: return 1.0f;
            }
        }

        // Public getters for UI
        public List<HeroClass> GetTeam() => team;
        public SetupStep GetCurrentStep() => currentStep;
        public HeroClass GetCurrentHeroClass() => currentHeroIndex < team.Count ? team[currentHeroIndex] : HeroClass.Archer;
        public List<Skill> GetAvailableSkills() => GameData.SKILLS.ContainsKey(GetCurrentHeroClass()) ? 
                                                   GameData.SKILLS[GetCurrentHeroClass()] : new List<Skill>();
    }
}