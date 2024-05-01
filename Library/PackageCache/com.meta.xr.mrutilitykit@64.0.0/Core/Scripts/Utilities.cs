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

using System;
using UnityEngine;
using System.Collections.Generic;

namespace Meta.XR.MRUtilityKit
{
    public static class Utilities
    {
        static Dictionary<GameObject, Bounds?> prefabBoundsCache = new();

        public static readonly float Sqrt2 = Mathf.Sqrt(2f);
        public static readonly float InvSqrt2 = 1f / Mathf.Sqrt(2f);

        public static Bounds? GetPrefabBounds(GameObject prefab)
        {
            if (prefabBoundsCache.TryGetValue(prefab, out Bounds? cachedBounds))
            {
                return cachedBounds;
            }
            Bounds? bounds = CalculateBoundsRecursively(prefab.transform);
            prefabBoundsCache.Add(prefab, bounds);
            return bounds;
        }

        static Bounds? CalculateBoundsRecursively(Transform transform)
        {
            Bounds? bounds = null;
            Renderer renderer = transform.GetComponent<Renderer>();

            if (renderer != null && renderer.bounds.size != Vector3.zero)
            {
                // If the current GameObject has a renderer component, include its bounds
                bounds = renderer.bounds;
            }

            // Recursively process children
            foreach (Transform child in transform.transform)
            {
                Bounds? childBounds = CalculateBoundsRecursively(child);
                if (childBounds != null)
                {
                    if (bounds != null)
                    {
                        var boundsValue = bounds.Value;
                        boundsValue.Encapsulate(childBounds.Value);
                        bounds = boundsValue;
                    }
                    else
                    {
                        bounds = childBounds;
                    }
                }
            }

            return bounds;
        }

        /// <summary>
        /// Gets the name of an anchor based on its semantic classification.
        /// </summary>
        /// <param name="anchorData">The Data.AnchorData object representing the anchor.</param>
        /// <returns>The name of the anchor, or "UNDEFINED_ANCHOR" if no semantic classification is available.</returns>
        public static string GetAnchorName(Data.AnchorData anchorData)
        {
            return anchorData.SemanticClassifications.Count != 0
                ? anchorData.SemanticClassifications[0]
                : "UNDEFINED_ANCHOR";
        }

        internal static Rect? GetPlaneRectFromAnchorData(Data.AnchorData data)
        {
            if (data.PlaneBounds == null) return null;
            return new Rect(data.PlaneBounds.Value.Min, data.PlaneBounds.Value.Max - data.PlaneBounds.Value.Min);
        }

        internal static Bounds? GetVolumeBoundsFromAnchorData(Data.AnchorData data)
        {
            if (data.VolumeBounds == null) return null;
            Vector3 volumeBoundsMin = data.VolumeBounds.Value.Min;
            Vector3 volumeBoundsMax = data.VolumeBounds.Value.Max;
            Vector3 volumeBoundsCenterOffset = (volumeBoundsMin + volumeBoundsMax) * 0.5f;
            return new Bounds(volumeBoundsCenterOffset, volumeBoundsMax - volumeBoundsMin);
        }

        internal static Mesh GetGlobalMeshFromAnchorData(Data.AnchorData data)
        {
            if (data.GlobalMesh == null) return null;
            return new Mesh()
            {
                vertices = data.GlobalMesh.Value.Positions,
                triangles = data.GlobalMesh.Value.Indices
            };
        }

        internal static void DestroyGameObjectAndChildren(GameObject gameObject)
        {
            if (gameObject == null) return;
            foreach (Transform child in gameObject.transform)
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            UnityEngine.Object.DestroyImmediate(gameObject.gameObject);
        }

        /// <summary>
        /// Replacement for LINQ
        /// </summary>
        public static bool SequenceEqual<T>(this List<T> list1, List<T> list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null && list2 != null) return false;
            if (list1 != null && list2 == null) return false;
            if (list1.Count != list2.Count)
            {
                return false;
            }
            for (int i = 0; i < list1.Count; i++)
            {
                if (!Equals(list1[i], list2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static string GetAnchorDatasString(List<Data.AnchorData> anchors)
        {
            string anchorsString = "Anchors: [";
            for (int i = 0; i < anchors.Count; i++)
            {
                if (i > 0)
                {
                    anchorsString += ", ";
                }
                anchorsString += anchors[i].ToString();
            }
            anchorsString += "]";
            return anchorsString;
        }
    }
}
