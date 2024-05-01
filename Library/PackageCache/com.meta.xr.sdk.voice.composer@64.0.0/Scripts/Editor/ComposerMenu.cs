/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Meta.WitAi;
using Meta.WitAi.Composer;
using Meta.WitAi.Composer.Handlers;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.TTS;
using Meta.WitAi.TTS.Integrations;
using Meta.WitAi.TTS.Utilities;

namespace Oculus.Voice.Composer
{
    public static class ComposerMenu
    {
        [MenuItem("Assets/Create/Voice SDK/Add App Composer Experience to Scene", false, 100)]
        public static void AddComposerServiceToScene()
        {
            // Get TTS Service
            #if COMPOSER_V46 || COMPOSER_V47
            TTSService ttsService = TTSEditorUtilities.CreateService(null);
            #else
            TTSService ttsService = TTSEditorUtilities.CreateService(null, true);
            #endif
            // Get TTS Speaker
            TTSSpeaker ttsSpeaker = TTSEditorUtilities.CreateSpeaker(null, ttsService);
            // Get voice service
            VoiceService voiceService = GetVoiceService();
            // Get composer service
            ComposerService composerService = CreateComposer(ttsSpeaker);

            // Apply composer
            composerService.VoiceService = voiceService;
            // Get wit configuration
            TTSWit ttsWit = ttsService.GetComponent<TTSWit>();
            WitConfiguration configuration = ttsWit?.RequestSettings.configuration;
            if (configuration == null)
            {
                // Get default wit configuration
                configuration = WitDataCreation.FindDefaultWitConfig();
                // Apply to tts service
                if (ttsWit != null)
                {
                    ttsWit.RequestSettings = new TTSWitRequestSettings()
                    {
                        configuration = configuration
                    };
                }
            }
            // Apply config to voice service if needed
            AppVoiceExperience voiceWit = voiceService.GetComponent<AppVoiceExperience>();
            if (configuration != null && voiceWit != null && voiceWit.RuntimeConfiguration?.witConfiguration == null)
            {
                voiceWit.RuntimeConfiguration = new WitRuntimeConfiguration()
                {
                    witConfiguration = configuration
                };
            }

            // Generate voice root if needed
            Transform voiceRoot = composerService.transform.parent;
            if (voiceRoot == null)
            {
                voiceRoot = GenerateGameObject("Voice").transform;
            }
            // Move into voice service's root if possible
            TrySetParent(voiceRoot, voiceService.transform.parent, false);
            // Move all services into voice root if possible
            TrySetParent(voiceService.transform, voiceRoot, true);
            TrySetParent(composerService.transform, voiceRoot, true);
            if (ttsService.transform.parent == null)
            {
                TrySetParent(ttsService.transform, voiceRoot, true);
            }
            TrySetParent(ttsSpeaker.transform, voiceRoot, true);

            // Select composer
            Selection.activeObject = composerService.gameObject;
        }

        // Get composer service
        private static ComposerService CreateComposer(TTSSpeaker speaker)
        {
            // Generate composer
            ComposerService composerService = GenerateGameObject("AppComposerExperience").AddComponent<AppComposerExperience>();

            // Add default speech handler
            ComposerSpeechHandler speechHandler = composerService.GetComponent<ComposerSpeechHandler>();
            if (speechHandler == null)
            {
                speechHandler = composerService.gameObject.AddComponent<ComposerSpeechHandler>();
            }

            // Add speaker to speech handler
            List<ComposerSpeakerData> speakers = new List<ComposerSpeakerData>();
            speakers.Add(new ComposerSpeakerData()
            {
                SpeakerName = "MAIN",
                Speaker = speaker
            });
            if (speechHandler.Speakers != null)
            {
                speakers.AddRange(speechHandler.Speakers);
            }
            speechHandler.Speakers = speakers.ToArray();

            // Add default action handler
            ComposerActionHandler actionHandler = composerService.GetComponent<ComposerActionHandler>();
            if (actionHandler == null)
            {
                actionHandler = composerService.gameObject.AddComponent<ComposerActionHandler>();
            }

            // Return service
            return composerService;
        }
        // Get voice service
        private static VoiceService GetVoiceService()
        {
            return GenerateGameObject("AppVoiceExperience").AddComponent<AppVoiceExperience>();
        }
        // Try set parent if not null
        private static bool TrySetParent(Transform child, Transform parent, bool worldPositionStays)
        {
            if (parent != null && parent != child)
            {
                child.SetParent(parent, worldPositionStays);
                return true;
            }
            return false;
        }
        // Generate with specified name
        private static GameObject GenerateGameObject(string name, Transform parent = null)
        {
            Transform result = new GameObject(name).transform;
            result.SetParent(parent);
            result.localPosition = Vector3.zero;
            result.localRotation = Quaternion.identity;
            result.localScale = Vector3.one;
            return result.gameObject;
        }
    }
}
