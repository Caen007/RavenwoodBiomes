using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Ravenwood.Biomes
{
    public sealed class TreeManager : MonoBehaviour
    {
        private static TreeManager instance;
        private static bool customContentRegistered;

        private static readonly Dictionary<string, GameObject> RegisteredTreePrefabs = new Dictionary<string, GameObject>();
        private static GameObject registeredSeedPrefab;
        private static GameObject registeredElixirPrefab;
        private static GameObject registeredSerumPrefab;
        private static AssetBundle registeredBundle;

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
            PickableMushroomManager.RegisterItemPrefabs(bundle);
            PickableMushroomManager.RegisterPickablePrefabs(bundle);
            WorkbenchManager.RegisterWorkbench(bundle);

            int preparedWorldCount = 0;
            int preparedCultivatorCount = 0;
            int missingWorldCount = 0;
            int missingCultivatorCount = 0;
            int preparedPickableCount = 0;
            int missingPickableCount = 0;

            IReadOnlyList<TreeConfigFile.TreeDefinition> trees = TreeConfigFile.Definitions;

            for (int i = 0; i < trees.Count; i++)
            {
                TreeConfigFile.TreeDefinition tree = trees[i];
                if (tree == null || string.IsNullOrWhiteSpace(tree.PrefabName))
                {
                    continue;
                }

                if (PickableMushroomManager.IsCultivatorMushroomPrefab(tree.PrefabName))
                {
                    GameObject pickablePrefab = bundle.LoadAsset<GameObject>(tree.PrefabName);
                    if (pickablePrefab == null)
                    {
                        missingPickableCount++;
                        Debug.LogWarning("[RavenwoodBiomes] Pickable cultivator prefab missing from asset bundle: " + tree.PrefabName);
                        continue;
                    }

                    PickableMushroomManager.PrepareCultivatorMushroomPrefab(pickablePrefab, tree);
                    RegisterPickableCultivatorTree(pickablePrefab, tree, bundle);
                    RegisteredTreePrefabs[tree.PrefabName] = pickablePrefab;
                    preparedPickableCount++;
                    continue;
                }

                GameObject worldPrefab = WorldVegetationManager.RegisterWorldVegetation(bundle, tree);
                if (worldPrefab == null)
                {
                    missingWorldCount++;
                }
                else
                {
                    RegisteredTreePrefabs[tree.PrefabName] = worldPrefab;
                    preparedWorldCount++;
                }

                GameObject cultivatorPrefab = CultivatorVegetationManager.RegisterCultivatorVegetation(bundle, tree);
                if (cultivatorPrefab == null)
                {
                    missingCultivatorCount++;
                }
                else
                {
                    preparedCultivatorCount++;
                }
            }

            RegisterRavenwoodWorkbenchRecipes();
            WorkbenchCustomRecipeConfig.MarkCoreRecipesRegistered();
            WorkbenchCustomRecipeConfig.TryRegisterWorkbenchRecipes();

            customContentRegistered = true;

            Debug.Log(
                "Tree prefab preparation complete. World: " + preparedWorldCount +
                ", Missing world: " + missingWorldCount +
                ", Cultivator: " + preparedCultivatorCount +
                ", Missing cultivator: " + missingCultivatorCount +
                ", Pickables: " + preparedPickableCount +
                ", Missing pickables: " + missingPickableCount +
                ", Seed registered: " + (registeredSeedPrefab != null) +
                ", Elixir registered: " + (registeredElixirPrefab != null) +
                ", Serum registered: " + (registeredSerumPrefab != null) +
                ", Green mushroom item registered: " + PickableMushroomManager.GreenMushroomItemRegistered +
                ", Purple mushroom item registered: " + PickableMushroomManager.PurpleMushroomItemRegistered +
                ", Ravenwood Alchemy Table recipes registered.");
        }

        public static GameObject GetPreparedTreePrefab(string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return null;
            }

            string worldPrefabName = RavenwoodPrefabUtility.GetWorldPrefabName(prefabName);
            RegisteredTreePrefabs.TryGetValue(worldPrefabName, out GameObject prefab);
            return prefab;
        }

        public static bool TryRestoreExistingTreeState(TreeRuntimeState runtime)
        {
            if (runtime == null || runtime.gameObject == null)
            {
                return false;
            }

            TreeConfigFile.BuildTreeDefinitions();

            string prefabName = RavenwoodPrefabUtility.GetWorldPrefabName(runtime.gameObject.name);
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

        private static void RegisterPickableCultivatorTree(GameObject prefab, TreeConfigFile.TreeDefinition tree, AssetBundle bundle)
        {
            if (prefab == null || tree == null)
            {
                return;
            }

            Sprite icon = PickableMushroomManager.ResolveCultivatorIcon(bundle, tree);
            PieceConfig pieceConfig = new PieceConfig();
            pieceConfig.Name = tree.DisplayName;
            pieceConfig.Description = tree.Description;
            pieceConfig.PieceTable = PieceTables.Cultivator;
            pieceConfig.Category = tree.Category;
            pieceConfig.Icon = icon;
            pieceConfig.Requirements = CloneRequirements(tree.Requirements);

            PieceManager.Instance.AddPiece(new CustomPiece(prefab, true, pieceConfig));
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

            ItemManager.Instance.AddRecipe(new CustomRecipe(recipeConfig));
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

            if (PickableMushroomManager.IsMushroomItemPrefabName(prefabName))
            {
                return PickableMushroomManager.IsMushroomItemMissing(prefabName);
            }

            return false;
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
            RavenwoodPrefabUtility.SetLayerRecursively(prefab, "item");
            RavenwoodPrefabUtility.EnsureSolidColliders(prefab);
            RavenwoodPrefabUtility.EnsureSeedRigidbody(prefab);
            RavenwoodPrefabUtility.RemoveRootPiece(prefab);
            RavenwoodPrefabUtility.RemoveItemDrops(prefab);
            RavenwoodPrefabUtility.RemovePickables(prefab);

            ZNetView znv = prefab.GetComponent<ZNetView>();
            if (znv == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            znv.m_persistent = true;
            znv.m_syncInitialScale = true;

            Sprite icon = RavenwoodPrefabUtility.LoadIconSprite(bundle, TreeRegistrar.RavenSeedPrefabName);

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
            RavenwoodPrefabUtility.SetLayerRecursively(prefab, "item");
            RavenwoodPrefabUtility.EnsureSolidColliders(prefab);
            RavenwoodPrefabUtility.EnsureSeedRigidbody(prefab);
            RavenwoodPrefabUtility.RemoveRootPiece(prefab);
            RavenwoodPrefabUtility.RemoveItemDrops(prefab);
            RavenwoodPrefabUtility.RemovePickables(prefab);

            ZNetView znv = prefab.GetComponent<ZNetView>();
            if (znv == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            znv.m_persistent = true;
            znv.m_syncInitialScale = true;

            Sprite icon = RavenwoodPrefabUtility.LoadIconSprite(bundle, TreeRegistrar.RavenElixirPrefabName);

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
            RavenwoodPrefabUtility.SetLayerRecursively(prefab, "item");
            RavenwoodPrefabUtility.EnsureSolidColliders(prefab);
            RavenwoodPrefabUtility.EnsureSeedRigidbody(prefab);
            RavenwoodPrefabUtility.RemoveRootPiece(prefab);
            RavenwoodPrefabUtility.RemoveItemDrops(prefab);
            RavenwoodPrefabUtility.RemovePickables(prefab);

            ZNetView znv = prefab.GetComponent<ZNetView>();
            if (znv == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            znv.m_persistent = true;
            znv.m_syncInitialScale = true;

            Sprite icon = RavenwoodPrefabUtility.LoadIconSprite(bundle, TreeRegistrar.RavenSerumPrefabName);

            ItemDrop itemDrop = prefab.AddComponent<ItemDrop>();
            itemDrop.m_autoPickup = true;
            itemDrop.m_autoDestroy = true;
            itemDrop.m_itemData = CreateSerumItemData(prefab, icon);
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

    }
}
