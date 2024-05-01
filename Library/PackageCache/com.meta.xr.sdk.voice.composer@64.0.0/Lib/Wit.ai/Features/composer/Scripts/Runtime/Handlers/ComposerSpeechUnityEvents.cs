/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using Meta.WitAi.Attributes;
using Meta.WitAi.Composer.Data;
using Meta.WitAi.Composer.Integrations;
using Meta.WitAi.Composer.Interfaces;
using Meta.WitAi.Events;
using Meta.WitAi.Json;
using Meta.WitAi.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.WitAi.Composer.Handlers
{
    /// <summary>
    /// Decoded composer response class
    /// </summary>
    [Serializable] public class ComposerResponseDataEvent : UnityEvent<ComposerResponseData> {}

    /// <summary>
    /// Provides UnityEvents that can be subscribed to when Composer triggers speech based events.
    /// </summary>
    public class ComposerSpeechUnityEvents : MonoBehaviour, IComposerSpeechHandler
    {
        [TooltipBox("Events for receipt of partial transcriptions")]
        [SerializeField] private StringEvent onPartialText;
        [SerializeField] private WitObjectEvent onPartialTextResponse;
        [SerializeField] private ComposerResponseDataEvent onPartialComposerResponse;

        [TooltipBox("Events for receipt of full transcriptions")]
        [SerializeField] private StringEvent onFullText;
        [SerializeField] private WitObjectEvent onFullTextResponse;
        [SerializeField] private ComposerResponseDataEvent onFullComposerResponse;

        /// <summary>
        /// Speaks the specified phrase
        /// </summary>
        /// <param name="sessionData">Specified composer, context data and response data</param>
        /// <param name="partial">Whether the requested phrase is returned via an early response or a final response.</param>
        public void SpeakPhrase(ComposerSessionData sessionData)
        {
            var composerResponse = sessionData.responseData;
            string phrase = composerResponse.responsePhrase;
            if (!composerResponse.responseIsFinal)
            {
                onPartialComposerResponse?.Invoke(composerResponse);
                onPartialTextResponse?.Invoke(HandleResponse(sessionData, phrase));
                onPartialText.Invoke(phrase);
            }
            else
            {
                onFullComposerResponse?.Invoke(composerResponse);
                onFullTextResponse?.Invoke(HandleResponse(sessionData, phrase));
                onFullText.Invoke(phrase);
            }
        }

        /// <summary>
        /// Attempt to obtain a WitResponseClass
        /// </summary>
        private WitResponseClass HandleResponse(ComposerSessionData sessionData, string text)
        {
            var speech = sessionData.responseData.witResponse?.AsObject;
            if (null == speech)
            {
                speech = new WitResponseClass();
                speech[WitComposerConstants.RESPONSE_NODE_Q] = text;
            }
            return speech;
        }

        /// <summary>
        /// Whether the specific session data response is still being spoken
        /// </summary>
        /// <param name="sessionData">Specified composer, context data and response data</param>
        public bool IsSpeaking(ComposerSessionData sessionData)
        {
            return false;
        }
    }
}
