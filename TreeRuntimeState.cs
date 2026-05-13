using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Ravenwood.Biomes
{
    public class TreeRuntimeState : MonoBehaviour
    {
        private const string PlayerPlacedZdoKey = "rvb_playerplaced";
        private const string CultivatorPrefabName = "Cultivator";
        private const string CultivatorPrefabSuffix = "_Cultivator";
        private const float RemoveRayDistance = 50f;

        private static readonly FieldInfo RightItemField = AccessTools.Field(typeof(Humanoid), "m_rightItem");
        private static readonly MethodInfo GetRightItemMethod = AccessTools.Method(typeof(Humanoid), "GetRightItem");
        private static readonly MethodInfo PieceGetCreatorMethod = AccessTools.Method(typeof(Piece), "GetCreator");
        private static readonly int RemoveRayMask = LayerMask.GetMask("piece", "piece_nonsolid", "item", "Default_small", "Default", "static_solid");
        private static Piece cachedValidatedRemovePiece;

        private readonly List<TreeDropEntry> dropEntries = new List<TreeDropEntry>();

        private bool handledDestroyed;
        private bool restoredExistingState;
        private bool suppressDropSpawn;
        private bool removalInProgress;
        private bool allowVanillaDropResourcesOnce;
        private bool destroyFeedbackPlayed;
        private bool placeFeedbackPlayed;
        private bool indestructible;
        private float lastKnownHealth = -1f;

        private WearNTear wear;
        private Destructible destructible;
        private TreeBase treeBase;
        private ZNetView znv;
        private Piece piece;

        public bool IsIndestructible
        {
            get { return indestructible; }
        }

        public void Configure(List<TreeDropEntry> entries, bool isIndestructible = false)
        {
            indestructible = isIndestructible;
            dropEntries.Clear();

            if (entries == null)
            {
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                TreeDropEntry entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                dropEntries.Add(new TreeDropEntry(entry.ItemName, entry.Min, entry.Max, entry.Chance));
            }
        }

        public void MarkPlayerPlaced()
        {
            znv = znv != null ? znv : GetComponent<ZNetView>();
            if (znv == null || !znv.IsValid())
            {
                PlayPlaceFeedback();
                return;
            }

            ZDO zdo = znv.GetZDO();
            if (zdo == null)
            {
                PlayPlaceFeedback();
                return;
            }

            bool wasPlaced = zdo.GetInt(PlayerPlacedZdoKey, 0) == 1;
            zdo.Set(PlayerPlacedZdoKey, 1);

            if (!wasPlaced)
            {
                PlayPlaceFeedback();
            }
        }

        public bool IsPlayerPlaced()
        {
            znv = znv != null ? znv : GetComponent<ZNetView>();
            if (znv == null || !znv.IsValid())
            {
                return false;
            }

            ZDO zdo = znv.GetZDO();
            if (zdo == null)
            {
                return false;
            }

            return zdo.GetInt(PlayerPlacedZdoKey, 0) == 1;
        }

        public void PrepareForCultivatorRemove()
        {
            allowVanillaDropResourcesOnce = false;
            suppressDropSpawn = true;
        }

        public bool ConsumeVanillaDropResourcesAllowance()
        {
            if (!allowVanillaDropResourcesOnce)
            {
                return false;
            }

            allowVanillaDropResourcesOnce = false;
            return true;
        }

        public void RequestCultivatorMiddleMouseRemove()
        {
            if (removalInProgress || handledDestroyed)
            {
                return;
            }

            if (!TryClaimOwnership())
            {
                return;
            }

            StartCoroutine(CultivatorRemoveRoutine());
        }

        public void HandleToolRemove()
        {
            if (indestructible)
            {
                suppressDropSpawn = true;
                return;
            }

            if (handledDestroyed)
            {
                suppressDropSpawn = true;
                return;
            }

            suppressDropSpawn = true;
            handledDestroyed = true;

            PlayDestroyFeedback();
            SpawnDrops();
        }

        private void Awake()
        {
            wear = GetComponent<WearNTear>();
            destructible = GetComponent<Destructible>();
            treeBase = GetComponent<TreeBase>();
            znv = GetComponent<ZNetView>();
            piece = GetComponent<Piece>();

            EnsurePieceRules();
            TryRestoreExistingState();
            lastKnownHealth = GetCurrentHealth();
        }

        private void OnDestroy()
        {
            TryHandleDestroyed(true);
        }

        private IEnumerator CultivatorRemoveRoutine()
        {
            removalInProgress = true;
            suppressDropSpawn = true;
            handledDestroyed = true;

            PlayDestroyFeedback();
            SpawnDrops();
            DisableInteraction();

            yield return new WaitForSeconds(0.32f);
            DestroySelf();
        }

        private void DisableInteraction()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
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

        public void EnsurePieceRules()
        {
            piece = piece != null ? piece : GetComponent<Piece>();
            if (piece == null)
            {
                return;
            }

            if (!IsPlayerPlaced() && GetPieceCreator(piece) == 0L)
            {
                piece.m_canBeRemoved = false;
                return;
            }

            piece.m_groundOnly = false;
            piece.m_canBeRemoved = true;
        }

        private void TryRestoreExistingState()
        {
            if (restoredExistingState)
            {
                return;
            }

            restoredExistingState = TreeManager.TryRestoreExistingTreeState(this);
            EnsurePieceRules();
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

        private bool CanHandleDestroyedState()
        {
            if (ZNet.instance != null)
            {
                return ZNet.instance.IsServer();
            }

            znv = znv != null ? znv : GetComponent<ZNetView>();
            if (znv == null)
            {
                return true;
            }

            return !znv.IsValid() || znv.IsOwner();
        }

        private void TryHandleDestroyed(bool fromOnDestroy)
        {
            if (handledDestroyed)
            {
                return;
            }

            if (suppressDropSpawn)
            {
                handledDestroyed = true;
                return;
            }

            if (indestructible)
            {
                return;
            }

            if (!CanHandleDestroyedState())
            {
                return;
            }

            wear = wear != null ? wear : GetComponent<WearNTear>();
            destructible = destructible != null ? destructible : GetComponent<Destructible>();

            if (wear == null && destructible == null)
            {
                if (!fromOnDestroy)
                {
                    return;
                }

                return;
            }

            if (wear != null)
            {
                if (wear.m_health > 0f)
                {
                    return;
                }
            }
            else
            {
                if (destructible.m_health > 0f)
                {
                    return;
                }
            }

            handledDestroyed = true;
            PlayDestroyFeedback();
            SpawnDrops();
        }

        public void HandleDamageStateChanged()
        {
            if (indestructible)
            {
                return;
            }

            float currentHealth = GetCurrentHealth();
            if (currentHealth < 0f)
            {
                return;
            }

            if (lastKnownHealth < 0f)
            {
                lastKnownHealth = currentHealth;
                return;
            }

            if (currentHealth <= 0f)
            {
                if (lastKnownHealth > 0f)
                {
                    PlayDestroyFeedback();
                }
            }
            else if (currentHealth < lastKnownHealth)
            {
                PlayHitFeedback();
            }

            lastKnownHealth = currentHealth;
        }

        private float GetCurrentHealth()
        {
            wear = wear != null ? wear : GetComponent<WearNTear>();
            if (wear != null)
            {
                return wear.m_health;
            }

            destructible = destructible != null ? destructible : GetComponent<Destructible>();
            if (destructible != null)
            {
                return destructible.m_health;
            }

            treeBase = treeBase != null ? treeBase : GetComponent<TreeBase>();
            if (treeBase != null)
            {
                return treeBase.m_health;
            }

            return -1f;
        }

        private void PlayHitFeedback()
        {
            TreeFeedbackEffects.PlayHit(CleanPrefabName(gameObject.name), transform.position, transform.rotation);
        }

        private void PlayDestroyFeedback()
        {
            if (destroyFeedbackPlayed)
            {
                return;
            }

            destroyFeedbackPlayed = true;
            TreeFeedbackEffects.PlayDestroy(CleanPrefabName(gameObject.name), transform.position, transform.rotation);
        }

        private void PlayPlaceFeedback()
        {
            if (placeFeedbackPlayed)
            {
                return;
            }

            placeFeedbackPlayed = true;
            TreeFeedbackEffects.PlayPlace(CleanPrefabName(gameObject.name), transform.position, transform.rotation);
        }

        private void SpawnDrops()
        {
            if (ZNetScene.instance == null || dropEntries.Count == 0)
            {
                return;
            }

            for (int i = 0; i < dropEntries.Count; i++)
            {
                TreeDropEntry entry = dropEntries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ItemName))
                {
                    continue;
                }

                float chance = Mathf.Clamp01(entry.Chance);
                if (chance <= 0f)
                {
                    continue;
                }

                if (chance < 1f && UnityEngine.Random.value > chance)
                {
                    continue;
                }

                GameObject dropPrefab = ZNetScene.instance.GetPrefab(entry.ItemName);
                if (dropPrefab == null)
                {
                    Debug.LogWarning("Tree drop prefab not found: " + entry.ItemName);
                    continue;
                }

                int min = Mathf.Max(0, entry.Min);
                int max = Mathf.Max(min, entry.Max);
                int amount = UnityEngine.Random.Range(min, max + 1);

                for (int dropIndex = 0; dropIndex < amount; dropIndex++)
                {
                    Vector3 dropPosition = transform.position + Vector3.up + UnityEngine.Random.insideUnitSphere * 0.15f;
                    dropPosition.y = Mathf.Max(dropPosition.y, transform.position.y + 0.5f);
                    Instantiate(dropPrefab, dropPosition, Quaternion.identity);
                }
            }
        }

        private static bool IsRavenwoodPickableMushroomName(string prefabName)
        {
            return string.Equals(prefabName, TreeRegistrar.GreenMushroomPrefabName, StringComparison.Ordinal) ||
                   string.Equals(prefabName, TreeRegistrar.PurpleMushroomPrefabName, StringComparison.Ordinal);
        }

        private static long GetPieceCreator(Piece targetPiece)
        {
            if (targetPiece == null || PieceGetCreatorMethod == null)
            {
                return 0L;
            }

            try
            {
                object value = PieceGetCreatorMethod.Invoke(targetPiece, null);
                if (value is long longValue)
                {
                    return longValue;
                }

                if (value is int intValue)
                {
                    return intValue;
                }
            }
            catch
            {
            }

            return 0L;
        }

        private static bool IsCultivator(ItemDrop.ItemData item)
        {
            if (item == null)
            {
                return false;
            }

            if (item.m_dropPrefab != null)
            {
                string prefabName = CleanPrefabName(item.m_dropPrefab.name);
                if (string.Equals(prefabName, CultivatorPrefabName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            if (item.m_shared != null)
            {
                if (string.Equals(item.m_shared.m_name, "$item_cultivator", StringComparison.Ordinal) ||
                    string.Equals(item.m_shared.m_name, "Cultivator", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static ItemDrop.ItemData GetEquippedRightItem(Player player)
        {
            if (player == null)
            {
                return null;
            }

            if (RightItemField != null)
            {
                try
                {
                    return RightItemField.GetValue(player) as ItemDrop.ItemData;
                }
                catch
                {
                }
            }

            if (GetRightItemMethod != null)
            {
                try
                {
                    return GetRightItemMethod.Invoke(player, null) as ItemDrop.ItemData;
                }
                catch
                {
                }
            }

            return null;
        }

        private static bool IsManagedTreePiece(Piece targetPiece)
        {
            if (targetPiece == null)
            {
                return false;
            }

            return targetPiece.GetComponent<TreeRuntimeState>() != null ||
                   targetPiece.GetComponent<TreeRegrowTimer>() != null;
        }

        private static bool CanCultivatorRemovePiece(Piece targetPiece)
        {
            if (!IsManagedTreePiece(targetPiece))
            {
                return false;
            }

            TreeRuntimeState runtime = targetPiece.GetComponent<TreeRuntimeState>();
            if (runtime != null && !runtime.IsPlayerPlaced())
            {
                return false;
            }

            return PrivateArea.CheckAccess(targetPiece.transform.position, 0f, true, false);
        }

        private static void ApplyCultivatorRemoveFlag(ItemDrop.ItemData item)
        {
            if (item == null || item.m_shared == null)
            {
                return;
            }

            PieceTable pieceTable = item.m_shared.m_buildPieces;
            if (pieceTable == null)
            {
                return;
            }

            pieceTable.m_canRemovePieces = true;
        }

        private static void EnsureCultivatorRemoveEnabled(Player player = null)
        {
            ObjectDB objectDb = ObjectDB.instance;
            if (objectDb != null)
            {
                GameObject cultivatorPrefab = objectDb.GetItemPrefab(CultivatorPrefabName);
                if (cultivatorPrefab != null)
                {
                    ItemDrop itemDrop = cultivatorPrefab.GetComponent<ItemDrop>();
                    if (itemDrop != null)
                    {
                        ApplyCultivatorRemoveFlag(itemDrop.m_itemData);
                    }
                }
            }

            Player targetPlayer = player != null ? player : Player.m_localPlayer;
            if (targetPlayer == null)
            {
                return;
            }

            ApplyCultivatorRemoveFlag(GetEquippedRightItem(targetPlayer));

            Inventory inventory = targetPlayer.GetInventory();
            if (inventory == null)
            {
                return;
            }

            List<ItemDrop.ItemData> items = inventory.GetAllItems();
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ItemDrop.ItemData item = items[i];
                if (!IsCultivator(item))
                {
                    continue;
                }

                ApplyCultivatorRemoveFlag(item);
            }
        }

        private static bool TryFindManagedPiece(Player player, out Piece targetPiece)
        {
            targetPiece = null;

            if (player == null || GameCamera.instance == null)
            {
                return false;
            }

            Transform cameraTransform = GameCamera.instance.transform;
            if (cameraTransform == null)
            {
                return false;
            }

            RaycastHit hit;
            if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, RemoveRayDistance, RemoveRayMask))
            {
                return false;
            }

            if (Vector3.Distance(hit.point, player.m_eye.position) > player.m_maxPlaceDistance)
            {
                return false;
            }

            targetPiece = hit.collider != null ? hit.collider.GetComponentInParent<Piece>() : null;
            return IsManagedTreePiece(targetPiece);
        }

        private static void CacheValidatedRemovePiece(Piece piece)
        {
            cachedValidatedRemovePiece = piece;
        }

        private static bool TryConsumeValidatedRemovePiece(Player player, out Piece targetPiece)
        {
            targetPiece = cachedValidatedRemovePiece;
            cachedValidatedRemovePiece = null;

            if (targetPiece == null)
            {
                return false;
            }

            if (player == null || player.m_eye == null)
            {
                return false;
            }

            if (Vector3.Distance(targetPiece.transform.position, player.m_eye.position) > player.m_maxPlaceDistance + 2f)
            {
                return false;
            }

            return IsManagedTreePiece(targetPiece);
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

            if (cleaned.EndsWith(CultivatorPrefabSuffix, StringComparison.Ordinal))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - CultivatorPrefabSuffix.Length).Trim();
            }

            return cleaned;
        }

        [HarmonyPatch(typeof(WearNTear), "RPC_Damage")]
        private static class WearNTear_RPC_Damage_Patch
        {
            private static bool Prefix(WearNTear __instance)
            {
                if (__instance == null)
                {
                    return true;
                }

                TreeRuntimeState runtime = __instance.GetComponent<TreeRuntimeState>();
                return runtime == null || !runtime.IsIndestructible;
            }

            private static void Postfix(WearNTear __instance)
            {
                if (__instance == null)
                {
                    return;
                }

                TreeRuntimeState runtime = __instance.GetComponent<TreeRuntimeState>();
                if (runtime == null)
                {
                    return;
                }

                runtime.HandleDamageStateChanged();
                runtime.TryHandleDestroyed(false);
            }
        }

        [HarmonyPatch(typeof(Destructible), "RPC_Damage")]
        private static class Destructible_RPC_Damage_Patch
        {
            private static bool Prefix(Destructible __instance)
            {
                if (__instance == null)
                {
                    return true;
                }

                TreeRuntimeState runtime = __instance.GetComponent<TreeRuntimeState>();
                return runtime == null || !runtime.IsIndestructible;
            }

            private static void Postfix(Destructible __instance)
            {
                if (__instance == null)
                {
                    return;
                }

                TreeRuntimeState runtime = __instance.GetComponent<TreeRuntimeState>();
                if (runtime == null)
                {
                    return;
                }

                runtime.HandleDamageStateChanged();
                runtime.TryHandleDestroyed(false);
            }
        }

        [HarmonyPatch(typeof(TreeBase), "RPC_Damage")]
        private static class TreeBase_RPC_Damage_Patch
        {
            private static bool Prefix(TreeBase __instance)
            {
                if (__instance == null)
                {
                    return true;
                }

                TreeRuntimeState runtime = __instance.GetComponent<TreeRuntimeState>();
                return runtime == null || !runtime.IsIndestructible;
            }
        }

        [HarmonyPatch(typeof(Piece), "SetCreator")]
        private static class Piece_SetCreator_Patch
        {
            private static void Postfix(Piece __instance)
            {
                if (__instance == null)
                {
                    return;
                }

                TreeRuntimeState runtime = __instance.GetComponent<TreeRuntimeState>();
                if (runtime == null)
                {
                    string prefabName = CleanPrefabName(__instance.gameObject.name);
                    if (IsRavenwoodPickableMushroomName(prefabName))
                    {
                        TreeFeedbackEffects.PlayPlace(prefabName, __instance.transform.position, __instance.transform.rotation);
                    }

                    return;
                }

                runtime.MarkPlayerPlaced();
                runtime.EnsurePieceRules();
            }
        }

        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        private static class ObjectDB_Awake_Patch
        {
            private static void Postfix()
            {
                EnsureCultivatorRemoveEnabled();
            }
        }

        [HarmonyPatch(typeof(Player), "Awake")]
        private static class Player_Awake_Patch
        {
            private static void Postfix()
            {
                EnsureCultivatorRemoveEnabled();
            }
        }

        [HarmonyPatch(typeof(Player), "CheckCanRemovePiece")]
        private static class Player_CheckCanRemovePiece_Patch
        {
            private static bool Prefix(Player __instance, Piece piece, ref bool __result)
            {
                if (!IsManagedTreePiece(piece))
                {
                    return true;
                }

                EnsureCultivatorRemoveEnabled(__instance);

                ItemDrop.ItemData equippedItem = GetEquippedRightItem(__instance);
                if (!IsCultivator(equippedItem))
                {
                    CacheValidatedRemovePiece(null);
                    __result = false;
                    return false;
                }

                bool canRemove = CanCultivatorRemovePiece(piece);
                CacheValidatedRemovePiece(canRemove ? piece : null);

                if (!canRemove && __instance != null)
                {
                    __instance.Message(MessageHud.MessageType.Center, "$msg_privatezone");
                }

                __result = canRemove;
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "RemovePiece")]
        private static class Player_RemovePiece_Patch
        {
            private static bool Prefix(Player __instance)
            {
                EnsureCultivatorRemoveEnabled(__instance);

                ItemDrop.ItemData equippedItem = GetEquippedRightItem(__instance);
                if (!IsCultivator(equippedItem))
                {
                    CacheValidatedRemovePiece(null);
                    return true;
                }

                Piece targetPiece;
                if (!TryConsumeValidatedRemovePiece(__instance, out targetPiece) &&
                    !TryFindManagedPiece(__instance, out targetPiece))
                {
                    return true;
                }

                if (!CanCultivatorRemovePiece(targetPiece))
                {
                    if (__instance != null)
                    {
                        __instance.Message(MessageHud.MessageType.Center, "$msg_privatezone");
                    }

                    return false;
                }

                TreeRuntimeState runtime = targetPiece.GetComponent<TreeRuntimeState>();
                if (runtime != null)
                {
                    runtime.RequestCultivatorMiddleMouseRemove();
                    return false;
                }

                TreeRegrowTimer timer = targetPiece.GetComponent<TreeRegrowTimer>();
                if (timer != null)
                {
                    timer.RequestSilentRemove();
                    return false;
                }

                return true;
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

                TreeRuntimeState runtime = __instance.GetComponent<TreeRuntimeState>();
                if (runtime == null)
                {
                    return true;
                }

                if (runtime.ConsumeVanillaDropResourcesAllowance())
                {
                    return true;
                }

                runtime.HandleToolRemove();
                return false;
            }
        }
    }
}
