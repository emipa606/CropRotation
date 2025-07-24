# .github/copilot-instructions.md

## Mod Overview and Purpose

The CropRotation mod for RimWorld introduces a system of crop rotation to the game, enhancing agricultural gameplay by adding depth and strategy to farming. The mod's goal is to simulate real-world agricultural practices, encouraging players to rotate their crops to maintain soil fertility and avoid negative consequences such as reduced yields and crop failures.

## Key Features and Systems

- **Crop Rotation System:** Introduces the concept of rotating crops in growing zones. Players must manage which crops are planted over seasons to optimize yield and soil productivity.
- **Historical Record Keeping:** Tracks crop history on each tile to determine the optimal crop for planting and to apply any penalties from poor rotation.
- **Yield Modifiers:** Implemented to reward effective crop rotation and punish monoculture practices by reducing yields.
- **Dynamic Plant Selection:** Commands and systems allow for seasonal plant scheduling, providing flexibility in agricultural planning.
- **Burn Down Crops Feature:** Players can clear out older, unproductive crops to prepare for new planting cycles.

## Coding Patterns and Conventions

### General Conventions

- The mod is structured around several key files each representing different components of mod functionality like job drivers, work givers, and plant management.
- Classes follow standard C# naming conventions with PascalCasing.
- Many classes are implemented as either public for game integration or internal when encapsulation is needed within the mod.

### Specific Conventions

- **Command Classes:** Extend `Command` or other command-related classes to provide player-interactive features, like `Command_SetExtraPlantToGrow`.
- **Map Components:** Use `MapComponent` for game state management related to map tiles and areas, important for managing growing zones in this mod.
- **Static Utility Classes:** Several classes are implemented as static, such as `Plant_GetInspectString`, to provide utility methods influential during gameplay events.
  
## XML Integration

While XML integration details are not provided in the summarized content, typical XML usage in RimWorld mods includes:

- **Def Files:** For defining new game entities, such as plants and items, that need to be integrated into existing game systems.
- **Patches:** Applying specific changes using XML patches to the core game files to adjust existing features without altering the original files.

## Harmony Patching

- **Purpose:** Harmony is used for patching the RimWorld codebase without directly modifying it. This is crucial for ensuring that mods remain compatible with updates to the game or with other mods.
- **Examples:** Static classes like `Fire_TrySpread` and `Plant_DeSpawn` likely incorporate Harmony patches to extend or replace gameplay logic in specific situations.
- **Implementation:** Implement patches within static classes to hook into methods from core game classes, providing additional or alternative functionality as needed.

## Suggestions for Copilot

- **Assist with Harmony Patch Methods:** Automatically suggest templates for common Harmony patch attributes such as `Prefix`, `Postfix`, and `Transpiler`.
- **XML Templates:** Provide boilerplate XML for defining new plants and zones, potentially including integration points like research requirements or biome compatibility.
- **Command and Result Prediction:** Assist with writing new player commands or gizmos by suggesting typical method signatures and interaction patterns.
- **Yield Calculation Helpers:** Suggest helper methods or inline calculations for determining yield modifiers based on crop history and soil conditions.
- **Seasonal System Expansion:** Propose implementations for expanding the seasonal crop system by suggesting data structure modifications or additional business logic to easily add or remove compatible crops per season.
