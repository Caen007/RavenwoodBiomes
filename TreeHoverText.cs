using System;

namespace Ravenwood.Biomes
{
    public class TreeHoverText : UnityEngine.MonoBehaviour, Hoverable
    {
        private string hoverName;

        private void Awake()
        {
            EnsureHoverName();
        }

        public void Configure(string prefabName)
        {
            hoverName = BuildHoverName(prefabName);
        }

        public string GetHoverText()
        {
            EnsureHoverName();
            return string.IsNullOrWhiteSpace(hoverName) ? "Tree" : hoverName;
        }

        public string GetHoverName()
        {
            EnsureHoverName();
            return string.IsNullOrWhiteSpace(hoverName) ? "Tree" : hoverName;
        }

        private void EnsureHoverName()
        {
            if (!string.IsNullOrWhiteSpace(hoverName))
            {
                return;
            }

            string prefabName = gameObject != null ? gameObject.name : string.Empty;
            if (!string.IsNullOrWhiteSpace(prefabName) && prefabName.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
            {
                prefabName = prefabName.Substring(0, prefabName.Length - "(Clone)".Length).Trim();
            }

            hoverName = BuildHoverName(prefabName);
        }

        private static string BuildHoverName(string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return "Tree";
            }

            return TreeConfigFile.GetDisplayName(prefabName, PrettifyPrefabName(prefabName));
        }

        private static string PrettifyPrefabName(string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return "Tree";
            }

            string result = prefabName.Trim();
            if (result.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Substring(0, result.Length - "(Clone)".Length).Trim();
            }

            if (result.EndsWith("_Regrow", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Substring(0, result.Length - "_Regrow".Length).Trim();
            }

            result = result.Replace("RWB_", string.Empty);
            result = result.Replace('_', ' ');
            return string.IsNullOrWhiteSpace(result) ? "Tree" : result;
        }
    }
}
