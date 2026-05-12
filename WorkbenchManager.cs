using System;
using System.Reflection;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Ravenwood.Biomes
{
    public static class WorkbenchManager
    {
        public const string WorkbenchPrefabName = "RWB_Alchemy_Table";
        public const string WorkbenchDisplayName = "Ravenwood Table";
        private const string WorkbenchDescription = "A Ravenwood alchemy table.";
        private const string WorkbenchCategory = "Crafting";
        private const string VanillaWorkbenchPrefabName = "piece_workbench";
        private const string BowlChildName = "bowl";
        private const float WorkbenchHealth = 400f;
        private const float DiscoverRange = 4f;
        private const float BuildRange = 20f;
        private const float UseDistance = 5f;
        private const float CraftSparkleFallbackHeight = 1.05f;
        private const float CraftSparkleBowlOffset = 0.25f;

        private static readonly string[] CraftDoneSparkleVfxPrefabNames =
        {
            "vfx_Potion_stamina_medium",
            "vfx_Potion_health_medium",
            "vfx_Pickable_Pick",
            "vfx_coin_pile_destroyed"
        };

        private static bool registered;
        private static bool copiedVanillaWorkbenchCraftEffects;
        private static GameObject registeredWorkbenchPrefab;

        public static void RegisterWorkbench(AssetBundle bundle)
        {
            if (registered || bundle == null)
            {
                return;
            }

            GameObject prefab = bundle.LoadAsset<GameObject>(WorkbenchPrefabName);
            if (prefab == null)
            {
                Debug.LogWarning("Workbench prefab missing from asset bundle: " + WorkbenchPrefabName);
                return;
            }

            PrepareWorkbenchPrefab(prefab);
            registeredWorkbenchPrefab = prefab;

            PieceConfig pieceConfig = new PieceConfig();
            pieceConfig.Name = WorkbenchDisplayName;
            pieceConfig.Description = WorkbenchDescription;
            pieceConfig.PieceTable = PieceTables.Hammer;
            pieceConfig.Category = WorkbenchCategory;
            pieceConfig.Icon = LoadIconSprite(bundle, WorkbenchPrefabName);
            pieceConfig.Requirements = CreateWorkbenchRequirements();

            PieceManager.Instance.AddPiece(new CustomPiece(prefab, true, pieceConfig));
            registered = true;
            TryCopyVanillaWorkbenchCraftEffects();
        }

        private static void PrepareWorkbenchPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            prefab.name = WorkbenchPrefabName;
            SetLayerRecursively(prefab, "piece");
            EnsureSolidColliders(prefab);
            EnsureNetworkComponents(prefab);
            EnsurePieceComponent(prefab);
            EnsureWearNTear(prefab);
            EnsureCraftingStation(prefab);
        }

        private static RequirementConfig[] CreateWorkbenchRequirements()
        {
            return new[]
            {
                new RequirementConfig("Wood", 20, 0, true),
                new RequirementConfig("Mushroom", 20, 0, true)
            };
        }

        private static void EnsureNetworkComponents(GameObject prefab)
        {
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
        }

        private static void EnsurePieceComponent(GameObject prefab)
        {
            Piece piece = prefab.GetComponent<Piece>();
            if (piece == null)
            {
                piece = prefab.AddComponent<Piece>();
            }

            piece.m_name = WorkbenchDisplayName;
            piece.m_description = WorkbenchDescription;
            piece.m_groundOnly = false;
            piece.m_canBeRemoved = true;
        }

        private static void EnsureWearNTear(GameObject prefab)
        {
            WearNTear wear = prefab.GetComponent<WearNTear>();
            if (wear == null)
            {
                wear = prefab.AddComponent<WearNTear>();
            }

            wear.m_health = WorkbenchHealth;
            wear.m_noRoofWear = true;
        }

        private static void EnsureCraftingStation(GameObject prefab)
        {
            CraftingStation station = prefab.GetComponent<CraftingStation>();
            if (station == null)
            {
                station = prefab.AddComponent<CraftingStation>();
            }

            SetFieldIfExists(station, "m_name", WorkbenchDisplayName);
            SetFieldIfExists(station, "m_discoverRange", DiscoverRange);
            SetFieldIfExists(station, "m_rangeBuild", BuildRange);
            SetFieldIfExists(station, "m_useDistance", UseDistance);
            SetFieldIfExists(station, "m_craftRequireRoof", false);
            SetFieldIfExists(station, "m_craftRequireFire", false);
            SetFieldIfExists(station, "m_showBasicRecipies", false);
            SetFieldIfExists(station, "m_showBasicRecipes", false);
            SetFieldIfExists(station, "m_haveRoof", true);
            SetFieldIfExists(station, "m_haveFire", true);
        }


        public static void TryCopyVanillaWorkbenchCraftEffects()
        {
            if (copiedVanillaWorkbenchCraftEffects)
            {
                return;
            }

            if (ZNetScene.instance == null)
            {
                return;
            }

            GameObject vanillaWorkbench = ZNetScene.instance.GetPrefab(VanillaWorkbenchPrefabName);
            if (vanillaWorkbench == null)
            {
                return;
            }

            CraftingStation vanillaStation = vanillaWorkbench.GetComponent<CraftingStation>();
            if (vanillaStation == null)
            {
                return;
            }

            bool copied = false;
            copied |= TryCopyCraftEffects(vanillaStation, registeredWorkbenchPrefab);

            GameObject sceneWorkbench = ZNetScene.instance.GetPrefab(WorkbenchPrefabName);
            if (sceneWorkbench != null && sceneWorkbench != registeredWorkbenchPrefab)
            {
                copied |= TryCopyCraftEffects(vanillaStation, sceneWorkbench);
            }

            if (copied)
            {
                copiedVanillaWorkbenchCraftEffects = true;
            }
        }

        private static bool TryCopyCraftEffects(CraftingStation vanillaStation, GameObject targetPrefab)
        {
            if (vanillaStation == null || targetPrefab == null)
            {
                return false;
            }

            CraftingStation targetStation = targetPrefab.GetComponent<CraftingStation>();
            if (targetStation == null)
            {
                return false;
            }

            bool copied = false;
            copied |= CopyFieldIfExists(vanillaStation, targetStation, "m_craftItemEffects");
            copied |= CopyFieldIfExists(vanillaStation, targetStation, "m_craftItemDoneEffects");
            copied |= TryAddCraftDoneSparkleEffect(targetStation, targetPrefab);
            return copied;
        }

        private static bool TryAddCraftDoneSparkleEffect(CraftingStation targetStation, GameObject targetPrefab)
        {
            if (targetStation == null || targetPrefab == null || ZNetScene.instance == null)
            {
                return false;
            }

            GameObject sparklePrefab = FindFirstAvailableEffectPrefab(CraftDoneSparkleVfxPrefabNames);
            if (sparklePrefab == null)
            {
                return false;
            }

            FieldInfo doneEffectsField = targetStation.GetType().GetField("m_craftItemDoneEffects", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (doneEffectsField == null || doneEffectsField.FieldType != typeof(EffectList))
            {
                return false;
            }

            EffectList sourceEffects = doneEffectsField.GetValue(targetStation) as EffectList;
            EffectList targetEffects = new EffectList();

            FieldInfo effectPrefabsField = typeof(EffectList).GetField("m_effectPrefabs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (effectPrefabsField == null)
            {
                return false;
            }

            Type effectDataType = effectPrefabsField.FieldType.GetElementType();
            if (effectDataType == null)
            {
                effectDataType = typeof(EffectList).GetNestedType("EffectData", BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (effectDataType == null)
            {
                return false;
            }

            Array existingEffects = sourceEffects != null ? effectPrefabsField.GetValue(sourceEffects) as Array : null;
            if (ContainsEffectPrefab(existingEffects, sparklePrefab))
            {
                return false;
            }

            int existingLength = existingEffects != null ? existingEffects.Length : 0;
            Array newEffects = Array.CreateInstance(effectDataType, existingLength + 1);

            for (int i = 0; i < existingLength; i++)
            {
                newEffects.SetValue(existingEffects.GetValue(i), i);
            }

            object sparkleEffect = Activator.CreateInstance(effectDataType);
            if (sparkleEffect == null)
            {
                return false;
            }

            SetFieldIfExists(sparkleEffect, "m_prefab", sparklePrefab);
            SetFieldIfExists(sparkleEffect, "m_enabled", true);
            SetFieldIfExists(sparkleEffect, "m_attach", false);
            SetFieldIfExists(sparkleEffect, "m_follow", false);
            SetFieldIfExists(sparkleEffect, "m_inheritParentRotation", true);
            SetFieldIfExists(sparkleEffect, "m_inheritParentScale", true);
            SetFieldIfExists(sparkleEffect, "m_randomRotation", true);
            SetFieldIfExists(sparkleEffect, "m_position", BuildCraftSparkleLocalPosition(targetPrefab));

            newEffects.SetValue(sparkleEffect, existingLength);
            effectPrefabsField.SetValue(targetEffects, newEffects);
            doneEffectsField.SetValue(targetStation, targetEffects);
            return true;
        }

        private static GameObject FindFirstAvailableEffectPrefab(string[] prefabNames)
        {
            if (prefabNames == null || ZNetScene.instance == null)
            {
                return null;
            }

            for (int i = 0; i < prefabNames.Length; i++)
            {
                string prefabName = prefabNames[i];
                if (string.IsNullOrWhiteSpace(prefabName))
                {
                    continue;
                }

                GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
                if (prefab != null)
                {
                    return prefab;
                }
            }

            return null;
        }

        private static bool ContainsEffectPrefab(Array effects, GameObject prefab)
        {
            if (effects == null || prefab == null)
            {
                return false;
            }

            for (int i = 0; i < effects.Length; i++)
            {
                object effect = effects.GetValue(i);
                if (effect == null)
                {
                    continue;
                }

                FieldInfo prefabField = effect.GetType().GetField("m_prefab", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prefabField == null)
                {
                    continue;
                }

                GameObject existingPrefab = prefabField.GetValue(effect) as GameObject;
                if (existingPrefab == prefab)
                {
                    return true;
                }
            }

            return false;
        }

        private static Vector3 BuildCraftSparkleLocalPosition(GameObject targetPrefab)
        {
            if (targetPrefab == null)
            {
                return Vector3.up * CraftSparkleFallbackHeight;
            }

            Transform bowl = FindChildRecursive(targetPrefab.transform, BowlChildName);
            if (bowl != null)
            {
                return targetPrefab.transform.InverseTransformPoint(bowl.position) + Vector3.up * CraftSparkleBowlOffset;
            }

            return Vector3.up * CraftSparkleFallbackHeight;
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            if (string.Equals(root.name, childName, StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static bool CopyFieldIfExists(object source, object target, string fieldName)
        {
            if (source == null || target == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return false;
            }

            FieldInfo sourceField = source.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo targetField = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (sourceField == null || targetField == null || sourceField.FieldType != targetField.FieldType)
            {
                return false;
            }

            try
            {
                targetField.SetValue(target, sourceField.GetValue(source));
                return true;
            }
            catch
            {
                return false;
            }
        }


        private static bool SetFieldIfExists(object instance, string fieldName, object value)
        {
            if (instance == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return false;
            }

            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                return false;
            }

            try
            {
                if (value == null || field.FieldType.IsInstanceOfType(value))
                {
                    field.SetValue(instance, value);
                    return true;
                }

                field.SetValue(instance, Convert.ChangeType(value, field.FieldType));
                return true;
            }
            catch
            {
                return false;
            }
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

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null)
                {
                    children[i].gameObject.layer = layer;
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

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            BoxCollider box = prefab.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = prefab.AddComponent<BoxCollider>();
            }

            box.center = prefab.transform.InverseTransformPoint(bounds.center);
            box.size = bounds.size;
            box.enabled = true;
            box.isTrigger = false;
        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        private static class ZNetScene_Awake_Patch
        {
            private static void Postfix()
            {
                TryCopyVanillaWorkbenchCraftEffects();
            }
        }

    }
}