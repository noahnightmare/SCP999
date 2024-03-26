using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Components;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SCP999.Handlers;

using PlayerEvent = Exiled.Events.Handlers.Player;

namespace SCP999.Abilities
{
    [CustomAbility]
    public class Healthy : ActiveAbility
    {
        public override string Name { get; set; } = "Healthy";

        public override string Description { get; set; } = "Allows SCP-999 to heal all players nearby a configurable amount of HP per second for a configurable amount of seconds.";

        public override float Duration { get; set; } = 15f;

        public override float Cooldown { get; set; } = 180f;

        [Description("Radius in which SCP 999 heals people with this ability")]
        public float Radius { get; set; } = 13f;

        [Description("Amount healed to players around SCP 999 per second.")]
        public int HealAmount { get; set; } = 2;

        [Description("Sound if Healthy is used (sound file name in EXILED/Configs/Sounds/)")]
        public string AbilitySound { get; set; } = "999AbilitySound.mp3";

        [Description("Volume of the sound above: 0 - 255")]
        public byte Volume { get; set; } = 255;

        public bool canUseHealthy = true;

        protected override void SubscribeEvents()
        {
            PlayerEvent.TogglingNoClip += OnPlayerTogglingNoClip;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            PlayerEvent.TogglingNoClip -= OnPlayerTogglingNoClip;
            base.UnsubscribeEvents();
        }

        protected override void AbilityUsed(Player player)
        {
            SoundHandler.PlayAudio(AbilitySound, Volume, true, "SCP-999", Vector3.zero);
            Timing.RunCoroutine(AbilityInProgress(player));

            base.AbilityUsed(player);
        }

        protected override void AbilityEnded(Player player)
        {
            SoundHandler.StopAudio();

            base.AbilityEnded(player); 
        }

        private void OnPlayerTogglingNoClip(TogglingNoClipEventArgs ev)
        {
            if (ev.IsAllowed) return;

            // checks if ability isn't currently being used
            if (canUseHealthy && Check(ev.Player, Exiled.CustomRoles.API.Features.Enums.CheckType.Available))
            {
                canUseHealthy = false;
                Timing.CallDelayed(0.25f, () => AbilityUsed(ev.Player));
            }
        }

        private IEnumerator<float> AbilityInProgress(Player player)
        {
            Timing.CallDelayed(Cooldown, () => { canUseHealthy = true; });

            // one tick per "duration" config
            for (int i = 0; i < Duration; i++)
            {
                yield return Timing.WaitForSeconds(1f);

                // heals each person for the heal amount in the specified radius
                foreach (Player p in Player.List.Where(x => Vector3.Distance(x.Position, player.Position) <= Radius)) 
                {
                    p.Heal(HealAmount);
                }
            }

            AbilityEnded(player);
        }
    }
}
