using System;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Ravenwood.Biomes
{
    public static class CultivatorVegetationManager
    {
        public static GameObject RegisterCultivatorVegetation(AssetBundle bundle, TreeConfigFile.TreeDefinition tree)
        {
            if (bundle == null || tree == null || string.IsNullOrWhiteSpace(tree.PrefabName))
            {
                return null;
            }

            string cultivatorPrefabName = RavenwoodPrefabUtility.GetCultivatorPrefabName(tree.PrefabName);
            GameObject prefab = bundle.LoadAsset<GameObject>(cultivatorPrefabName);
            if (prefab == null)
            {
                Debug.LogWarning("[RavenwoodBiomes] Cultivator vegetation prefab missing from asset bundle: " + cultivatorPrefabName + " for " + tree.PrefabName);
                return null;
            }

            PrepareCultivatorVegetationPrefab(prefab, tree, cultivatorPrefabName);
            RegisterCultivatorPiece(prefab, tree, bundle);
            return prefab;
        }

        private static void PrepareCultivatorVegetationPrefab(GameObject prefab, TreeConfigFile.TreeDefinition tree, string cultivatorPrefabName)
        {
            if (prefab == null || tree == null)
            {
                return;
            }

            prefab.name = cultivatorPrefabName;

            RavenwoodPrefabUtility.RemoveItemDrops(prefab);
            RavenwoodPrefabUtility.RemovePickables(prefab);
            RavenwoodPrefabUtility.EnsureSolidColliders(prefab);
            RavenwoodPrefabUtility.EnsureZNetView(prefab);

            if (IsEternalVegetation(tree, cultivatorPrefabName))
            {
                PrepareEternalCultivatorPrefab(prefab, tree);
            }
            else
            {
                PrepareStandardCultivatorPrefab(prefab, tree);
            }

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

        private static void RegisterCultivatorPiece(GameObject prefab, TreeConfigFile.TreeDefinition tree, AssetBundle bundle)
        {
            if (prefab == null || tree == null)
            {
                return;
            }

            PieceConfig pieceConfig = new PieceConfig();
            pieceConfig.Name = tree.DisplayName;
            pieceConfig.Description = tree.Description;
            pieceConfig.PieceTable = PieceTables.Cultivator;
            pieceConfig.Category = tree.Category;
            pieceConfig.Icon = RavenwoodPrefabUtility.LoadIconSprite(bundle, tree.PrefabName);
            pieceConfig.Requirements = CloneRequirements(tree.Requirements);

            PieceManager.Instance.AddPiece(new CustomPiece(prefab, true, pieceConfig));
        }

        private static void PrepareStandardCultivatorPrefab(GameObject prefab, TreeConfigFile.TreeDefinition tree)
        {
            RavenwoodPrefabUtility.EnsurePiece(prefab, tree);

            if (prefab.GetComponent<ZSyncTransform>() == null)
            {
                prefab.AddComponent<ZSyncTransform>();
            }

            WearNTear wear = prefab.GetComponent<WearNTear>();
            Destructible destructible = prefab.GetComponent<Destructible>();
            TreeBase treeBase = prefab.GetComponent<TreeBase>();

            if (wear == null && destructible == null && treeBase == null)
            {
                wear = prefab.AddComponent<WearNTear>();
                wear.m_health = ResolveDefaultHealth(tree.PrefabName);
            }

            if (wear != null)
            {
                if (wear.m_health <= 0f)
                {
                    wear.m_health = ResolveDefaultHealth(tree.PrefabName);
                }

                wear.m_noRoofWear = true;
            }

            if (tree.Indestructible)
            {
                ApplyIndestructibleDamageProfile(prefab);
            }
        }

        private static void PrepareEternalCultivatorPrefab(GameObject prefab, TreeConfigFile.TreeDefinition tree)
        {
            RavenwoodPrefabUtility.RemoveComponentsInChildren<Destructible>(prefab);
            RavenwoodPrefabUtility.RemoveComponentsInChildren<TreeLog>(prefab);
            RavenwoodPrefabUtility.RemoveComponentsInChildren<Plant>(prefab);
            RavenwoodPrefabUtility.RemoveComponentsInChildren<WearNTear>(prefab);
            RavenwoodPrefabUtility.RemoveComponentsInChildren<Rigidbody>(prefab);
            RavenwoodPrefabUtility.RemoveComponentsInChildren<TreeBase>(prefab);
            RavenwoodPrefabUtility.RemoveComponentsInChildren<StaticPhysics>(prefab);
            RavenwoodPrefabUtility.RemoveComponentsInChildren<ZSyncTransform>(prefab);

            RavenwoodPrefabUtility.SetLayerRecursively(prefab, "piece");
            ApplyEternalSupportColliderLayer(prefab);
            RavenwoodPrefabUtility.EnsurePiece(prefab, tree);
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

                RavenwoodPrefabUtility.SetLayerRecursively(child.gameObject, "static_solid");
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

            TreeBase treeBase = prefab.GetComponent<TreeBase>();
            if (treeBase != null)
            {
                treeBase.m_health = 1000000f;
                treeBase.m_minToolTier = 1073741823;
            }
        }

        private static bool IsEternalVegetation(TreeConfigFile.TreeDefinition tree, string cultivatorPrefabName)
        {
            if (tree != null && tree.Indestructible)
            {
                return true;
            }

            return RavenwoodPrefabUtility.IsEternalPrefabName(tree != null ? tree.PrefabName : string.Empty) ||
                   RavenwoodPrefabUtility.IsEternalPrefabName(cultivatorPrefabName);
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
    }
}
