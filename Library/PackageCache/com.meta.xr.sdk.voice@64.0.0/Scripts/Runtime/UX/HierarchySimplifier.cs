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

using UnityEngine;

namespace Oculus.Voice.UX
{
    /// <summary>
    /// A monobehaviour tagging object which can be used to hide/show objects in the hierarchy
    /// </summary>
    public class HierarchySimplifier : MonoBehaviour
    {
        /// <summary>
        /// A basic flag for whether to hide the object on startup, by default.
        /// </summary>
        [Tooltip("Whether to hide the object on startup, by default.")]
        [SerializeField]
        public bool hideByDefault = true;

        /// <summary>
        /// Toggles the HideInHierarchy flag in all game objects containing a HierarchySimplifier underneath
        /// the given object.
        /// </summary>
        /// <param name="obj">parent object under which all affected objects will be modified</param>
        /// <param name="hideObjects">whether to hide affected objects in the hierarchy</param>
        public static void HideSubObjects(GameObject obj, bool hideObjects)
        {
            var affected = obj.GetComponentsInChildren<HierarchySimplifier>();
            foreach (HierarchySimplifier aff in affected)
            {
                ToggleShowInHierarchyFlag(aff.gameObject, hideObjects);
            }
        }

        private void OnValidate()
        {
            ToggleShowInHierarchyFlag(this.gameObject, hideByDefault);
        }

        public static void ToggleShowInHierarchyFlag(GameObject obj, bool hideObject)
        {
            obj.hideFlags = hideObject ?
                obj.hideFlags | HideFlags.HideInHierarchy :
                obj.hideFlags & ~HideFlags.HideInHierarchy ;
        }
    }
}
