using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ravenwood.Biomes
{
    public static class RavenwoodPrefabUtility
    {
        public const string WorldPrefix = "RWB_";
        public const string CultivatorPrefix = "RWBC_";

        public static string GetCultivatorPrefabName(string worldPrefabName)
        {
            string clean = CleanPrefabName(worldPrefabName);
            if (string.IsNullOrWhiteSpace(clean))
            {
                return string.Empty;
            }

            if (clean.StartsWith(CultivatorPrefix, StringComparison.Ordinal))
            {
                return clean;
            }

            if (clean.StartsWith(WorldPrefix, StringComparison.Ordinal))
            {
                return CultivatorPrefix + clean.Substring(WorldPrefix.Length);
            }

            return CultivatorPrefix + clean;
        }

        public static string GetWorldPrefabName(string prefabName)
        {
            string clean = CleanPrefabName(prefabName);
            if (string.IsNullOrWhiteSpace(clean))
            {
                return string.Empty;
            }

            if (clean.StartsWith(CultivatorPrefix, StringComparison.Ordinal))
            {
                return WorldPrefix + clean.Substring(CultivatorPrefix.Length);
            }

            return clean;
        }

        public static bool IsCultivatorPrefabName(string prefabName)
        {
            string clean = CleanPrefabName(prefabName);
            return clean.StartsWith(CultivatorPrefix, StringComparison.Ordinal);
        }

        public static bool IsEternalPrefabName(string prefabName)
        {
            string worldPrefabName = GetWorldPrefabName(prefabName);
            return worldPrefabName.StartsWith("RWB_Eternal_", StringComparison.Ordinal);
        }

        public static string CleanPrefabName(string prefabName)
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

        public static Sprite LoadIconSprite(AssetBundle bundle, string assetName)
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

        public static void SetLayerRecursively(GameObject root, string layerName)
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

        public static void RemoveComponentsInChildren<T>(GameObject root) where T : Component
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

        public static void RemoveItemDrops(GameObject root)
        {
            RemoveComponentsInChildren<ItemDrop>(root);
        }

        public static void RemovePickables(GameObject root)
        {
            RemoveComponentsInChildren<Pickable>(root);
        }

        public static void RemoveRootPiece(GameObject prefab)
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

        public static ZNetView EnsureZNetView(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            ZNetView znv = prefab.GetComponent<ZNetView>();
            if (znv == null)
            {
                znv = prefab.AddComponent<ZNetView>();
            }

            znv.m_persistent = true;
            znv.m_syncInitialScale = true;
            return znv;
        }

        public static Piece EnsurePiece(GameObject prefab, TreeConfigFile.TreeDefinition tree)
        {
            if (prefab == null || tree == null)
            {
                return null;
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
            return piece;
        }

        public static void EnsureSeedRigidbody(GameObject prefab)
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

        public static void EnsureRootItemCollider(GameObject prefab)
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

        public static void EnsureSolidColliders(GameObject prefab)
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
    }
}
