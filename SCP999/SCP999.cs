using Exiled.API.Features;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;
using PlayerRoles.Spectating;
using System;
using Exiled.Events.Handlers;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using SCP999.Handlers;
using HarmonyLib;
using System.Reflection;
using Interactables.Verification;

namespace SCP999
{
    public class SCP999 : Plugin<Config>
    {
        public override string Author => "@noahxo";
        public override string Name => "SCP999";
        public override string Prefix => Name;
        public override Version RequiredExiledVersion { get; } = new Version(8, 8, 0);
        public override Version Version { get; } = new Version(1, 0, 0);

        private Harmony _harmony;

        private string HarmonyId { get; } = "noahxo.dev";

        public static SCP999 Instance;

        public EventHandlers _handlers;

        public override void OnEnabled()
        {
            Instance = this;

            Config.LoadConfigs();

            Config.RoleConfigs.Scp999.Register();

            SCPSLAudioApi.Startup.SetupDependencies();
            SoundHandler.InitialiseDirectory();

            RegisterEvents();
            RegisterPatch();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;

            CustomRole.UnregisterRoles();

            UnregisterEvents();
            UnregisterPatch();

            base.OnDisabled();
        }

        private void RegisterEvents()
        {
            _handlers = new EventHandlers();

            Server.RoundStarted += _handlers.OnRoundStarted;
        }

        private void UnregisterEvents()
        {
            Server.RoundStarted -= _handlers.OnRoundStarted;

            _handlers = null;
        }

        private void RegisterPatch()
        {
            try
            {
                _harmony = new(HarmonyId);
                _harmony.PatchAll();
            }
            catch (HarmonyException ex)
            {
                Log.Error($"[RegisterPatch] Patching Failed : {ex}");
            }
        }

        private void UnregisterPatch()
        {
            _harmony.UnpatchAll();
            _harmony = null;
        }
    }
}
