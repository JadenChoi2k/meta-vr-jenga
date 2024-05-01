/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using Meta.WitAi.Json;
using Meta.WitAi.TTS.Data;

namespace Meta.WitAi.Composer.Data
{
    [Serializable]
    public class ComposerResponseData
    {
        /// <summary>
        /// Whether this response expects additional user input
        /// </summary>
        public bool expectsInput;

        /// <summary>
        /// The action id to be called automatically if desired
        /// </summary>
        public string actionID;

        /// <summary>
        /// Whether the response is from 'response' or 'partial_response'
        /// </summary>
        public bool responseIsFinal;

        /// <summary>
        /// Response text to be displayed for accessibility or visual purposes.
        /// </summary>
        public string responsePhrase;

        /// <summary>
        /// Response phrase returned from the composer for tts purposes
        /// </summary>
        public string responseTts;

        /// <summary>
        /// The voice settings to be used if desired
        /// </summary>
        public TTSVoiceSettings responseTtsSettings;

        /// <summary>
        /// Response for any errors
        /// </summary>
        public string error;

        /// <summary>
        /// The raw wit response
        /// </summary>
        [NonSerialized] public WitResponseNode witResponse;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ComposerResponseData() {}

        /// <summary>
        /// Error constructor
        /// </summary>
        public ComposerResponseData(string newError)
        {
            error = newError;
        }
    }
}
