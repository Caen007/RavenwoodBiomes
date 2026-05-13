using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Ravenwood.Biomes
{
    public class TreeRegrowTimer : MonoBehaviour, Hoverable, Interactable
    {
        private const string TreeRespawnerKey = "treeRespawner";
        private const string PickedKey = "picked";
        private const string StartTicksZdoKey = "rvb_regrow_startticks";
        private const string DurationMinutesZdoKey = "rvb_regrow_durationminutes";
        private const string CompletedZdoKey = "rvb_regrow_completed";
        private const string PlantTimeZdoKey = "plantTime";
        private const float DefaultRegrowMinutes = 1500f;
        private const string DestroyVfxPrefabName = "vfx_goblin_woodwall_destroyed";
        private const string DestroySfxPrefabName = "sfx_wood_destroyed";

        private ZNetView znv;
        private Renderer[] cachedRenderers;
        private Collider[] cachedColliders;
        private float nextStateCheckTime;
        private bool removalInProgress;
        private string pendingRespawnPrefabName;
        private float pendingRespawnMinutes = -1f;

        private void Awake()
        {
            znv = GetComponent<ZNetView>();
            EnsureCarrierPieceRules();
            CacheComponents();
            TryApplyPendingCarrierData();
            DisableVanillaPlantGrowth();
            EnsureVisible();
            EnsureInitialized();
            TryHandleRespawnDisabled();
        }

        private void OnEnable()
        {
            EnsureCarrierPieceRules();
            CacheComponents();
            TryApplyPendingCarrierData();
            DisableVanillaPlantGrowth();
            EnsureVisible();
            EnsureInitialized();
            TryHandleRespawnDisabled();
        }

        private void LateUpdate()
        {
            TryApplyPendingCarrierData();
            EnsureVisible();

            if (TryHandleRespawnDisabled())
            {
                return;
            }

            if (Time.time < nextStateCheckTime)
            {
                return;
            }

            nextStateCheckTime = Time.time + 1f;
            UpdateRespawnState();
        }

        public void ConfigureCarrier(string respawnPrefabName, float respawnMinutes)
        {
            pendingRespawnPrefabName = respawnPrefabName ?? string.Empty;
            pendingRespawnMinutes = Mathf.Max(0f, respawnMinutes);
            EnsureCarrierPieceRules();
            TryApplyPendingCarrierData();
            EnsureVisible();
        }

        private void EnsureCarrierPieceRules()
        {
            Piece piece = GetComponent<Piece>();
            if (piece == null)
            {
                piece = gameObject.AddComponent<Piece>();
            }

            piece.m_groundOnly = false;
            piece.m_canBeRemoved = true;
        }

        public string GetHoverName()
        {
            return BuildHoverName();
        }

        public string GetHoverText()
        {
            return BuildHoverText();
        }

        public bool Interact(Humanoid character, bool hold, bool alt)
        {
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public void RequestSilentRemove()
        {
            if (removalInProgress)
            {
                return;
            }

            StartCoroutine(SilentRemoveRoutine());
        }

        public string BuildHoverName()
        {
            string prefabName = ResolveRespawnPrefabName();
            return TreeConfigFile.GetRegrowCarrierDisplayName(prefabName);
        }

        public string BuildHoverText()
        {
            string name = BuildHoverName();
            if (!IsRespawnEnabledForCarrier())
            {
                return name + "\n<color=orange>Respawn disabled</color>";
            }

            double remainingSeconds = GetRemainingSeconds();

            if (remainingSeconds > 0d)
            {
                return name + "\n<color=orange>Grows in " + FormatRemaining(TimeSpan.FromSeconds(remainingSeconds)) + "</color>";
            }


            return name + "\n<color=orange>Growing now...</color>";
        }

        public bool IsRegrowCarrier()
        {
            ZDO zdo = GetCarrierZdo();
            return zdo != null && zdo.GetInt(TreeRespawnerKey, 0) == 1;
        }

        private IEnumerator SilentRemoveRoutine()
        {
            removalInProgress = true;

            float timeout = 1f;
            while (!TryClaimOwnership() && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            ZDO zdo = GetCarrierZdo();
            if (zdo != null && CanWriteZdo())
            {
                zdo.Set(CompletedZdoKey, 1);
                zdo.Set(PickedKey, 1);
            }

            PlayDestroyEffects();
            DisableInteraction();
            yield return new WaitForSeconds(0.05f);
            DestroySelf();
        }

        private void CacheComponents()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            cachedColliders = GetComponentsInChildren<Collider>(true);
        }

        private void TryApplyPendingCarrierData()
        {
            if (string.IsNullOrWhiteSpace(pendingRespawnPrefabName))
            {
                return;
            }

            znv = znv != null ? znv : GetComponent<ZNetView>();
            if (znv == null || !znv.IsValid())
            {
                return;
            }

            if (!CanWriteZdo())
            {
                TryClaimOwnership();
                if (!CanWriteZdo())
                {
                    return;
                }
            }

            ZDO zdo = znv.GetZDO();
            if (zdo == null)
            {
                return;
            }

            long startTicks = GetNetworkTime().Ticks;
            zdo.Set(TreeRespawnerKey, 1);
            zdo.Set(PickedKey, 1);
            zdo.Set(CompletedZdoKey, 0);
            zdo.Set("respawnprefab", pendingRespawnPrefabName);
            zdo.Set("rvb_respawnprefab", pendingRespawnPrefabName);
            zdo.Set("rvb_regrowminutes", Mathf.Max(0f, pendingRespawnMinutes > 0f ? pendingRespawnMinutes : DefaultRegrowMinutes));
            zdo.Set(DurationMinutesZdoKey, Mathf.Max(0f, pendingRespawnMinutes > 0f ? pendingRespawnMinutes : DefaultRegrowMinutes));

            if (zdo.GetLong(StartTicksZdoKey, 0L) <= 0L)
            {
                zdo.Set(StartTicksZdoKey, startTicks);
            }

            if (zdo.GetLong(PlantTimeZdoKey, 0L) <= 0L)
            {
                zdo.Set(PlantTimeZdoKey, startTicks);
            }

            pendingRespawnPrefabName = null;
            pendingRespawnMinutes = -1f;
        }

        private void DisableVanillaPlantGrowth()
        {
            if (!IsRegrowCarrier())
            {
                return;
            }

            Plant plant = GetComponent<Plant>();
            if (plant != null)
            {
                plant.enabled = false;
            }
        }

        private void EnsureInitialized()
        {
            ZDO zdo = GetCarrierZdo();
            if (zdo == null || !CanWriteZdo())
            {
                return;
            }

            long startTicks = zdo.GetLong(StartTicksZdoKey, 0L);
            if (startTicks <= 0L)
            {
                startTicks = GetNetworkTime().Ticks;
                zdo.Set(StartTicksZdoKey, startTicks);
            }

            if (zdo.GetLong(PlantTimeZdoKey, 0L) <= 0L)
            {
                zdo.Set(PlantTimeZdoKey, startTicks);
            }

            float configuredMinutes = ResolveConfiguredRespawnMinutes(zdo);
            float existingMinutes = zdo.GetFloat(DurationMinutesZdoKey, -1f);
            if (Mathf.Abs(existingMinutes - configuredMinutes) > 0.01f)
            {
                zdo.Set(DurationMinutesZdoKey, configuredMinutes);
            }

            if (zdo.GetInt(CompletedZdoKey, 0) == 0 && GetRemainingSeconds(zdo) > 0d && zdo.GetInt(PickedKey, 1) != 1)
            {
                zdo.Set(PickedKey, 1);
            }
        }

        private void UpdateRespawnState()
        {
            ZDO zdo = GetCarrierZdo();
            if (zdo == null || zdo.GetInt(CompletedZdoKey, 0) == 1)
            {
                return;
            }

            double remainingSeconds = GetRemainingSeconds(zdo);
            if (remainingSeconds > 0d)
            {
                if (CanWriteZdo() && zdo.GetInt(PickedKey, 1) != 1)
                {
                    zdo.Set(PickedKey, 1);
                }

                return;
            }


            if (!CanWriteZdo())
            {
                return;
            }

            zdo.Set(CompletedZdoKey, 1);
            zdo.Set(PickedKey, 0);
        }

        private void EnsureVisible()
        {
            if (!IsRegrowCarrier() && string.IsNullOrWhiteSpace(pendingRespawnPrefabName))
            {
                return;
            }

            if (cachedRenderers == null || cachedRenderers.Length == 0)
            {
                cachedRenderers = GetComponentsInChildren<Renderer>(true);
            }

            if (cachedColliders == null || cachedColliders.Length == 0)
            {
                cachedColliders = GetComponentsInChildren<Collider>(true);
            }

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                Renderer renderer = cachedRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!renderer.gameObject.activeSelf)
                {
                    renderer.gameObject.SetActive(true);
                }

                renderer.enabled = true;
            }

            for (int i = 0; i < cachedColliders.Length; i++)
            {
                Collider collider = cachedColliders[i];
                if (collider == null)
                {
                    continue;
                }

                if (!collider.gameObject.activeSelf)
                {
                    collider.gameObject.SetActive(true);
                }

                collider.enabled = true;
            }

            ItemDrop itemDrop = GetComponent<ItemDrop>();
            if (itemDrop != null)
            {
                itemDrop.enabled = false;
            }
        }

        private ZDO GetCarrierZdo()
        {
            znv = znv != null ? znv : GetComponent<ZNetView>();
            if (znv == null || !znv.IsValid())
            {
                return null;
            }

            ZDO zdo = znv.GetZDO();
            if (zdo == null || zdo.GetInt(TreeRespawnerKey, 0) != 1)
            {
                return null;
            }

            return zdo;
        }

        private bool CanWriteZdo()
        {
            znv = znv != null ? znv : GetComponent<ZNetView>();
            if (znv == null || !znv.IsValid())
            {
                return false;
            }

            if (ZNet.instance != null && ZNet.instance.IsServer())
            {
                return true;
            }

            return znv.IsOwner();
        }

        private bool TryClaimOwnership()
        {
            znv = znv != null ? znv : GetComponent<ZNetView>();
            if (znv == null || !znv.IsValid())
            {
                return true;
            }

            if (ZNet.instance != null && ZNet.instance.IsServer())
            {
                return true;
            }

            if (!znv.IsOwner())
            {
                znv.ClaimOwnership();
            }

            return znv.IsOwner();
        }

        private double GetRemainingSeconds()
        {
            ZDO zdo = GetCarrierZdo();
            if (zdo == null)
            {
                return 0d;
            }

            return GetRemainingSeconds(zdo);
        }

        private double GetRemainingSeconds(ZDO zdo)
        {
            if (zdo == null)
            {
                return 0d;
            }

            long startTicks = zdo.GetLong(StartTicksZdoKey, 0L);
            float durationMinutes = zdo.GetFloat(DurationMinutesZdoKey, 0f);

            if (startTicks <= 0L || durationMinutes <= 0f)
            {
                return 0d;
            }

            DateTime startTime = new DateTime(startTicks, DateTimeKind.Utc);
            DateTime now = GetNetworkTime();

            double elapsedSeconds = (now - startTime).TotalSeconds;
            double totalSeconds = durationMinutes * 60d;

            return Math.Max(0d, totalSeconds - elapsedSeconds);
        }

        private static DateTime GetNetworkTime()
        {
            if (ZNet.instance != null)
            {
                return ZNet.instance.GetTime();
            }

            return DateTime.UtcNow;
        }

        private bool TryHandleRespawnDisabled()
        {
            if (!IsRegrowCarrier() || removalInProgress)
            {
                return false;
            }

            if (IsRespawnEnabledForCarrier())
            {
                return false;
            }

            if (CanWriteZdo())
            {
                RequestSilentRemove();
            }

            return true;
        }

        private bool IsRespawnEnabledForCarrier()
        {
            return IsRespawnEnabledForPrefab(ResolveRespawnPrefabName());
        }

        private bool IsRespawnEnabledForPrefab(string prefabName)
        {
            return string.Equals(prefabName, TreeRegistrar.GreenMushroomPrefabName, StringComparison.Ordinal) ||
                   string.Equals(prefabName, TreeRegistrar.PurpleMushroomPrefabName, StringComparison.Ordinal);
        }

        private float ResolveConfiguredRespawnMinutes(ZDO zdo)
        {
            float fallbackMinutes = ResolveLegacyRespawnMinutes(zdo);
            string prefabName = ResolveRespawnPrefabName(zdo);
            if (!string.IsNullOrWhiteSpace(prefabName))
            {
                return Mathf.Max(0f, TreeConfigFile.GetRespawnMinutes(prefabName, fallbackMinutes));
            }

            return Mathf.Max(0f, fallbackMinutes);
        }

        private float ResolveLegacyRespawnMinutes(ZDO zdo)
        {
            if (zdo == null)
            {
                return DefaultRegrowMinutes;
            }

            float minutes = zdo.GetFloat("rvb_regrowminutes", 0f);
            if (minutes <= 0f)
            {
                minutes = zdo.GetFloat("Pickable.m_respawnTimeMinutes", 0f);
            }

            if (minutes <= 0f)
            {
                minutes = zdo.GetFloat("Pickable.m_respawnTimeInitMin", 0f);
            }

            if (minutes <= 0f)
            {
                minutes = DefaultRegrowMinutes;
            }

            return Mathf.Max(0f, minutes);
        }

        private string ResolveRespawnPrefabName()
        {
            return ResolveRespawnPrefabName(GetCarrierZdo());
        }

        private string ResolveRespawnPrefabName(ZDO zdo)
        {
            if (zdo != null)
            {
                string prefabName = zdo.GetString("respawnprefab", string.Empty);
                if (!string.IsNullOrWhiteSpace(prefabName))
                {
                    return prefabName;
                }

                prefabName = zdo.GetString("rvb_respawnprefab", string.Empty);
                if (!string.IsNullOrWhiteSpace(prefabName))
                {
                    return prefabName;
                }
            }

            return string.Empty;
        }

        private static string FormatRemaining(TimeSpan remaining)
        {
            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt((float)remaining.TotalSeconds));
            int days = totalSeconds / 86400;
            int hours = (totalSeconds % 86400) / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            if (days > 0)
            {
                return days + "d " + hours + "h " + minutes + "m";
            }

            if (hours > 0)
            {
                return hours + "h " + minutes + "m " + seconds + "s";
            }

            if (minutes > 0)
            {
                return minutes + "m " + seconds + "s";
            }

            return seconds + "s";
        }

        private void PlayDestroyEffects()
        {
            if (ZNetScene.instance == null)
            {
                return;
            }

            GameObject destroyVfx = ZNetScene.instance.GetPrefab(DestroyVfxPrefabName);
            if (destroyVfx != null)
            {
                Instantiate(destroyVfx, transform.position, transform.rotation);
            }

            GameObject destroySfx = ZNetScene.instance.GetPrefab(DestroySfxPrefabName);
            if (destroySfx != null)
            {
                Instantiate(destroySfx, transform.position, transform.rotation);
            }
        }

        private void DisableInteraction()
        {
            if (cachedColliders == null || cachedColliders.Length == 0)
            {
                cachedColliders = GetComponentsInChildren<Collider>(true);
            }

            for (int i = 0; i < cachedColliders.Length; i++)
            {
                if (cachedColliders[i] != null)
                {
                    cachedColliders[i].enabled = false;
                }
            }
        }

        private void DestroySelf()
        {
            if (gameObject == null)
            {
                return;
            }

            if (ZNetScene.instance != null)
            {
                ZNetScene.instance.Destroy(gameObject);
                return;
            }

            Destroy(gameObject);
        }

        private static TreeRegrowTimer TryGetTimer(Component component)
        {
            if (component == null)
            {
                return null;
            }

            TreeRegrowTimer timer = component.GetComponent<TreeRegrowTimer>();
            if (timer != null)
            {
                timer.TryApplyPendingCarrierData();
                timer.TryHandleRespawnDisabled();
                return timer.IsRegrowCarrier() ? timer : null;
            }

            ZNetView view = component.GetComponent<ZNetView>();
            if (view == null)
            {
                view = component.GetComponentInParent<ZNetView>();
            }

            if (view == null || !view.IsValid())
            {
                return null;
            }

            ZDO zdo = view.GetZDO();
            if (zdo == null || zdo.GetInt(TreeRespawnerKey, 0) != 1)
            {
                return null;
            }

            GameObject target = view.gameObject;
            timer = target.GetComponent<TreeRegrowTimer>();
            if (timer == null)
            {
                timer = target.AddComponent<TreeRegrowTimer>();
            }

            timer.DisableVanillaPlantGrowth();
            timer.EnsureInitialized();
            timer.TryHandleRespawnDisabled();
            return timer;
        }

        [HarmonyPatch(typeof(ZNetView), "Awake")]
        private static class ZNetView_Awake_Patch
        {
            private static void Postfix(ZNetView __instance)
            {
                TryGetTimer(__instance);
            }
        }

        [HarmonyPatch(typeof(Pickable), "GetHoverText")]
        private static class Pickable_GetHoverText_Patch
        {
            private static bool Prefix(Pickable __instance, ref string __result)
            {
                TreeRegrowTimer timer = TryGetTimer(__instance);
                if (timer == null)
                {
                    return true;
                }

                __result = timer.BuildHoverText();
                return false;
            }
        }

        [HarmonyPatch(typeof(Pickable), "GetHoverName")]
        private static class Pickable_GetHoverName_Patch
        {
            private static bool Prefix(Pickable __instance, ref string __result)
            {
                TreeRegrowTimer timer = TryGetTimer(__instance);
                if (timer == null)
                {
                    return true;
                }

                __result = timer.BuildHoverName();
                return false;
            }
        }

        [HarmonyPatch(typeof(Pickable), "Interact")]
        private static class Pickable_Interact_Patch
        {
            private static bool Prefix(Pickable __instance, ref bool __result)
            {
                TreeRegrowTimer timer = TryGetTimer(__instance);
                if (timer == null)
                {
                    return true;
                }

                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Plant), "GetHoverText")]
        private static class Plant_GetHoverText_Patch
        {
            private static bool Prefix(Plant __instance, ref string __result)
            {
                TreeRegrowTimer timer = TryGetTimer(__instance);
                if (timer == null)
                {
                    return true;
                }

                __result = timer.BuildHoverText();
                return false;
            }
        }

        [HarmonyPatch(typeof(Plant), "GetHoverName")]
        private static class Plant_GetHoverName_Patch
        {
            private static bool Prefix(Plant __instance, ref string __result)
            {
                TreeRegrowTimer timer = TryGetTimer(__instance);
                if (timer == null)
                {
                    return true;
                }

                __result = timer.BuildHoverName();
                return false;
            }
        }

        [HarmonyPatch(typeof(Piece), "DropResources")]
        private static class Piece_DropResources_Patch
        {
            private static bool Prefix(Piece __instance)
            {
                if (__instance == null)
                {
                    return true;
                }

                TreeRegrowTimer timer = __instance.GetComponent<TreeRegrowTimer>();
                if (timer == null)
                {
                    return true;
                }

                timer.RequestSilentRemove();
                return false;
            }
        }

        [HarmonyPatch(typeof(WearNTear), "RPC_Damage")]
        private static class WearNTear_RPC_Damage_Patch
        {
            private static void Postfix(WearNTear __instance)
            {
                if (__instance == null)
                {
                    return;
                }

                TreeRegrowTimer timer = __instance.GetComponent<TreeRegrowTimer>();
                if (timer == null || __instance.m_health > 0f)
                {
                    return;
                }

                timer.RequestSilentRemove();
            }
        }
    }
}
