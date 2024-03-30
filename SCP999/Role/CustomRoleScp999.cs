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
using System;
using Exiled.API.Features.Roles;

namespace SCP999.Role
{
    [CustomRole(RoleTypeId.Scp0492)]
    public class CustomRoleScp999 : CustomRole
    {
        public override uint Id { get; set; } = 20;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492;
        public override int MaxHealth { get; set; } = 1000;
        public int HumeShield { get; set; } = 2500;

        public float HumeShieldRegenerationRate { get; set; } = 10;
        public float Chance { get; set; } = 15f;
        public override string Name { get; set; } = "SCP-999";
        public override string Description { get; set; } = "A large gelatinous mass of translucent orange slime, reacting with overwhelming elation with nearby players";
        public override string CustomInfo { get; set; } = "SCP-999";
        public override Vector3 Scale { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);
        [Description("The speed of SCP 999. Use any value from -255 to 255 (higher value = higher speed. Using a minus value will apply the Disabled & Sinkhole effect)")]
        public short Speed { get; set; } = 1;
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

        private CoroutineHandle coro;
        private Dictionary<Player, SchematicObject> schematics = new Dictionary<Player, SchematicObject>();
        public double nextHumeRegenRate;

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

        public override void AddRole(Player player)
        {
            if (Check(player))
            {
                Log.Info($"Player {player.Nickname} was assigned 999 but already had the role. Skipping AddRole...");
                return;
            }

            base.AddRole(player);
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player == null) return;

            // Handles setting values on spawn
            if (Check(ev.Player))
            {
                Timing.CallDelayed(0.25f, () => { ev.Player.HumeShield = HumeShield; });
                ev.Player.EnableEffect<Invisible>();

                if (Speed >= 0) ev.Player.EnableEffect<MovementBoost>((byte)Speed);
                else { ev.Player.EnableEffect<Disabled>((byte)Speed, 0, false); ev.Player.EnableEffect<Sinkhole>((byte)-Speed, 0, false); }

                try
                {
                    // spawn schematic and assign it's parent as the player to follow the player
                    SchematicObject sch = ObjectSpawner.SpawnSchematic(Schematic, ev.Player.Position, ev.Player.Rotation, ev.Player.Scale, null, false);
                    // API.SpawnedObjects.Add(sch);
                    schematics.Add(ev.Player, sch);

                    sch.transform.SetParent(ev.Player.GameObject.transform);

                    // fixes issue with schematic being too high
                    sch.transform.localPosition = new Vector3(0, 0 - 1, 0);

                    // coro = Timing.RunCoroutine(AnimationHandler(ev.Player).CancelWith(ev.Player.GameObject));
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

                // API.SpawnedObjects.FirstOrDefault(s => s.name == $"CustomSchematic-{Schematic}")?.Destroy();

                if (schematics.TryGetValue(ev.Player, out SchematicObject sch))
                {
                    sch?.Destroy();
                    schematics?.Remove(ev.Player);
                }

                if (coro.IsRunning) { Timing.KillCoroutines(coro); }
            }
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;

            if (Check(ev.Player))
            {
                ev.Player.DisableEffect<Invisible>();

                // API.SpawnedObjects.FirstOrDefault(s => s.name == $"CustomSchematic-{Schematic}")?.Destroy();

                if (schematics.TryGetValue(ev.Player, out SchematicObject sch))
                {
                    sch?.Destroy();
                    schematics?.Remove(ev.Player);
                }

                if (coro.IsRunning) { Timing.KillCoroutines(coro); }
            }
        }

        // experimental animation code
        /* private IEnumerator<float> AnimationHandler(Player player)
        {
            yield return Timing.WaitForSeconds(0.1f);
            for (; ;)
            {
                if (sch != null)
                {
                    try
                    {
                        if (player.Velocity.magnitude < 0.1f && !sch.AnimationController.Equals("999Idle"))
                        {
                            sch.AnimationController.Play("999Idle");
                        }
                        else if (player.Velocity.magnitude >= 0.1f && !sch.AnimationController.Equals("999Run"))
                        {
                            sch.AnimationController.Play("999Run");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error occured: {e.Message}");
                        Log.Error($"Stack Trace: {e.StackTrace}");
                        throw;
                    }
                }
            }
        } */
    }
}
