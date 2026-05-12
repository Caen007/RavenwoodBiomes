using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Ravenwood.Biomes
{
    public static class WorkbenchCustomRecipeConfig
    {
        private const string ConfigFileName = "Ravenwood.Biomes.CustomWorkbenchRecipes.cfg";
        private const string SettingsSection = "Custom Recipes";
        private const string BuiltInPotionsSection = "Built-in Potion Recipes";
        private const int RecipeSlotCount = 30;

        private static ConfigFile customRecipeConfig;
        private static ConfigEntry<bool> enableCustomRecipes;
        private static ConfigEntry<bool> enableBuiltInPotionRecipes;
        private static readonly List<ConfigEntry<string>> recipeSlots = new List<ConfigEntry<string>>();
        private static bool initialized;
        private static bool registered;
        private static bool builtInPotionRecipesRegistered;
        private static bool coreRecipesRegistered;

        private static readonly Dictionary<string, int> AlchemyRecipeSortWeights = new Dictionary<string, int>
        {
            { "Recipe_RWB_Ravenwood_Seed", 10 },
            { "Recipe_RWB_Ravenwood_Serum", 20 },
            { "Recipe_RWB_Ravenwood_Elixir", 30 },
            { "Recipe_RWB_Red_Mushroom", 40 },
            { "Recipe_RWB_Yellow_Mushroom", 50 },
            { "Recipe_RWB_Green_Mushroom", 60 },
            { "Recipe_RWB_Blue_Mushroom", 70 },
            { "Recipe_RWB_Purple_Mushroom", 80 },
            { "Recipe_RWB_RP_PotionMinorFortitude", 100 },
            { "Recipe_RWB_RP_PotionMinorDefense", 110 },
            { "Recipe_RWB_RP_PotionMinorIntellect", 120 },
            { "Recipe_RWB_RP_PotionMinorAgility", 130 },
            { "Recipe_RWB_RP_PotionMediumFortitude", 200 },
            { "Recipe_RWB_RP_PotionMediumDefense", 210 },
            { "Recipe_RWB_RP_PotionMediumIntellect", 220 },
            { "Recipe_RWB_RP_PotionMediumAgility", 230 },
            { "Recipe_RWB_RP_PotionMajorFortitude", 300 },
            { "Recipe_RWB_RP_PotionMajorDefense", 310 },
            { "Recipe_RWB_RP_PotionMajorIntellect", 320 },
            { "Recipe_RWB_RP_PotionMajorAgility", 330 },
            { "Recipe_RWB_RP_PotionMythicFortitude", 400 },
            { "Recipe_RWB_RP_PotionMythicDefense", 410 },
            { "Recipe_RWB_RP_PotionMythicIntellect", 420 },
            { "Recipe_RWB_RP_PotionMythicAgility", 430 }
        };

        private static readonly Dictionary<string, int> AlchemyOutputSortWeights = new Dictionary<string, int>
        {
            { "RWB_Ravenwood_Seed", 10 },
            { "RWB_Ravenwood_Serum", 20 },
            { "RWB_Ravenwood_Elixir", 30 },
            { "Mushroom", 40 },
            { "MushroomYellow", 50 },
            { "RWB_Green_Mushroom", 60 },
            { "MushroomBlue", 70 },
            { "RWB_Purple_Mushroom", 80 },
            { "RP_PotionMinorFortitude", 100 },
            { "RP_PotionMinorDefense", 110 },
            { "RP_PotionMinorIntellect", 120 },
            { "RP_PotionMinorAgility", 130 },
            { "RP_PotionMediumFortitude", 200 },
            { "RP_PotionMediumDefense", 210 },
            { "RP_PotionMediumIntellect", 220 },
            { "RP_PotionMediumAgility", 230 },
            { "RP_PotionMajorFortitude", 300 },
            { "RP_PotionMajorDefense", 310 },
            { "RP_PotionMajorIntellect", 320 },
            { "RP_PotionMajorAgility", 330 },
            { "RP_PotionMythicFortitude", 400 },
            { "RP_PotionMythicDefense", 410 },
            { "RP_PotionMythicIntellect", 420 },
            { "RP_PotionMythicAgility", 430 }
        };

        private static readonly BuiltInPotionRecipe[] BuiltInPotionRecipes =
        {
            // Minor potions
            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMinorFortitude",
                "RP_PotionMinorFortitude",
                1,
                Req(3, "Essence_Fire"),
                Req(3, "Mushroom"),
                Req(3, "Raspberry"),
                Req(3, "Dandelion")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMinorDefense",
                "RP_PotionMinorDefense",
                1,
                Req(3, "Essence_Poison"),
                Req(3, "RWB_Green_Mushroom"),
                Req(3, "Raspberry"),
                Req(3, "Coal")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMinorIntellect",
                "RP_PotionMinorIntellect",
                1,
                Req(3, "Essence_Frost"),
                Req(3, "MushroomBlue", "BlueMushroom"),
                Req(3, "Raspberry"),
                Req(3, "NeckTail", "NeckTails")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMinorAgility",
                "RP_PotionMinorAgility",
                1,
                Req(3, "Essence_Lightning"),
                Req(3, "MushroomYellow", "YellowMushroom"),
                Req(3, "Raspberry"),
                Req(3, "Honey")),

            // Medium potions
            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMediumFortitude",
                "RP_PotionMediumFortitude",
                1,
                Req(5, "Essence_Fire"),
                Req(5, "Mushroom"),
                Req(5, "Blueberries"),
                Req(5, "Thistle")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMediumDefense",
                "RP_PotionMediumDefense",
                1,
                Req(5, "Essence_Poison"),
                Req(5, "RWB_Green_Mushroom"),
                Req(5, "Blueberries"),
                Req(5, "Guck")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMediumIntellect",
                "RP_PotionMediumIntellect",
                1,
                Req(5, "Essence_Frost"),
                Req(5, "MushroomBlue", "BlueMushroom"),
                Req(5, "Blueberries"),
                Req(5, "GreydwarfEye", "GreydwarfEyes", "Greydwarf Eyes")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMediumAgility",
                "RP_PotionMediumAgility",
                1,
                Req(5, "Essence_Lightning"),
                Req(5, "MushroomYellow", "YellowMushroom"),
                Req(5, "Blueberries"),
                Req(5, "Carrot", "Carrots")),

            // Major potions
            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMajorFortitude",
                "RP_PotionMajorFortitude",
                1,
                Req(8, "Essence_Fire"),
                Req(8, "Mushroom"),
                Req(8, "Cloudberry", "Cloudberries"),
                Req(8, "MushroomJotunPuffs", "JotunPuffs", "Jotunnpuffs")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMajorDefense",
                "RP_PotionMajorDefense",
                1,
                Req(8, "Essence_Poison"),
                Req(8, "RWB_Green_Mushroom"),
                Req(8, "Cloudberry", "Cloudberries"),
                Req(8, "Tar")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMajorIntellect",
                "RP_PotionMajorIntellect",
                1,
                Req(8, "Essence_Frost"),
                Req(8, "MushroomBlue", "BlueMushroom"),
                Req(8, "Cloudberry", "Cloudberries"),
                Req(8, "FreezeGland")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMajorAgility",
                "RP_PotionMajorAgility",
                1,
                Req(8, "Essence_Lightning"),
                Req(8, "MushroomYellow", "YellowMushroom"),
                Req(8, "Cloudberry", "Cloudberries"),
                Req(8, "Onion", "Onions")),

            // Mythic potions
            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMythicFortitude",
                "RP_PotionMythicFortitude",
                1,
                Req(10, "Essence_Fire"),
                Req(10, "Mushroom"),
                Req(10, "Vineberry", "Vineberries"),
                Req(10, "Fiddleheadfern", "Fiddlehead", "Fiddleheads")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMythicDefense",
                "RP_PotionMythicDefense",
                1,
                Req(10, "Essence_Poison"),
                Req(10, "RWB_Green_Mushroom"),
                Req(10, "Vineberry", "Vineberries"),
                Req(10, "Sap")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMythicIntellect",
                "RP_PotionMythicIntellect",
                1,
                Req(10, "Essence_Frost"),
                Req(10, "MushroomBlue", "BlueMushroom"),
                Req(10, "Vineberry", "Vineberries"),
                Req(10, "MushroomMagecap", "Magecap", "Magecaps")),

            new BuiltInPotionRecipe(
                "Recipe_RWB_RP_PotionMythicAgility",
                "RP_PotionMythicAgility",
                1,
                Req(10, "Essence_Lightning"),
                Req(10, "MushroomYellow", "YellowMushroom"),
                Req(10, "Vineberry", "Vineberries"),
                Req(10, "MushroomSmokePuff", "SmokePuff", "SmokePuffs"))
        };

        public static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            string configPath = Path.Combine(Paths.ConfigPath, ConfigFileName);
            customRecipeConfig = new ConfigFile(configPath, true);

            enableBuiltInPotionRecipes = customRecipeConfig.Bind(
                BuiltInPotionsSection,
                "EnablePotionRecipes",
                true,
                "Enable built-in Ravenwood Alchemy Table potion recipes. Recipes are skipped safely when the potion prefab mod or required item prefabs are missing.");

            enableCustomRecipes = customRecipeConfig.Bind(
                SettingsSection,
                "EnableCustomRecipes",
                false,
                "Enable custom recipes for the Ravenwood Alchemy Table.");

            recipeSlots.Clear();
            for (int i = 1; i <= RecipeSlotCount; i++)
            {
                string key = "Recipe" + i.ToString("00");
                recipeSlots.Add(customRecipeConfig.Bind(
                    SettingsSection,
                    key,
                    string.Empty,
                    "Format: OutputPrefab,OutputAmount,RequirementPrefab:Amount,RequirementPrefab:Amount Example: Coins,10,Wood:1,Stone:1"));
            }

            initialized = true;
        }

        public static void TryRegisterWorkbenchRecipes()
        {
            Initialize();

            if (!coreRecipesRegistered)
            {
                return;
            }

            RegisterBuiltInPotionRecipes();
            RegisterCustomRecipes();
        }

        public static void MarkCoreRecipesRegistered()
        {
            coreRecipesRegistered = true;
        }

        public static void RegisterBuiltInPotionRecipes()
        {
            Initialize();

            if (builtInPotionRecipesRegistered)
            {
                return;
            }

            if (enableBuiltInPotionRecipes == null || !enableBuiltInPotionRecipes.Value)
            {
                builtInPotionRecipesRegistered = true;
                return;
            }

            if (!IsObjectDbReady())
            {
                return;
            }

            int registeredCount = 0;
            int skippedCount = 0;

            for (int i = 0; i < BuiltInPotionRecipes.Length; i++)
            {
                BuiltInPotionRecipe recipe = BuiltInPotionRecipes[i];
                if (RegisterBuiltInPotionRecipe(recipe))
                {
                    registeredCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            builtInPotionRecipesRegistered = true;
            Debug.Log("[RavenwoodBiomes] Built-in Ravenwood Alchemy Table potion recipes registered: " + registeredCount + ", skipped: " + skippedCount);
        }

        public static void RegisterCustomRecipes()
        {
            Initialize();

            if (registered)
            {
                return;
            }

            if (enableCustomRecipes == null || !enableCustomRecipes.Value)
            {
                registered = true;
                return;
            }

            if (!IsObjectDbReady())
            {
                return;
            }

            int registeredCount = 0;
            for (int i = 0; i < recipeSlots.Count; i++)
            {
                ConfigEntry<string> slot = recipeSlots[i];
                if (slot == null || string.IsNullOrWhiteSpace(slot.Value))
                {
                    continue;
                }

                CustomWorkbenchRecipe recipe;
                string error;
                if (!TryParseRecipe(slot.Value, i + 1, out recipe, out error))
                {
                    Debug.LogWarning("[RavenwoodBiomes] Custom workbench recipe " + (i + 1) + " skipped: " + error);
                    continue;
                }

                if (RegisterCustomRecipe(recipe, i + 1))
                {
                    registeredCount++;
                }
            }

            registered = true;
            Debug.Log("[RavenwoodBiomes] Custom Ravenwood Alchemy Table recipes registered: " + registeredCount);
        }

        private static bool RegisterBuiltInPotionRecipe(BuiltInPotionRecipe recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            if (!ItemPrefabExists(recipe.OutputPrefab))
            {
                return false;
            }

            List<RequirementConfig> requirements = new List<RequirementConfig>();
            string missingRequirement;
            if (!TryResolveBuiltInRequirements(recipe, requirements, out missingRequirement))
            {
                Debug.LogWarning("[RavenwoodBiomes] Built-in potion recipe " + recipe.OutputPrefab + " skipped: " + missingRequirement);
                return false;
            }

            return AddWorkbenchRecipe(recipe.RecipeName, recipe.OutputPrefab, recipe.OutputAmount, requirements.ToArray(), "Built-in potion recipe " + recipe.OutputPrefab);
        }

        private static bool RegisterCustomRecipe(CustomWorkbenchRecipe recipe, int slotNumber)
        {
            if (recipe == null)
            {
                return false;
            }

            string validationError;
            if (!TryValidateRecipePrefabs(recipe.OutputPrefab, recipe.Requirements, out validationError))
            {
                Debug.LogWarning("[RavenwoodBiomes] Custom workbench recipe " + slotNumber + " skipped: " + validationError);
                return false;
            }

            return AddWorkbenchRecipe(recipe.RecipeName, recipe.OutputPrefab, recipe.OutputAmount, recipe.Requirements.ToArray(), "Custom workbench recipe " + slotNumber);
        }

        private static bool AddWorkbenchRecipe(string recipeName, string outputPrefab, int outputAmount, RequirementConfig[] requirements, string label)
        {
            if (string.IsNullOrWhiteSpace(recipeName))
            {
                Debug.LogWarning("[RavenwoodBiomes] " + label + " skipped: recipe name is empty.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(outputPrefab))
            {
                Debug.LogWarning("[RavenwoodBiomes] " + label + " skipped: output prefab is empty.");
                return false;
            }

            if (outputAmount <= 0)
            {
                Debug.LogWarning("[RavenwoodBiomes] " + label + " skipped: output amount must be above 0.");
                return false;
            }

            if (requirements == null || requirements.Length == 0)
            {
                Debug.LogWarning("[RavenwoodBiomes] " + label + " skipped: no requirements configured.");
                return false;
            }

            try
            {
                RecipeConfig recipeConfig = new RecipeConfig();
                recipeConfig.Name = recipeName;
                recipeConfig.Item = outputPrefab;
                recipeConfig.Amount = outputAmount;
                recipeConfig.CraftingStation = WorkbenchManager.WorkbenchPrefabName;
                recipeConfig.MinStationLevel = 1;
                recipeConfig.Requirements = requirements;

                int sortWeight = GetAlchemyRecipeSortWeight(recipeName, outputPrefab, 1000);
                ApplyRecipeConfigSortWeight(recipeConfig, sortWeight);

                ItemManager.Instance.AddRecipe(new CustomRecipe(recipeConfig));
                ApplyKnownAlchemyRecipeSortWeights();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[RavenwoodBiomes] " + label + " skipped: " + ex.Message);
                return false;
            }
        }

        public static int GetAlchemyRecipeSortWeight(string recipeName, string outputPrefab, int fallback)
        {
            int sortWeight;
            string normalizedRecipeName = NormalizeRecipeName(recipeName);
            if (!string.IsNullOrWhiteSpace(normalizedRecipeName) && AlchemyRecipeSortWeights.TryGetValue(normalizedRecipeName, out sortWeight))
            {
                return sortWeight;
            }

            string normalizedOutputPrefab = NormalizeRecipeName(outputPrefab);
            if (!string.IsNullOrWhiteSpace(normalizedOutputPrefab) && AlchemyOutputSortWeights.TryGetValue(normalizedOutputPrefab, out sortWeight))
            {
                return sortWeight;
            }

            return fallback;
        }

        public static void ApplyRecipeConfigSortWeight(RecipeConfig recipeConfig, int sortWeight)
        {
            if (recipeConfig == null)
            {
                return;
            }

            Type configType = recipeConfig.GetType();
            string[] memberNames =
            {
                "ListSortWeight",
                "listSortWeight",
                "SortWeight",
                "m_listSortWeight"
            };

            for (int i = 0; i < memberNames.Length; i++)
            {
                System.Reflection.PropertyInfo property = configType.GetProperty(
                    memberNames[i],
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                if (property != null && property.CanWrite && property.PropertyType == typeof(int))
                {
                    property.SetValue(recipeConfig, sortWeight, null);
                    return;
                }

                System.Reflection.FieldInfo field = configType.GetField(
                    memberNames[i],
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                if (field != null && field.FieldType == typeof(int))
                {
                    field.SetValue(recipeConfig, sortWeight);
                    return;
                }
            }
        }

        public static void ApplyKnownAlchemyRecipeSortWeights()
        {
            if (ObjectDB.instance == null || ObjectDB.instance.m_recipes == null)
            {
                return;
            }

            List<Recipe> orderedAlchemyRecipes = new List<Recipe>();
            int firstAlchemyRecipeIndex = -1;

            for (int i = 0; i < ObjectDB.instance.m_recipes.Count; i++)
            {
                Recipe recipe = ObjectDB.instance.m_recipes[i];
                if (recipe == null)
                {
                    continue;
                }

                int sortWeight;
                if (TryGetAlchemyRecipeSortWeight(recipe, out sortWeight))
                {
                    recipe.m_listSortWeight = sortWeight;
                    orderedAlchemyRecipes.Add(recipe);

                    if (firstAlchemyRecipeIndex < 0)
                    {
                        firstAlchemyRecipeIndex = i;
                    }
                }
            }

            if (orderedAlchemyRecipes.Count <= 1 || firstAlchemyRecipeIndex < 0)
            {
                return;
            }

            orderedAlchemyRecipes.Sort(CompareAlchemyRecipes);
            ObjectDB.instance.m_recipes.RemoveAll(IsKnownAlchemyRecipe);
            ObjectDB.instance.m_recipes.InsertRange(Math.Min(firstAlchemyRecipeIndex, ObjectDB.instance.m_recipes.Count), orderedAlchemyRecipes);
        }

        private static int CompareAlchemyRecipes(Recipe left, Recipe right)
        {
            int leftSortWeight;
            int rightSortWeight;
            TryGetAlchemyRecipeSortWeight(left, out leftSortWeight);
            TryGetAlchemyRecipeSortWeight(right, out rightSortWeight);

            int weightCompare = leftSortWeight.CompareTo(rightSortWeight);
            if (weightCompare != 0)
            {
                return weightCompare;
            }

            return string.Compare(NormalizeRecipeName(left != null ? left.name : string.Empty), NormalizeRecipeName(right != null ? right.name : string.Empty), StringComparison.Ordinal);
        }

        private static void SortAvailableAlchemyRecipes(List<Recipe> available)
        {
            if (available == null || available.Count <= 1)
            {
                return;
            }

            List<Recipe> alchemyRecipes = new List<Recipe>();
            List<int> alchemyIndexes = new List<int>();

            for (int i = 0; i < available.Count; i++)
            {
                Recipe recipe = available[i];
                if (IsKnownAlchemyRecipe(recipe))
                {
                    alchemyRecipes.Add(recipe);
                    alchemyIndexes.Add(i);
                }
            }

            if (alchemyRecipes.Count <= 1)
            {
                return;
            }

            alchemyRecipes.Sort(CompareAlchemyRecipes);

            for (int i = 0; i < alchemyIndexes.Count; i++)
            {
                available[alchemyIndexes[i]] = alchemyRecipes[i];
            }
        }

        private static bool IsKnownAlchemyRecipe(Recipe recipe)
        {
            int sortWeight;
            return TryGetAlchemyRecipeSortWeight(recipe, out sortWeight);
        }

        private static bool TryGetAlchemyRecipeSortWeight(Recipe recipe, out int sortWeight)
        {
            sortWeight = 0;

            if (recipe == null)
            {
                return false;
            }

            string normalizedRecipeName = NormalizeRecipeName(recipe.name);
            if (!string.IsNullOrWhiteSpace(normalizedRecipeName) && AlchemyRecipeSortWeights.TryGetValue(normalizedRecipeName, out sortWeight))
            {
                return true;
            }

            if (!IsRavenwoodAlchemyStation(recipe.m_craftingStation) || recipe.m_item == null)
            {
                return false;
            }

            string normalizedOutputPrefab = NormalizeRecipeName(recipe.m_item.name);
            return !string.IsNullOrWhiteSpace(normalizedOutputPrefab) && AlchemyOutputSortWeights.TryGetValue(normalizedOutputPrefab, out sortWeight);
        }

        private static bool IsRavenwoodAlchemyStation(CraftingStation station)
        {
            if (station == null || station.gameObject == null)
            {
                return false;
            }

            string stationName = NormalizeRecipeName(station.gameObject.name);
            return string.Equals(stationName, WorkbenchManager.WorkbenchPrefabName, StringComparison.Ordinal);
        }

        private static string NormalizeRecipeName(string recipeName)
        {
            if (string.IsNullOrWhiteSpace(recipeName))
            {
                return string.Empty;
            }

            string normalized = recipeName.Trim();
            const string cloneSuffix = "(Clone)";
            if (normalized.EndsWith(cloneSuffix, StringComparison.Ordinal))
            {
                normalized = normalized.Substring(0, normalized.Length - cloneSuffix.Length).Trim();
            }

            return normalized;
        }

        private static bool TryResolveBuiltInRequirements(BuiltInPotionRecipe recipe, List<RequirementConfig> requirements, out string error)
        {
            error = string.Empty;

            if (recipe == null || recipe.Requirements == null || recipe.Requirements.Length == 0)
            {
                error = "no requirements configured.";
                return false;
            }

            for (int i = 0; i < recipe.Requirements.Length; i++)
            {
                BuiltInPotionRequirement requirement = recipe.Requirements[i];
                if (requirement == null || requirement.ItemPrefabs == null || requirement.ItemPrefabs.Length == 0)
                {
                    error = "requirement " + (i + 1) + " has no item prefab candidates.";
                    return false;
                }

                string resolvedPrefab;
                if (!TryResolveItemPrefab(requirement.ItemPrefabs, out resolvedPrefab))
                {
                    if (!TryUseSafeBuiltInFallback(requirement.ItemPrefabs, out resolvedPrefab))
                    {
                        error = "missing requirement prefab: " + string.Join(" or ", requirement.ItemPrefabs);
                        return false;
                    }
                }

                requirements.Add(new RequirementConfig(resolvedPrefab, requirement.Amount, 0, false));
            }

            return true;
        }

        private static bool TryValidateRecipePrefabs(string outputPrefab, List<RequirementConfig> requirements, out string error)
        {
            error = string.Empty;

            if (!ItemPrefabExists(outputPrefab))
            {
                error = "missing output prefab: " + outputPrefab;
                return false;
            }

            if (requirements == null || requirements.Count == 0)
            {
                error = "recipe has no requirements.";
                return false;
            }

            for (int i = 0; i < requirements.Count; i++)
            {
                RequirementConfig requirement = requirements[i];
                if (requirement == null || string.IsNullOrWhiteSpace(requirement.Item))
                {
                    error = "requirement " + (i + 1) + " is empty.";
                    return false;
                }

                if (!ItemPrefabExists(requirement.Item))
                {
                    error = "missing requirement prefab: " + requirement.Item;
                    return false;
                }
            }

            return true;
        }

        private static bool TryResolveItemPrefab(string[] prefabCandidates, out string resolvedPrefab)
        {
            resolvedPrefab = string.Empty;

            if (prefabCandidates == null)
            {
                return false;
            }

            for (int i = 0; i < prefabCandidates.Length; i++)
            {
                string candidate = prefabCandidates[i];
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (ItemPrefabExists(candidate))
                {
                    resolvedPrefab = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool TryUseSafeBuiltInFallback(string[] prefabCandidates, out string resolvedPrefab)
        {
            resolvedPrefab = string.Empty;

            if (prefabCandidates == null || prefabCandidates.Length == 0)
            {
                return false;
            }

            if (!IsKnownVanillaRequirement(prefabCandidates))
            {
                return false;
            }

            for (int i = 0; i < prefabCandidates.Length; i++)
            {
                string candidate = prefabCandidates[i];
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    resolvedPrefab = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool IsKnownVanillaRequirement(string[] prefabCandidates)
        {
            if (prefabCandidates == null)
            {
                return false;
            }

            for (int i = 0; i < prefabCandidates.Length; i++)
            {
                string candidate = prefabCandidates[i];
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (IsKnownVanillaRequirement(candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsKnownVanillaRequirement(string prefabName)
        {
            switch (prefabName)
            {
                case "Mushroom":
                case "MushroomYellow":
                case "YellowMushroom":
                case "MushroomBlue":
                case "BlueMushroom":
                case "MushroomSmokePuff":
                case "SmokePuff":
                case "SmokePuffs":
                case "MushroomJotunPuffs":
                case "JotunPuffs":
                case "Jotunnpuffs":
                case "MushroomMagecap":
                case "Magecap":
                case "Magecaps":
                case "Raspberry":
                case "Dandelion":
                case "Coal":
                case "Blueberries":
                case "Guck":
                case "Cloudberry":
                case "Cloudberries":
                case "Tar":
                case "Sap":
                case "NeckTail":
                case "NeckTails":
                case "GreydwarfEye":
                case "GreydwarfEyes":
                case "Greydwarf Eyes":
                case "FreezeGland":
                case "Carrot":
                case "Carrots":
                case "Onion":
                case "Onions":
                case "Honey":
                case "Thistle":
                case "Vineberry":
                case "Vineberries":
                case "Fiddleheadfern":
                case "Fiddlehead":
                case "Fiddleheads":
                case "RoyalJelly":
                case "Royal Jelly":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsKnownRavenwoodItemPrefab(string prefabName)
        {
            return string.Equals(prefabName, TreeRegistrar.RavenSeedPrefabName, StringComparison.Ordinal) ||
                   string.Equals(prefabName, TreeRegistrar.RavenSerumPrefabName, StringComparison.Ordinal) ||
                   string.Equals(prefabName, TreeRegistrar.RavenElixirPrefabName, StringComparison.Ordinal) ||
                   string.Equals(prefabName, TreeRegistrar.GreenMushroomItemPrefabName, StringComparison.Ordinal) ||
                   string.Equals(prefabName, TreeRegistrar.PurpleMushroomItemPrefabName, StringComparison.Ordinal);
        }

        private static bool ItemPrefabExists(string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return false;
            }

            if (coreRecipesRegistered && IsKnownRavenwoodItemPrefab(prefabName))
            {
                return true;
            }

            ObjectDB objectDb = ObjectDB.instance;
            if (objectDb != null)
            {
                try
                {
                    if (objectDb.GetItemPrefab(prefabName) != null)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            if (ZNetScene.instance != null)
            {
                try
                {
                    if (ZNetScene.instance.GetPrefab(prefabName) != null)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        private static bool IsObjectDbReady()
        {
            ObjectDB objectDb = ObjectDB.instance;
            return objectDb != null && objectDb.m_items != null && objectDb.m_items.Count > 0;
        }

        private static bool TryParseRecipe(string rawValue, int slotNumber, out CustomWorkbenchRecipe recipe, out string error)
        {
            recipe = null;
            error = string.Empty;

            string[] parts = rawValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                error = "Recipe needs output prefab, output amount, and at least one requirement.";
                return false;
            }

            string outputPrefab = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(outputPrefab))
            {
                error = "Output prefab is empty.";
                return false;
            }

            int outputAmount;
            if (!int.TryParse(parts[1].Trim(), out outputAmount) || outputAmount <= 0)
            {
                error = "Output amount must be a number above 0.";
                return false;
            }

            List<RequirementConfig> requirements = new List<RequirementConfig>();
            for (int i = 2; i < parts.Length; i++)
            {
                string requirementRaw = parts[i].Trim();
                if (string.IsNullOrWhiteSpace(requirementRaw))
                {
                    continue;
                }

                RequirementConfig requirement;
                if (!TryParseRequirement(requirementRaw, out requirement, out error))
                {
                    return false;
                }

                requirements.Add(requirement);
            }

            if (requirements.Count == 0)
            {
                error = "Recipe has no valid requirements.";
                return false;
            }

            recipe = new CustomWorkbenchRecipe();
            recipe.RecipeName = "Recipe_RWB_CustomWorkbench_" + slotNumber.ToString("00") + "_" + SanitizeName(outputPrefab);
            recipe.OutputPrefab = outputPrefab;
            recipe.OutputAmount = outputAmount;
            recipe.Requirements = requirements;
            return true;
        }

        private static bool TryParseRequirement(string rawValue, out RequirementConfig requirement, out string error)
        {
            requirement = null;
            error = string.Empty;

            string[] parts = rawValue.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                error = "Requirement must use ItemPrefab:Amount format. Bad requirement: " + rawValue;
                return false;
            }

            string itemPrefab = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(itemPrefab))
            {
                error = "Requirement item prefab is empty.";
                return false;
            }

            int amount;
            if (!int.TryParse(parts[1].Trim(), out amount) || amount <= 0)
            {
                error = "Requirement amount must be a number above 0. Bad requirement: " + rawValue;
                return false;
            }

            requirement = new RequirementConfig(itemPrefab, amount, 0, false);
            return true;
        }

        private static BuiltInPotionRequirement Req(int amount, params string[] itemPrefabs)
        {
            return new BuiltInPotionRequirement(itemPrefabs, amount);
        }

        private static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Unknown";
            }

            char[] chars = value.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        private static class ObjectDB_Awake_Patch
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix()
            {
                TryRegisterWorkbenchRecipes();
            }
        }

        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        private static class ObjectDbAwakeRecipeSortPatch
        {
            private static void Postfix()
            {
                ApplyKnownAlchemyRecipeSortWeights();
            }
        }

        [HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
        private static class ObjectDbCopyOtherDbRecipeSortPatch
        {
            private static void Postfix()
            {
                ApplyKnownAlchemyRecipeSortWeights();
            }
        }

        [HarmonyPatch(typeof(Player), "GetAvailableRecipes")]
        private static class PlayerGetAvailableRecipesRecipeSortPatch
        {
            private static void Postfix(ref List<Recipe> available)
            {
                SortAvailableAlchemyRecipes(available);
            }
        }

        private sealed class BuiltInPotionRecipe
        {
            public readonly string RecipeName;
            public readonly string OutputPrefab;
            public readonly int OutputAmount;
            public readonly BuiltInPotionRequirement[] Requirements;

            public BuiltInPotionRecipe(string recipeName, string outputPrefab, int outputAmount, params BuiltInPotionRequirement[] requirements)
            {
                RecipeName = recipeName;
                OutputPrefab = outputPrefab;
                OutputAmount = outputAmount;
                Requirements = requirements;
            }
        }

        private sealed class BuiltInPotionRequirement
        {
            public readonly string[] ItemPrefabs;
            public readonly int Amount;

            public BuiltInPotionRequirement(string[] itemPrefabs, int amount)
            {
                ItemPrefabs = itemPrefabs;
                Amount = amount;
            }
        }

        private sealed class CustomWorkbenchRecipe
        {
            public string RecipeName;
            public string OutputPrefab;
            public int OutputAmount;
            public List<RequirementConfig> Requirements;
        }
    }
}