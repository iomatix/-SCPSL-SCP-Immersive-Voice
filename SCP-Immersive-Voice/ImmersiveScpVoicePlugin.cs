using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Extensions.Plugin;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using Mirror;
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
    /// Manages patching vectors, modular event routing domains, and thread-safe dynamic graph allocations.
    /// </summary>
    public class ImmersiveScpVoicePlugin : Plugin<ImmersiveScpVoiceConfig>
    {
        #region Plugin Metadata
        public override string Name => "SCP Immersive Voice";
        public override string Description => "Enables proximity voice chat for SCPs and adds real-time audio DSP effects.";
        public override string Author => "iomatix";
        public override Version Version => new(2, 0, 0);
        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
        public override LoadPriority Priority { get; } = LoadPriority.High;
        #endregion

        #region Subsystem Properties
        internal CoreVoiceHandler CoreVoiceHandler { get; private set; }
        internal Scp096AudioHandler Scp096AudioHandler { get; private set; }
        internal Scp939AudioHandler Scp939AudioHandler { get; private set; }
        internal Scp3114AudioHandler Scp3114AudioHandler { get; private set; }
        internal Scp106AudioHandler Scp106AudioHandler { get; private set; }
        internal ScpVoiceManager VoiceManager { get; private set; }
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

            // FLUENT API ALIGNMENT:
            // Constructing modular subsystem dependencies potokowo straight through the PluginBuilder engine.
            PluginBuilder.Create(this)
                .InitializeModule(() => VoiceManager = new ScpVoiceManager())
                .InitializeModule(() => CoreVoiceHandler = new CoreVoiceHandler(Config, VoiceManager))
                .InitializeModule(() => Scp096AudioHandler = new Scp096AudioHandler())
                .InitializeModule(() => Scp939AudioHandler = new Scp939AudioHandler())
                .InitializeModule(() => Scp3114AudioHandler = new Scp3114AudioHandler())
                .InitializeModule(() => Scp106AudioHandler = new Scp106AudioHandler());

            ScpVoiceProfiles.VoiceManagerInstance = VoiceManager;

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

            RegisterScpFeaturePipelines();

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

            DismantleScpFeaturePipelines();

            // Evict background updater thread component from active Unity heap
            if (_updaterObject is not null)
            {
                GameObject.Destroy(_updaterObject);
                _updaterObject = null;
            }

            ScpVoiceProfiles.DynamicProviders.Clear();
            VoiceManager?.StopAllSessions();

            // Explicitly dereference subsystems to permit immediate clean garbage collection
            CoreVoiceHandler = null;
            Scp096AudioHandler = null;
            Scp939AudioHandler = null;
            Scp3114AudioHandler = null;
            Scp106AudioHandler = null;
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
        /// Consolidated triple-duplicated procedural cleanup loops into a single atomic eviction pipeline.
        /// Guaranteed execution safety across high-frequency role swaps, disconnects, and combat kills.
        /// </summary>
        private void PurgePlayerContext(Player player)
        {
            if (player is null) return;

            VoiceManager?.StopSession(player);
            ScpVoiceProfiles.ClearCacheFor(player);

            // Evict target entity parameters across individual anomaly handler domains instantly
            Scp096AudioHandler?.Manager?.RemovePlayer(player);
            Scp939AudioHandler?.Manager?.RemovePlayer(player);
            Scp3114AudioHandler?.Manager?.RemovePlayer(player);
            Scp106AudioHandler?.Manager?.RemovePlayer(player);
        }
        #endregion

        #region Pipeline Configurations
        private void RegisterScpFeaturePipelines()
        {
            // --- SCP-096 Pipeline Registration ---
            Scp096Events.ChangedState += Scp096AudioHandler.On096ChangedState;
            Scp096Events.Charging += Scp096AudioHandler.On096Charging;
            Scp096Events.Charged += Scp096AudioHandler.On096Charged;
            Scp096Events.PryingGate += Scp096AudioHandler.On096PryingGate;
            Scp096Events.PriedGate += Scp096AudioHandler.On096PriedGate;
            ScpVoiceProfiles.DynamicProviders.Enqueue(Scp096AudioHandler.Manager);

            // --- SCP-939 Pipeline Registration ---
            Scp939Events.MimickingEnvironment += Scp939AudioHandler.On939MimickingEnvironment;
            Scp939Events.MimickedEnvironment += Scp939AudioHandler.On939MimickedEnvironment;
            Scp939Events.Focused += Scp939AudioHandler.On939Focused;
            Scp939Events.Attacking += Scp939AudioHandler.On939Attacking;
            Scp939Events.Attacked += Scp939AudioHandler.On939Attacked;
            Scp939Events.Lunging += Scp939AudioHandler.On939Lunging;
            Scp939Events.Lunged += Scp939AudioHandler.On939Lunged;
            Scp939Events.CreatingAmnesticCloud += Scp939AudioHandler.On939CreatingAmnesticCloud;
            Scp939Events.CreatedAmnesticCloud += Scp939AudioHandler.On939CreatedAmnesticCloud;
            ScpVoiceProfiles.DynamicProviders.Enqueue(Scp939AudioHandler.Manager);

            // --- SCP-3114 Pipeline Registration ---
            Scp3114Events.Disguising += Scp3114AudioHandler.On3114Disguising;
            Scp3114Events.Disguised += Scp3114AudioHandler.On3114Disguised;
            Scp3114Events.Revealing += Scp3114AudioHandler.On3114Revealing;
            Scp3114Events.Revealed += Scp3114AudioHandler.On3114Revealed;
            Scp3114Events.StrangleStarting += Scp3114AudioHandler.On3114StrangleStarting;
            Scp3114Events.StrangleStarted += Scp3114AudioHandler.On3114StrangleStarted;
            Scp3114Events.StrangleAborting += Scp3114AudioHandler.On3114StrangleAborting;
            Scp3114Events.StrangleAborted += Scp3114AudioHandler.On3114StrangleAborted;
            ScpVoiceProfiles.DynamicProviders.Enqueue(Scp3114AudioHandler.Manager);

            // --- SCP-106 Pipeline Registration ---
            Scp106Events.ChangedStalkMode += Scp106AudioHandler.On106ChangedStalkMode;
            Scp106Events.ChangedVigor += Scp106AudioHandler.On106ChangedVigor;
            Scp106Events.TeleportingPlayer += Scp106AudioHandler.On106TeleportingPlayer;
            Scp106Events.UsingHunterAtlas += Scp106AudioHandler.On106UsingHunterAtlas;
            ScpVoiceProfiles.DynamicProviders.Enqueue(Scp106AudioHandler.Manager);
        }

        private void DismantleScpFeaturePipelines()
        {
            if (Scp096AudioHandler is not null)
            {
                Scp096Events.ChangedState -= Scp096AudioHandler.On096ChangedState;
                Scp096Events.Charging -= Scp096AudioHandler.On096Charging;
                Scp096Events.Charged -= Scp096AudioHandler.On096Charged;
                Scp096Events.PryingGate -= Scp096AudioHandler.On096PryingGate;
                Scp096Events.PriedGate -= Scp096AudioHandler.On096PriedGate;
            }

            if (Scp939AudioHandler is not null)
            {
                Scp939Events.MimickingEnvironment -= Scp939AudioHandler.On939MimickingEnvironment;
                Scp939Events.MimickedEnvironment -= Scp939AudioHandler.On939MimickedEnvironment;
                Scp939Events.Focused -= Scp939AudioHandler.On939Focused;
                Scp939Events.Attacking -= Scp939AudioHandler.On939Attacking;
                Scp939Events.Attacked -= Scp939AudioHandler.On939Attacked;
                Scp939Events.Lunging -= Scp939AudioHandler.On939Lunging;
                Scp939Events.Lunged -= Scp939AudioHandler.On939Lunged;
                Scp939Events.CreatingAmnesticCloud -= Scp939AudioHandler.On939CreatingAmnesticCloud;
                Scp939Events.CreatedAmnesticCloud -= Scp939AudioHandler.On939CreatedAmnesticCloud;
            }

            if (Scp3114AudioHandler is not null)
            {
                Scp3114Events.Disguising -= Scp3114AudioHandler.On3114Disguising;
                Scp3114Events.Disguised -= Scp3114AudioHandler.On3114Disguised;
                Scp3114Events.Revealing -= Scp3114AudioHandler.On3114Revealing;
                Scp3114Events.Revealed -= Scp3114AudioHandler.On3114Revealed;
                Scp3114Events.StrangleStarting -= Scp3114AudioHandler.On3114StrangleStarting;
                Scp3114Events.StrangleStarted -= Scp3114AudioHandler.On3114StrangleStarted;
                Scp3114Events.StrangleAborting -= Scp3114AudioHandler.On3114StrangleAborting;
                Scp3114Events.StrangleAborted -= Scp3114AudioHandler.On3114StrangleAborted;
            }

            if (Scp106AudioHandler is not null)
            {
                Scp106Events.ChangedStalkMode -= Scp106AudioHandler.On106ChangedStalkMode;
                Scp106Events.ChangedVigor -= Scp106AudioHandler.On106ChangedVigor;
                Scp106Events.TeleportingPlayer -= Scp106AudioHandler.On106TeleportingPlayer;
                Scp106Events.UsingHunterAtlas -= Scp106AudioHandler.On106UsingHunterAtlas;
            }
        }
        #endregion
    }
}