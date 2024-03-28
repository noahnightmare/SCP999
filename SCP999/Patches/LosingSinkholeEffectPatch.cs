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
    using Exiled.API.Extensions;

    [HarmonyPatch(typeof(StatusEffectBase), nameof(StatusEffectBase.ServerDisable))]
    public class LosingSinkholeEffectPatch
    {
        // patches losing the sinkhole effect when trying to lose an effect as 999
        public static bool Prefix(StatusEffectBase __instance)
        {
            if (!Player.TryGet(__instance.gameObject, out Player player))
                return true;

            if (!CustomRole.Get(typeof(CustomRoleScp999)).Check(player))
                return true;

            if (!__instance.TryGetEffectType(out EffectType effect))
                return true;

            if (effect != EffectType.SinkHole) 
                return true;

            return false;
        }
    }
}