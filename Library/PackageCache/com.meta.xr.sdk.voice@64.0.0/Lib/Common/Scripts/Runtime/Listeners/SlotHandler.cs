using System;
using Oculus.Assistant.VoiceCommand.Data;
using Oculus.Voice.Core.Utilities;
using UnityEngine;
using UnityEngine.Events;


namespace Oculus.Assistant.VoiceCommand.Listeners
{
    [Serializable]
    public class SlotHandler
    {
        [Tooltip("The name of the slot to listen for")]
        public string slotName;
        public OnCommandSlotReceived onCommandSlotReceived = new OnCommandSlotReceived();
        public override string ToString()
        {
            return slotName;
        }
    }

    [Serializable]
    public class VoiceCommandResultHandler : VoiceCommandListener
    {
        public Configuration.VoiceCommand voiceCommand;
        public VoiceCommandCallbackEvent onVoiceCommandReceived = new VoiceCommandCallbackEvent();
        [ArrayElementTitle("slotName", "Unassigned Slot")]
        public SlotHandler[] slotHandlers = Array.Empty<SlotHandler>();

        public void OnCallback(VoiceCommandResult result)
        {
            if (voiceCommand.actionId == result.ActionId)
            {
                onVoiceCommandReceived.Invoke(result);
                foreach (var slotHandler in slotHandlers)
                {
                    if (result.TryGetSlot(slotHandler.slotName, out string value))
                    {
                        slotHandler.onCommandSlotReceived.Invoke(value);
                    }
                }
            }
        }
    }


    #region Callback Events

    [Serializable]
    public class OnCommandSlotReceived : UnityEvent<string>
    {
    }

    [Serializable]
    public class VoiceCommandCallbackEvent : UnityEvent<VoiceCommandResult>
    {
    }

    #endregion
}
