using System;
using System.Reflection;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ravenwood.Biomes
{
    public static class PickableMushroomManager
    {
        private static GameObject registeredGreenMushroomItemPrefab;
        private static GameObject registeredPurpleMushroomItemPrefab;
        private static GameObject registeredGreenMushroomPickablePrefab;
        private static GameObject registeredPurpleMushroomPickablePrefab;

        public static bool GreenMushroomItemRegistered
        {
            get { return registeredGreenMushroomItemPrefab != null; }
        }

        public static bool PurpleMushroomItemRegistered
        {
            get { return registeredPurpleMushroomItemPrefab != null; }
        }

        public static bool IsMushroomItemPrefabName(string prefabName)
        {
            return string.Equals(prefabName, TreeRegistrar.GreenMushroomItemPrefabName, StringComparison.Ordinal) ||
                   string.Equals(prefabName, TreeRegistrar.PurpleMushroomItemPrefabName, StringComparison.Ordinal);
        }

        public static bool IsMushroomItemMissing(string prefabName)
        {
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


        public static void RegisterItemPrefabs(AssetBundle bundle)
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



        public static void RegisterPickablePrefabs(AssetBundle bundle)
        {
            registeredGreenMushroomPickablePrefab = RegisterPickableMushroomPrefab(
                bundle,
                TreeRegistrar.GreenMushroomPrefabName,
                TreeRegistrar.GreenMushroomPickableDisplayName,
                registeredGreenMushroomItemPrefab,
                registeredGreenMushroomPickablePrefab);

            registeredPurpleMushroomPickablePrefab = RegisterPickableMushroomPrefab(
                bundle,
                TreeRegistrar.PurpleMushroomPrefabName,
                TreeRegistrar.PurpleMushroomPickableDisplayName,
                registeredPurpleMushroomItemPrefab,
                registeredPurpleMushroomPickablePrefab);
        }



        public static bool IsCultivatorMushroomPrefab(string prefabName)
        {
            return string.Equals(prefabName, TreeRegistrar.GreenMushroomPrefabName, StringComparison.Ordinal) ||
                   string.Equals(prefabName, TreeRegistrar.PurpleMushroomPrefabName, StringComparison.Ordinal);
        }



        public static Sprite ResolveCultivatorIcon(AssetBundle bundle, TreeConfigFile.TreeDefinition tree)
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



        public static void PrepareCultivatorMushroomPrefab(GameObject prefab, TreeConfigFile.TreeDefinition tree)
        {
            if (prefab == null || tree == null)
            {
                return;
            }

            if (string.Equals(tree.PrefabName, TreeRegistrar.GreenMushroomPrefabName, StringComparison.Ordinal))
            {
                PreparePickableMushroomPrefab(prefab, tree.PrefabName, TreeRegistrar.GreenMushroomPickableDisplayName, registeredGreenMushroomItemPrefab);
            }
            else if (string.Equals(tree.PrefabName, TreeRegistrar.PurpleMushroomPrefabName, StringComparison.Ordinal))
            {
                PreparePickableMushroomPrefab(prefab, tree.PrefabName, TreeRegistrar.PurpleMushroomPickableDisplayName, registeredPurpleMushroomItemPrefab);
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
            piece.m_canBeRemoved = true;
        }



        private static GameObject RegisterPickableMushroomPrefab(
            AssetBundle bundle,
            string pickablePrefabName,
            string pickableDisplayName,
            GameObject itemPrefab,
            GameObject alreadyRegistered)
        {
            if (alreadyRegistered != null)
            {
                return alreadyRegistered;
            }

            if (bundle == null || string.IsNullOrWhiteSpace(pickablePrefabName))
            {
                return null;
            }

            GameObject pickablePrefab = bundle.LoadAsset<GameObject>(pickablePrefabName);
            if (pickablePrefab == null)
            {
                Debug.LogWarning("Pickable mushroom prefab missing from asset bundle: " + pickablePrefabName);
                return null;
            }

            if (itemPrefab == null)
            {
                Debug.LogWarning("Pickable mushroom skipped because item prefab is missing: " + pickablePrefabName);
                return null;
            }

            PreparePickableMushroomPrefab(pickablePrefab, pickablePrefabName, pickableDisplayName, itemPrefab);
            return pickablePrefab;
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



        private static string GetCultivatorMushroomItemPrefabName(string prefabName)
        {
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



        private static void PreparePickableMushroomPrefab(GameObject prefab, string prefabName, string displayName, GameObject itemPrefab)
        {
            if (prefab == null || itemPrefab == null)
            {
                return;
            }

            prefab.name = prefabName;
            RemoveItemDrops(prefab);
            RemoveComponentsInChildren<Piece>(prefab);
            RemoveComponentsInChildren<Rigidbody>(prefab);
            RemoveComponentsInChildren<TreeRuntimeState>(prefab);
            RemoveComponentsInChildren<TreeHoverText>(prefab);
            RemoveComponentsInChildren<WearNTear>(prefab);
            RemoveComponentsInChildren<Destructible>(prefab);

            ZNetView znv = prefab.GetComponent<ZNetView>();
            if (znv == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            znv.m_persistent = true;
            znv.m_syncInitialScale = true;

            Pickable pickable = prefab.GetComponent<Pickable>();
            Pickable[] pickables = prefab.GetComponentsInChildren<Pickable>(true);
            for (int i = 0; i < pickables.Length; i++)
            {
                Pickable current = pickables[i];
                if (current != null && current != pickable)
                {
                    Object.DestroyImmediate(current, true);
                }
            }

            if (pickable == null)
            {
                pickable = prefab.AddComponent<Pickable>();
            }

            pickable.m_overrideName = displayName;
            pickable.m_itemPrefab = itemPrefab;
            pickable.m_amount = 1;
            pickable.m_minAmountScaled = 1;

            ApplyPickableMushroomVisualState(prefab, pickable);
            ApplyVanillaMushroomRespawnProfile(pickable);
        }



        private static void ApplyPickableMushroomVisualState(GameObject prefab, Pickable pickable)
        {
            if (prefab == null || pickable == null)
            {
                return;
            }

            Transform visual = prefab.transform.Find("visual");
            if (visual != null)
            {
                pickable.m_hideWhenPicked = visual.gameObject;
            }
            else if (pickable.m_hideWhenPicked == null)
            {
                Debug.LogWarning("[RavenwoodBiomes] Pickable mushroom is missing child 'visual': " + prefab.name);
            }

            Transform bulb = prefab.transform.Find("pickable_bulb");
            if (bulb == null)
            {
                bulb = prefab.transform.Find("picked_bulb");
            }

            if (bulb == null)
            {
                Debug.LogWarning("[RavenwoodBiomes] Pickable mushroom is missing child 'pickable_bulb': " + prefab.name);
                return;
            }

            if (visual != null && bulb.IsChildOf(visual))
            {
                Debug.LogWarning("[RavenwoodBiomes] pickable_bulb must be a sibling of 'visual', not inside it: " + prefab.name);
            }

            bulb.gameObject.SetActive(true);
        }



        private static void ApplyVanillaMushroomRespawnProfile(Pickable pickable)
        {
            if (pickable == null)
            {
                return;
            }

            const float vanillaMushroomRespawnMinutes = 240f;
            SetPickableFloatField(pickable, "m_respawnTimeMinutes", vanillaMushroomRespawnMinutes);
            SetPickableFloatField(pickable, "m_respawnTimeInitMin", 0f);
            SetPickableFloatField(pickable, "m_respawnTimeInitMax", 0f);
            SetPickableBoolField(pickable, "m_defaultPicked", false);
            SetPickableBoolField(pickable, "m_defaultEnabled", true);
            SetPickableBoolField(pickable, "m_harvestable", true);
        }



        private static void SetPickableFloatField(Pickable pickable, string fieldName, float value)
        {
            if (pickable == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }

            FieldInfo field = typeof(Pickable).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(float))
            {
                return;
            }

            field.SetValue(pickable, value);
        }



        private static void SetPickableBoolField(Pickable pickable, string fieldName, bool value)
        {
            if (pickable == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }

            FieldInfo field = typeof(Pickable).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(bool))
            {
                return;
            }

            field.SetValue(pickable, value);
        }



        private static void PrepareMushroomItemPrefab(GameObject prefab, string prefabName, string displayName, string description, AssetBundle bundle)
        {
            if (prefab == null)
            {
                return;
            }

            ItemDrop existingItemDrop = prefab.GetComponent<ItemDrop>();
            ItemDrop.ItemData.SharedData existingShared = existingItemDrop != null && existingItemDrop.m_itemData != null
                ? existingItemDrop.m_itemData.m_shared
                : null;

            prefab.name = prefabName;
            SetLayerRecursively(prefab, "item");
            EnsureSolidColliders(prefab);
            EnsureRootItemCollider(prefab);
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

            Sprite icon = LoadIconSprite(bundle, prefabName);

            ItemDrop itemDrop = prefab.AddComponent<ItemDrop>();
            itemDrop.m_autoPickup = true;
            itemDrop.m_autoDestroy = true;
            itemDrop.m_itemData = CreateMushroomItemData(prefab, prefabName, displayName, description, icon, existingShared);
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
            ResolveMushroomFoodStats(prefabName, out float food, out float stamina, out float eitr);
            shared.m_food = food;
            shared.m_foodStamina = stamina;
            shared.m_foodEitr = eitr;
            shared.m_foodBurnTime = shared.m_foodBurnTime > 0f ? shared.m_foodBurnTime : 900f;
            shared.m_foodRegen = shared.m_foodRegen > 0f ? shared.m_foodRegen : 1f;
            shared.m_attackForce = Mathf.Max(0f, shared.m_attackForce);
            shared.m_timedBlockBonus = shared.m_timedBlockBonus > 0f ? shared.m_timedBlockBonus : 1f;
            shared.m_skillType = Skills.SkillType.None;
            shared.m_icons = icon != null ? new[] { icon } : Array.Empty<Sprite>();

            itemData.m_shared = shared;
            return itemData;
        }



        private static void ResolveMushroomFoodStats(string prefabName, out float food, out float stamina, out float eitr)
        {
            if (string.Equals(prefabName, TreeRegistrar.GreenMushroomItemPrefabName, StringComparison.Ordinal))
            {
                food = 30f;
                stamina = 10f;
                eitr = 0f;
                return;
            }

            if (string.Equals(prefabName, TreeRegistrar.PurpleMushroomItemPrefabName, StringComparison.Ordinal))
            {
                food = 22f;
                stamina = 22f;
                eitr = 22f;
                return;
            }

            food = 15f;
            stamina = 15f;
            eitr = 0f;
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



        private static void EnsureRootItemCollider(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            BoxCollider box = prefab.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = prefab.AddComponent<BoxCollider>();
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                box.center = prefab.transform.InverseTransformPoint(bounds.center);
                box.size = bounds.size;
            }
            else if (box.size == Vector3.zero)
            {
                box.size = new Vector3(0.4f, 0.4f, 0.4f);
            }

            box.enabled = true;
            box.isTrigger = false;
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


    }
}
