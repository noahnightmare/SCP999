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
    using PlayerStatsSystem;
    using Mirror;
    using UnityEngine;
    using Exiled.API.Features.Roles;
    using PlayerRoles.PlayableScps.Scp049.Zombies;

    [HarmonyPatch(typeof(ZombieShieldController), nameof(ZombieShieldController.HsMax), MethodType.Getter)]
    public class RecoverHumeShieldPatch
    {
        public static bool Prefix(ZombieShieldController __instance, ref float __result)
        {
            if (!Player.TryGet(__instance.Owner, out Player player))
                return true;

            if (!CustomRole.Get(typeof(CustomRoleScp999)).Check(player))
                return true;

            __result = SCP999.Instance.Config.RoleConfigs.Scp999.HumeShield;
            return false;
        }
    }

    [HarmonyPatch(typeof(ZombieShieldController), nameof(ZombieShieldController.HsRegeneration), MethodType.Getter)]
    public class RecoverHumeShieldPatch2
    {
        public static bool Prefix(ZombieShieldController __instance, ref float __result)
        {
            if (!Player.TryGet(__instance.Owner, out Player player))
                return true;

            if (!CustomRole.Get(typeof(CustomRoleScp999)).Check(player))
                return true;

            if (SCP999.Instance.Config.RoleConfigs.Scp999.nextHumeRegenRate < 5)
            {
                __result = 0f;
            }
            __result = SCP999.Instance.Config.RoleConfigs.Scp999.HumeShieldRegenerationRate;
            return false;
        }
    }
}