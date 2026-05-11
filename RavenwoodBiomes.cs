using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Ravenwood.Biomes
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public class RavenwoodBiomes : BaseUnityPlugin
    {
        public const string PluginGUID = "Ravenwood.Biomes";
        public const string PluginName = "Ravenwood Biomes";
        public const string PluginVersion = "1.0.0";
        private const string PreferredCategoryName = "Raven Biomes";
        private const string ScaledCategoryName = "Raven Scaled";
        private const string EternalCategoryName = "Raven Eternal";

        private AssetBundle treesBundle;

        public static string GetPreferredCategoryName()
        {
            return PreferredCategoryName;
        }

        public static string GetScaledCategoryName()
        {
            return ScaledCategoryName;
        }

        public static string GetEternalCategoryName()
        {
            return EternalCategoryName;
        }

        private void Awake()
        {
            new Harmony("ravenwood.biomes").PatchAll();

            const string resourcePath = "RavenwoodBiomes.Ravenwoodtrees";
            treesBundle = EmbeddedAssetBundleLoader.LoadBundle(resourcePath);

            if (treesBundle == null)
            {
                Logger.LogError("Failed to load embedded AssetBundle.");
                return;
            }

            TreeManager.Initialize(this, treesBundle, Config);
            WorkbenchManager.RegisterWorkbench(treesBundle);
            TreeManager.RegisterTreePrefabs(treesBundle);
        }
    }

    public static class EmbeddedAssetBundleLoader
    {
        public static AssetBundle LoadBundle(string resourcePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    Debug.LogError("AssetBundle resource not found: " + resourcePath);
                    return null;
                }

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return AssetBundle.LoadFromMemory(buffer);
            }
        }
    }
}
