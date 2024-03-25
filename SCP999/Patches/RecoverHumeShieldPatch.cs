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
    using PlayerRoles.PlayableScps.HumeShield;

    [HarmonyPatch(typeof(DynamicHumeShieldController), nameof(DynamicHumeShieldController.HsMax), MethodType.Getter)]
    public class RecoverHumeShieldPatch
    {
        public static bool Prefix(DynamicHumeShieldController __instance, ref float __result)
        {
            // patch max hume shield hp value
            if (!Player.TryGet(__instance.gameObject, out Player player))
                return true;

            if (CustomRole.Get(SCP999.Instance.Config.RoleConfigs.Scp999.Id).Check(player))
            {
                __result = SCP999.Instance.Config.RoleConfigs.Scp999.HumeShield;
                return false;
            }

            return true;
        }
    }
}