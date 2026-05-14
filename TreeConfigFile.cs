using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace Ravenwood.Biomes
{
    public static class TreeConfigFile
    {
        private const float DefaultFallbackRespawnMinutes = 1500f;
        private const float DefaultPickableMushroomRespawnMinutes = 240f;
        private static readonly List<TreeDefinition> TreeDefinitions = new List<TreeDefinition>();
        private static ConfigEntry<bool> EnableRavenwoodElixirRecipe;
        private static ConfigEntry<bool> EnableVegetationBreakDebug;
        private static ConfigEntry<bool> EnableVegetationBreakDebugVerbose;
        private static ConfigEntry<int> VegetationBreakDebugMaxLogsPerPrefab;
        private static ConfigEntry<float> VegetationBreakDebugSuspiciousAliveSeconds;

        public sealed class TreeDefinition
        {
            public string PrefabName;
            public string DisplayName;
            public string Description;
            public string Category;
            public Jotunn.Configs.RequirementConfig[] Requirements;
            public string DefaultDropItem;
            public int DefaultDropMin;
            public int DefaultDropMax;
            public bool DefaultEnableDrops;
            public string SeedPrefabName;
            public int DefaultSeedDropMin;
            public int DefaultSeedDropMax;
            public float DefaultSeedDropChance;
            public bool DefaultEnableSeedDrops;
            public bool DefaultEnableRespawn;
            public float DefaultRespawnMinutes;
            public bool Indestructible;

            public ConfigEntry<bool> EnableDrops;
            public ConfigEntry<string> DropItem;
            public ConfigEntry<int> DropMin;
            public ConfigEntry<int> DropMax;
            public ConfigEntry<bool> EnableSeedDrops;
            public ConfigEntry<int> SeedDropMin;
            public ConfigEntry<int> SeedDropMax;
            public ConfigEntry<float> SeedDropChance;
            public ConfigEntry<bool> EnableRespawn;
            public ConfigEntry<float> RespawnMinutes;
        }

        public static IReadOnlyList<TreeDefinition> Definitions
        {
            get { return TreeDefinitions; }
        }

        public static void Initialize(ConfigFile configFile)
        {
            if (configFile == null)
            {
                return;
            }

            BuildTreeDefinitions();
            BindTreeConfigs(configFile);
            BindElixirConfig(configFile);
            BindDebugConfig(configFile);
        }

        public static void BuildTreeDefinitions()
        {
            if (TreeDefinitions.Count > 0)
            {
                return;
            }

            for (int i = 0; i < TreeRegistrar.AllRegistrations.Count; i++)
            {
                TreeRegistration registration = TreeRegistrar.AllRegistrations[i];
                if (registration == null || string.IsNullOrWhiteSpace(registration.PrefabName))
                {
                    continue;
                }

                TreeDefinition tree = new TreeDefinition();
                tree.PrefabName = registration.PrefabName;
                tree.DisplayName = registration.DisplayName;
                tree.Description = registration.Description;
                tree.Category = registration.Category;
                tree.Requirements = CloneRequirements(registration.Requirements);
                tree.DefaultDropItem = registration.DefaultDropItem;
                tree.DefaultDropMin = registration.DefaultDropMin;
                tree.DefaultDropMax = registration.DefaultDropMax;
                tree.DefaultEnableDrops = registration.DefaultEnableDrops;
                tree.SeedPrefabName = registration.SeedPrefabName;
                tree.DefaultSeedDropMin = registration.DefaultSeedDropMin;
                tree.DefaultSeedDropMax = registration.DefaultSeedDropMax;
                tree.DefaultSeedDropChance = Mathf.Clamp01(registration.DefaultSeedDropChance);
                tree.DefaultEnableSeedDrops = registration.DefaultEnableSeedDrops;
                tree.DefaultEnableRespawn = registration.DefaultEnableRespawn;
                tree.DefaultRespawnMinutes = Mathf.Max(0f, registration.DefaultRespawnMinutes);
                tree.Indestructible = registration.DefaultIndestructible;
                TreeDefinitions.Add(tree);
            }
        }

        private static Jotunn.Configs.RequirementConfig[] CloneRequirements(Jotunn.Configs.RequirementConfig[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<Jotunn.Configs.RequirementConfig>();
            }

            Jotunn.Configs.RequirementConfig[] clone = new Jotunn.Configs.RequirementConfig[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                Jotunn.Configs.RequirementConfig requirement = source[i];
                if (requirement == null)
                {
                    continue;
                }

                clone[i] = new Jotunn.Configs.RequirementConfig(requirement.Item, requirement.Amount, requirement.AmountPerLevel, requirement.Recover);
            }

            return clone;
        }

        private static void BindElixirConfig(ConfigFile config)
        {
            EnableRavenwoodElixirRecipe = config.Bind(
                "Ravenwood Elixir",
                "EnableElixirRecipe",
                true,
                "Enable or disable the Ravenwood Elixir recipe at the cauldron. Enable this to show the elixir recipe and allow normal crafting of Eternal trees.");
        }

        public static bool IsRavenwoodElixirRecipeEnabled()
        {
            return EnableRavenwoodElixirRecipe != null && EnableRavenwoodElixirRecipe.Value;
        }

        private static void BindDebugConfig(ConfigFile config)
        {
            EnableVegetationBreakDebug = config.Bind(
                "Debug",
                "EnableVegetationBreakDebug",
                true,
                "Enable temporary Ravenwood vegetation break debug logging. Use only while testing world/YAML vegetation break issues.");

            EnableVegetationBreakDebugVerbose = config.Bind(
                "Debug",
                "EnableVegetationBreakDebugVerbose",
                false,
                "Enable verbose Ravenwood vegetation debug logs. Leave false for break testing to avoid log spam.");

            VegetationBreakDebugMaxLogsPerPrefab = config.Bind(
                "Debug",
                "VegetationBreakDebugMaxLogsPerPrefab",
                1,
                "Maximum suspicious break debug lines per prefab and event type. Use 0 for unlimited.");

            VegetationBreakDebugSuspiciousAliveSeconds = config.Bind(
                "Debug",
                "VegetationBreakDebugSuspiciousAliveSeconds",
                1f,
                "Only log health-positive destroy events within this many seconds after spawn.");
        }

        public static bool IsVegetationBreakDebugEnabled()
        {
            return EnableVegetationBreakDebug != null && EnableVegetationBreakDebug.Value;
        }

        public static bool IsVegetationBreakDebugVerboseEnabled()
        {
            return EnableVegetationBreakDebugVerbose != null && EnableVegetationBreakDebugVerbose.Value;
        }

        public static int GetVegetationBreakDebugMaxLogsPerPrefab()
        {
            return VegetationBreakDebugMaxLogsPerPrefab != null ? Mathf.Max(0, VegetationBreakDebugMaxLogsPerPrefab.Value) : 1;
        }

        public static float GetVegetationBreakDebugSuspiciousAliveSeconds()
        {
            return VegetationBreakDebugSuspiciousAliveSeconds != null ? Mathf.Max(0f, VegetationBreakDebugSuspiciousAliveSeconds.Value) : 1f;
        }

        private static void BindTreeConfigs(ConfigFile config)
        {
            for (int i = 0; i < TreeDefinitions.Count; i++)
            {
                TreeDefinition tree = TreeDefinitions[i];
                string section = "Tree." + tree.PrefabName;

                tree.EnableDrops = config.Bind(section, "EnableDrops", tree.DefaultEnableDrops, "Enable or disable regular drops for " + tree.PrefabName + ".");
                tree.DropItem = config.Bind(section, "DropItem", tree.DefaultDropItem, "Regular drop item prefab for " + tree.PrefabName + ". Leave blank to disable the regular drop.");
                tree.DropMin = config.Bind(section, "DropMin", tree.DefaultDropMin, "Minimum regular drop amount for " + tree.PrefabName + ".");
                tree.DropMax = config.Bind(section, "DropMax", tree.DefaultDropMax, "Maximum regular drop amount for " + tree.PrefabName + ".");

                tree.EnableSeedDrops = config.Bind(section, "EnableSeedDrops", tree.DefaultEnableSeedDrops, "Enable or disable Raven Seed drops for " + tree.PrefabName + ".");
                tree.SeedDropMin = config.Bind(section, "SeedDropMin", tree.DefaultSeedDropMin, "Minimum Raven Seed drop amount for " + tree.PrefabName + ".");
                tree.SeedDropMax = config.Bind(section, "SeedDropMax", tree.DefaultSeedDropMax, "Maximum Raven Seed drop amount for " + tree.PrefabName + ".");
                tree.SeedDropChance = config.Bind(section, "SeedDropChance", tree.DefaultSeedDropChance, "Chance from 0.0 to 1.0 for " + tree.PrefabName + " to drop Raven Seed.");

                tree.EnableRespawn = config.Bind(section, "EnableRespawn", tree.DefaultEnableRespawn, "Enable or disable cultivator regrow for " + tree.PrefabName + ".");
                tree.RespawnMinutes = config.Bind(section, "RespawnMinutes", tree.DefaultRespawnMinutes, "Regrow time in real-time minutes for cultivator-placed " + tree.PrefabName + ".");
            }
        }

        public static List<TreeDropEntry> BuildDropEntries(TreeDefinition tree)
        {
            List<TreeDropEntry> entries = new List<TreeDropEntry>();
            if (tree == null)
            {
                return entries;
            }

            bool enableDrops = tree.EnableDrops != null ? tree.EnableDrops.Value : tree.DefaultEnableDrops;
            if (enableDrops)
            {
                int min = tree.DropMin != null ? tree.DropMin.Value : tree.DefaultDropMin;
                int max = tree.DropMax != null ? tree.DropMax.Value : tree.DefaultDropMax;
                min = Mathf.Max(0, min);
                max = Mathf.Max(min, max);

                string itemName = tree.DropItem != null ? tree.DropItem.Value : tree.DefaultDropItem;
                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    entries.Add(new TreeDropEntry(itemName, min, max, 1f));
                }
            }

            bool enableSeedDrops = tree.EnableSeedDrops != null ? tree.EnableSeedDrops.Value : tree.DefaultEnableSeedDrops;
            if (enableSeedDrops && !string.IsNullOrWhiteSpace(tree.SeedPrefabName))
            {
                int seedMin = tree.SeedDropMin != null ? tree.SeedDropMin.Value : tree.DefaultSeedDropMin;
                int seedMax = tree.SeedDropMax != null ? tree.SeedDropMax.Value : tree.DefaultSeedDropMax;
                float chance = tree.SeedDropChance != null ? tree.SeedDropChance.Value : tree.DefaultSeedDropChance;

                seedMin = Mathf.Max(0, seedMin);
                seedMax = Mathf.Max(seedMin, seedMax);
                chance = Mathf.Clamp01(chance);

                if (seedMax > 0 && chance > 0f)
                {
                    entries.Add(new TreeDropEntry(tree.SeedPrefabName, seedMin, seedMax, chance));
                }
            }

            return entries;
        }

        public static TreeDefinition FindTreeDefinition(string prefabName)
        {
            for (int i = 0; i < TreeDefinitions.Count; i++)
            {
                TreeDefinition tree = TreeDefinitions[i];
                if (tree != null && string.Equals(tree.PrefabName, prefabName, StringComparison.Ordinal))
                {
                    return tree;
                }
            }

            return null;
        }

        private static bool IsRavenwoodPickableMushroomPrefab(string prefabName)
        {
            return string.Equals(prefabName, TreeRegistrar.GreenMushroomPrefabName, StringComparison.Ordinal) ||
                   string.Equals(prefabName, TreeRegistrar.PurpleMushroomPrefabName, StringComparison.Ordinal);
        }

        public static string GetRegrowCarrierPrefab(string prefabName)
        {
            BuildTreeDefinitions();

            TreeDefinition tree = FindTreeDefinition(prefabName);
            if (tree == null || string.IsNullOrWhiteSpace(tree.SeedPrefabName))
            {
                return TreeRegistrar.RavenSeedPrefabName;
            }

            return tree.SeedPrefabName;
        }

        public static string GetRegrowCarrierDisplayName(string prefabName)
        {
            BuildTreeDefinitions();

            TreeDefinition tree = FindTreeDefinition(prefabName);
            if (tree == null || string.IsNullOrWhiteSpace(tree.SeedPrefabName))
            {
                return TreeRegistrar.RavenSeedDisplayName;
            }

            if (string.Equals(tree.SeedPrefabName, TreeRegistrar.RavenSeedPrefabName, StringComparison.Ordinal))
            {
                return TreeRegistrar.RavenSeedDisplayName;
            }

            return tree.SeedPrefabName;
        }

        public static bool GetRespawnEnabled(string prefabName, bool fallback = true)
        {
            BuildTreeDefinitions();

            if (IsRavenwoodPickableMushroomPrefab(prefabName))
            {
                return true;
            }

            TreeDefinition tree = FindTreeDefinition(prefabName);
            if (tree == null)
            {
                return fallback;
            }

            return tree.EnableRespawn != null ? tree.EnableRespawn.Value : tree.DefaultEnableRespawn;
        }

        public static float GetRespawnMinutes(string prefabName, float fallback = DefaultFallbackRespawnMinutes)
        {
            BuildTreeDefinitions();

            TreeDefinition tree = FindTreeDefinition(prefabName);
            if (tree == null)
            {
                return Mathf.Max(0f, fallback);
            }

            float minutes = tree.RespawnMinutes != null ? tree.RespawnMinutes.Value : tree.DefaultRespawnMinutes;
            if (IsRavenwoodPickableMushroomPrefab(prefabName) && minutes <= 0f)
            {
                minutes = DefaultPickableMushroomRespawnMinutes;
            }

            return Mathf.Max(0f, minutes);
        }

        public static string GetDisplayName(string prefabName, string fallback = null)
        {
            BuildTreeDefinitions();

            TreeDefinition tree = FindTreeDefinition(prefabName);
            if (tree != null && !string.IsNullOrWhiteSpace(tree.DisplayName))
            {
                return tree.DisplayName;
            }

            return fallback ?? prefabName;
        }
    }
}
