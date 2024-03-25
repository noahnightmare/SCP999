using Exiled.API.Features.Attributes;
using Exiled.API.Features.Components;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Mirror;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utf8Json.Resolvers.Internal;
using PlayerEvent = Exiled.Events.Handlers.Player;

namespace SCP999.Abilities
{
    [CustomAbility]
    public class HealOthersOnHit : PassiveAbility
    {
        public override string Name { get; set; } = "Heal Others on Hit";
        public override string Description { get; set; } = "Heals other players when SCP 999 hits them.";

        [Description("Amount of HP to give when attacking someone as SCP 999")]
        public float HealthGiveOnAttack { get; set; } = 5f;

        protected override void SubscribeEvents()
        {
            PlayerEvent.Hurting += OnHurting;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            PlayerEvent.Hurting -= OnHurting;
            base.UnsubscribeEvents();
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker == null || ev.Player == null) return;

            // Handles healing the attacked player when SCP 999 attacks someone
            if (Check(ev.Attacker))
            {
                ev.Player.Heal(HealthGiveOnAttack);
                ev.IsAllowed = false;
            }
        }
    }
}
