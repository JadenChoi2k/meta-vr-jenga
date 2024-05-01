/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

namespace Meta.WitAi.Composer.Interfaces
{
    public interface IComposerSpeechHandler
    {
        /// <summary>
        /// Speaks the specified phrase
        /// </summary>
        /// <param name="sessionData">Specified composer, context data and response data</param>
        void SpeakPhrase(ComposerSessionData sessionData);

        /// <summary>
        /// Whether the specific session data response is still being spoken
        /// </summary>
        /// <param name="sessionData">Specified composer, context data and response data</param>
        bool IsSpeaking(ComposerSessionData sessionData);
    }
}
