namespace ScpImmersiveVoice
{
    using HarmonyLib;
    using LabApi.Events;
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Events.Arguments.ServerEvents;
    using LabApi.Events.Handlers;
    using LabApi.Features;
    using LabApi.Features.Console;
    using LabApi.Loader.Features.Plugins;
    using LabApi.Loader.Features.Plugins.Enums;
    using SCP_Immersive_Voice.Managers;
    using SCP_Immersive_Voice.Presets.Dynamics;
    using SCP_Immersive_Voice.VoiceProfiles;
    using ScpImmersiveVoice.Config;
    using ScpImmersiveVoice.EventHandlers;
    using System;
    using UnityEngine;

    public class ImmersiveScpVoicePlugin : Plugin<ImmersiveScpVoiceConfig>
    {
        #region Plugin Metadata
        public override string Name => "SCP Voice Chat";
        public override string Description => "Enables proximity voice chat for SCPs and adds audio effects";
        public override string Author => "iomatix";
        public override Version Version => new Version(0, 6, 1);
        public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);
        #endregion

        public override LoadPriority Priority { get; } = LoadPriority.High;

        #region Handlers and Managers
        private ScpVoiceEventHandler _eventHandler;
        private ScpVoiceManager _voiceManager;
        #endregion

        #region Harmony
        private Harmony _harmony;
        #endregion

        #region Unity Objects
        private GameObject _updaterObject;
        #endregion

        public static ImmersiveScpVoiceConfig StaticConfig { get; private set; }
        private static bool _isEnabled = false;
        public static bool IsEnabled { get { return _isEnabled; } }

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
            _harmony = new Harmony("scp.immersive.voice.opus.patch");
            _harmony.PatchAll();

            StaticConfig = Config;
            _voiceManager = new ScpVoiceManager();
            _eventHandler = new ScpVoiceEventHandler(Config, _voiceManager);

            ServerEvents.RoundStarted += OnRoundStart;
            ServerEvents.RoundEnded += OnRoundEnd;

            #region Player Events for Cleanup (LOCAL)
            PlayerEvents.ChangedRole += OnChangedRole;
            PlayerEvents.Left += OnPlayerLeft;
            PlayerEvents.Death += OnPlayerDied;
            #endregion

            #region Initialize Updater
            if (_updaterObject == null)
            {
                _updaterObject = new GameObject("ScpVoiceUpdater");
                GameObject.DontDestroyOnLoad(_updaterObject);

                var updater = _updaterObject.AddComponent<ScpVoiceUpdater>();
                updater.Init(_voiceManager);
            }
            #endregion

            PlayerEvents.SendingVoiceMessage += _eventHandler.OnSendingVoiceMessage;
            //PlayerEvents.ReceivingVoiceMessage += _eventHandler.OnReceivingVoiceMessage;
            #region SCP Event Handlers
            Scp096Events.Enraging += _eventHandler.On096Enraging;
            Scp096Events.Enraged += _eventHandler.On096Enraged;
            Scp096Events.StartCrying += _eventHandler.On096StartingCrying;
            Scp096Events.StartedCrying += _eventHandler.On096StartedCrying;
            Scp096Events.TryingNotToCry += _eventHandler.On096TryingNotToCry;
            Scp096Events.Charging += _eventHandler.On096Charging;
            Scp096Events.Charged += _eventHandler.On096Charged;
            ScpVoiceProfiles.DynamicProviders.Add(
                new Scp096DynamicPresetProvider(_eventHandler.Scp096States)
            );

            Scp939Events.MimickingEnvironment += _eventHandler.On939MimickingEnvironment;
            Scp939Events.MimickedEnvironment += _eventHandler.On939MimickedEnvironment;
            Scp939Events.Focused += _eventHandler.On939Focused;
            Scp939Events.Attacking += _eventHandler.On939Attacking;
            Scp939Events.Attacked += _eventHandler.On939Attacked;
            Scp939Events.Lunging += _eventHandler.On939Lunging;
            Scp939Events.Lunged += _eventHandler.On939Lunged;
            Scp939Events.CreatingAmnesticCloud += _eventHandler.On939CreatingAmnesticCloud;
            Scp939Events.CreatedAmnesticCloud += _eventHandler.On939CreatedAmnesticCloud;
            ScpVoiceProfiles.DynamicProviders.Add(
                new Scp939DynamicPresetProvider(_eventHandler.Scp939States)
            );

            Scp3114Events.Disguising += _eventHandler.On3114Disguising;
            Scp3114Events.Disguised += _eventHandler.On3114Disguised;
            Scp3114Events.Revealing += _eventHandler.On3114Revealing;
            Scp3114Events.Revealed += _eventHandler.On3114Revealed;
            Scp3114Events.StrangleStarting += _eventHandler.On3114StrangleStarting;
            Scp3114Events.StrangleStarted += _eventHandler.On3114StrangleStarted;
            Scp3114Events.StrangleAborting += _eventHandler.On3114StrangleAborting;
            Scp3114Events.StrangleAborted += _eventHandler.On3114StrangleAborted;
            ScpVoiceProfiles.DynamicProviders.Add(
                new Scp3114DynamicPresetProvider(_eventHandler.Scp3114States)
            );
            #endregion

            LabApi.Features.Console.Logger.Info("[SCP Voice Chat] - Plugin Enabled");

        }

        public override void Disable()
        {
            _harmony?.UnpatchAll("scp.immersive.voice.opus.patch");

            if (_eventHandler != null)
            {
                ServerEvents.RoundStarted -= OnRoundStart;
                ServerEvents.RoundEnded -= OnRoundEnd;

                #region Player Events for Cleanup
                PlayerEvents.ChangedRole -= OnChangedRole;
                PlayerEvents.Left -= OnPlayerLeft;
                PlayerEvents.Death -= OnPlayerDied;
                #endregion

                #region Disable Updater
                if (_updaterObject != null)
                {
                    GameObject.Destroy(_updaterObject);
                    _updaterObject = null;
                }
                #endregion

                PlayerEvents.SendingVoiceMessage -= _eventHandler.OnSendingVoiceMessage;
                //PlayerEvents.ReceivingVoiceMessage -= _eventHandler.OnReceivingVoiceMessage;

                #region SCP Event Handlers
                Scp096Events.Enraging -= _eventHandler.On096Enraging;
                Scp096Events.Enraged -= _eventHandler.On096Enraged;
                Scp096Events.StartCrying -= _eventHandler.On096StartingCrying;
                Scp096Events.StartedCrying -= _eventHandler.On096StartedCrying;
                Scp096Events.TryingNotToCry -= _eventHandler.On096TryingNotToCry;
                Scp096Events.Charging -= _eventHandler.On096Charging;
                Scp096Events.Charged -= _eventHandler.On096Charged;

                Scp939Events.MimickingEnvironment -= _eventHandler.On939MimickingEnvironment;
                Scp939Events.MimickedEnvironment -= _eventHandler.On939MimickedEnvironment;
                Scp939Events.Focused -= _eventHandler.On939Focused;
                Scp939Events.Attacking -= _eventHandler.On939Attacking;
                Scp939Events.Attacked -= _eventHandler.On939Attacked;
                Scp939Events.Lunging -= _eventHandler.On939Lunging;
                Scp939Events.Lunged -= _eventHandler.On939Lunged;
                Scp939Events.CreatingAmnesticCloud -= _eventHandler.On939CreatingAmnesticCloud;
                Scp939Events.CreatedAmnesticCloud -= _eventHandler.On939CreatedAmnesticCloud;

                Scp3114Events.Disguising -= _eventHandler.On3114Disguising;
                Scp3114Events.Disguised -= _eventHandler.On3114Disguised;
                Scp3114Events.Revealing -= _eventHandler.On3114Revealing;
                Scp3114Events.Revealed -= _eventHandler.On3114Revealed;
                Scp3114Events.StrangleStarting -= _eventHandler.On3114StrangleStarting;
                Scp3114Events.StrangleStarted -= _eventHandler.On3114StrangleStarted;
                Scp3114Events.StrangleAborting -= _eventHandler.On3114StrangleAborting;
                Scp3114Events.StrangleAborted -= _eventHandler.On3114StrangleAborted;
                #endregion

                _voiceManager.StopAllSessions();
                _eventHandler = null;
                _voiceManager = null;
            }
        }

        #region Local Event Handlers
        #region Exit/Death Player Events
        private void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            _voiceManager.StopSession(ev.Player);
        }

        private void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            if (ev.Player == null) return;

            _voiceManager.StopSession(ev.Player);
        }

        private void OnPlayerDied(PlayerDeathEventArgs ev)
        {
            _voiceManager.StopSession(ev.Player);
        }

        #endregion
        #endregion
    }
}