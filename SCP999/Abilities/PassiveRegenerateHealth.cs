using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using MapEditorReborn.API;
using MapEditorReborn.Events.Handlers;
using MEC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlayerEvent = Exiled.Events.Handlers.Player;

namespace SCP999.Abilities
{
    [CustomAbility]
    public class PassiveRegenerateHealth : PassiveAbility
    {
        public override string Name { get; set; } = "Regenerate Health Passively";
        public override string Description { get; set; } = "Regenerates SCP 999\'s HP when he\'s not being attacked.";

        [Description("Amount of health to recover every second if SCP 999 doesn't take damage")]
        public float HealthRegainOverTime { get; set; } = 5f;

        [Description("Amount of time after being attacked that SCP 999 can recover health")]
        public float TimeBeforeHealthRecover { get; set; } = 10f;

        private bool canRegenerateHealth = true;
        private CoroutineHandle coro;

        protected override void SubscribeEvents()
        {
            PlayerEvent.Spawned += OnSpawned;
            PlayerEvent.Hurting += OnHurting;
            PlayerEvent.ChangingRole += OnChangingRole;
            PlayerEvent.Died += OnDied;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            PlayerEvent.Spawned -= OnSpawned;
            PlayerEvent.Hurting -= OnHurting;
            PlayerEvent.ChangingRole -= OnChangingRole;
            PlayerEvent.Died -= OnDied;
            base.UnsubscribeEvents();
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player == null) return;

            // Handles running coro on spawn
            if (Check(ev.Player))
            {
                coro = Timing.RunCoroutine(HealthHandler());
            }
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            // Handles the canRegenerateHealth variable - set to true normally but if damaged set to false, and set back to true after x time
            if (Check(ev.Player))
            {
                if (canRegenerateHealth)
                {
                    canRegenerateHealth = false;
                    Timing.CallDelayed(TimeBeforeHealthRecover, () => { canRegenerateHealth = true; });
                }
            }
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;

            if (Check(ev.Player))
            {
                Timing.KillCoroutines(coro);
            }
        }

        private void OnDied(DiedEventArgs ev)
        {
            if (ev.Player == null) return;

            // Handles stopping the coroutine when player dies
            if (Check(ev.Player))
            {
                Timing.KillCoroutines(coro);
            }
        }

        private IEnumerator<float> HealthHandler()
        {
            // coro that handles healing for player when they are liable to be healed
            for (; ; )
            {
                yield return Timing.WaitForSeconds(1f);

                if (canRegenerateHealth)
                {
                    Player p = Player.List.FirstOrDefault(x => x.IsAlive && Check(x));

                    if (p != null)
                    {
                        p.Heal(HealthRegainOverTime);
                    }
                };
            }
        }
    }
}
