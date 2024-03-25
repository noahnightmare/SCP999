using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
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
            if (rng.Next(1, 101) <= SCP999.Instance.Config.RoleConfigs.Scp999.Chance)
            {
                SCP999.Instance.Config.RoleConfigs.Scp999.AddRole(Player.List.Where(x => x.IsScp).ToList().RandomItem());
            }
        }
    }
}
