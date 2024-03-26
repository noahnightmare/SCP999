using System.Collections.Generic;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomRoles.API.Features;
using Exiled.CustomRoles.API;
using Exiled.API.Features;
using PlayerRoles;
using MapEditorReborn.API.Extensions;
using MapEditorReborn.API.Features;
using MapEditorReborn.API;

using PlayerEvent = Exiled.Events.Handlers.Player;
using System.ComponentModel;
using SCP999.Abilities;
using UnityEngine;
using Exiled.Events.EventArgs.Player;
using MEC;
using System.Linq;
using YamlDotNet.Serialization;
using Exiled.API.Enums;
using CustomPlayerEffects;
using MapEditorReborn.API.Features.Objects;

namespace SCP999.Role
{
    [CustomRole(RoleTypeId.Scp0492)]
    public class CustomRoleScp999 : CustomRole
    {
        public override uint Id { get; set; } = 20;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492;
        public override int MaxHealth { get; set; } = 1000;
        public int HumeShield { get; set; } = 2500;
        public float Chance { get; set; } = 15f;
        public override string Name { get; set; } = "SCP-999";
        public override string Description { get; set; } = "A large gelatinous mass of translucent orange slime, reacting with overwhelming elation with nearby players";
        public override string CustomInfo { get; set; } = "SCP-999";
        public override Vector3 Scale { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);
        [Description("The speed of SCP 999. Use any value from 0-255 (higher value = higher speed)")]
        public byte Speed { get; set; } = 1;
        public override string ConsoleMessage { get; set; } = "Vous avez apparue dans un rôle personnalisé !";
        public override Exiled.API.Features.Broadcast Broadcast { get; set; } = new Exiled.API.Features.Broadcast ("Vous êtes SCP-999 !", (ushort)10, true, global::Broadcast.BroadcastFlags.Normal);
        public override string AbilityUsage { get; set; } = "Pour activer la capacité appuyer sur la touche [Noclip], [ALT] !";

        [Description("Cassie configs for SCP 999")]
        public string CassieAnnouncementOnDeath { get; set; } = "SCP 9 9 9 has been successfully neutralized";
        public string CassieSubtitlesOnDeath { get; set; } = "SCP-999 a été neutralisé avec succès !";

        [Description("Limit of the amount of people that can become SCP 999 in a round, and spawn position")]
        public override SpawnProperties SpawnProperties { get; set; } = new()
        {
            Limit = 1,
            StaticSpawnPoints = new List<StaticSpawnPoint>
            {
                new()
                {
                    Name = "Spawn Point",
                    Position = new UnityEngine.Vector3(1f, 1f, 1f),
                    Chance = 100
                }
            }
        };

        [Description("Schematic name for SCP-999 (It will look for MapEditorReborn/Schematics/<name>/<name>.json)")]
        public string Schematic { get; set; } = "999";

        public override List<CustomAbility> CustomAbilities { get; set; } = new()
        {
            new Healthy(),
            new HealOthersOnHit(),
            new PassiveRegenerateHealth(),
        };

        /// <summary>
        /// All unneeded attributes that are part of the default custom role class but aren't used here. Ignored on YAML because these values are unused in here
        /// </summary>
        [YamlIgnore] public override Dictionary<AmmoType, ushort> Ammo { get; set; } = default;
        [YamlIgnore] public override Dictionary<RoleTypeId, float> CustomRoleFFMultiplier { get; set; } = default;
        [YamlIgnore] public override bool DisplayCustomItemMessages { get; set; } = default;
        [YamlIgnore] public override bool IgnoreSpawnSystem { get; set; } = default;
        [YamlIgnore] public override List<string> Inventory { get; set; } = default;
        [YamlIgnore] public override bool KeepInventoryOnSpawn { get; set; } = default;
        [YamlIgnore] public override bool KeepPositionOnSpawn { get; set; } = default;
        [YamlIgnore] public override bool KeepRoleOnChangingRole { get; set; } = default;
        [YamlIgnore] public override bool KeepRoleOnDeath { get; set; } = default;
        [YamlIgnore] public override bool RemovalKillsPlayer { get; set; } = default;
        [YamlIgnore] public override float SpawnChance { get; set; } = default;

        private CoroutineHandle coro;

        protected override void SubscribeEvents()
        {
            PlayerEvent.Spawned += OnSpawned;
            PlayerEvent.Dying += OnDying;
            PlayerEvent.ChangingRole += OnChangingRole;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            PlayerEvent.Spawned -= OnSpawned;
            PlayerEvent.Dying -= OnDying;
            PlayerEvent.ChangingRole -= OnChangingRole;
            base.UnsubscribeEvents();
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player == null) return;

            // Handles setting values on spawn
            if (Check(ev.Player))
            {
                ev.Player.HumeShield = HumeShield;
                ev.Player.EnableEffect<Invisible>();
                ev.Player.EnableEffect<MovementBoost>(Speed);

                try
                {
                    // spawn schematic and assign it's parent as the player to follow the player
                    SchematicObject sch = ObjectSpawner.SpawnSchematic(Schematic, ev.Player.Position, ev.Player.Rotation, ev.Player.Scale, null, false);

                    sch.transform.SetParent(ev.Player.GameObject.transform);

                    // fixes issue with schematic being too high
                    sch.transform.localPosition = new Vector3(0, 0 - Scale.y, 0);

                    coro = Timing.RunCoroutine(AnimationHandler(ev.Player, sch));
                }
                catch
                {
                    Log.Error($"Error spawning/assigning {Schematic} schematic!");
                }
            }
        }

        private void OnDying(DyingEventArgs ev)
        {
            if (ev.Player == null) return;

            // Handles sending the cassie message on death
            if (Check(ev.Player))
            {
                Cassie.MessageTranslated(CassieAnnouncementOnDeath, CassieSubtitlesOnDeath, true, true, true);
                ev.Player.DisableEffect<Invisible>();

                API.SpawnedObjects.FirstOrDefault(s => s.name == Schematic)?.Destroy();

                if (coro.IsRunning) { Timing.KillCoroutines(coro); }
            }
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;

            if (Check(ev.Player))
            {
                ev.Player.DisableEffect<Invisible>();

                API.SpawnedObjects.FirstOrDefault(s => s.name == Schematic)?.Destroy();

                if (coro.IsRunning) { Timing.KillCoroutines(coro); }
            }
        }

        private IEnumerator<float> AnimationHandler(Player player, SchematicObject sch)
        {
            yield return Timing.WaitForSeconds(0.1f);
            for (; ;)
            {
                if (player.GameObject.GetComponent<Rigidbody>().velocity.magnitude < 0.1f && !sch.AnimationController.Equals("999Idle"))
                {
                    sch.AnimationController.Play("999Idle");
                }
                else if (player.GameObject.GetComponent<Rigidbody>().velocity.magnitude >= 0.1f && !sch.AnimationController.Equals("999Run"))
                {
                    sch.AnimationController.Play("999Run");
                }
            }
        }
    }
}
