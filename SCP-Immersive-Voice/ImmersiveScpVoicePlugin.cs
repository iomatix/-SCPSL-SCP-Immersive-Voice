namespace ScpImmersiveVoice
{
    using HarmonyLib;
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.ServerEvents;
    using LabApi.Events.Handlers;
    using LabApi.Features;
    using LabApi.Loader.Features.Plugins;
    using LabApi.Loader.Features.Plugins.Enums;
    using SCP_Immersive_Voice.Managers;
    using SCP_Immersive_Voice.Presets.Dynamics;
    using SCP_Immersive_Voice.Presets.Dynamics.Providers;
    using SCP_Immersive_Voice.VoiceProfiles;
    using ScpImmersiveVoice.Config;
    using ScpImmersiveVoice.EventHandlers;
    using ScpImmersiveVoice.Patches;
    using System;
    using UnityEngine;

    public class ImmersiveScpVoicePlugin : Plugin<ImmersiveScpVoiceConfig>
    {
        #region Plugin Metadata
        public override string Name => "SCP Voice Chat";
        public override string Description => "Enables proximity voice chat for SCPs and adds audio effects";
        public override string Author => "iomatix";
        public override Version Version => new Version(1, 0, 0); // Promowany do stabilnej wersji produkcyjnej
        public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);
        #endregion

        public override LoadPriority Priority { get; } = LoadPriority.High;

        #region Granular AAA Handlers and Managers
        private CoreVoiceHandler _coreVoiceHandler;
        private Scp096AudioHandler _scp096AudioHandler;
        private Scp939AudioHandler _scp939AudioHandler;
        private Scp3114AudioHandler _scp3114AudioHandler;
        private Scp106AudioHandler _scp106AudioHandler;
        private ScpVoiceManager _voiceManager;
        #endregion

        #region Harmony Instance
        private Harmony _harmony;
        #endregion

        #region Unity Components
        private GameObject _updaterObject;
        #endregion

        #region Global Shared State
        public static ImmersiveScpVoiceConfig StaticConfig { get; private set; }
        private static bool _isEnabled = false;
        public static bool IsEnabled => _isEnabled;
        #endregion

        public void OnRoundStart()
        {
            _isEnabled = true;
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            _isEnabled = false;
        }

        public override void Enable()
        {
            StaticConfig = Config;
            _harmony = new Harmony("scp.immersive.voice.opus.patch");

            // Execute dynamic byte patching safely
            _harmony.CreateClassProcessor(typeof(OpusEncoderPatch)).Patch();
            _harmony.CreateClassProcessor(typeof(OpusDecoderPatch)).Patch();

            // Initialize underlying high-performance structures
            _voiceManager = new ScpVoiceManager();

            // Allocate separated handler context domains
            _coreVoiceHandler = new CoreVoiceHandler(Config, _voiceManager);
            _scp096AudioHandler = new Scp096AudioHandler();
            _scp939AudioHandler = new Scp939AudioHandler();
            _scp3114AudioHandler = new Scp3114AudioHandler();
            _scp106AudioHandler = new Scp106AudioHandler();

            // Establish engine lifecycle bindings
            ServerEvents.RoundStarted += OnRoundStart;
            ServerEvents.RoundEnded += OnRoundEnd;

            // Global connection security hooks
            PlayerEvents.ChangedRole += OnChangedRole;
            PlayerEvents.Left += OnPlayerLeft;
            PlayerEvents.Death += OnPlayerDied;

            // Core voice data pipeline routing hook
            PlayerEvents.SendingVoiceMessage += _coreVoiceHandler.OnSendingVoiceMessage;

            // Register background streaming thread behavior
            if (_updaterObject == null)
            {
                _updaterObject = new GameObject("ScpVoiceUpdater");
                GameObject.DontDestroyOnLoad(_updaterObject);

                var updater = _updaterObject.AddComponent<ScpVoiceUpdater>();
                updater.Init(_voiceManager);
            }

            #region Connect Structural Dynamic Providers and Handlers

            // --- SCP-096 Pipeline Config ---
            Scp096Events.Enraging += _scp096AudioHandler.On096Enraging;
            Scp096Events.Enraged += _scp096AudioHandler.On096Enraged;
            Scp096Events.StartCrying += _scp096AudioHandler.On096StartingCrying;
            Scp096Events.StartedCrying += _scp096AudioHandler.On096StartedCrying;
            Scp096Events.TryingNotToCry += _scp096AudioHandler.On096TryingNotToCry;
            Scp096Events.Charging += _scp096AudioHandler.On096Charging;
            Scp096Events.Charged += _scp096AudioHandler.On096Charged;
            ScpVoiceProfiles.DynamicProviders.Add(new Scp096DynamicPresetProvider(_scp096AudioHandler));

            // --- SCP-939 Pipeline Config ---
            Scp939Events.MimickingEnvironment += _scp939AudioHandler.On939MimickingEnvironment;
            Scp939Events.MimickedEnvironment += _scp939AudioHandler.On939MimickedEnvironment;
            Scp939Events.Focused += _scp939AudioHandler.On939Focused;
            Scp939Events.Attacking += _scp939AudioHandler.On939Attacking;
            Scp939Events.Attacked += _scp939AudioHandler.On939Attacked;
            Scp939Events.Lunging += _scp939AudioHandler.On939Lunging;
            Scp939Events.Lunged += _scp939AudioHandler.On939Lunged;
            Scp939Events.CreatingAmnesticCloud += _scp939AudioHandler.On939CreatingAmnesticCloud;
            Scp939Events.CreatedAmnesticCloud += _scp939AudioHandler.On939CreatedAmnesticCloud;
            ScpVoiceProfiles.DynamicProviders.Add(new Scp939DynamicPresetProvider(_scp939AudioHandler.States));

            // --- SCP-3114 Pipeline Config ---
            Scp3114Events.Disguising += _scp3114AudioHandler.On3114Disguising;
            Scp3114Events.Disguised += _scp3114AudioHandler.On3114Disguised;
            Scp3114Events.Revealing += _scp3114AudioHandler.On3114Revealing;
            Scp3114Events.Revealed += _scp3114AudioHandler.On3114Revealed;
            Scp3114Events.StrangleStarting += _scp3114AudioHandler.On3114StrangleStarting;
            Scp3114Events.StrangleStarted += _scp3114AudioHandler.On3114StrangleStarted;
            Scp3114Events.StrangleAborting += _scp3114AudioHandler.On3114StrangleAborting;
            Scp3114Events.StrangleAborted += _scp3114AudioHandler.On3114StrangleAborted;
            ScpVoiceProfiles.DynamicProviders.Add(new Scp3114DynamicPresetProvider(_scp3114AudioHandler));

            // --- SCP-106 Pipeline Config ---
            Scp106Events.ChangedStalkMode += _scp106AudioHandler.On106ChangedStalkMode;
            Scp106Events.ChangedVigor += _scp106AudioHandler.On106ChangedVigor;
            Scp106Events.TeleportingPlayer += _scp106AudioHandler.On106TeleportingPlayer;
            Scp106Events.UsingHunterAtlas += _scp106AudioHandler.On106UsingHunterAtlas;
            ScpVoiceProfiles.DynamicProviders.Add(new Scp106DynamicPresetProvider(_scp106AudioHandler));

            #endregion

            LabApi.Features.Console.Logger.Info("[SCP Voice Chat] - Modular AAA Engine Successfully Online.");
        }

        public override void Disable()
        {
            // Execute absolute clean unpatching
            _harmony?.UnpatchAll("scp.immersive.voice.opus.patch");

            // Absolute unbinding of core execution matrix to guarantee zero memory leaks
            ServerEvents.RoundStarted -= OnRoundStart;
            ServerEvents.RoundEnded -= OnRoundEnd;

            PlayerEvents.ChangedRole -= OnChangedRole;
            PlayerEvents.Left -= OnPlayerLeft;
            PlayerEvents.Death -= OnPlayerDied;

            PlayerEvents.SendingVoiceMessage -= _coreVoiceHandler.OnSendingVoiceMessage;

            #region Dismantle SCP Event Handlers Safely

            // --- Unbind 096 ---
            if (_scp096AudioHandler != null)
            {
                Scp096Events.Enraging -= _scp096AudioHandler.On096Enraging;
                Scp096Events.Enraged -= _scp096AudioHandler.On096Enraged;
                Scp096Events.StartCrying -= _scp096AudioHandler.On096StartingCrying;
                Scp096Events.StartedCrying -= _scp096AudioHandler.On096StartedCrying;
                Scp096Events.TryingNotToCry -= _scp096AudioHandler.On096TryingNotToCry;
                Scp096Events.Charging -= _scp096AudioHandler.On096Charging;
                Scp096Events.Charged -= _scp096AudioHandler.On096Charged;
            }

            // --- Unbind 939 ---
            if (_scp939AudioHandler != null)
            {
                Scp939Events.MimickingEnvironment -= _scp939AudioHandler.On939MimickingEnvironment;
                Scp939Events.MimickedEnvironment -= _scp939AudioHandler.On939MimickedEnvironment;
                Scp939Events.Focused -= _scp939AudioHandler.On939Focused;
                Scp939Events.Attacking -= _scp939AudioHandler.On939Attacking;
                Scp939Events.Attacked -= _scp939AudioHandler.On939Attacked;
                Scp939Events.Lunging -= _scp939AudioHandler.On939Lunging;
                Scp939Events.Lunged -= _scp939AudioHandler.On939Lunged;
                Scp939Events.CreatingAmnesticCloud -= _scp939AudioHandler.On939CreatingAmnesticCloud;
                Scp939Events.CreatedAmnesticCloud -= _scp939AudioHandler.On939CreatedAmnesticCloud;
            }

            // --- Unbind 3114 ---
            if (_scp3114AudioHandler != null)
            {
                Scp3114Events.Disguising -= _scp3114AudioHandler.On3114Disguising;
                Scp3114Events.Disguised -= _scp3114AudioHandler.On3114Disguised;
                Scp3114Events.Revealing -= _scp3114AudioHandler.On3114Revealing;
                Scp3114Events.Revealed -= _scp3114AudioHandler.On3114Revealed;
                Scp3114Events.StrangleStarting -= _scp3114AudioHandler.On3114StrangleStarting;
                Scp3114Events.StrangleStarted -= _scp3114AudioHandler.On3114StrangleStarted;
                Scp3114Events.StrangleAborting -= _scp3114AudioHandler.On3114StrangleAborting;
                Scp3114Events.StrangleAborted -= _scp3114AudioHandler.On3114StrangleAborted;
            }

            // --- Unbind 106 ---
            if (_scp106AudioHandler != null)
            {
                Scp106Events.ChangedStalkMode -= _scp106AudioHandler.On106ChangedStalkMode;
                Scp106Events.ChangedVigor -= _scp106AudioHandler.On106ChangedVigor;
                Scp106Events.TeleportingPlayer -= _scp106AudioHandler.On106TeleportingPlayer;
                Scp106Events.UsingHunterAtlas -= _scp106AudioHandler.On106UsingHunterAtlas;
            }

            #endregion

            // Kill Unity background lifecycle clock
            if (_updaterObject != null)
            {
                GameObject.Destroy(_updaterObject);
                _updaterObject = null;
            }

            // Flush tracking structures completely
            ScpVoiceProfiles.DynamicProviders.Clear();
            _voiceManager?.StopAllSessions();

            // Dereference handlers to allow standard GC sweep
            _coreVoiceHandler = null;
            _scp096AudioHandler = null;
            _scp939AudioHandler = null;
            _scp3114AudioHandler = null;
            _scp106AudioHandler = null;
            _voiceManager = null;

            LabApi.Features.Console.Logger.Info("[SCP Voice Chat] - Modular AAA Engine Safely Offline.");
        }

        #region Local Event Router Methods (Dismantle Sessions)
        private void OnChaningRole(PlayerChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;
            _voiceManager?.StopSession(ev.Player);
            ScpVoiceProfiles.ClearCacheFor(ev.Player);
            _scp096AudioHandler?.OnChangingRole(ev);
            _scp939AudioHandler?.OnChangingRole(ev);
            _scp3114AudioHandler?.OnChangingRole(ev);
            _scp106AudioHandler?.OnChangingRole(ev);
        }

        private void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            if (ev.Player == null) return;
            _voiceManager?.StopSession(ev.Player);
            ScpVoiceProfiles.ClearCacheFor(ev.Player);
            _scp096AudioHandler?.OnPlayerDied(null); // Direct state wipe on leave
            _scp939AudioHandler?.OnPlayerDied(null);
            _scp3114AudioHandler?.OnPlayerDied(null);
            _scp106AudioHandler?.OnPlayerDied(null);
        }

        private void OnPlayerDied(PlayerDeathEventArgs ev)
        {
            if (ev.Player == null) return;
            _voiceManager?.StopSession(ev.Player);
            ScpVoiceProfiles.ClearCacheFor(ev.Player);
            _scp096AudioHandler?.OnPlayerDied(null);
            _scp939AudioHandler?.OnPlayerDied(null);
            _scp3114AudioHandler?.OnPlayerDied(null);
            _scp106AudioHandler?.OnPlayerDied(null);
        }
        #endregion
    }
}