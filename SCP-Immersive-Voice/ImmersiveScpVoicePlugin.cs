using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using Mirror;
using SCP_Immersive_Voice.AudioProcessing.Interfaces;
using SCP_Immersive_Voice.Managers;
using SCP_Immersive_Voice.VoiceProfiles;
using ScpImmersiveVoice.Config;
using ScpImmersiveVoice.EventHandlers;
using ScpImmersiveVoice.Patches;
using System;
using UnityEngine;
using Logger = LabApi.Extensions.Misc.iLogger;

namespace ScpImmersiveVoice
{
    /// <summary>
    /// The primary framework lifecycle controller for SCP Immersive Voice.
    /// Governs modular decoupled component boundaries and dynamic sub-feature state transitions.
    /// </summary>
    public class ImmersiveScpVoicePlugin : Plugin<ImmersiveScpVoiceConfig>
    {
        #region Plugin Metadata
        public override string Name => "SCP Immersive Voice";
        public override string Description => "Enables proximity voice chat for SCPs and adds real-time audio DSP effects.";
        public override string Author => "iomatix";
        public override Version Version => new(2, 1, 2);
        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
        public override LoadPriority Priority { get; } = LoadPriority.High;
        #endregion

        #region Core Subsystem Handlers
        internal CoreVoiceHandler CoreVoiceHandler { get; private set; }
        internal ScpVoiceManager VoiceManager { get; private set; }
        #endregion

        #region Polymorphic Subsystem Registry Matrix
        private IScpAudioSubsystem[] _subsystems;
        #endregion

        #region Infrastructure Fields
        private Harmony _harmony;
        private GameObject _updaterObject;
        #endregion

        #region Global Shared States
        public static ImmersiveScpVoiceConfig StaticConfig { get; private set; }
        public static bool IsEnabled { get; private set; }
        public static bool IsDebug { get; private set; }
        #endregion

        #region Lifecycle Hooks
        public override void Enable()
        {
            StaticConfig = Config;
            IsDebug = Config?.Debug ?? false;

            _harmony = new Harmony("scp.immersive.voice.opus.patch");

            // Execute dynamic low-level byte patching safely
            _harmony.CreateClassProcessor(typeof(OpusEncoderPatch)).Patch();
            _harmony.CreateClassProcessor(typeof(OpusDecoderPatch)).Patch();

            // Construct infrastructure dependencies
            VoiceManager = new ScpVoiceManager();
            CoreVoiceHandler = new CoreVoiceHandler(Config, VoiceManager);
            ScpVoiceProfiles.VoiceManagerInstance = VoiceManager;

            // COMPOSITE PATTERN: Registering individual handlers polimorphically inside a flat registry array.
            // Eliminates OCP violations; adding a new SCP type requires 0 changes to this class framework structure.
            _subsystems = new IScpAudioSubsystem[]
            {
                new Scp096AudioHandler(),
                new Scp939AudioHandler(),
                new Scp3114AudioHandler(),
                new Scp106AudioHandler()
            };

            // Bind systemic engine environment lifecycles
            ServerEvents.RoundStarted += OnRoundStart;
            ServerEvents.RoundEnded += OnRoundEnd;

            // Global entity state and routing listeners
            PlayerEvents.ChangedRole += OnChangedRole;
            PlayerEvents.Left += OnPlayerLeft;
            PlayerEvents.Death += OnPlayerDied;
            PlayerEvents.SendingVoiceMessage += CoreVoiceHandler.OnSendingVoiceMessage;

            // Instantiate hardware clock updater component cleanly
            if (_updaterObject is null)
            {
                _updaterObject = new GameObject("ScpVoiceUpdater");
                GameObject.DontDestroyOnLoad(_updaterObject);

                var updater = _updaterObject.AddComponent<ScpVoiceUpdater>();
                updater.Init(VoiceManager);
            }

            // Execute automated sequential pipeline attachments
            int count = _subsystems.Length;
            for (int i = 0; i < count; i++)
            {
                _subsystems[i]?.BindPipelines();
            }

            Logger.Info(Name, $"Modular Voice Engine Successfully Online - v{Version} by {Author}");
        }

        public override void Disable()
        {
            _harmony?.UnpatchAll("scp.immersive.voice.opus.patch");

            // Detach macro server tracking lifecycles
            ServerEvents.RoundStarted -= OnRoundStart;
            ServerEvents.RoundEnded -= OnRoundEnd;

            PlayerEvents.ChangedRole -= OnChangedRole;
            PlayerEvents.Left -= OnPlayerLeft;
            PlayerEvents.Death -= OnPlayerDied;

            if (CoreVoiceHandler is not null)
                PlayerEvents.SendingVoiceMessage -= CoreVoiceHandler.OnSendingVoiceMessage;

            // Execute automated sequential pipeline tear-downs via flat array loops
            if (_subsystems is not null)
            {
                int count = _subsystems.Length;
                for (int i = 0; i < count; i++)
                {
                    _subsystems[i]?.UnbindPipelines();
                }
                _subsystems = null;
            }

            // Evict background updater thread component from active Unity heap
            if (_updaterObject is not null)
            {
                GameObject.Destroy(_updaterObject);
                _updaterObject = null;
            }

            ScpVoiceProfiles.DynamicProviders.Clear();
            VoiceManager?.StopAllSessions();

            // Explicitly dereference core components to permit immediate clean garbage collection
            CoreVoiceHandler = null;
            VoiceManager = null;
            StaticConfig = null;
            IsEnabled = false;

            Logger.Info(Name, "Modular Voice Engine Safely Forced Offline.");
        }
        #endregion

        #region Event Handlers
        private static void OnRoundStart() => IsEnabled = true;
        private static void OnRoundEnd(RoundEndedEventArgs ev) => IsEnabled = false;

        private void OnChangedRole(PlayerChangedRoleEventArgs ev) => PurgePlayerContext(ev?.Player);
        private void OnPlayerLeft(PlayerLeftEventArgs ev) => PurgePlayerContext(ev?.Player);
        private void OnPlayerDied(PlayerDeathEventArgs ev) => PurgePlayerContext(ev?.Player);

        /// <summary>
        /// Consolidated dynamic procedural cleanup iteration sweep over active registry modules.
        /// Guaranteed zero allocation cost across high-frequency entity lifecycle swaps.
        /// </summary>
        private void PurgePlayerContext(Player player)
        {
            if (player is null) return;

            VoiceManager?.StopSession(player);
            ScpVoiceProfiles.ClearCacheFor(player);

            if (_subsystems is null) return;

            // Linear traversal across abstractions ensures maximum speed and total code isolation
            int count = _subsystems.Length;
            for (int i = 0; i < count; i++)
            {
                _subsystems[i]?.PurgePlayer(player);
            }
        }
        #endregion
    }
}