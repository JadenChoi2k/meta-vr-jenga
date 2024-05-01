// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using UnityEngine;

namespace Oculus.Assistant.VoiceCommand.Configuration
{
    [CreateAssetMenu(fileName = "Action-VoiceCommandActionName", menuName = "Voice SDK/Voice Command Action")]
    public class VoiceCommand : ScriptableObject
    {
        public string actionId;

        public virtual byte[] InputData { get; }

        public override string ToString()
        {
            return name;
        }
    }
}
