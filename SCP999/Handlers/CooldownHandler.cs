using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utf8Json.Resolvers.Internal;

namespace SCP999.Handlers
{
    public class CooldownHandler
    {
        private Dictionary<Player, DateTime> playerCooldowns = new Dictionary<Player, DateTime>();

        public bool IsOnCooldown(Player sender, out double remainingSeconds)
        {
            if (playerCooldowns.TryGetValue(sender, out var expiration) && expiration > DateTime.UtcNow)
            {
                remainingSeconds = (expiration - DateTime.UtcNow).TotalSeconds;
                return true;
            }

            remainingSeconds = 0;
            return false;
        }

        public void PutOnCooldown(Player key, TimeSpan duration)
        {
            playerCooldowns[key] = DateTime.UtcNow + duration;
        }

        public Dictionary<Player, DateTime> GetPlayerCooldowns() 
        {
            return playerCooldowns;
        }
    }
}
