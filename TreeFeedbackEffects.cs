using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ravenwood.Biomes
{
    public static class TreeFeedbackEffects
    {
        public const string PlaceSfxPrefabName = "sfx_build_cultivator";
        public const string PlaceSfxFallbackPrefabName = "sfx_build_hammer_wood";

        public const string PickSfxPrefabName = "sfx_pickable_pick";
        public const string PickSfxFallbackPrefabName = "sfx_item_pickup";
        public const string PickSfxFallbackPrefabNameAlt = "sfx_gui_craftitem";

        public const string HitSfxPrefabName = "sfx_tree_hit_abomination";
        public const string HitSfxFallbackPrefabName = "sfx_tree_hit";
        public const string HitSfxFallbackPrefabNameAlt = "sfx_wood_hit";

        public const string DestroyVfxPrefabName = "vfx_tree_fall_hit";
        public const string DestroyVfxFallbackPrefabName = "vfx_goblin_woodwall_destroyed";

        public const string DestroySfxPrefabName = "sfx_tree_fall";
        public const string DestroySfxFallbackPrefabName = "sfx_tree_fall_hit";
        public const string DestroySfxFallbackPrefabNameAlt = "sfx_tree_fall_abomination";
        public const string DestroySfxFallbackPrefabNameWood = "sfx_wood_destroyed";
        public const string DestroySfxFallbackPrefabNameWoodAlt = "sfx_wood_break";

        // Add or remove prefab names here to control which prefabs use placement sound.
        private static readonly HashSet<string> PlaceFeedbackPrefabs = new HashSet<string>(StringComparer.Ordinal)
        {
//Trees
            "RWB_Tree1",
            "RWB_Tree2",
            "RWB_Tree3",
            "RWB_Tree4",
            "RWB_Tree5",
            "RWB_Tree6",
            "RWB_Tree7",
            "RWB_Tree8",
            "RWB_Tree9",
            "RWB_Tree10",
            "RWB_Tree11",
            "RWB_ScaledTree1",
            "RWB_ScaledTree2",
            "RWB_ScaledTree3",
            "RWB_ScaledTree4",
            "RWB_ScaledTree6",
            "RWB_ScaledTree7",
            "RWB_ScaledTree10",
            "RWB_ScaledTree11",
            "RWB_ScaledTree11_purple",
            "RWB_ScaledTree11_blue",
            "RWB_Eternal_ScaledTree11",
            "RWB_Eternal_ScaledTree11_purple",

//Fungi
            "RWB_Fungi1",
            "RWB_Fungi2",
            "RWB_Fungi3",
            "RWB_Fungi4",
            "RWB_Fungi5",
            "RWB_Fungi6",
            "RWB_Fungi7",
            "RWB_ScaledFungi1",
            "RWB_ScaledFungi3",
            "RWB_ScaledFungi4",

//Mushrooms
            "RWB_Mushroom1",
            "RWB_Mushroom2",
            "RWB_Mushroom3",
            "RWB_Mushroom4",
            "RWB_Mushroom5",
            "RWB_Mushroom6",
            "RWB_Mushroom7",
            "RWB_Mushroom8",
            "RWB_Purple_Mushroom",
            "RWB_Pickable_Green_Mushroom",
            "RWB_Pickable_Purple_Mushroom",
            "RWB_ScaledMushroom1",
            "RWB_ScaledMushroom2",
            "RWB_ScaledMushroom3",
            "RWB_ScaledMushroom4",
            "RWB_ScaledMushroom7",

//Plants
            "RWB_Plant1",
            "RWB_Plant2",
            "RWB_Plant3",
            "RWB_Plant4",
            "RWB_Plant5",
            "RWB_Plant6",
            "RWB_Plant7",
            "RWB_Plant8",
            "RWB_Plant9",
            "RWB_Plant10",
            "RWB_Plant11",
            "RWB_Plant12",
            "RWB_Plant13",
            "RWB_Plant14",
            "RWB_Plant15",
            "RWB_Plant16",
            "RWB_Grass1",
            "RWB_ScaledPlant1",
            "RWB_ScaledPlant2",
            "RWB_ScaledPlant3",
            "RWB_ScaledPlant4",
            "RWB_ScaledPlant7",
            "RWB_ScaledPlant8",
            "RWB_ScaledPlant9",
            "RWB_ScaledPlant10",
            "RWB_ScaledPlant11",
            "RWB_ScaledPlant12",
            "RWB_ScaledPlant13",
            "RWB_ScaledPlant14",
            "RWB_ScaledPlant15"
        };

        // Add or remove prefab names here to control which prefabs use pick-up sound.
        private static readonly HashSet<string> PickFeedbackPrefabs = new HashSet<string>(StringComparer.Ordinal)
        {
            "Pickable_Mushroom",
            "Pickable_Mushroom_yellow",
            "Pickable_Mushroom_blue",
            "RWB_Pickable_Green_Mushroom",
            "RWB_Pickable_Purple_Mushroom"
        };

        // Add or remove prefab names here to control which prefabs use axe-hit sound.
        private static readonly HashSet<string> HitFeedbackPrefabs = new HashSet<string>(StringComparer.Ordinal)
        {
//Trees
            "RWB_Tree1",
            "RWB_Tree2",
            "RWB_Tree3",
            "RWB_Tree4",
            "RWB_Tree5",
            "RWB_Tree6",
            "RWB_Tree7",
            "RWB_Tree8",
            "RWB_Tree9",
            "RWB_Tree10",
            "RWB_Tree11",
            "RWB_ScaledTree1",
            "RWB_ScaledTree2",
            "RWB_ScaledTree3",
            "RWB_ScaledTree4",
            "RWB_ScaledTree6",
            "RWB_ScaledTree7",
            "RWB_ScaledTree10",
            "RWB_ScaledTree11",
            "RWB_ScaledTree11_purple",
            "RWB_ScaledTree11_blue",

//Fungi
            "RWB_Fungi1",
            "RWB_Fungi2",
            "RWB_Fungi3",
            "RWB_Fungi4",
            "RWB_Fungi5",
            "RWB_Fungi6",
            "RWB_Fungi7",
            "RWB_ScaledFungi1",
            "RWB_ScaledFungi3",
            "RWB_ScaledFungi4",

//Mushrooms
            "RWB_Mushroom1",
            "RWB_Mushroom2",
            "RWB_Mushroom3",
            "RWB_Mushroom4",
            "RWB_Mushroom5",
            "RWB_Mushroom6",
            "RWB_Mushroom7",
            "RWB_Mushroom8",
            "RWB_Purple_Mushroom",
            "RWB_ScaledMushroom1",
            "RWB_ScaledMushroom2",
            "RWB_ScaledMushroom3",
            "RWB_ScaledMushroom4",
            "RWB_ScaledMushroom7",

//Plants
            "RWB_Plant1",
            "RWB_Plant2",
            "RWB_Plant3",
            "RWB_Plant4",
            "RWB_Plant5",
            "RWB_Plant6",
            "RWB_Plant7",
            "RWB_Plant8",
            "RWB_Plant9",
            "RWB_Plant10",
            "RWB_Plant11",
            "RWB_Plant12",
            "RWB_Plant13",
            "RWB_Plant14",
            "RWB_Plant15",
            "RWB_Plant16",
            "RWB_Grass1",
            "RWB_ScaledPlant1",
            "RWB_ScaledPlant2",
            "RWB_ScaledPlant3",
            "RWB_ScaledPlant4",
            "RWB_ScaledPlant7",
            "RWB_ScaledPlant8",
            "RWB_ScaledPlant9",
            "RWB_ScaledPlant10",
            "RWB_ScaledPlant11",
            "RWB_ScaledPlant12",
            "RWB_ScaledPlant13",
            "RWB_ScaledPlant14",
            "RWB_ScaledPlant15"
        };

        // Add or remove prefab names here to control which prefabs use tree-fall destroy sound.
        private static readonly HashSet<string> TreeFallDestroySoundPrefabs = new HashSet<string>(StringComparer.Ordinal)
        {
//Trees
            "RWB_Tree1",
            "RWB_Tree2",
            "RWB_Tree3",
            "RWB_Tree4",
            "RWB_Tree5",
            "RWB_Tree6",
            "RWB_Tree7",
            "RWB_Tree8",
            "RWB_Tree9",
            "RWB_Tree10",
            "RWB_Tree11",
            "RWB_ScaledTree1",
            "RWB_ScaledTree2",
            "RWB_ScaledTree3",
            "RWB_ScaledTree4",
            "RWB_ScaledTree6",
            "RWB_ScaledTree7",
            "RWB_ScaledTree10",
            "RWB_ScaledTree11",
            "RWB_ScaledTree11_purple",
            "RWB_ScaledTree11_blue"
        };

        // Add or remove prefab names here to control which prefabs use short wood-break destroy sound.
        private static readonly HashSet<string> WoodBreakDestroySoundPrefabs = new HashSet<string>(StringComparer.Ordinal)
        {
//Fungi
            "RWB_Fungi1",
            "RWB_Fungi2",
            "RWB_Fungi3",
            "RWB_Fungi4",
            "RWB_Fungi5",
            "RWB_Fungi6",
            "RWB_Fungi7",
            "RWB_ScaledFungi1",
            "RWB_ScaledFungi3",
            "RWB_ScaledFungi4",

//Mushrooms
            "RWB_Mushroom1",
            "RWB_Mushroom2",
            "RWB_Mushroom3",
            "RWB_Mushroom4",
            "RWB_Mushroom5",
            "RWB_Mushroom6",
            "RWB_Mushroom7",
            "RWB_Mushroom8",
            "RWB_Purple_Mushroom",
            "RWB_ScaledMushroom1",
            "RWB_ScaledMushroom2",
            "RWB_ScaledMushroom3",
            "RWB_ScaledMushroom4",
            "RWB_ScaledMushroom7",

//Plants
            "RWB_Plant1",
            "RWB_Plant2",
            "RWB_Plant3",
            "RWB_Plant4",
            "RWB_Plant5",
            "RWB_Plant6",
            "RWB_Plant7",
            "RWB_Plant8",
            "RWB_Plant9",
            "RWB_Plant10",
            "RWB_Plant11",
            "RWB_Plant12",
            "RWB_Plant13",
            "RWB_Plant14",
            "RWB_Plant15",
            "RWB_Plant16",
            "RWB_Plant17",
            "RWB_Grass1",
            "RWB_ScaledPlant1",
            "RWB_ScaledPlant2",
            "RWB_ScaledPlant3",
            "RWB_ScaledPlant4",
            "RWB_ScaledPlant7",
            "RWB_ScaledPlant8",
            "RWB_ScaledPlant9",
            "RWB_ScaledPlant10",
            "RWB_ScaledPlant11",
            "RWB_ScaledPlant12",
            "RWB_ScaledPlant13",
            "RWB_ScaledPlant14",
            "RWB_ScaledPlant15"
        };

        // These keep the old large/medium vegetation health balance after the prefab rename.
        private static readonly HashSet<string> NormalVegetationHealthPrefabs = new HashSet<string>(StringComparer.Ordinal)
        {
//Trees
            "RWB_Tree1",
            "RWB_Tree2",
            "RWB_Tree3",
            "RWB_Tree4",
            "RWB_Tree5",
            "RWB_Tree6",
            "RWB_Tree7",
            "RWB_Tree8",
            "RWB_Tree9",
            "RWB_Tree10",
            "RWB_Tree11",
            "RWB_ScaledTree1",
            "RWB_ScaledTree2",
            "RWB_ScaledTree3",
            "RWB_ScaledTree4",
            "RWB_ScaledTree6",
            "RWB_ScaledTree7",
            "RWB_ScaledTree10",
            "RWB_ScaledTree11",
            "RWB_ScaledTree11_purple",
            "RWB_ScaledTree11_blue",

//Fungi
            "RWB_ScaledFungi1",
            "RWB_ScaledFungi3",
            "RWB_ScaledFungi4",

//Mushrooms
            "RWB_Mushroom1",
            "RWB_Mushroom2",
            "RWB_Mushroom3",
            "RWB_ScaledMushroom1",
            "RWB_ScaledMushroom2",
            "RWB_ScaledMushroom3",
            "RWB_ScaledMushroom4",
            "RWB_ScaledMushroom7",

//Plants
            "RWB_Plant1",
            "RWB_Plant2",
            "RWB_Plant3",
            "RWB_Plant4",
            "RWB_Plant5",
            "RWB_Plant6",
            "RWB_ScaledPlant1",
            "RWB_ScaledPlant2",
            "RWB_ScaledPlant3",
            "RWB_ScaledPlant4",
            "RWB_ScaledPlant7",
            "RWB_ScaledPlant8",
            "RWB_ScaledPlant9",
            "RWB_ScaledPlant10",
            "RWB_ScaledPlant11",
            "RWB_ScaledPlant12",
            "RWB_ScaledPlant13",
            "RWB_ScaledPlant14",
            "RWB_ScaledPlant15"
        };

        // These keep the old small-vegetation balance after the prefab rename.
        private static readonly HashSet<string> SmallVegetationPrefabs = new HashSet<string>(StringComparer.Ordinal)
        {
//Fungi
            "RWB_Fungi1",
            "RWB_Fungi2",
            "RWB_Fungi3",
            "RWB_Fungi4",
            "RWB_Fungi5",
            "RWB_Fungi6",
            "RWB_Fungi7",

//Mushrooms
            "RWB_Mushroom4",
            "RWB_Mushroom5",
            "RWB_Mushroom6",
            "RWB_Mushroom7",
            "RWB_Mushroom8",
            "RWB_Purple_Mushroom",

//Plants
            "RWB_Plant7",
            "RWB_Plant8",
            "RWB_Plant9",
            "RWB_Plant10",
            "RWB_Plant11",
            "RWB_Plant12",
            "RWB_Plant13",
            "RWB_Plant14",
            "RWB_Plant15",
            "RWB_Plant16",
            "RWB_Grass1"
        };

        public static bool IsTreeFallDestroySoundPrefab(string prefabName)
        {
            return TreeFallDestroySoundPrefabs.Contains(CleanPrefabName(prefabName));
        }

        public static bool IsNormalVegetationHealthPrefab(string prefabName)
        {
            return NormalVegetationHealthPrefabs.Contains(CleanPrefabName(prefabName));
        }

        public static bool IsSmallVegetationPrefab(string prefabName)
        {
            return SmallVegetationPrefabs.Contains(CleanPrefabName(prefabName));
        }

        public static void PlayPlace(string prefabName, Vector3 position, Quaternion rotation)
        {
            if (!PlaceFeedbackPrefabs.Contains(CleanPrefabName(prefabName)))
            {
                return;
            }

            SpawnFirstAvailable(position, rotation, PlaceSfxPrefabName, PlaceSfxFallbackPrefabName);
        }

        public static void PlayPick(string prefabName, Vector3 position, Quaternion rotation)
        {
            if (!PickFeedbackPrefabs.Contains(CleanPrefabName(prefabName)))
            {
                return;
            }

            SpawnFirstAvailable(position, rotation, PickSfxPrefabName, PickSfxFallbackPrefabName, PickSfxFallbackPrefabNameAlt);
        }

        public static void PlayHit(string prefabName, Vector3 position, Quaternion rotation)
        {
            if (!HitFeedbackPrefabs.Contains(CleanPrefabName(prefabName)))
            {
                return;
            }

            SpawnFirstAvailable(position, rotation, HitSfxPrefabName, HitSfxFallbackPrefabName, HitSfxFallbackPrefabNameAlt);
        }

        public static void PlayDestroy(string prefabName, Vector3 position, Quaternion rotation)
        {
            string cleanName = CleanPrefabName(prefabName);

            if (!TreeFallDestroySoundPrefabs.Contains(cleanName) && !WoodBreakDestroySoundPrefabs.Contains(cleanName))
            {
                return;
            }

            SpawnFirstAvailable(position, rotation, DestroyVfxPrefabName, DestroyVfxFallbackPrefabName);

            if (TreeFallDestroySoundPrefabs.Contains(cleanName))
            {
                SpawnFirstAvailable(
                    position,
                    rotation,
                    DestroySfxPrefabName,
                    DestroySfxFallbackPrefabName,
                    DestroySfxFallbackPrefabNameAlt,
                    DestroySfxFallbackPrefabNameWood,
                    DestroySfxFallbackPrefabNameWoodAlt);

                return;
            }

            SpawnFirstAvailable(position, rotation, DestroySfxFallbackPrefabNameWood, DestroySfxFallbackPrefabNameWoodAlt);
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

        private static void SpawnFirstAvailable(Vector3 position, Quaternion rotation, params string[] prefabNames)
        {
            if (prefabNames == null || prefabNames.Length == 0 || ZNetScene.instance == null)
            {
                return;
            }

            for (int i = 0; i < prefabNames.Length; i++)
            {
                string prefabName = prefabNames[i];
                if (string.IsNullOrWhiteSpace(prefabName))
                {
                    continue;
                }

                GameObject effectPrefab = ZNetScene.instance.GetPrefab(prefabName);
                if (effectPrefab == null)
                {
                    continue;
                }

                UnityEngine.Object.Instantiate(effectPrefab, position, rotation);
                return;
            }
        }
    }
}
