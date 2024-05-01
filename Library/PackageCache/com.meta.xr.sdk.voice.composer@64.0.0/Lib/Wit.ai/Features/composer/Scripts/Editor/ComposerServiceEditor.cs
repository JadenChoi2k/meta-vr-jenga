/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Meta.WitAi.Composer
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ComposerService), true)]
    public class ComposerServiceEditor : Editor
    {
        private GUIStyle _foldoutStyle;
        private GUIStyle _textStyle;

        // custom inspector variables
        private readonly bool _showDefaultInspector = false;
        private Dictionary<string, Action<string>> _customRenderers;

        private SerializedProperty _versionTagProperty;
        private string[] _versionTagNames;

        private void EnsureSetup()
        {
            if (_customRenderers == null)
            {
                _customRenderers = new Dictionary<string, Action<string>>();
            }

            if (null != _foldoutStyle) return;

            _foldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold ,
                fontSize = 20,
            };

            _textStyle = EditorStyles.label;
            _textStyle.wordWrap = true;
        }

        public override void OnInspectorGUI()
        {
            EnsureSetup();

            if (_showDefaultInspector)
            {
                DrawDefaultInspector();
            }
            else
            {
                DrawEnhancedInspector();
            }

            if (Application.isPlaying)
            {
                var composer = (ComposerService)target;
                GUILayout.Label("Context Map");
                GUILayout.TextArea(composer.CurrentContextMap?.Data?.ToString());
            }
        }

        private void DrawEnhancedInspector()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                if (_customRenderers.ContainsKey(iterator.propertyPath))
                {
                    _customRenderers[iterator.propertyPath](iterator.propertyPath);
                    GenerateTooltip(iterator.tooltip);
                    continue;
                }

                enterChildren = false;
                EditorGUILayout.PropertyField(iterator, true);
                GenerateTooltip(iterator.tooltip);
            }
            serializedObject.ApplyModifiedProperties();
        }
        private void GenerateTooltip(string text)
        {
            if(string.IsNullOrEmpty(text)) return;

            var propRect = GUILayoutUtility.GetLastRect();
            GUI.Label(propRect, new GUIContent("", text));
        }
        private void OnEnable()
        {
            if (!Application.isPlaying) return;

            if (target is ComposerService composer)
            {
                composer.Events.OnComposerContextMapChange.AddListener(OnContextMapChanged);
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;

            if (target is ComposerService composer)
            {
                composer.Events.OnComposerContextMapChange.RemoveListener(OnContextMapChanged);
            }
        }

        private void OnContextMapChanged(ComposerSessionData arg0)
        {
            Repaint();
        }
    }
}
