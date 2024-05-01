/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Meta.WitAi.Composer.Data;
using Meta.WitAi.Json;
using Meta.WitAi.TTS.Integrations;

namespace Meta.WitAi.Composer.Integrations
{
    public static class WitComposerResponseExtensions
    {
        /// <summary>
        /// Decode composer response data in order to ensure reduce WitResponseNode decoding
        /// </summary>
        public static ComposerResponseData GetComposerResponse(this WitResponseNode response)
        {
            ComposerResponseData composerResponseData = new ComposerResponseData();
            composerResponseData.witResponse = response;
            composerResponseData.error = response.GetError();
            composerResponseData.expectsInput = response.GetExpectsInput();
            composerResponseData.actionID = response.GetActionId();
            composerResponseData.responseIsFinal = response.GetIsResponseFinal();
            composerResponseData.responsePhrase = response.GetResponseText();
            composerResponseData.responseTts = response.GetTTS();
            composerResponseData.responseTtsSettings = response.GetTTSSettings();
            return composerResponseData;
        }

        /// <summary>
        /// Gets the 'expects_input' bool from the WitResponseNode if applicable
        /// </summary>
        public static bool GetExpectsInput(this WitResponseNode response)
        {
            return response?[WitComposerConstants.RESPONSE_NODE_EXPECTS_INPUT].AsBool ?? false;
        }

        /// <summary>
        /// Determines if action is final using 'is_final'
        /// </summary>
        public static bool GetIsActionFinal(this WitResponseNode response)
        {
            return response?.GetIsFinal() ?? false;
        }

        /// <summary>
        /// Gets the 'action' string from the WitResponseNode if applicable
        /// </summary>
        public static string GetActionId(this WitResponseNode response)
        {
            return response?[WitComposerConstants.RESPONSE_NODE_ACTION].Value ?? string.Empty;
        }

        /// <summary>
        /// Determines if final response exists or not
        /// </summary>
        public static bool GetIsResponseFinal(this WitResponseNode response)
        {
            return response?.GetFinalResponse() != null;
        }

        /// <summary>
        /// Returns written text meant to be displayed for accessibility or visual purposes.
        ///
        /// Note: This text can be used as a fallback for TTS, but if you are passing data to a TTS system you will
        /// more likely want to use GetSpeechTTS
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Returns the text or string.Empty if no text was found</returns>
        public static string GetResponseText(this WitResponseNode response)
        {
            return response?.GetResponse()?.SafeGet(WitComposerConstants.RESPONSE_NODE_TEXT)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Gets the speech node from a wit response.
        ///
        /// NOTE: this can either be a root response with a partial/final or the contents of that node.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>A speech object {"q": "something", "voice": "some voice", ...} or null.</returns>
        public static WitResponseClass GetSpeech(this WitResponseNode response)
        {
            return response?.GetResponse()?.SafeGet(WitComposerConstants.RESPONSE_NODE_SPEECH)?.AsObject ?? null;
        }

        /// <summary>
        /// Returns text that is optimized for TTS if present or the regular response text if no TTS optimized text is present.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Returns the text or string.Empty if no text was found</returns>
        public static string GetTTS(this WitResponseNode response)
        {
            return response?.GetSpeech()?.SafeGet(WitComposerConstants.RESPONSE_NODE_Q)?.Value ?? response.GetResponseText();
        }

        /// <summary>
        /// Finds the full TTS response object
        /// </summary>
        /// <param name="data">Data including voice preset data as well as TTS to convert to speech.</param>
        /// <returns>Returns null if no tts data was found in the hierarchy</returns>
        public static TTSWitVoiceSettings GetTTSSettings(this WitResponseNode response)
        {
            var speechObject = response?.GetSpeech();
            if (speechObject == null)
            {
                return null;
            }
            var settings = new TTSWitVoiceSettings();
            settings.Decode(speechObject);
            return settings;
        }
    }
}
