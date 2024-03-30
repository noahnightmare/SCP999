using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using SCP999.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP999
{
    public class EventHandlers
    {
        private System.Random rng = new System.Random();
        // Selects a random SCP player to be SCP 999 at configurable chance
        public void OnRoundStarted()
        {
            Timing.CallDelayed(0.25f, () =>
            {
                if (Player.List.Where(x => x.IsScp).Count() > 0)
                {
                    if (rng.Next(1, 101) <= SCP999.Instance.Config.RoleConfigs.Scp999.Chance)
                    {
                        CustomRole.Get(typeof(CustomRoleScp999)).AddRole(Player.List.Where(x => x.IsScp).GetRandomValue());
                    }
                }
            });
        }
    }
}
