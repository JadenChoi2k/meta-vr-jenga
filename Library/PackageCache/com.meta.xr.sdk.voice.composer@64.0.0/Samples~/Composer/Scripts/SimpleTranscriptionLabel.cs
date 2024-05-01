/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using UnityEngine.UI;

namespace Meta.WitAi.Composer.Samples
{
    public class SimpleTranscriptionLabel : MonoBehaviour
    {
        // Voice service
        [SerializeField] private VoiceService _voiceService;
        // Transcription label
        [SerializeField] private Text _label;
        // Initial prompt
        [SerializeField] private string _promptListen = "Listening...";
        // Color for prompt text
        [SerializeField] private Color _promptColor = Color.cyan;
        // Color for text while building
        [SerializeField] private Color _partialColor = Color.gray;
        // Color for error text
        [SerializeField] private Color _errorColor = Color.red;
        // Final color
        private string _promptBegin;
        private Color _finalColor;
        private bool _gotTranscription = false;

        // Grab label
        private void Awake()
        {
            if (_label == null)
            {
                _label = gameObject.GetComponent<Text>();
            }
            if (_label != null)
            {
                _promptBegin = _label.text;
                _finalColor = _label.color;
            }
        }
        // Reset
        private void OnEnable()
        {
            // Set prompt
            SetText(_promptBegin);
            SetTextColor(_promptColor);

            // Add delegates
            if (_voiceService != null)
            {
                _voiceService.VoiceEvents.OnStartListening.AddListener(OnStartListening);
                _voiceService.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
                _voiceService.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);
                _voiceService.VoiceEvents.OnStoppedListeningDueToDeactivation.AddListener(OnCancelled);
                _voiceService.VoiceEvents.OnAborted.AddListener(OnCancelled);
                _voiceService.VoiceEvents.OnError.AddListener(OnError);
            }
            else
            {
                VLog.E("Transcription Label - Needs a Voice Service reference");
            }
        }
        // Remove delegates
        private void OnDisable()
        {
            if (_voiceService != null)
            {
                _voiceService.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
                _voiceService.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
                _voiceService.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);
                _voiceService.VoiceEvents.OnStoppedListeningDueToDeactivation.RemoveListener(OnCancelled);
                _voiceService.VoiceEvents.OnAborted.RemoveListener(OnCancelled);
                _voiceService.VoiceEvents.OnError.RemoveListener(OnError);
            }
        }

        // Called when beginning to listen for audio
        private void OnStartListening()
        {
            // Set prompt
            SetText(_promptListen);
            SetTextColor(_promptColor);
            _gotTranscription = false;
        }
        // Called while building transcription
        private void OnPartialTranscription(string transcription)
        {
            SetText(transcription);
            if (!_gotTranscription)
            {
                _gotTranscription = true;
                SetTextColor(_partialColor);
            }
        }
        // Called when transcription is complete
        private void OnFullTranscription(string transcription)
        {
            SetText(transcription);
            SetTextColor(_finalColor);
        }
        // Cancelled
        private void OnCancelled()
        {
            SetText(_promptBegin);
            SetTextColor(_promptColor);
        }
        // Add error color
        private void OnError(string code, string message)
        {
            SetText($"Error {code}\n{message}");
            SetTextColor(_errorColor);
        }

        private void SetText(string text)
        {
            if (_label != null)
            {
                _label.text = text;
            }
        }

        private void SetTextColor(Color newColor)
        {
            if (_label != null)
            {
                _label.color = newColor;
            }
        }
    }
}
