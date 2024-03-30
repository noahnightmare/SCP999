using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using MapEditorReborn.API;
using MapEditorReborn.Events.Handlers;
using MEC;
using SCP999.Handlers;
using SCP999.Role;
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

        private CooldownHandler cooldownHandler;
        private CoroutineHandle coro;

        protected override void SubscribeEvents()
        {
            ServerEvent.WaitingForPlayers += OnWaitingForPlayers;
            ServerEvent.RoundStarted += OnRoundStarted;
            ServerEvent.RestartingRound += OnRestartingRound;
            PlayerEvent.Spawned += OnSpawned;
            PlayerEvent.Hurting += OnHurting;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            ServerEvent.WaitingForPlayers -= OnWaitingForPlayers;
            ServerEvent.RoundStarted -= OnRoundStarted;
            ServerEvent.RestartingRound -= OnRestartingRound;
            PlayerEvent.Spawned -= OnSpawned;
            PlayerEvent.Hurting -= OnHurting;
            base.UnsubscribeEvents();
        }

        private void OnWaitingForPlayers()
        {
            cooldownHandler = new CooldownHandler();
        }

        private void OnRoundStarted()
        {
            // Handles running coro on start
            Timing.CallDelayed(0.25f, () =>
            {
                coro = Timing.RunCoroutine(HealthHandler());
            });
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (Check(ev.Player))
            {
                // initialise cooldowns for players spawning as 999
                cooldownHandler.PutOnCooldown(ev.Player, TimeSpan.FromSeconds(0));
                SCP999.Instance.Config.RoleConfigs.Scp999.playerHumeShieldCooldowns.PutOnCooldown(ev.Player, TimeSpan.FromSeconds(0));
            }
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            // Handles the health regeneration cooldown
            if (Check(ev.Player))
            {
                cooldownHandler.PutOnCooldown(ev.Player, TimeSpan.FromSeconds(TimeBeforeHealthRecover));

                SCP999.Instance.Config.RoleConfigs.Scp999.playerHumeShieldCooldowns.PutOnCooldown(ev.Player, TimeSpan.FromSeconds(5));
            }
        }

        private void OnRestartingRound()
        {
            Timing.KillCoroutines(coro);
            cooldownHandler = null;
        }

        private IEnumerator<float> HealthHandler()
        {
            // coro that handles healing for player when they are liable to be healed
            for (; ; )
            {
                yield return Timing.WaitForSeconds(1f);

                foreach(Player player in cooldownHandler.GetPlayerCooldowns()?.Keys.Where(x => CustomRole.Get(typeof(CustomRoleScp999)).Check(x)))
                {
                    if (!cooldownHandler.IsOnCooldown(player, out double remainingSeconds))
                    {
                        player.Heal(HealthRegainOverTime);
                    } 
                }
            }
        }
    }
}
