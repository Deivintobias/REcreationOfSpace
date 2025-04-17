# Diablo-Style RPG with Farming and Crafting

A top-down RPG that combines combat mechanics with farming and crafting systems, built in Unity.

## Features

### Core Systems
- Procedural world generation with biomes
- Biblical timeline from Creation to 2025
- Historical locations and events
- Resource gathering and management
- Enemy AI with patrolling and combat behaviors
- Experience and leveling system
- Team system for character management

### Farming System
- Placeable farm plots
- Multiple crop types with growth stages
- Watering and fertilizing mechanics
- Crop harvesting and resource generation

### Crafting System
- Multiple workbench types (Basic, Smithy, Alchemy)
- Progressive crafting recipes
- Crafting skill leveling
- Resource requirements and crafting times

### Character System
- Multiple character classes
- Equipment slots (weapons, armor, accessories, tools)
- Skill progression for different activities
- Character stats and attributes

### Menu Systems
- Main menu with save/load functionality
- In-game pause menu
- Character menu with stats and equipment
- Settings menu with audio and graphics options

## Controls

### Basic Movement
- WASD - Move character
- Mouse - Rotate camera
- Left Click - Attack
- Space - Dodge/Roll

### Interaction Controls
- E - Interact with objects/NPCs
- Q - Plow farm plot
- R - Water farm plot
- F - Plant crop
- G - Harvest crop

### Menu Controls
- ESC - Toggle pause menu
- C - Toggle character menu
- I - Toggle inventory
- M - Toggle map

## Development Setup

1. Clone the repository
```bash
git clone https://github.com/yourusername/your-repo-name.git
```

2. Open the project in Unity
- Unity version: 2022.x or later
- Universal Render Pipeline (URP)

3. Open the MainMenu scene
- Located in Assets/Scenes/MainMenu
- Press Play to start testing

4. Scene Structure
- MainMenu scene - Game entry point
- OutdoorsScene - Main gameplay area

### World Generation
- Chunk-based terrain generation
- Multiple biomes (Ocean, Beach, Plains, Forest, Mountain, Desert)
- Dynamic terrain features (mountains, plateaus, valleys, rivers)
- Temperature and moisture-based environment
- Era-specific environmental changes

### Visual Effects
- Era-specific visual settings
  * Creation era: Bright, ethereal atmosphere
  * Great Flood era: Dark, stormy environment
  * Modern era: Realistic lighting and effects
- Dynamic weather systems (rain, snow, dust storms)
- Divine effects for biblical events
- Biome-specific atmospheric effects
- Ambient sound system for each environment
- Advanced post-processing effects

### Historical Timeline
- Interactive timeline UI (T key)
- Biblical eras with historical events
- Scripture references and descriptions
- Historical locations with information displays
- Modern era technology (2025)

## Project Structure

```
Assets/
├── Scripts/
│   ├── Setup/           # Game initialization and setup
│   ├── Player/          # Player controls and mechanics
│   ├── Combat/          # Combat system
│   ├── Farming/         # Farming mechanics
│   ├── Crafting/        # Crafting system
│   ├── Character/       # Character stats and progression
│   ├── UI/              # User interface elements
│   ├── World/          
│   │   ├── ChunkManager.cs     # Chunk-based world generation
│   │   ├── BiomeManager.cs     # Biome and terrain features
│   │   ├── Timeline.cs         # Historical timeline system
│   │   ├── HistoricalInfo.cs   # Historical location data
│   │   └── WorldManager.cs     # World state management
│   └── Resources/       # Resource system
├── Scenes/
│   ├── MainMenu.unity
│   └── OutdoorsScene.unity
└── Prefabs/             # Reusable game objects
```

## Adding New Features

### Adding a New Crop Type
1. Open GameSetup.cs
2. Add new crop data to SetupCropTypes()
3. Create growth stage prefabs
4. Add required resources

### Adding a New Crafting Recipe
1. Open GameSetup.cs
2. Add recipe to SetupCraftingRecipes()
3. Define resource requirements
4. Set crafting time and level requirement

### Adding a New Character Class
1. Open CharacterMenu.cs
2. Add class definition to CharacterClass array
3. Set stat multipliers and bonuses
4. Create class icon

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Unity Technologies
- Asset creators (list specific assets used)
- Community contributors
