// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections.Generic;

namespace Oculus.Assistant.VoiceCommand.Data
{
    public class VoiceCommandResult
    {
        private string actionId;
        private byte[] commandOutput;
        private string utterance;
        private string interactionId;
        private string debugPhraseMatched;

        private Dictionary<string, string> slotValues;

        public string ActionId => actionId;
        public byte[] CommandOutput => commandOutput;
        public string InteractionId => interactionId;
        public string DebugPhraseMatched => debugPhraseMatched;
        public string Utterance => utterance;
        public Dictionary<string, string> MatchedSlots => slotValues;

        public abstract class Builder
        {
            public abstract string ActionId { get; }
            public abstract byte[] CommandOutput { get; }
            public abstract string InteractionId { get; }
            public abstract string DebugPhraseMatched { get; }
            public abstract string Utterance { get; }

            public abstract Dictionary<string, string> SlotValues { get; }

            public virtual VoiceCommandResult Build()
            {
                return new VoiceCommandResult()
                {
                    actionId = ActionId,
                    commandOutput = CommandOutput,
                    interactionId = InteractionId,
                    debugPhraseMatched = DebugPhraseMatched,
                    utterance = Utterance,
                    slotValues = SlotValues
                };
            }
        }

        public override string ToString()
        {
            var message = "{" +
                          "\n  actionId = " + actionId +
                          ",\n  commandOutput = " + (null != commandOutput ? commandOutput.Length + " bytes" : "null") +
                          ",\n  interactionId = " + interactionId +
                          ",\n  utterance = " + utterance +
                          ",\n  matchedSlots = [";

            foreach (var slot in slotValues)
            {
                message += "\n    " + slot.Key + " = " + slot.Value + ",";
            }

            message += "\n  ]";
            return message + "\n}";
        }

        public string this[string slotName]
        {
            get => slotValues.TryGetValue(slotName, out var slotValue) ? slotValue : null;
        }

        public bool TryGetSlot(string slotName, out string slotValue)
        {
            return slotValues.TryGetValue(slotName, out slotValue);
        }

        public bool HasSlot(string slotName)
        {
            return slotValues.ContainsKey(slotName);
        }
    }
}
