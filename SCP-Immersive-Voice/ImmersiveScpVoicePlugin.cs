namespace ScpImmersiveVoice
{
    using LabApi.Events.Handlers;
    using LabApi.Features;
    using LabApi.Loader.Features.Plugins;
    using LabApi.Loader.Features.Plugins.Enums;
    using SCP_Immersive_Voice.Presets.Dynamics;
    using SCP_Immersive_Voice.VoiceProfiles;
    using ScpImmersiveVoice.Config;
    using ScpImmersiveVoice.EventHandlers;
    using System;

    public class ImmersiveScpVoicePlugin : Plugin<ImmersiveScpVoiceConfig>
    {
        public override string Name => "SCP Voice Chat";
        public override string Description => "Enables proximity voice chat for SCPs and adds audio effects";
        public override string Author => "iomatix";
        public override Version Version => new Version(0, 1, 0);
        public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);

        public override LoadPriority Priority { get; } = LoadPriority.High;

        private ScpVoiceEventHandler _eventHandler;


        public override void Enable()
        {
            _eventHandler = new ScpVoiceEventHandler(Config);
            PlayerEvents.SendingVoiceMessage += _eventHandler.OnSendingVoiceMessage;
            PlayerEvents.ReceivingVoiceMessage += _eventHandler.OnReceivingVoiceMessage;
            
            Scp096Events.Enraged += _eventHandler.On096Enraged;
            Scp096Events.StartedCrying += _eventHandler.On096StartedCrying;
            Scp096Events.TryingNotToCry += _eventHandler.On096TryingNotToCry;
            Scp096Events.Charging += _eventHandler.On096Charging;

            ScpVoiceProfiles.DynamicProviders.Add(
                new Scp096DynamicPresetProvider(_eventHandler.Scp096States)
            );

        }

        public override void Disable()
        {
            if (_eventHandler != null)
            {
                PlayerEvents.SendingVoiceMessage -= _eventHandler.OnSendingVoiceMessage;
                PlayerEvents.ReceivingVoiceMessage -= _eventHandler.OnReceivingVoiceMessage;
                
                Scp096Events.Enraged -= _eventHandler.On096Enraged;
                Scp096Events.StartedCrying -= _eventHandler.On096StartedCrying;
                Scp096Events.TryingNotToCry -= _eventHandler.On096TryingNotToCry;
                Scp096Events.Charging -= _eventHandler.On096Charging;
                _eventHandler = null;
            }
        }
    }
}