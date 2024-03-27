namespace SCP999.Patches
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomPlayerEffects;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using HarmonyLib;
    using Exiled.CustomRoles.API.Features;
    using PlayerRoles.PlayableScps.HumeShield;
    using global::SCP999.Role;

    [HarmonyPatch(typeof(DynamicHumeShieldController), nameof(DynamicHumeShieldController.HsMax), MethodType.Getter)]
    public class RecoverHumeShieldPatch
    {
        public static bool Prefix(DynamicHumeShieldController __instance, ref float __result)
        {
            // patch max hume shield hp value
            if (!Player.TryGet(__instance.Owner, out Player player))
                return true;

            if (CustomRole.Get(typeof(CustomRoleScp999)).Check(player))
            {
                __result = SCP999.Instance.Config.RoleConfigs.Scp999.HumeShield;
                return false;
            }

            return true;
        }
    }
}