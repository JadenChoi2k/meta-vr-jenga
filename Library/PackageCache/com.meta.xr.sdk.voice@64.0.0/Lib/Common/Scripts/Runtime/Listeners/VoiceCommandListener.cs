// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using Oculus.Assistant.VoiceCommand.Data;

namespace Oculus.Assistant.VoiceCommand.Listeners
{
    public interface VoiceCommandListener
    {
        void OnCallback(VoiceCommandResult result);
    }
}
