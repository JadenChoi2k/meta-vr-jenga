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
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Meta.XR.MRUtilityKit
{
    public static class Data
    {
        [Serializable]
        public struct TransformData
        {
            [JsonProperty("Translation")] public Vector3 Translation;
            [JsonProperty("Rotation")] public Vector3 Rotation;
            [JsonProperty("Scale")] public Vector3 Scale;
        }

        [Serializable]
        public struct PlaneBoundsData
        {
            [JsonProperty("Min")] public Vector2 Min;
            [JsonProperty("Max")] public Vector2 Max;
        }

        [Serializable]
        public struct VolumeBoundsData
        {
            [JsonProperty("Min")] public Vector3 Min;
            [JsonProperty("Max")] public Vector3 Max;
        }

        [Serializable]
        public struct AnchorData
        {
            // When serializing the Anchor, we only serialize its UUID. The handle is not preserved as this
            // is only valid on the device where it is loaded and not across sessions.
            [JsonProperty("UUID")] public OVRAnchor Anchor;
            [JsonProperty("SemanticClassifications")] public List<String> SemanticClassifications;
            [JsonProperty("Transform")] public TransformData Transform;
            [JsonProperty("PlaneBounds")] public PlaneBoundsData? PlaneBounds;
            [JsonProperty("VolumeBounds")] public VolumeBoundsData? VolumeBounds;
            [JsonProperty("PlaneBoundary2D")] public List<Vector2> PlaneBoundary2D;
            [JsonProperty("GlobalMesh")] public GlobalMeshData? GlobalMesh;
        }

        [Serializable]
        public struct GlobalMeshData
        {
            [JsonProperty("Positions")] public Vector3[] Positions;
            [JsonProperty("Indices")] public int[] Indices;
        }

        [Serializable]
        public struct RoomLayoutData
        {
            [JsonProperty("FloorUuid")] public Guid FloorUuid;
            [JsonProperty("CeilingUuid")] public Guid CeilingUuid;
            [JsonProperty("WallsUuid")] public List<Guid> WallsUuid;
        }

        [Serializable]
        public struct RoomData
        {
            // When serializing the Anchor, we only serialize its UUID. The handle is not preserved as this
            // is only valid on the device where it is loaded and not across sessions.
            [JsonProperty("UUID")] public OVRAnchor Anchor;
            [JsonProperty("RoomLayout")] public RoomLayoutData RoomLayout;
            [JsonProperty("Anchors")] public List<AnchorData> Anchors;
        }

        [Serializable]
        public struct SceneData
        {
            [JsonProperty("CoordinateSystem")] public SerializationHelpers.CoordinateSystem CoordinateSystem;
            [JsonProperty("Rooms")] public List<RoomData> Rooms;
        }
    }
}
