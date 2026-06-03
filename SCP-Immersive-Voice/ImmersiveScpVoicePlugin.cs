namespace ScpImmersiveVoice
{
    using LabApi.Events;
    using LabApi.Events.Arguments.ServerEvents;
    using LabApi.Events.Handlers;
    using LabApi.Features;
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
        public override Version Version => new Version(0, 5, 0);
        public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);
        #endregion

        public override LoadPriority Priority { get; } = LoadPriority.High;

        #region Handlers and Managers
        private ScpVoiceEventHandler _eventHandler;
        private ScpVoiceManager _voiceManager;
        #endregion

        #region Unity Objects
        private GameObject _updaterObject;
        #endregion

        public static ImmersiveScpVoiceConfig StaticConfig { get; private set; }

        public void OnRoundStart()
        {
            Enable();
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Disable();
        }

        public override void Enable()
        {
            StaticConfig = Config;
            _eventHandler = new ScpVoiceEventHandler(Config);
            _voiceManager = new ScpVoiceManager();

            ServerEvents.RoundStarted += OnRoundStart;
            ServerEvents.RoundEnded += OnRoundEnd;

            #region Initialize Updater
            if (_updaterObject == null)
            {
                _updaterObject = new GameObject("ScpVoiceUpdater");
                GameObject.DontDestroyOnLoad(_updaterObject);
                _updaterObject.AddComponent<ScpVoiceUpdater>();
            }
            #endregion

            PlayerEvents.SendingVoiceMessage += _eventHandler.OnSendingVoiceMessage;
            PlayerEvents.ReceivingVoiceMessage += _eventHandler.OnReceivingVoiceMessage;
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



        }

        public override void Disable()
        {
            if (_eventHandler != null)
            {
                ServerEvents.RoundStarted -= OnRoundStart;
                ServerEvents.RoundEnded -= OnRoundEnd;

                #region Disable Updater
                if (_updaterObject != null)
                {
                    GameObject.Destroy(_updaterObject);
                    _updaterObject = null;
                }
                #endregion

                PlayerEvents.SendingVoiceMessage -= _eventHandler.OnSendingVoiceMessage;
                PlayerEvents.ReceivingVoiceMessage -= _eventHandler.OnReceivingVoiceMessage;

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

                _eventHandler = null;
            }
        }
    }
}