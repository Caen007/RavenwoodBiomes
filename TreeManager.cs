using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ravenwood.Biomes
{
    public sealed class TreeManager : MonoBehaviour
    {
        private static TreeManager instance;
        private static bool customContentRegistered;

        private const string EternalSupportPrefabName = "RWB_Eternal_ScaledTree11";

        private static readonly Dictionary<string, GameObject> RegisteredTreePrefabs = new Dictionary<string, GameObject>();
        private static GameObject registeredSeedPrefab;
        private static GameObject registeredElixirPrefab;
        private static GameObject registeredSerumPrefab;
        private static GameObject registeredGreenMushroomItemPrefab;
        private static GameObject registeredPurpleMushroomItemPrefab;
        private static AssetBundle registeredBundle;
        private static bool vanillaCultivatorMushroomsRegistered;

        public static void Initialize(BaseUnityPlugin plugin, AssetBundle bundle, ConfigFile config)
        {
            TreeConfigFile.Initialize(config);
            WorkbenchCustomRecipeConfig.Initialize();

            if (instance == null)
            {
                instance = plugin.GetComponent<TreeManager>();
                if (instance == null)
                {
                    instance = plugin.gameObject.AddComponent<TreeManager>();
                }
            }
        }

        public static void RegisterTreePrefabs(AssetBundle bundle)
        {
            if (customContentRegistered || bundle == null)
            {
                return;
            }

            registeredBundle = bundle;
            TreeConfigFile.BuildTreeDefinitions();
            RegisterSeedPrefab(bundle);
            RegisterElixirPrefab(bundle);
            RegisterSerumPrefab(bundle);
            RegisterMushroomItemPrefabs(bundle);
            WorkbenchManager.RegisterWorkbench(bundle);

            int preparedTreeCount = 0;
            int missingTreeCount = 0;

            IReadOnlyList<TreeConfigFile.TreeDefinition> trees = TreeConfigFile.Definitions;

            for (int i = 0; i < trees.Count; i++)
            {
                TreeConfigFile.TreeDefinition tree = trees[i];
                GameObject prefab = ResolveCultivatorPrefab(bundle, tree);

                if (prefab == null)
                {
                    if (!IsVanillaCultivatorMushroomPrefab(tree.PrefabName))
                    {
                        missingTreeCount++;
                        Debug.LogWarning("Tree prefab missing from asset bundle: " + tree.PrefabName);
                    }

                    continue;
                }

                if (IsCultivatorMushroomPrefab(tree.PrefabName))
                {
                    PrepareCultivatorMushroomPrefab(prefab, tree, bundle);
                }
                else
                {
                    PrepareTreePrefab(prefab, tree);
                }

                RegisterCultivatorTree(prefab, tree, bundle);
                RegisteredTreePrefabs[tree.PrefabName] = prefab;
                preparedTreeCount++;
            }

            RegisterRavenwoodWorkbenchRecipes();
            WorkbenchCustomRecipeConfig.MarkCoreRecipesRegistered();
            WorkbenchCustomRecipeConfig.TryRegisterWorkbenchRecipes();
            TryRegisterVanillaCultivatorMushrooms();

            customContentRegistered = true;

            Debug.Log(
                "Tree prefab preparation complete. Prepared trees: " + preparedTreeCount +
                ", Missing trees: " + missingTreeCount +
                ", Seed registered: " + (registeredSeedPrefab != null) +
                ", Elixir registered: " + (registeredElixirPrefab != null) +
                ", Serum registered: " + (registeredSerumPrefab != null) +
                ", Green mushroom item registered: " + (registeredGreenMushroomItemPrefab != null) +
                ", Purple mushroom item registered: " + (registeredPurpleMushroomItemPrefab != null) +
                ", Ravenwood Alchemy Table recipes registered.");
        }

        public static GameObject GetPreparedTreePrefab(string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return null;
            }

            RegisteredTreePrefabs.TryGetValue(prefabName, out GameObject prefab);
            return prefab;
        }

        private static void RegisterSeedPrefab(AssetBundle bundle)
        {
            GameObject seedPrefab = bundle.LoadAsset<GameObject>(TreeRegistrar.RavenSeedPrefabName);
            if (seedPrefab == null)
            {
                Debug.LogWarning("Seed prefab missing from asset bundle: " + TreeRegistrar.RavenSeedPrefabName);
                return;
            }

            PrepareSeedPrefab(seedPrefab, bundle);

            ItemDrop itemDrop = seedPrefab.GetComponent<ItemDrop>();
            if (itemDrop != null)
            {
                ItemManager.Instance.AddItem(new CustomItem(seedPrefab, true));
            }
            else
            {
                PrefabManager.Instance.AddPrefab(new CustomPrefab(seedPrefab, true));
            }

            registeredSeedPrefab = seedPrefab;
        }

        private static void RegisterElixirPrefab(AssetBundle bundle)
        {
            GameObject elixirPrefab = bundle.LoadAsset<GameObject>(TreeRegistrar.RavenElixirPrefabName);
            if (elixirPrefab == null)
            {
                Debug.LogWarning("Elixir prefab missing from asset bundle: " + TreeRegistrar.RavenElixirPrefabName);
                return;
            }

            PrepareElixirPrefab(elixirPrefab, bundle);

            ItemDrop itemDrop = elixirPrefab.GetComponent<ItemDrop>();
            if (itemDrop != null)
            {
                ItemManager.Instance.AddItem(new CustomItem(elixirPrefab, true));
            }
            else
            {
                PrefabManager.Instance.AddPrefab(new CustomPrefab(elixirPrefab, true));
            }

            registeredElixirPrefab = elixirPrefab;
        }

        private static void RegisterSerumPrefab(AssetBundle bundle)
        {
            GameObject serumPrefab = bundle.LoadAsset<GameObject>(TreeRegistrar.RavenSerumPrefabName);
            if (serumPrefab == null)
            {
                Debug.LogWarning("Serum prefab missing from asset bundle: " + TreeRegistrar.RavenSerumPrefabName);
                return;
            }

            PrepareSerumPrefab(serumPrefab, bundle);

            ItemDrop itemDrop = serumPrefab.GetComponent<ItemDrop>();
            if (itemDrop != null)
            {
                ItemManager.Instance.AddItem(new CustomItem(serumPrefab, true));
            }
            else
            {
                PrefabManager.Instance.AddPrefab(new CustomPrefab(serumPrefab, true));
            }

            registeredSerumPrefab = serumPrefab;
        }

        private static void RegisterMushroomItemPrefabs(AssetBundle bundle)
        {
            registeredGreenMushroomItemPrefab = RegisterMushroomItemPrefab(
                bundle,
                TreeRegistrar.GreenMushroomItemPrefabName,
                TreeRegistrar.GreenMushroomDisplayName,
                "A strange green Ravenwood mushroom.",
                registeredGreenMushroomItemPrefab);

            registeredPurpleMushroomItemPrefab = RegisterMushroomItemPrefab(
                bundle,
                TreeRegistrar.PurpleMushroomItemPrefabName,
                TreeRegistrar.PurpleMushroomDisplayName,
                "A strange purple Ravenwood mushroom.",
                registeredPurpleMushroomItemPrefab);
        }

        private static GameObject RegisterMushroomItemPrefab(
            AssetBundle bundle,
            string prefabName,
            string displayName,
            string description,
            GameObject alreadyRegistered)
        {
            if (alreadyRegistered != null)
            {
                return alreadyRegistered;
            }

            if (bundle == null || string.IsNullOrWhiteSpace(prefabName))
            {
                return null;
            }

            GameObject itemPrefab = bundle.LoadAsset<GameObject>(prefabName);
            if (itemPrefab == null)
            {
                Debug.LogWarning("Mushroom item prefab missing from asset bundle: " + prefabName);
                return null;
            }

            PrepareMushroomItemPrefab(itemPrefab, prefabName, displayName, description, bundle);

            ItemDrop itemDrop = itemPrefab.GetComponent<ItemDrop>();
            if (itemDrop != null)
            {
                ItemManager.Instance.AddItem(new CustomItem(itemPrefab, true));
            }
            else
            {
                PrefabManager.Instance.AddPrefab(new CustomPrefab(itemPrefab, true));
            }

            return itemPrefab;
        }

        private static void RegisterElixirRecipe()
        {
            RegisterRecipe(
                "Recipe_" + TreeRegistrar.RavenElixirPrefabName,
                TreeRegistrar.RavenElixirPrefabName,
                1,
                new[]
                {
                    new RequirementConfig(TreeRegistrar.RavenSerumPrefabName, 1, 0, false),
                    new RequirementConfig(TreeRegistrar.GreenMushroomItemPrefabName, 3, 0, false),
                    new RequirementConfig("MushroomBlue", 2, 0, false),
                    new RequirementConfig(TreeRegistrar.PurpleMushroomItemPrefabName, 1, 0, false)
                });
        }

        private static void RegisterRavenwoodWorkbenchRecipes()
        {
            RegisterRecipe(
                "Recipe_" + TreeRegistrar.RavenSeedPrefabName,
                TreeRegistrar.RavenSeedPrefabName,
                1,
                new[]
                {
                    new RequirementConfig("FirCone", 1, 0, false),
                    new RequirementConfig("PineCone", 1, 0, false),
                    new RequirementConfig("BeechSeeds", 1, 0, false),
                    new RequirementConfig("AncientSeed", 1, 0, false)
                });

            RegisterRecipe(
                "Recipe_" + TreeRegistrar.RavenSerumPrefabName,
                TreeRegistrar.RavenSerumPrefabName,
                1,
                new[]
                {
                    new RequirementConfig("Mushroom", 4, 0, false),
                    new RequirementConfig("MushroomYellow", 3, 0, false),
                    new RequirementConfig(TreeRegistrar.GreenMushroomItemPrefabName, 2, 0, false),
                    new RequirementConfig("MushroomBlue", 1, 0, false)
                });

            RegisterElixirRecipe();

            RegisterRecipe(
                "Recipe_RWB_Red_Mushroom",
                "Mushroom",
                1,
                new[]
                {
                    new RequirementConfig(TreeRegistrar.RavenSeedPrefabName, 1, 0, false),
                    new RequirementConfig("RawMeat", 1, 0, false),
                    new RequirementConfig("Raspberry", 1, 0, false),
                    new RequirementConfig("Bloodbag", 1, 0, false)
                });

            RegisterRecipe(
                "Recipe_RWB_Yellow_Mushroom",
                "MushroomYellow",
                1,
                new[]
                {
                    new RequirementConfig(TreeRegistrar.RavenSeedPrefabName, 1, 0, false),
                    new RequirementConfig("Dandelion", 1, 0, false),
                    new RequirementConfig("Resin", 1, 0, false),
                    new RequirementConfig("Honey", 1, 0, false)
                });

            RegisterRecipe(
                "Recipe_RWB_Green_Mushroom",
                TreeRegistrar.GreenMushroomItemPrefabName,
                1,
                new[]
                {
                    new RequirementConfig(TreeRegistrar.RavenSeedPrefabName, 1, 0, false),
                    new RequirementConfig("NeckTail", 1, 0, false),
                    new RequirementConfig("Pukeberries", 1, 0, false),
                    new RequirementConfig("Guck", 1, 0, false)
                });

            RegisterRecipe(
                "Recipe_RWB_Blue_Mushroom",
                "MushroomBlue",
                1,
                new[]
                {
                    new RequirementConfig(TreeRegistrar.RavenSeedPrefabName, 1, 0, false),
                    new RequirementConfig("Thistle", 1, 0, false),
                    new RequirementConfig("Blueberries", 1, 0, false),
                    new RequirementConfig("FreezeGland", 1, 0, false)
                });

            RegisterRecipe(
                "Recipe_RWB_Purple_Mushroom",
                TreeRegistrar.PurpleMushroomItemPrefabName,
                1,
                new[]
                {
                    new RequirementConfig("Mushroom", 1, 0, false),
                    new RequirementConfig("MushroomYellow", 1, 0, false),
                    new RequirementConfig(TreeRegistrar.GreenMushroomItemPrefabName, 1, 0, false),
                    new RequirementConfig("MushroomBlue", 1, 0, false)
                });
        }

        private static void RegisterRecipe(string name, string item, int amount, RequirementConfig[] requirements)
        {
            if (!CanRegisterRecipe(item, requirements))
            {
                Debug.LogWarning("[RavenwoodBiomes] Alchemy Table recipe skipped because a Ravenwood item prefab is missing: " + name);
                return;
            }

            RecipeConfig recipeConfig = new RecipeConfig();
            recipeConfig.Name = name;
            recipeConfig.Item = item;
            recipeConfig.Amount = amount;
            recipeConfig.CraftingStation = WorkbenchManager.WorkbenchPrefabName;
            recipeConfig.MinStationLevel = 1;
            recipeConfig.Requirements = requirements;

            int sortWeight = WorkbenchCustomRecipeConfig.GetAlchemyRecipeSortWeight(name, item, 100);
            WorkbenchCustomRecipeConfig.ApplyRecipeConfigSortWeight(recipeConfig, sortWeight);

            ItemManager.Instance.AddRecipe(new CustomRecipe(recipeConfig));
            WorkbenchCustomRecipeConfig.ApplyKnownAlchemyRecipeSortWeights();
        }

        private static bool CanRegisterRecipe(string item, RequirementConfig[] requirements)
        {
            if (IsMissingRavenwoodItemPrefab(item))
            {
                return false;
            }

            if (requirements == null)
            {
                return true;
            }

            for (int i = 0; i < requirements.Length; i++)
            {
                RequirementConfig requirement = requirements[i];
                if (requirement != null && IsMissingRavenwoodItemPrefab(requirement.Item))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsMissingRavenwoodItemPrefab(string prefabName)
        {
            if (string.Equals(prefabName, TreeRegistrar.RavenSeedPrefabName, StringComparison.Ordinal))
            {
                return registeredSeedPrefab == null;
            }

            if (string.Equals(prefabName, TreeRegistrar.RavenElixirPrefabName, StringComparison.Ordinal))
            {
                return registeredElixirPrefab == null;
            }

            if (string.Equals(prefabName, TreeRegistrar.RavenSerumPrefabName, StringComparison.Ordinal))
            {
                return registeredSerumPrefab == null;
            }

            if (string.Equals(prefabName, TreeRegistrar.GreenMushroomItemPrefabName, StringComparison.Ordinal))
            {
                return registeredGreenMushroomItemPrefab == null;
            }

            if (string.Equals(prefabName, TreeRegistrar.PurpleMushroomItemPrefabName, StringComparison.Ordinal))
            {
                return registeredPurpleMushroomItemPrefab == null;
            }

            return false;
        }

        private static string GetCultivatorIconAssetName(TreeConfigFile.TreeDefinition tree)
        {
            if (tree == null || string.IsNullOrWhiteSpace(tree.PrefabName))
            {
                return string.Empty;
            }

            if (string.Equals(tree.PrefabName, TreeRegistrar.GreenMushroomPrefabName, StringComparison.Ordinal))
            {
                return TreeRegistrar.GreenMushroomItemPrefabName;
            }

            if (string.Equals(tree.PrefabName, TreeRegistrar.PurpleMushroomPrefabName, StringComparison.Ordinal))
            {
                return TreeRegistrar.PurpleMushroomItemPrefabName;
            }

            return tree.PrefabName;
        }


        private static GameObject ResolveCultivatorPrefab(AssetBundle bundle, TreeConfigFile.TreeDefinition tree)
        {
            if (tree == null || string.IsNullOrWhiteSpace(tree.PrefabName))
            {
                return null;
            }

            GameObject prefab = bundle != null ? bundle.LoadAsset<GameObject>(tree.PrefabName) : null;
            if (prefab != null)
            {
                return prefab;
            }

            if (!IsVanillaCultivatorMushroomPrefab(tree.PrefabName) || ZNetScene.instance == null)
            {
                return null;
            }

            return ZNetScene.instance.GetPrefab(tree.PrefabName);
        }

        private static bool IsCultivatorMushroomPrefab(string prefabName)
        {
            return IsVanillaCultivatorMushroomPrefab(prefabName) ||
                   string.Equals(prefabName, TreeRegistrar.GreenMushroomPrefabName, StringComparison.Ordinal) ||
                   string.Equals(prefabName, TreeRegistrar.PurpleMushroomPrefabName, StringComparison.Ordinal);
        }

        private static bool IsVanillaCultivatorMushroomPrefab(string prefabName)
        {
            return string.Equals(prefabName, "Pickable_Mushroom", StringComparison.Ordinal) ||
                   string.Equals(prefabName, "Pickable_Mushroom_yellow", StringComparison.Ordinal) ||
                   string.Equals(prefabName, "Pickable_Mushroom_blue", StringComparison.Ordinal);
        }

        private static string GetCultivatorMushroomItemPrefabName(string prefabName)
        {
            if (string.Equals(prefabName, "Pickable_Mushroom", StringComparison.Ordinal))
            {
                return "Mushroom";
            }

            if (string.Equals(prefabName, "Pickable_Mushroom_yellow", StringComparison.Ordinal))
            {
                return "MushroomYellow";
            }

            if (string.Equals(prefabName, "Pickable_Mushroom_blue", StringComparison.Ordinal))
            {
                return "MushroomBlue";
            }

            if (string.Equals(prefabName, TreeRegistrar.GreenMushroomPrefabName, StringComparison.Ordinal))
            {
                return TreeRegistrar.GreenMushroomItemPrefabName;
            }

            if (string.Equals(prefabName, TreeRegistrar.PurpleMushroomPrefabName, StringComparison.Ordinal))
            {
                return TreeRegistrar.PurpleMushroomItemPrefabName;
            }

            return string.Empty;
        }

        private static GameObject ResolveCultivatorMushroomItemPrefab(AssetBundle bundle, string pickablePrefabName)
        {
            string itemPrefabName = GetCultivatorMushroomItemPrefabName(pickablePrefabName);
            if (string.IsNullOrWhiteSpace(itemPrefabName))
            {
                return null;
            }

            if (string.Equals(itemPrefabName, TreeRegistrar.GreenMushroomItemPrefabName, StringComparison.Ordinal) && registeredGreenMushroomItemPrefab != null)
            {
                return registeredGreenMushroomItemPrefab;
            }

            if (string.Equals(itemPrefabName, TreeRegistrar.PurpleMushroomItemPrefabName, StringComparison.Ordinal) && registeredPurpleMushroomItemPrefab != null)
            {
                return registeredPurpleMushroomItemPrefab;
            }

            if (ObjectDB.instance != null)
            {
                GameObject objectDbPrefab = ObjectDB.instance.GetItemPrefab(itemPrefabName);
                if (objectDbPrefab != null)
                {
                    return objectDbPrefab;
                }
            }

            if (ZNetScene.instance != null)
            {
                GameObject znetPrefab = ZNetScene.instance.GetPrefab(itemPrefabName);
                if (znetPrefab != null)
                {
                    return znetPrefab;
                }
            }

            return bundle != null ? bundle.LoadAsset<GameObject>(itemPrefabName) : null;
        }

        private static void EnsurePickableMushroomItemPrefab(GameObject prefab, TreeConfigFile.TreeDefinition tree, AssetBundle bundle)
        {
            if (prefab == null || tree == null)
            {
                return;
            }

            Pickable pickable = prefab.GetComponent<Pickable>();
            if (pickable == null)
            {
                pickable = prefab.AddComponent<Pickable>();
            }

            GameObject itemPrefab = ResolveCultivatorMushroomItemPrefab(bundle, tree.PrefabName);
            if (itemPrefab == null)
            {
                Debug.LogWarning("[RavenwoodBiomes] Pickable mushroom item prefab missing for: " + tree.PrefabName);
                return;
            }

            FieldInfo itemPrefabField = AccessTools.Field(typeof(Pickable), "m_itemPrefab");
            if (itemPrefabField != null)
            {
                itemPrefabField.SetValue(pickable, itemPrefab);
            }

            FieldInfo amountField = AccessTools.Field(typeof(Pickable), "m_amount");
            if (amountField != null)
            {
                amountField.SetValue(pickable, 1);
            }
        }

        private static Sprite ResolveCultivatorIcon(AssetBundle bundle, TreeConfigFile.TreeDefinition tree)
        {
            if (tree == null)
            {
                return null;
            }

            Sprite icon = LoadIconSprite(bundle, GetCultivatorIconAssetName(tree));
            if (icon != null)
            {
                return icon;
            }

            string itemPrefabName = GetCultivatorMushroomItemPrefabName(tree.PrefabName);
            if (string.IsNullOrWhiteSpace(itemPrefabName) || ZNetScene.instance == null)
            {
                return null;
            }

            GameObject itemPrefab = ZNetScene.instance.GetPrefab(itemPrefabName);
            ItemDrop itemDrop = itemPrefab != null ? itemPrefab.GetComponent<ItemDrop>() : null;
            if (itemDrop == null || itemDrop.m_itemData == null || itemDrop.m_itemData.m_shared == null)
            {
                return null;
            }

            Sprite[] icons = itemDrop.m_itemData.m_shared.m_icons;
            return icons != null && icons.Length > 0 ? icons[0] : null;
        }

        private static void PrepareCultivatorMushroomPrefab(GameObject prefab, TreeConfigFile.TreeDefinition tree, AssetBundle bundle)
        {
            if (prefab == null || tree == null)
            {
                return;
            }

            ZNetView znv = prefab.GetComponent<ZNetView>();
            if (znv == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            znv.m_persistent = true;
            znv.m_syncInitialScale = true;

            Piece piece = prefab.GetComponent<Piece>();
            if (piece == null)
            {
                piece = prefab.AddComponent<Piece>();
            }

            piece.m_name = tree.DisplayName;
            piece.m_description = tree.Description;
            piece.m_groundOnly = false;
            piece.m_canBeRemoved = false;

            EnsurePickableMushroomItemPrefab(prefab, tree, bundle);
        }

        private static void TryRegisterVanillaCultivatorMushrooms()
        {
            if (vanillaCultivatorMushroomsRegistered || ZNetScene.instance == null)
            {
                return;
            }

            TreeConfigFile.BuildTreeDefinitions();

            int needed = 0;
            int registeredNow = 0;
            IReadOnlyList<TreeConfigFile.TreeDefinition> trees = TreeConfigFile.Definitions;

            for (int i = 0; i < trees.Count; i++)
            {
                TreeConfigFile.TreeDefinition tree = trees[i];
                if (tree == null || !IsVanillaCultivatorMushroomPrefab(tree.PrefabName))
                {
                    continue;
                }

                needed++;

                if (RegisteredTreePrefabs.ContainsKey(tree.PrefabName))
                {
                    registeredNow++;
                    continue;
                }

                GameObject prefab = ZNetScene.instance.GetPrefab(tree.PrefabName);
                if (prefab == null)
                {
                    continue;
                }

                PrepareCultivatorMushroomPrefab(prefab, tree, registeredBundle);
                RegisterCultivatorTree(prefab, tree, registeredBundle);
                RegisteredTreePrefabs[tree.PrefabName] = prefab;
                registeredNow++;
            }

            vanillaCultivatorMushroomsRegistered = needed > 0 && registeredNow == needed;
        }

        private static void RegisterCultivatorTree(GameObject prefab, TreeConfigFile.TreeDefinition tree, AssetBundle bundle)
        {
            if (prefab == null || tree == null)
            {
                return;
            }

            Sprite icon = ResolveCultivatorIcon(bundle, tree);
            PieceConfig pieceConfig = new PieceConfig();
            pieceConfig.Name = tree.DisplayName;
            pieceConfig.Description = tree.Description;
            pieceConfig.PieceTable = PieceTables.Cultivator;
            pieceConfig.Category = tree.Category;
            pieceConfig.Icon = icon;
            pieceConfig.Requirements = CloneRequirements(tree.Requirements);

            PieceManager.Instance.AddPiece(new CustomPiece(prefab, true, pieceConfig));
        }

        private static RequirementConfig[] CloneRequirements(RequirementConfig[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<RequirementConfig>();
            }

            RequirementConfig[] clone = new RequirementConfig[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                RequirementConfig requirement = source[i];
                if (requirement == null)
                {
                    continue;
                }

                clone[i] = new RequirementConfig(requirement.Item, requirement.Amount, requirement.AmountPerLevel, requirement.Recover);
            }

            return clone;
        }

        private static void PrepareSeedPrefab(GameObject prefab, AssetBundle bundle)
        {
            if (prefab == null)
            {
                return;
            }

            prefab.name = TreeRegistrar.RavenSeedPrefabName;
            SetLayerRecursively(prefab, "item");
            EnsureSolidColliders(prefab);
            EnsureSeedRigidbody(prefab);
            RemoveSeedPiece(prefab);
            RemoveItemDrops(prefab);
            RemovePickables(prefab);

            ZNetView znv = prefab.GetComponent<ZNetView>();
            if (znv == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            znv.m_persistent = true;
            znv.m_syncInitialScale = true;

            Sprite icon = LoadIconSprite(bundle, TreeRegistrar.RavenSeedPrefabName);

            ItemDrop itemDrop = prefab.AddComponent<ItemDrop>();
            itemDrop.m_autoPickup = true;
            itemDrop.m_autoDestroy = true;
            itemDrop.m_itemData = CreateSeedItemData(prefab, icon);
        }

        private static void PrepareElixirPrefab(GameObject prefab, AssetBundle bundle)
        {
            if (prefab == null)
            {
                return;
            }

            prefab.name = TreeRegistrar.RavenElixirPrefabName;
            SetLayerRecursively(prefab, "item");
            EnsureSolidColliders(prefab);
            EnsureSeedRigidbody(prefab);
            RemoveSeedPiece(prefab);
            RemoveItemDrops(prefab);
            RemovePickables(prefab);

            ZNetView znv = prefab.GetComponent<ZNetView>();
            if (znv == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            znv.m_persistent = true;
            znv.m_syncInitialScale = true;

            Sprite icon = LoadIconSprite(bundle, TreeRegistrar.RavenElixirPrefabName);

            ItemDrop itemDrop = prefab.AddComponent<ItemDrop>();
            itemDrop.m_autoPickup = true;
            itemDrop.m_autoDestroy = true;
            itemDrop.m_itemData = CreateElixirItemData(prefab, icon);
        }

        private static void PrepareSerumPrefab(GameObject prefab, AssetBundle bundle)
        {
            if (prefab == null)
            {
                return;
            }

            prefab.name = TreeRegistrar.RavenSerumPrefabName;
            SetLayerRecursively(prefab, "item");
            EnsureSolidColliders(prefab);
            EnsureSeedRigidbody(prefab);
            RemoveSeedPiece(prefab);
            RemoveItemDrops(prefab);
            RemovePickables(prefab);

            ZNetView znv = prefab.GetComponent<ZNetView>();
            if (znv == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            znv.m_persistent = true;
            znv.m_syncInitialScale = true;

            Sprite icon = LoadIconSprite(bundle, TreeRegistrar.RavenSerumPrefabName);

            ItemDrop itemDrop = prefab.AddComponent<ItemDrop>();
            itemDrop.m_autoPickup = true;
            itemDrop.m_autoDestroy = true;
            itemDrop.m_itemData = CreateSerumItemData(prefab, icon);
        }

        private static void PrepareMushroomItemPrefab(GameObject prefab, string prefabName, string displayName, string description, AssetBundle bundle)
        {
            if (prefab == null)
            {
                return;
            }

            prefab.name = prefabName;

            ZNetView znv = prefab.GetComponent<ZNetView>();
            if (znv == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            znv.m_persistent = true;
            znv.m_syncInitialScale = true;

            Sprite icon = LoadIconSprite(bundle, prefabName);

            ItemDrop itemDrop = prefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                itemDrop = prefab.AddComponent<ItemDrop>();
            }

            ItemDrop.ItemData.SharedData existingShared = itemDrop.m_itemData != null
                ? itemDrop.m_itemData.m_shared
                : null;

            itemDrop.m_autoPickup = true;
            itemDrop.m_autoDestroy = true;
            itemDrop.m_itemData = CreateMushroomItemData(prefab, prefabName, displayName, description, icon, existingShared);
        }

        private static ItemDrop.ItemData CreateSeedItemData(GameObject prefab, Sprite icon)
        {
            ItemDrop.ItemData itemData = new ItemDrop.ItemData();
            itemData.m_dropPrefab = prefab;
            itemData.m_stack = 1;
            itemData.m_durability = 100f;
            itemData.m_quality = 1;
            itemData.m_variant = 0;
            itemData.m_worldLevel = 0;

            ItemDrop.ItemData.SharedData shared = new ItemDrop.ItemData.SharedData();
            shared.m_name = TreeRegistrar.RavenSeedDisplayName;
            shared.m_description = "A strange seed from Ravenwood.";
            shared.m_dlc = string.Empty;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            shared.m_maxStackSize = 100;
            shared.m_maxQuality = 1;
            shared.m_weight = 0.1f;
            shared.m_value = 0;
            shared.m_teleportable = true;
            shared.m_variants = 1;
            shared.m_useDurability = false;
            shared.m_destroyBroken = false;
            shared.m_canBeReparied = false;
            shared.m_food = 0f;
            shared.m_foodStamina = 0f;
            shared.m_foodEitr = 0f;
            shared.m_foodBurnTime = 0f;
            shared.m_foodRegen = 0f;
            shared.m_attackForce = 0f;
            shared.m_timedBlockBonus = 1f;
            shared.m_skillType = Skills.SkillType.None;
            shared.m_icons = icon != null ? new[] { icon } : Array.Empty<Sprite>();

            itemData.m_shared = shared;
            return itemData;
        }

        private static ItemDrop.ItemData CreateElixirItemData(GameObject prefab, Sprite icon)
        {
            ItemDrop.ItemData itemData = new ItemDrop.ItemData();
            itemData.m_dropPrefab = prefab;
            itemData.m_stack = 1;
            itemData.m_durability = 100f;
            itemData.m_quality = 1;
            itemData.m_variant = 0;
            itemData.m_worldLevel = 0;

            ItemDrop.ItemData.SharedData shared = new ItemDrop.ItemData.SharedData();
            shared.m_name = TreeRegistrar.RavenElixirDisplayName;
            shared.m_description = "A rare elixir used to grow eternal Ravenwood.";
            shared.m_dlc = string.Empty;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            shared.m_maxStackSize = 50;
            shared.m_maxQuality = 1;
            shared.m_weight = 0.2f;
            shared.m_value = 0;
            shared.m_teleportable = true;
            shared.m_variants = 1;
            shared.m_useDurability = false;
            shared.m_destroyBroken = false;
            shared.m_canBeReparied = false;
            shared.m_food = 0f;
            shared.m_foodStamina = 0f;
            shared.m_foodEitr = 0f;
            shared.m_foodBurnTime = 0f;
            shared.m_foodRegen = 0f;
            shared.m_attackForce = 0f;
            shared.m_timedBlockBonus = 1f;
            shared.m_skillType = Skills.SkillType.None;
            shared.m_icons = icon != null ? new[] { icon } : Array.Empty<Sprite>();

            itemData.m_shared = shared;
            return itemData;
        }

        private static ItemDrop.ItemData CreateSerumItemData(GameObject prefab, Sprite icon)
        {
            ItemDrop.ItemData itemData = new ItemDrop.ItemData();
            itemData.m_dropPrefab = prefab;
            itemData.m_stack = 1;
            itemData.m_durability = 100f;
            itemData.m_quality = 1;
            itemData.m_variant = 0;
            itemData.m_worldLevel = 0;

            ItemDrop.ItemData.SharedData shared = new ItemDrop.ItemData.SharedData();
            shared.m_name = TreeRegistrar.RavenSerumDisplayName;
            shared.m_description = "A Ravenwood serum used to grow scaled Ravenwood vegetations.";
            shared.m_dlc = string.Empty;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            shared.m_maxStackSize = 50;
            shared.m_maxQuality = 1;
            shared.m_weight = 0.2f;
            shared.m_value = 0;
            shared.m_teleportable = true;
            shared.m_variants = 1;
            shared.m_useDurability = false;
            shared.m_destroyBroken = false;
            shared.m_canBeReparied = false;
            shared.m_food = 0f;
            shared.m_foodStamina = 0f;
            shared.m_foodEitr = 0f;
            shared.m_foodBurnTime = 0f;
            shared.m_foodRegen = 0f;
            shared.m_attackForce = 0f;
            shared.m_timedBlockBonus = 1f;
            shared.m_skillType = Skills.SkillType.None;
            shared.m_icons = icon != null ? new[] { icon } : Array.Empty<Sprite>();

            itemData.m_shared = shared;
            return itemData;
        }

        private static ItemDrop.ItemData CreateMushroomItemData(GameObject prefab, string prefabName, string displayName, string description, Sprite icon, ItemDrop.ItemData.SharedData existingShared)
        {
            ItemDrop.ItemData itemData = new ItemDrop.ItemData();
            itemData.m_dropPrefab = prefab;
            itemData.m_stack = 1;
            itemData.m_durability = 100f;
            itemData.m_quality = 1;
            itemData.m_variant = 0;
            itemData.m_worldLevel = 0;

            ItemDrop.ItemData.SharedData shared = existingShared ?? new ItemDrop.ItemData.SharedData();
            shared.m_name = displayName;
            shared.m_description = description;
            shared.m_dlc = string.Empty;
            shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            shared.m_maxStackSize = 50;
            shared.m_maxQuality = 1;
            shared.m_weight = 0.1f;
            shared.m_value = shared.m_value > 0 ? shared.m_value : 0;
            shared.m_teleportable = true;
            shared.m_variants = shared.m_variants > 0 ? shared.m_variants : 1;
            shared.m_useDurability = false;
            shared.m_destroyBroken = false;
            shared.m_canBeReparied = false;

            if (string.Equals(prefabName, TreeRegistrar.GreenMushroomItemPrefabName, StringComparison.Ordinal))
            {
                shared.m_food = 30f;
                shared.m_foodStamina = 10f;
                shared.m_foodEitr = 0f;
            }
            else if (string.Equals(prefabName, TreeRegistrar.PurpleMushroomItemPrefabName, StringComparison.Ordinal))
            {
                shared.m_food = 22f;
                shared.m_foodStamina = 22f;
                shared.m_foodEitr = 22f;
            }
            else
            {
                shared.m_food = shared.m_food > 0f ? shared.m_food : 15f;
                shared.m_foodStamina = shared.m_foodStamina > 0f ? shared.m_foodStamina : 15f;
                shared.m_foodEitr = Mathf.Max(0f, shared.m_foodEitr);
            }

            shared.m_foodBurnTime = shared.m_foodBurnTime > 0f ? shared.m_foodBurnTime : 900f;
            shared.m_foodRegen = shared.m_foodRegen > 0f ? shared.m_foodRegen : 1f;
            shared.m_attackForce = Mathf.Max(0f, shared.m_attackForce);
            shared.m_timedBlockBonus = shared.m_timedBlockBonus > 0f ? shared.m_timedBlockBonus : 1f;
            shared.m_skillType = Skills.SkillType.None;
            shared.m_icons = icon != null ? new[] { icon } : Array.Empty<Sprite>();

            itemData.m_shared = shared;
            return itemData;
        }

        private static Sprite LoadIconSprite(AssetBundle bundle, string assetName)
        {
            if (bundle == null || string.IsNullOrWhiteSpace(assetName))
            {
                return null;
            }

            try
            {
                return bundle.LoadAsset<Sprite>(assetName);
            }
            catch
            {
                return null;
            }
        }

        private static void PrepareTreePrefab(GameObject prefab, TreeConfigFile.TreeDefinition tree)
        {
            if (prefab == null || tree == null)
            {
                return;
            }

            prefab.name = tree.PrefabName;

            RemoveItemDrops(prefab);
            RemovePickables(prefab);

            if (IsEternalSupportTree(tree.PrefabName))
            {
                PrepareEternalSupportTreePrefab(prefab, tree);
            }
            else
            {
                PrepareStandardTreePrefab(prefab, tree);
            }

            EnsureSolidColliders(prefab);

            TreeRuntimeState runtime = prefab.GetComponent<TreeRuntimeState>();
            if (runtime == null)
            {
                runtime = prefab.AddComponent<TreeRuntimeState>();
            }

            runtime.Configure(TreeConfigFile.BuildDropEntries(tree), tree.Indestructible);

            TreeHoverText hover = prefab.GetComponent<TreeHoverText>();
            if (hover == null)
            {
                hover = prefab.AddComponent<TreeHoverText>();
            }

            hover.Configure(tree.PrefabName);
        }

        private static void PrepareStandardTreePrefab(GameObject prefab, TreeConfigFile.TreeDefinition tree)
        {
            Destructible destructible = prefab.GetComponent<Destructible>();
            WearNTear wear = prefab.GetComponent<WearNTear>();
            ZNetView znv = prefab.GetComponent<ZNetView>();

            if (znv == null && destructible == null && wear == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            if (znv != null)
            {
                znv.m_persistent = true;
                znv.m_syncInitialScale = true;
            }

            if (destructible == null && wear == null)
            {
                if (prefab.GetComponent<ZSyncTransform>() == null)
                {
                    prefab.AddComponent<ZSyncTransform>();
                }

                Piece piece = prefab.GetComponent<Piece>();
                if (piece == null)
                {
                    piece = prefab.AddComponent<Piece>();
                }

                piece.m_name = tree.DisplayName;
                piece.m_description = tree.Description;
                piece.m_groundOnly = false;
                piece.m_canBeRemoved = true;

                wear = prefab.GetComponent<WearNTear>();
                if (wear == null)
                {
                    wear = prefab.AddComponent<WearNTear>();
                }

                wear.m_health = ResolveDefaultHealth(tree.PrefabName);
                wear.m_noRoofWear = true;
            }
            else
            {
                if (wear != null)
                {
                    wear.m_noRoofWear = true;
                }

                if (destructible != null)
                {
                }
            }

            if (tree.Indestructible)
            {
                ApplyIndestructibleDamageProfile(prefab);
            }
        }

        private static void PrepareEternalSupportTreePrefab(GameObject prefab, TreeConfigFile.TreeDefinition tree)
        {
            if (prefab == null || tree == null)
            {
                return;
            }

            RemoveEternalVegetationComponents(prefab);
            RemoveComponentsInChildren<WearNTear>(prefab);
            RemoveComponentsInChildren<Rigidbody>(prefab);

            SetLayerRecursively(prefab, "piece");
            ApplyEternalSupportColliderLayer(prefab);
            EnsureStaticSupportPhysics(prefab);
            EnsureEternalTreeBaseProfile(prefab);

            ZNetView znv = prefab.GetComponent<ZNetView>();
            if (znv == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            znv.m_persistent = true;
            znv.m_syncInitialScale = true;

            if (prefab.GetComponent<ZSyncTransform>() == null)
            {
                prefab.AddComponent<ZSyncTransform>();
            }

            Piece piece = prefab.GetComponent<Piece>();
            if (piece == null)
            {
                piece = prefab.AddComponent<Piece>();
            }

            piece.m_name = tree.DisplayName;
            piece.m_description = tree.Description;
            piece.m_groundOnly = false;
            piece.m_canBeRemoved = true;

            Debug.Log("[RavenwoodBiomes] Eternal support tree prepared: root piece layer, support collider static_solid, TreeBase + StaticPhysics profile.");
        }

        private static bool IsEternalSupportTree(string prefabName)
        {
            return string.Equals(prefabName, EternalSupportPrefabName, StringComparison.Ordinal);
        }

        private static void RemoveEternalVegetationComponents(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            RemoveComponentsInChildren<Destructible>(prefab);
            RemoveComponentsInChildren<TreeLog>(prefab);
            RemoveComponentsInChildren<Plant>(prefab);
        }

        private static void RemoveComponentsInChildren<T>(GameObject root) where T : Component
        {
            if (root == null)
            {
                return;
            }

            T[] components = root.GetComponentsInChildren<T>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null)
                {
                    Object.DestroyImmediate(components[i], true);
                }
            }
        }

        private static void EnsureStaticSupportPhysics(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            StaticPhysics staticPhysics = prefab.GetComponent<StaticPhysics>();
            if (staticPhysics == null)
            {
                prefab.AddComponent<StaticPhysics>();
            }
        }

        private static void EnsureEternalTreeBaseProfile(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            TreeBase treeBase = prefab.GetComponent<TreeBase>();
            if (treeBase == null)
            {
                treeBase = prefab.AddComponent<TreeBase>();
            }

            treeBase.m_health = 1000000f;
            treeBase.m_minToolTier = 1073741823;
        }

        private static void ApplyEternalSupportColliderLayer(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            int staticSolidLayer = LayerMask.NameToLayer("static_solid");
            if (staticSolidLayer < 0)
            {
                return;
            }

            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform child = transforms[i];
                if (child == null || child.gameObject == prefab)
                {
                    continue;
                }

                string childName = child.name;
                bool supportCollider = string.Equals(childName, "Collider", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(childName, "SupportCollider", StringComparison.OrdinalIgnoreCase) ||
                                       childName.IndexOf("support", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       childName.IndexOf("collider", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!supportCollider)
                {
                    continue;
                }

                SetLayerRecursively(child.gameObject, "static_solid");
                DisableRenderers(child.gameObject);
                EnsureColliderTree(child.gameObject);
            }
        }

        private static void DisableRenderers(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = false;
                }
            }
        }

        private static void EnsureColliderTree(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null)
                {
                    continue;
                }

                collider.enabled = true;
                collider.isTrigger = false;
            }
        }

        public static bool TryRestoreExistingTreeState(TreeRuntimeState runtime)
        {
            if (runtime == null || runtime.gameObject == null)
            {
                return false;
            }

            TreeConfigFile.BuildTreeDefinitions();

            string prefabName = CleanPrefabName(runtime.gameObject.name);
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return false;
            }

            TreeConfigFile.TreeDefinition tree = TreeConfigFile.FindTreeDefinition(prefabName);
            if (tree == null)
            {
                return false;
            }

            runtime.Configure(TreeConfigFile.BuildDropEntries(tree), tree.Indestructible);

            TreeHoverText hover = runtime.GetComponent<TreeHoverText>();
            if (hover != null)
            {
                hover.Configure(tree.PrefabName);
            }

            return true;
        }

        private static void RemoveSeedPiece(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            Piece piece = prefab.GetComponent<Piece>();
            if (piece != null)
            {
                Object.DestroyImmediate(piece, true);
            }
        }

        private static void EnsureSeedRigidbody(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            Rigidbody body = prefab.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = prefab.AddComponent<Rigidbody>();
            }

            body.mass = 1f;
            body.drag = 0f;
            body.angularDrag = 0.05f;
            body.useGravity = true;
            body.isKinematic = false;
            body.interpolation = RigidbodyInterpolation.None;
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        private static void SetLayerRecursively(GameObject root, string layerName)
        {
            if (root == null || string.IsNullOrWhiteSpace(layerName))
            {
                return;
            }

            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                return;
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child != null)
                {
                    child.gameObject.layer = layer;
                }
            }
        }

        private static void EnsureSolidColliders(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            Collider[] colliders = prefab.GetComponentsInChildren<Collider>(true);
            if (colliders != null && colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    Collider collider = colliders[i];
                    if (collider == null)
                    {
                        continue;
                    }

                    collider.enabled = true;
                    collider.isTrigger = false;
                }

                return;
            }

            BoxCollider box = prefab.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = prefab.AddComponent<BoxCollider>();
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            box.center = prefab.transform.InverseTransformPoint(bounds.center);
            box.size = bounds.size;
            box.enabled = true;
            box.isTrigger = false;
        }

        private static void ApplyIndestructibleDamageProfile(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            WearNTear wear = prefab.GetComponent<WearNTear>();
            if (wear == null)
            {
                wear = prefab.AddComponent<WearNTear>();
            }

            wear.m_health = 1000000f;
            wear.m_noRoofWear = true;

            Destructible destructible = prefab.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.m_health = 1000000f;
            }
        }

        private static float ResolveDefaultHealth(string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return 200f;
            }

            if (TreeFeedbackEffects.IsSmallVegetationPrefab(prefabName))
            {
                return 100f;
            }

            if (TreeFeedbackEffects.IsNormalVegetationHealthPrefab(prefabName))
            {
                return 300f;
            }

            return 200f;
        }

        private static void RemoveItemDrops(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            ItemDrop[] itemDrops = root.GetComponentsInChildren<ItemDrop>(true);
            for (int i = 0; i < itemDrops.Length; i++)
            {
                if (itemDrops[i] != null)
                {
                    Object.DestroyImmediate(itemDrops[i], true);
                }
            }
        }

        private static void RemovePickables(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Pickable[] pickables = root.GetComponentsInChildren<Pickable>(true);
            for (int i = 0; i < pickables.Length; i++)
            {
                if (pickables[i] != null)
                {
                    Object.DestroyImmediate(pickables[i], true);
                }
            }
        }

        private static string CleanPrefabName(string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return string.Empty;
            }

            string cleaned = prefabName.Trim();
            if (cleaned.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - "(Clone)".Length).Trim();
            }

            return cleaned;
        }


        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        private static class ZNetScene_Awake_Patch
        {
            private static void Postfix()
            {
                TryRegisterVanillaCultivatorMushrooms();
            }
        }
    }
}
