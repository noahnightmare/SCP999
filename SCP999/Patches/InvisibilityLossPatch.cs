namespace SCP999.Patches
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomPlayerEffects;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using HarmonyLib;
    using Exiled.CustomRoles.API.Features;
    using global::SCP999.Role;
    using Interactables.Verification;
    using System.Reflection.Emit;
    using Interactables;
    using InventorySystem.Disarming;
    using InventorySystem.Items;
    using PlayerRoles.FirstPersonControl;
    using UnityEngine;

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.OnInteract))]
    public class InvisibilityLossPatchNuke
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            // patch losing invisibility for 999 small zombie model (NUKE LEVERS)
            if (!Player.TryGet(__instance._hub.gameObject, out Player player))
                return true;

            if (CustomRole.Get(typeof(CustomRoleScp999)).Check(player))
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(StandardDistanceVerification), nameof(StandardDistanceVerification.ServerCanInteract))]
    public class InvisibilityLossPatch
    {
        public static bool Prefix(StandardDistanceVerification __instance, ReferenceHub hub, InteractableCollider collider, ref bool __result)
        {
            // patch losing invisibility for 999 small zombie model
            if (!Player.TryGet(hub.gameObject, out Player player))
                return true;

            if (!__instance._allowHandcuffed && !PlayerInteract.CanDisarmedInteract && hub.inventory.IsDisarmed())
            {
                __result = false;
            }
            if (hub.interCoordinator.AnyBlocker(BlockedInteraction.GeneralInteractions))
            {
                __result = false;
            }
            IFpcRole fpcRole = hub.roleManager.CurrentRole as IFpcRole;
            if (fpcRole == null)
            {
                __result = false;
            }
            Transform transform = collider.transform;
            if (Vector3.Distance(fpcRole.FpcModule.Position, transform.position + transform.TransformDirection(collider.VerificationOffset)) > __instance._maxDistance * 1.4f)
            {
                __result = false;
            }
            if (__instance._cancel268 && !CustomRole.Get(typeof(CustomRoleScp999)).Check(player))
            {
                hub.playerEffectsController.DisableEffect<Invisible>();
            }
            __result = true;
            return false;
        }
    }
}