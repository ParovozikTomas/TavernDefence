using System.Collections.Generic;

namespace TavernDefense
{
    public static class GameData
    {
        public static readonly List<HeroClassData> HERO_CLASSES = new List<HeroClassData>
        {
            new HeroClassData { name = HeroClass.Mage, icon = "🧙", description = "A powerful sorcerer, weaving magic to control the battlefield." },
            new HeroClassData { name = HeroClass.Warrior, icon = "🛡️", description = "A stalwart defender, holding the line with brute force." },
            new HeroClassData { name = HeroClass.Archer, icon = "🏹", description = "Master of the bow, dealing precise damage from afar." }
        };

        public static readonly Dictionary<HeroClass, List<Skill>> SKILLS = new Dictionary<HeroClass, List<Skill>>
        {
            { HeroClass.Archer, new List<Skill>
                {
                    new Skill { id = "archer-1", name = "Rain of Arrows", description = "Deals 50 damage over a 100px area for 5s.", icon = "🌧️", cooldown = 10, areaOfEffect = 100, damage = 50, duration = 5 },
                    new Skill { id = "archer-2", name = "Piercing Shot", description = "Fires a shot dealing 75 damage to a single target.", icon = "🎯", cooldown = 8, areaOfEffect = 0, damage = 75 },
                    new Skill { id = "archer-3", name = "Swift Quiver", description = "Increases all archers' attack speed by 30% for 10s.", icon = "⏩", cooldown = 20, areaOfEffect = 0 },
                    new Skill { id = "archer-4", name = "Marked for Death", description = "Marks an enemy. All attacks deal 25% more damage to it for 10s.", icon = "☠️", cooldown = 15, areaOfEffect = 0 },
                    new Skill { id = "archer-5", name = "Volley", description = "Fires a cone of arrows, dealing 40 damage to enemies hit.", icon = "🏹", cooldown = 12, areaOfEffect = 120, damage = 40 }
                }
            },
            { HeroClass.Warrior, new List<Skill>
                {
                    new Skill { id = "warrior-1", name = "Stalwart Shield", description = "Stuns enemies in a 75px area for 3s.", icon = "🛡️", cooldown = 12, areaOfEffect = 75 },
                    new Skill { id = "warrior-2", name = "Battle Cry", description = "All heroes gain +10 damage for 10s.", icon = "🗣️", cooldown = 18, areaOfEffect = 0, duration = 10, buff = new Buff { type = "damage", value = 10 } },
                    new Skill { id = "warrior-3", name = "Whirlwind", description = "Deals 60 damage to all enemies around the warrior.", icon = "🌪️", cooldown = 10, areaOfEffect = 100, damage = 60 },
                    new Skill { id = "warrior-4", name = "Last Stand", description = "Becomes invulnerable for 5 seconds when health is low.", icon = "❤️‍🩹", cooldown = 60, areaOfEffect = 0 },
                    new Skill { id = "warrior-5", name = "Charge", description = "Charges to a location, pushing enemies back.", icon = "💨", cooldown = 15, areaOfEffect = 50 }
                }
            },
            { HeroClass.Mage, new List<Skill>
                {
                    new Skill { id = "mage-1", name = "Healing Field", description = "Heals allies in a 120px area for a total of 50 HP over 5s.", icon = "✨", cooldown = 15, areaOfEffect = 120, healing = 50, duration = 5 },
                    new Skill { id = "mage-2", name = "Fireball", description = "Launches a fireball dealing 100 damage to a target.", icon = "🔥", cooldown = 8, areaOfEffect = 0, damage = 100 },
                    new Skill { id = "mage-3", name = "Arcane Ward", description = "Grants a 50 HP shield to an ally for 10s.", icon = "🔮", cooldown = 12, areaOfEffect = 0 },
                    new Skill { id = "mage-4", name = "Chain Lightning", description = "Lightning strikes up to 3 enemies for 40 damage each.", icon = "⚡", cooldown = 10, areaOfEffect = 150, damage = 40, maxTargets = 3 },
                    new Skill { id = "mage-5", name = "Polymorph", description = "Turns an enemy into a harmless sheep for 5s.", icon = "🐑", cooldown = 20, areaOfEffect = 0 }
                }
            }
        };
    }

    public class HeroClassData
    {
        public HeroClass name;
        public string icon;
        public string description;
    }
}