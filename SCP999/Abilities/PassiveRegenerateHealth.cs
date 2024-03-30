﻿using CustomPlayerEffects;
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
using Utf8Json.Resolvers.Internal;
using PlayerEvent = Exiled.Events.Handlers.Player;
using ServerEvent = Exiled.Events.Handlers.Server;

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

        public static Dictionary<Player, DateTime> playerRegenerationTimers = new Dictionary<Player, DateTime>();
        private CoroutineHandle coro;

        protected override void SubscribeEvents()
        {
            ServerEvent.RoundStarted += OnRoundStarted;
            PlayerEvent.Hurting += OnHurting;
            PlayerEvent.ChangingRole += OnChangingRole;
            PlayerEvent.Died += OnDied;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            ServerEvent.RoundStarted -= OnRoundStarted;
            PlayerEvent.Hurting -= OnHurting;
            PlayerEvent.ChangingRole -= OnChangingRole;
            PlayerEvent.Died -= OnDied;
            base.UnsubscribeEvents();
        }

        private void OnRoundStarted()
        {
            // Handles running coro on start
            Timing.CallDelayed(0.25f, () =>
            {
                coro = Timing.RunCoroutine(HealthHandler());
            });
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            // Handles the canRegenerateHealth variable - set to true normally but if damaged set to false, and set back to true after x time
            if (Check(ev.Player))
            {
                if (CanRegenerateHealth(ev.Player, out double remainingSeconds) && remainingSeconds <= 0)
                {
                    ResetRegenerateHealth(ev.Player, TimeSpan.FromSeconds(TimeBeforeHealthRecover));
                }

                SCP999.Instance.Config.RoleConfigs.Scp999.nextHumeRegenRate = 0;
            }
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;

            if (Check(ev.Player))
            {
                if (coro.IsRunning) { Timing.KillCoroutines(coro); }
            }
        }

        private void OnDied(DiedEventArgs ev)
        {
            if (ev.Player == null) return;

            // Handles stopping the coroutine when player dies
            if (Check(ev.Player))
            {
                if (coro.IsRunning) { Timing.KillCoroutines(coro); }
            }
        }

        private IEnumerator<float> HealthHandler()
        {
            // coro that handles healing for player when they are liable to be healed
            for (; ; )
            {
                yield return Timing.WaitForSeconds(1f);

                foreach(Player player in playerRegenerationTimers?.Keys)
                {
                    if (CanRegenerateHealth(player, out double remainingSeconds))
                    {
                        player.Heal(HealthRegainOverTime);
                    } 
                }

                SCP999.Instance.Config.RoleConfigs.Scp999.nextHumeRegenRate += 1;
            }
        }

        public bool CanRegenerateHealth(Player sender, out double remainingSeconds)
        {
            if (playerRegenerationTimers.TryGetValue(sender, out var expiration) && expiration > DateTime.UtcNow)
            {
                remainingSeconds = (expiration - DateTime.UtcNow).TotalSeconds;
                return true;
            }

            remainingSeconds = 0;
            return false;
        }

        public void ResetRegenerateHealth(Player key, TimeSpan duration)
        {
            playerRegenerationTimers[key] = DateTime.UtcNow + duration;
        }
    }
}
