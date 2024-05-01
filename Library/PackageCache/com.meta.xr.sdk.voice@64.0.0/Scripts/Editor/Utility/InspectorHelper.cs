/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using UnityEditor;
using UnityEngine;
namespace Oculus.Voice.Utility
{
    /// <summary>
    /// A collection of useful functions for drawing 
    /// </summary>
    public abstract class InspectorHelper
    {
        /// <summary>
        /// Draws all the given properties of the given serializedObject with the included header
        /// </summary>
        /// <param name="serializedObject">the object on which exist the given properties</param>
        /// <param name="propertyNames">raw text names of the properties to draw</param>
        /// <param name="header">the header text to use, when appropriate</param>
        public static void DrawSystemProperties(SerializedObject serializedObject, string[] propertyNames, string header)
        {
            bool shouldDisplayHeader = false;

            // Check if any of the properties is null
            foreach (string propertyName in propertyNames)
            {
                var property = serializedObject.FindProperty(propertyName);
                if (property.objectReferenceValue == null)
                {
                    shouldDisplayHeader = true;
                    break;
                }
            }

            // Display the header if needed
            if (shouldDisplayHeader)
            {
                DrawHeader(header);
            }

            // Display the properties that are null
            foreach (string propertyName in propertyNames)
            {
                var property = serializedObject.FindProperty(propertyName);
                if (property.objectReferenceValue == null)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
        }

        /// <summary>
        /// Draws the given header text with the given tooltip text
        /// </summary>
        public static void DrawHeader(string header, string tooltip = "")
        {
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField(new GUIContent(header, tooltip), EditorStyles.boldLabel);
        }

        /// <summary>
        /// Does the default drawing for the given property on the given object
        /// </summary>
        public static void DrawProperty(SerializedObject serializedObject, string propertyName)
        {
            var property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(property);
        }

        /// <summary>
        /// Draws a dropdown for the given object 
        /// </summary>
        /// <param name="serializedObject">the object containing the relevant data</param>
        /// <param name="position">The position rect from the calling OnGUI</param>
        /// <param name="labelName">the label text to show</param>
        /// <param name="propertyName">the raw text name of the property to set</param>
        /// <param name="options">the dropdown options to be presented</param>
        /// <param name="defaultValue">What value to save to the property when nothing is selected.</param>
        public static void DrawDropDown(SerializedObject serializedObject, Rect position, string labelName, string propertyName, string[] options, string defaultValue = "")
        {
            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);
            
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            GUIContent guiContent = new GUIContent(labelName, property.tooltip);

            int currentIndex = Array.FindIndex(options, val=>val.Equals(property.stringValue));
            EditorGUI.LabelField(labelRect, guiContent);
            
            int selectedIndex = EditorGUI.Popup(fieldRect, currentIndex,options);
            property.stringValue = selectedIndex >= 0 ? options[selectedIndex] : defaultValue;
        }

        /// <summary>
        /// Draws a horizontal line for visual separation.
        /// </summary>
        public static void DrawHorizontalLine()
        {
            GUILayout.Space(10); // Add some space above the line
            Rect dividerRect = GUILayoutUtility.GetRect(0, 2, GUILayout.ExpandWidth(true));
            GUI.Box(dividerRect, ""); // Draw the line
            GUILayout.Space(10); // Add some space below the line
        }
    }
}
