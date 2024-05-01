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
    public class SimpleEventButton : MonoBehaviour
    {
        // Voice service
        [SerializeField] private VoiceService _voiceService;
        // Activation button label
        [SerializeField] private Button _button;
        // Event text
        [SerializeField] private string _eventText;
        [SerializeField] private string _eventTextKey = "[COLOR]";
        [SerializeField] private string[] _eventTextValueOptions;

        // Grab label
        private void Awake()
        {
            if (_button == null)
            {
                _button = gameObject.GetComponent<Button>();
            }
            if (_voiceService == null)
            {
                VLog.W($"{GetType()} ({gameObject.name}) - Needs a Voice Service reference");
            }
        }
        // Reset
        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(PerformEvent);
            }
        }
        // Remove delegates
        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(PerformEvent);
            }
        }

        // Perform activation with event text
        private void PerformEvent()
        {
            string eventText = GetRandomEventText();
            Debug.Log($"Activate Event\nText: {eventText}");
            _voiceService.Activate(eventText);
        }
        // Get event text
        private string GetRandomEventText()
        {
            string result = _eventText;
            int index = result.IndexOf(_eventTextKey);
            while (index != -1 && _eventTextValueOptions.Length > 0)
            {
                // Get random index
                int optionIndex = UnityEngine.Random.Range(0, _eventTextValueOptions.Length);
                string option = _eventTextValueOptions[optionIndex];

                // Replace event text
                result = $"{result.Substring(0, index)}{option}{result.Substring(index + _eventTextKey.Length)}";

                // Find next
                index = result.IndexOf(_eventTextKey);
            }
            return result;
        }
    }
}
