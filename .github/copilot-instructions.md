# GitHub Copilot Instructions for Crop Rotation Mod

Welcome to the Crop Rotation mod for RimWorld! This mod introduces a realistic farming system by implementing crop rotation to maintain soil fertility and reduce pest infestations. Below are detailed guidelines to help you efficiently contribute to the mod using GitHub Copilot.

## Mod Overview and Purpose

The Crop Rotation mod aims to enhance the farming experience in RimWorld by simulating real-world crop rotation practices. It adjusts crop yields based on the number of times a particular crop is planted in the same location. Rotating crops not only improves soil fertility but also ensures a balanced agricultural cycle.

## Key Features and Systems

- **Yield Decay**: Crops planted repeatedly in the same spot experience a yield decay. This simulates soil exhaustion over time.
- **Yield Increase**: Switching to a different crop in the same growing zone increases yield, simulating nutrient replenishment.
- **Crop Rotation Options**: Players can select multiple crops for growing zones, allowing pawns to automatically switch crops after each harvest.
- **Seasonal Rotation**: Option to assign a different crop to grow for each season, enhancing agricultural planning.
- **Burning Crops**: Pawns can burn crops to increase yield, an action that requires the "Basic Crop Rotation" research.
- **Mod Options**: Configuration options to tweak yield decay, increase, and other parameters for a balanced farming experience.

## Coding Patterns and Conventions

- **Mod Structure**: The mod follows a structured directory layout with separate folders for C# source files and XML definition files.
- **Namespace and Class Naming**: Use `CropRotation` as the primary namespace and prefix class names with `CropRotation` for clarity and consistency.
- **Harmony Patching**: Utilize Harmony for patching existing RimWorld methods to integrate custom behavior.
- **XML Definitions**: Ensure XML files are well-organized and adhere to RimWorld's XML schema for smooth integration.

## XML Integration

- **JobDefs.xml**: Contains custom job definitions for crop rotation actions.
- **ResearchProjects.xml**: Defines research projects required for certain crop rotation features.
- **UpdateInfo.xml**: Provides update information and compatibility details.

### Example XML Entry

xml
<JobDef>
  <defName>JobDef_CropRotation</defName>
  <label>Crop Rotation</label>
  <description>Rotate crops to improve yield.</description>
  <doWhileInDanger>false</doWhileInDanger>
  <workTags>
    <li>WorkTag_Growing</li>
  </workTags>
  <workType>WorkType_Growing</workType>
</JobDef>


## Harmony Patching

The mod extensively uses Harmony to patch existing RimWorld methods. Below is an example of a Harmony patch that modifies the yield of crops.

### Example Harmony Patch

csharp
using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation.HarmonyPatches
{
    [HarmonyPatch(typeof(Plant_DeliberatelyCultivated), "DeliberatelyCultivated")]
    public static class Plant_DeliberatelyCultivated_DeliberatelyCultivated
    {
        [HarmonyPostfix]
        public static void Postfix(ref Plant __instance, ref float __result)
        {
            if (CropRotation.IsValidCrop(__instance))
            {
                __result *= CropRotation.GetYieldMultiplier(__instance);
            }
        }
    }
}


## Suggestions for Copilot

1. **Use Descriptive Class Names**: Ensure class names are descriptive and follow the `CropRotation` prefix.
2. **Leverage Harmony for Patching**: Utilize Harmony to patch existing methods and integrate custom behavior.
3. **Consistent XML Structure**: Maintain a consistent XML structure to ensure smooth integration and readability.
4. **Mod Options Configuration**: Focus on implementing mod options for customizing yield parameters and behaviors.
5. **Documentation and Comments**: Include clear documentation and comments in your code to enhance maintainability.

By following these guidelines, you can contribute effectively to the Crop Rotation mod, enhancing its features and ensuring a realistic and engaging farming experience for players.

Happy coding!


This detailed `.github/copilot-instructions.md` file provides a comprehensive guide for contributors to the Crop Rotation mod, covering mod overview, key features, coding patterns, XML integration, Harmony patching, and suggestions for using GitHub Copilot.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

