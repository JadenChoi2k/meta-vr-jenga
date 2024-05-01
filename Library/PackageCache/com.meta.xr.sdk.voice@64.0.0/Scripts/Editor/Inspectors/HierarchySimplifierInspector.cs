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

using UnityEditor;
using UnityEngine;

namespace Oculus.Voice.UX
{
    /// <summary>
    /// An inspector and menu addition to help avoid someone adding the the monobehaviour
    /// and then losing their object.
    /// </summary>
    [CustomEditor(typeof(HierarchySimplifier))]
    public class HierarchySimplifierInspector : Editor
    {
        private const string Warning = "<b><color=#ff0000ff><size=25>WARNING!</size></color></b>"
            + "\nThis object will disappear from the hierarchy if <i><color=#999999ff>Hide By Default</color></i> is set to true."
            + "\nTo show it again, use the <i><color=#999999ff>" + MenuPath + "</color></i> menu item.";
        private const string MenuPath = "Oculus/Voice SDK/Show all advanced game objects";

        /// <summary>
        /// A simple menu item to show all objects in the scene, in case they were accidentally hidden.
        /// </summary>
        [MenuItem(MenuPath)]
        static void ShowAllHiddenGameObjects()
        {
            foreach (var obj in FindObjectsOfType<HierarchySimplifier>())
            {
                HierarchySimplifier.ToggleShowInHierarchyFlag(obj.gameObject, false);
            }
        }

        public override void OnInspectorGUI()
        {
            GUIStyle warningStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                wordWrap = true,
                richText = true
            };

            EditorGUILayout.TextArea(Warning, warningStyle);
            DrawDefaultInspector();
        }
    }
}
