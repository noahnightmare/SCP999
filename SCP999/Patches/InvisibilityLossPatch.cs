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

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.OnInteract))]
    public class InvisibilityLossPatch
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            Log.Info("Interact detected");
            // patch losing invisibility for 999 small zombie model
            if (!Player.TryGet(__instance._hub.gameObject, out Player player))
                return true;

            Log.Info("player found");

            if (CustomRole.Get(typeof(CustomRoleScp999)).Check(player))
            {
                Log.Info("Player has custom role");
                return false;
            }

            return true;
        }
    }
}