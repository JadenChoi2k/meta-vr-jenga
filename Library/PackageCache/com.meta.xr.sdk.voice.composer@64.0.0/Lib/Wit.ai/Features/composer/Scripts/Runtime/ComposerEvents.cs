/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using Meta.WitAi.Composer.Data;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.WitAi.Composer
{
    [Serializable]
    public class ComposerEvents
    {
        [Header("Session Events")]
        /// <summary>
        /// Callback for session begin
        /// </summary>
        public ComposerSessionEvent OnComposerSessionBegin;
        /// <summary>
        /// Callback for manual session end
        /// </summary>
        public ComposerSessionEvent OnComposerSessionEnd;

        [Header("Setup Events")]
        /// <summary>
        /// Callback when active set changes
        /// </summary>
        public ComposerActiveEvent OnComposerActiveChange;
        /// <summary>
        /// Callback for the setting of context data
        /// </summary>
        public ComposerSessionEvent OnComposerContextMapChange;
        /// <summary>
        /// Composer service activated
        /// </summary>
        public ComposerSessionEvent OnComposerActivation;
        /// <summary>
        /// Composer service request initialized
        /// </summary>
        public ComposerSessionEvent OnComposerRequestInit;

        [Header("Response Events")]
        /// <summary>
        /// Composer service request began
        /// </summary>
        public ComposerSessionEvent OnComposerRequestBegin;
        /// <summary>
        /// Callback a composer response
        /// </summary>
        public ComposerSessionEvent OnComposerResponse;
        /// <summary>
        /// Callback for when a composer response fails
        /// </summary>
        public ComposerSessionEvent OnComposerError;
        /// <summary>
        /// Callback for when a composer response canceled
        /// </summary>
        public ComposerSessionEvent OnComposerCanceled;

        [Header("Handler Events")]
        /// <summary>
        /// Callback for when a composer graph expects input
        /// </summary>
        public ComposerSessionEvent OnComposerExpectsInput;
        /// <summary>
        /// Callback for when a composer graph will read a phrase
        /// </summary>
        public ComposerSessionEvent OnComposerSpeakPhrase;
        /// <summary>
        /// Callback for when a composer graph should perform an action
        /// </summary>
        public ComposerSessionEvent OnComposerPerformAction;
        /// <summary>
        /// On composer graph completion.  This can be used to end the session
        /// or clear the context map.
        /// </summary>
        public ComposerSessionEvent OnComposerComplete;
    }

    /// <summary>
    /// Composer session data used to provide easier access to all classes
    /// involved in a composer event.
    /// </summary>
    [Serializable]
    public class ComposerSessionData
    {
        /// <summary>
        /// The current session id
        /// </summary>
        public string sessionID;

        /// <summary>
        /// The Composer graph version to use
        /// </summary>
        public string versionTag;

        /// <summary>
        /// The composer service used in this session
        /// </summary>
        public ComposerService composer;

        /// <summary>
        /// The composer's input context data associated with this session
        /// </summary>
        public ComposerContextMap contextMap;

        /// <summary>
        /// The composer's response data if applicable
        /// </summary>
        public ComposerResponseData responseData;
    }

    // Active change event
    [Serializable]
    public class ComposerActiveEvent : UnityEvent<ComposerService, bool> { }

    // Unity event that passes ComposerSessionData as a parameter
    [Serializable]
    public class ComposerSessionEvent : UnityEvent<ComposerSessionData> { }
}
