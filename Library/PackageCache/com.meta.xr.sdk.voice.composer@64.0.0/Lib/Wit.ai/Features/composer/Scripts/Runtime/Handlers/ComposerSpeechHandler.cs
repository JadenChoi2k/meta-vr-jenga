/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using Meta.WitAi.Composer.Data;
using Meta.WitAi.Composer.Interfaces;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.Composer.Handlers
{
    [Serializable]
    public struct ComposerSpeakerData
    {
        [SerializeField] [FormerlySerializedAs("speakerName")]
        public string SpeakerName;
        [SerializeField] [FormerlySerializedAs("speaker")]
        public TTSSpeaker Speaker;
    }

    public class ComposerSpeechHandler : MonoBehaviour, IComposerSpeechHandler
    {
        [Tooltip("If true, queues tts phrases and plays them back in order.  If false, stops previous phrase to play new phrases")]
        public bool queuePhrases = true;

        /// <summary>
        /// Key to be used with context map in order to determine speaker
        /// If this key is not in the context map, the first speaker index will be used
        /// If this key is found but no corresponding speaker is found,
        /// an error will be logged
        /// </summary>
        [SerializeField] [FormerlySerializedAs("speakerNameContextMapKey")]
        public string SpeakerNameContextMapKey = "wit_composer_speaker";

        /// <summary>
        /// The speakers that can be activated via composer
        /// </summary>
#if UNITY_2021_3_2 || UNITY_2021_3_3 || UNITY_2021_3_4 || UNITY_2021_3_5
        [NonReorderable]
#endif
        [SerializeField] [FormerlySerializedAs("_speakers")]
        public ComposerSpeakerData[] Speakers;

        // Uses TTS to read back the response phrase
        public void SpeakPhrase(ComposerSessionData sessionData)
        {
            // Get speaker
            string phrase = sessionData.responseData.responsePhrase;
            TTSSpeaker speaker = GetSpeaker(sessionData);
            if (speaker == null)
            {
                VLog.E($"Composer Speech Handler - No Speaker Found\nPhrase: {phrase}\nPartial: {sessionData.responseData.responseIsFinal}");
                return;
            }

            // Set override
            if (sessionData.responseData.responseTtsSettings != null)
            {
                speaker.SetVoiceOverride(sessionData.responseData.responseTtsSettings);
            }

            // Adds phrase to speech queue
            if (queuePhrases)
            {
                speaker.SpeakQueued(phrase);
            }
            // Speak phrase while interrupting previous
            else
            {
                speaker.Speak(phrase);
            }
        }

        // Return true while speaking
        public bool IsSpeaking(ComposerSessionData sessionData)
        {
            // Get speaker
            TTSSpeaker speaker = GetSpeaker(sessionData);
            if (speaker != null)
            {
                return speaker.IsLoading || speaker.IsSpeaking;
            }
            return false;
        }

        // Get speaker
        private TTSSpeaker GetSpeaker(ComposerSessionData sessionData)
        {
            // None if no speakers
            if (Speakers == null || Speakers.Length <= 0)
            {
                return null;
            }

            // Get speaker index
            int index = 0;

            // Find speaker name in context map
            string speakerName = GetSpeakerName(sessionData.contextMap);
            if (!string.IsNullOrEmpty(speakerName))
            {
                // Get index of provided speaker name
                index = Array.FindIndex(Speakers,
                    (s) => string.Equals(s.SpeakerName, speakerName, StringComparison.CurrentCultureIgnoreCase));

                // If speaker name not found in TTSSpeaker list, return null
                if (index == -1)
                {
                    return null;
                }
            }

            // Return speaker
            return Speakers[index].Speaker;
        }

        // Get speaker name
        public string GetSpeakerName(ComposerContextMap contextMap)
        {
            return contextMap == null || contextMap.Data == null ? null : contextMap.Data[SpeakerNameContextMapKey].Value;
        }
    }
}
