/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Meta.WitAi.Requests;

namespace Meta.WitAi.Composer.Interfaces
{
    public interface IComposerRequestHandler
    {
        /// <summary>
        /// Handles wrapping composer web request
        /// setup prior to performing request
        /// </summary>
        /// <param name="sessionData">Session data including composer, voice service, and more</param>
        /// <param name="request">Request used to perform composer functionality if desired</param>
        void OnComposerRequestSetup(ComposerSessionData sessionData, VoiceServiceRequest request);
    }
}
