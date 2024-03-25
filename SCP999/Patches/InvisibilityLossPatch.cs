namespace LiteDecontamination.Patches
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomPlayerEffects;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using HarmonyLib;
    using Exiled.CustomRoles.API.Features;
    using SCP999;


    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.OnInteract))]
    public class InvisibilityLossPatch
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            // patch losing invisibility for 999 small zombie model
            if (!Player.TryGet(__instance.gameObject, out Player player))
                return true;

            if (CustomRole.Get(SCP999.Instance.Config.RoleConfigs.Scp999.Id).Check(player) &&
                player.GetEffect(EffectType.Invisible).IsEnabled)
            {
                return false;
            }

            return true;
        }
    }
}