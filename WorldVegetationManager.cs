using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Ravenwood.Biomes
{
    public static class WorldVegetationManager
    {
        public static GameObject RegisterWorldVegetation(AssetBundle bundle, TreeConfigFile.TreeDefinition tree)
        {
            if (bundle == null || tree == null || string.IsNullOrWhiteSpace(tree.PrefabName))
            {
                return null;
            }

            GameObject prefab = bundle.LoadAsset<GameObject>(tree.PrefabName);
            if (prefab == null)
            {
                Debug.LogWarning("[RavenwoodBiomes] World vegetation prefab missing from asset bundle: " + tree.PrefabName);
                return null;
            }

            PrepareWorldVegetationPrefab(prefab, tree);
            PrefabManager.Instance.AddPrefab(new CustomPrefab(prefab, true));
            return prefab;
        }

        private static void PrepareWorldVegetationPrefab(GameObject prefab, TreeConfigFile.TreeDefinition tree)
        {
            if (prefab == null || tree == null)
            {
                return;
            }

            prefab.name = tree.PrefabName;

            RavenwoodPrefabUtility.RemoveItemDrops(prefab);
            RavenwoodPrefabUtility.RemovePickables(prefab);

            // World/YAML vegetation must not carry build-piece logic.
            RavenwoodPrefabUtility.RemoveComponentsInChildren<Piece>(prefab);
            RavenwoodPrefabUtility.RemoveComponentsInChildren<WearNTear>(prefab);
            RavenwoodPrefabUtility.RemoveComponentsInChildren<ZSyncTransform>(prefab);

            RavenwoodPrefabUtility.EnsureSolidColliders(prefab);
            EnsureWorldDamageProfile(prefab, tree);

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

        private static void EnsureWorldDamageProfile(GameObject prefab, TreeConfigFile.TreeDefinition tree)
        {
            if (prefab == null || tree == null)
            {
                return;
            }

            Destructible destructible = prefab.GetComponent<Destructible>();
            TreeBase treeBase = prefab.GetComponent<TreeBase>();

            if (destructible == null && treeBase == null)
            {
                RavenwoodPrefabUtility.EnsureZNetView(prefab);
                destructible = prefab.AddComponent<Destructible>();
                destructible.m_health = ResolveDefaultHealth(tree.PrefabName);
            }
            else if (destructible != null)
            {
                RavenwoodPrefabUtility.EnsureZNetView(prefab);
                if (destructible.m_health <= 0f)
                {
                    destructible.m_health = ResolveDefaultHealth(tree.PrefabName);
                }
            }
            else if (treeBase != null)
            {
                RavenwoodPrefabUtility.EnsureZNetView(prefab);
                if (treeBase.m_health <= 0f)
                {
                    treeBase.m_health = ResolveDefaultHealth(tree.PrefabName);
                }
            }

            if (IsEternalVegetation(tree))
            {
                ApplyIndestructibleDamageProfile(prefab);
            }
        }

        private static bool IsEternalVegetation(TreeConfigFile.TreeDefinition tree)
        {
            if (tree != null && tree.Indestructible)
            {
                return true;
            }

            return RavenwoodPrefabUtility.IsEternalPrefabName(tree != null ? tree.PrefabName : string.Empty);
        }

        private static void ApplyIndestructibleDamageProfile(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

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
