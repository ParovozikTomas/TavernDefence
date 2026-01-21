# Tavern Defense - Unity Version

This is a Unity C# implementation of the Tavern Defense game originally built with React/TypeScript. The project maintains the same core gameplay mechanics while adapting to Unity's architecture.

## Project Structure

```
UnityTavernDefense/
├── Assets/
│   ├── Scripts/
│   │   ├── GameTypes/           # Core data structures
│   │   │   ├── HeroClass.cs     # Hero class enum
│   │   │   ├── Skill.cs         # Skill definition
│   │   │   ├── Hero.cs          # Hero data structure
│   │   │   ├── Enemy.cs         # Enemy data structure
│   │   │   ├── ActiveSkill.cs   # Active skill instance
│   │   │   └── Projectile.cs    # Projectile data structure
│   │   ├── GameManager.cs       # Main game logic
│   │   ├── HeroSetupManager.cs  # Hero selection phase
│   │   ├── SkillBarController.cs # Skill usage handler
│   │   ├── GameUIController.cs  # UI management
│   │   ├── Tavern.cs           # Tavern representation
│   │   ├── HeroController.cs   # Individual hero behavior
│   │   ├── EnemyController.cs  # Individual enemy behavior
│   │   ├── ProjectileController.cs # Projectile behavior
│   │   └── SceneController.cs  # Overall scene management
│   └── GameData.cs             # Static game data
```

## Core Components

### Game Types
- **HeroClass**: Enum for Archer, Warrior, Mage
- **Skill**: Defines all possible skills with their properties
- **Hero**: Player character with stats, position, and abilities
- **Enemy**: Enemy units with health, damage, and AI
- **Projectile**: Moving attacks fired by heroes
- **ActiveSkill**: Temporary skill effects in the game world

### Game Systems

#### GameManager
Central hub for all game logic:
- Wave spawning and management
- Combat calculations (damage, healing, targeting)
- Hero AI (movement, attacking patterns)
- Projectile physics
- Game state management

#### HeroSetupManager
Handles the hero selection phase:
- Allows players to select 5 heroes
- Assigns skills to each selected hero
- Initializes the game with proper starting values

#### Controllers
- **HeroController**: Manages individual hero behavior in the scene
- **EnemyController**: Manages individual enemy behavior in the scene
- **ProjectileController**: Handles projectile movement and collision

#### UI Controllers
- **GameUIController**: Manages in-game UI elements
- **SkillBarController**: Handles skill activation

## Key Features Implemented

1. **Three Hero Classes**:
   - Archer: Ranged attacker with various arrow-based skills
   - Warrior: Melee fighter with defensive and crowd control skills
   - Mage: Support/magic user with healing and damage spells

2. **Combat System**:
   - Real-time combat with projectile physics
   - Area-of-effect skills and targeted abilities
   - Health management and death tracking

3. **Wave System**:
   - Progressive difficulty with increasing enemy count and strength
   - Day counter that affects enemy stats

4. **Economy System**:
   - Gold earned from defeating enemies
   - Resource management for upgrades (in future implementations)

5. **Hero Placement**:
   - Drag-and-drop positioning during gameplay
   - Base positioning system for returning heroes

## Usage

To integrate this into a Unity project:

1. Copy the Scripts folder into your Unity project's Assets folder
2. Create a GameManager object in your scene and attach the GameManager.cs script
3. Create UI elements and connect them to the GameUIController
4. Set up hero/enemy prefabs with appropriate controllers
5. Configure the SceneController to manage the overall flow

## Game Balance

The balance is maintained from the original implementation:
- Enemy health increases with day number: `20 + day * 5`
- Enemy damage increases with day number: `5 + day * 2`
- Enemy spawn rate decreases as day number increases
- Different hero classes have appropriate ranges and attack speeds

## Differences from Original

While maintaining the same core gameplay, the Unity version adapts to Unity's architecture:
- Component-based design instead of functional React components
- Physics simulation using Unity's engine
- Traditional game loop instead of React's rendering cycle
- Object pooling considerations for performance (to be implemented)

## Next Steps

Potential improvements for the Unity version:
- Visual effects for skills and combat
- Audio system integration
- Particle systems for projectiles
- Animation controllers for characters
- Performance optimizations
- Mobile touch controls