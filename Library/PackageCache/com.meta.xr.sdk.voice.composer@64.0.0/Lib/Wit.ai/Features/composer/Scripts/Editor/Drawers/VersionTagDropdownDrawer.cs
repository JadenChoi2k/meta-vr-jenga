/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using Meta.WitAi.Composer.Attributes;

namespace Meta.WitAi.Composer.Drawers
{
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    [CustomPropertyDrawer(typeof(VersionTagDropdownAttribute))]
    public class VersionDropdownDrawer : PropertyDrawer
    {
        private ComposerService _composerService;
        private string[] _versionTagNames;

        private void SetupTagVersionDropDown()
        {
            if (!_composerService ||
                _composerService.VoiceService == null ||
                _composerService.VoiceService.WitConfiguration == null) return;

            var versionTags = _composerService.VoiceService.WitConfiguration.GetApplicationInfo().versionTags;
            var names = null != versionTags ? versionTags.Select(instance => instance.name).ToList() : new List<string>();
            names.Insert(0, "Current");
            _versionTagNames = names.ToArray();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _composerService = (ComposerService) property.serializedObject.targetObject;
            EnsureSetup();
            if (!_composerService || !_composerService.VoiceService || !_composerService.VoiceService.WitConfiguration)
            {
                EditorGUILayout.LabelField("Version Tag", "No wit configuration.");
                return;
            }

            if (property.propertyType == SerializedPropertyType.String)
            {
                var lastIndex = Array.IndexOf(_versionTagNames,property.stringValue);
                lastIndex = lastIndex < 1 ? 0 : lastIndex;
                int selectedIndex = EditorGUI.Popup(position, label.text, lastIndex, _versionTagNames);
                property.stringValue = selectedIndex<1?string.Empty:_versionTagNames[selectedIndex];
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        private void EnsureSetup()
        {
            if (_versionTagNames == null)
            {
                SetupTagVersionDropDown();
            }
        }
    }
}
