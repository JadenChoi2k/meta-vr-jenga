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

using Meta.WitAi;
using Meta.WitAi.Configuration;
using Meta.WitAi.Requests;
using UnityEngine;
using UnityEngine.UI;

namespace Oculus.VoiceSDK.UX
{
    /// <summary>
    /// Simple button behaviour for activating audio on a referenced voice service
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class VoiceActivationButton : MonoBehaviour
    {
        /// <summary>
        /// Reference to the voice service that will be activated or deactivated
        /// </summary>
        [Tooltip("Reference to the current voice service")]
        [SerializeField] private VoiceService _voiceService;

        /// <summary>
        /// Text to be shown while the voice service is not actively recording
        /// </summary>
        [Tooltip("Text to be shown while the voice service is not actively recording")]
        [SerializeField] private string _activateText = "Activate";

        /// <summary>
        /// Whether to immediately send data to service or to wait for the audio threshold
        /// </summary>
        [Tooltip("Whether to immediately send data to service or to wait for the audio threshold")]
        [SerializeField] private bool _activateImmediately = false;

        /// <summary>
        /// Text to be shown while the voice service is actively recording
        /// </summary>
        [Tooltip("Text to be shown while the voice service is actively recording")]
        [SerializeField] private string _deactivateText = "Deactivate";

        /// <summary>
        /// Whether to immediately abort request activation on deactivate
        /// </summary>
        [Tooltip("Whether to immediately abort request activation on deactivate")]
        [SerializeField] private bool _deactivateAndAbort = false;

        // The button to be observed
        private Button _button;
        // The button label to be adjusted with state
        private Text _buttonLabel;
        // Current audio request for specific deactivation
        private VoiceServiceRequest _request;
        // Whether an audio request is still activated or not
        private bool _isActive = false;

        // Get button, label & service if needed
        private void Awake()
        {
            _buttonLabel = GetComponentInChildren<Text>();
            _button = GetComponent<Button>();
            if (_voiceService == null)
            {
                _voiceService = FindObjectOfType<VoiceService>();
            }
        }

        // Add delegates
        private void OnEnable()
        {
            RefreshActive();
            if (_voiceService != null)
            {
                _voiceService.VoiceEvents.OnStartListening.AddListener(OnStartListening);
                _voiceService.VoiceEvents.OnStoppedListening.AddListener(OnStopListening);
            }
            if (_button != null)
            {
                _button.onClick.AddListener(OnClick);
            }
        }

        // Remove delegates
        private void OnDisable()
        {
            _isActive = false;
            if (_voiceService != null)
            {
                _voiceService.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
                _voiceService.VoiceEvents.OnStoppedListening.RemoveListener(OnStopListening);
            }
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }

        // On click, activate if not active & deactivate if active
        private void OnClick()
        {
            if (!_isActive)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }

        // Activate depending on settings
        private void Activate()
        {
            if (!_activateImmediately)
            {
                _request = _voiceService.Activate(new WitRequestOptions(), new VoiceServiceRequestEvents());
            }
            else
            {
                _request = _voiceService.ActivateImmediately(new WitRequestOptions(), new VoiceServiceRequestEvents());
            }
        }

        // Deactivate depending on settings
        private void Deactivate()
        {
            // Deactivate audio via request or service itself
            if (!_deactivateAndAbort)
            {
                if (_request != null)
                {
                    _request.DeactivateAudio();
                }
                else
                {
                    _voiceService.Deactivate();
                }
            }
            // Deactivate & abort this request
            else if (_request != null)
            {
                _request.Cancel();
            }
        }

        // Request initialized
        private void OnStartListening()
        {
            _isActive = true;
            RefreshActive();
        }
        // Request completed
        private void OnStopListening()
        {
            _isActive = false;
            _request = null;
            RefreshActive();
        }

        // Refresh active text
        private void RefreshActive()
        {
            if (_buttonLabel != null)
            {
                _buttonLabel.text = _isActive ? _deactivateText : _activateText;
            }
        }
    }
}
