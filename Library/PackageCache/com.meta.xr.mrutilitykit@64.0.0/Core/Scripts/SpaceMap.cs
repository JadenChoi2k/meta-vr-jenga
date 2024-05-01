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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.MRUtilityKit
{
    public class SpaceMap : MonoBehaviour
    {
        [Tooltip("When the scene data is loaded, this controls what room(s) the prefabs will spawn in.")]
        public MRUK.RoomFilter CreateOnStart = MRUK.RoomFilter.AllRooms;

        // Instead of creating a texture all in code, we prefer an external texture
        // (this is so you can more easily link to the texture from the inspector, for example to use it in a material)
        // The trade-off for this simplicity is that the texture must have certain import settings, shown here
        // NOTE: You shouldn't be using a large texture size here for performance reasons.
        // Performance will be improved in future versions.
        [Tooltip("Texture requirements: Read/Write enabled, RGBA 32 bit format. Texture suggestions: Wrap Mode = Clamped, size small (<128x128)")]
        public Texture2D TextureMap;

        Vector3 MapCenter = Vector3.zero;
        Bounds MapBounds = new Bounds();
        Color[,] Pixels;
        int PixelDimensions = 128;

        public Gradient MapGradient = new Gradient();
        [Tooltip("How far inside the room the left end of the Texture Gradient should appear. 0 is at the surface, negative is inside the room.")]
        public float InnerBorder = -0.5f;
        [Tooltip("How far outside the room the right end of the Texture Gradient should appear. 0 is at the surface, positive is outside the room.")]
        public float OuterBorder = 0.0f;
        [Tooltip("How much the texture map should extend from the room bounds, in meters. Should ideally be greater than or equal to outerPosition.")]
        public float MapBorder = 0.0f;

        const string MATERIAL_PROPERTY_NAME = "_SpaceMap";
        const string PARAMETER_PROPERTY_NAME = "_SpaceMapParams";

        private void Start()
        {
            if (MRUK.Instance && CreateOnStart != MRUK.RoomFilter.None)
            {
                MRUK.Instance.RegisterSceneLoadedCallback(() =>
                {
                    switch (CreateOnStart)
                    {
                        case MRUK.RoomFilter.AllRooms:
                            CalculateMap();
                            break;
                        case MRUK.RoomFilter.CurrentRoomOnly:
                            CalculateMap(MRUK.Instance.GetCurrentRoom());
                            break;
                    }
                });
            }
        }

        public void CalculateMap(MRUKRoom room = null)
        {
            if (TextureMap == null)
            {
                Debug.LogWarning("No texture specified for Space Map");
                return;
            }

            // one-time startup behavior
            InitializeMapValues(room);

            // We may want to do this in chunks, to avoid a loading hitch as this gets more complex
            // TODO: send event when texture is done being calculated
            StartCoroutine(CalculatePixels(room));

            float textureScale = Mathf.Max(MapBounds.size.x, MapBounds.size.z) + MapBorder * 2;
            Shader.SetGlobalTexture(MATERIAL_PROPERTY_NAME, TextureMap);
            Vector4 textureParams = new Vector4(textureScale, textureScale, MapCenter.x, MapCenter.z);
            Shader.SetGlobalVector(PARAMETER_PROPERTY_NAME, textureParams);
        }

        void InitializeMapValues(MRUKRoom room)
        {
            PixelDimensions = TextureMap.width;
            Pixels = new Color[PixelDimensions, PixelDimensions];

            if (room != null)
            {
                MapBounds = room.GetRoomBounds();
            }
            else
            {
                MapBounds = new();
                foreach (var currentRoom in MRUK.Instance.Rooms)
                {
                    MapBounds.Encapsulate(currentRoom.GetRoomBounds());
                }
            }

            MapCenter = new Vector3(MapBounds.center.x, MapBounds.min.y, MapBounds.center.z);
            transform.position = MapCenter;

            float largestDimension = Mathf.Max(MapBounds.size.x, MapBounds.size.z);
            // make the texture square
            Vector3 mapScale = new Vector3(largestDimension + MapBorder * 2, MapBounds.size.y, largestDimension + MapBorder * 2);
            transform.localScale = mapScale;
        }

        /// <summary>
        /// A surface-relative value of how far a position is from all SceneAPI surfaces
        /// negative values are behind the surface i.e. outside of the room or inside a volume
        /// </summary>
        public float GetSurfaceDistance(MRUKRoom room, Vector3 worldPosition)
        {
            float closestDist = Mathf.Infinity;
            float sign = 1f;
            if (room != null)
            {
                closestDist = room.TryGetClosestSurfacePosition(worldPosition, out Vector3 closestPos, out MRUKAnchor closestAnchor, LabelFilter.Excluded(new List<string> { OVRSceneManager.Classification.Floor, OVRSceneManager.Classification.Ceiling }));
                sign = room.IsPositionInRoom(worldPosition, false) ? 1 : -1;
            }
            else
            {
                foreach (var currentRoom in MRUK.Instance.Rooms)
                {
                    var dist = currentRoom.TryGetClosestSurfacePosition(worldPosition, out Vector3 closestPos, out MRUKAnchor closestAnchor, LabelFilter.Excluded(new List<string> { OVRSceneManager.Classification.Floor, OVRSceneManager.Classification.Ceiling }));
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        sign = currentRoom.IsPositionInRoom(worldPosition) ? 1 : -1;
                    }
                }
            }
            float surfaceDistance = closestDist * sign;
            return surfaceDistance;
        }

        IEnumerator CalculatePixels(MRUKRoom room)
        {
            float halfPixel = 0.5f / PixelDimensions;
            float largestDimension = Mathf.Max(MapBounds.size.x, MapBounds.size.z) + MapBorder * 2;
            for (int x = 0; x < PixelDimensions; x++)
            {
                for (int y = 0; y < PixelDimensions; y++)
                {
                    // convert texel coordinate to world position
                    float xWorld = x / (float)PixelDimensions - 0.5f + halfPixel;
                    float yWorld = y / (float)PixelDimensions - 0.5f + halfPixel;
                    Vector3 worldPos = new Vector3(xWorld * largestDimension + MapCenter.x, 0, yWorld * largestDimension + MapCenter.z);
                    float distToSurface = -GetSurfaceDistance(room, worldPos);
                    float normalizedDist = Mathf.Clamp01((distToSurface - InnerBorder) / (OuterBorder - InnerBorder));
                    Color averageColor = MapGradient.Evaluate(normalizedDist);
                    Pixels[x, y] = averageColor;
                    TextureMap.SetPixel(x, y, averageColor);
                }
            }
            TextureMap.Apply();

            yield return null;
        }

        public void ResetFreespace()
        {
            for (int x = 0; x < PixelDimensions; x++)
            {
                for (int y = 0; y < PixelDimensions; y++)
                {
                    Pixels[x, y] = Color.black;
                }
            }
        }

        /// <summary>
        /// Color clamps to edge color if worldPosition is off-grid.
        /// getBilinear blends the color between pixels.
        /// </summary>
        public Color GetColorAtPosition(Vector3 worldPosition, bool getBilinear = true)
        {
            // GetPixelBilinear requires UV coordinates (0...1)
            // GetPixel requires pixel counts (0...pixelDimensions)
            if (getBilinear)
            {
                Vector2 UVcoords = GetPixelFromWorldPosition(worldPosition, true);
                return TextureMap.GetPixelBilinear(UVcoords.x, UVcoords.y);
            }
            else
            {
                Vector2 pixelCoords = GetPixelFromWorldPosition(worldPosition);
                int x = Mathf.FloorToInt(pixelCoords.x);
                int y = Mathf.FloorToInt(pixelCoords.y);
                return TextureMap.GetPixel(x, y);
            }
        }

        Vector2 GetPixelFromWorldPosition(Vector3 worldPosition, bool normalizedUV = false)
        {
            Vector3 localSpace = worldPosition - MapCenter;
            Vector2 scaledSpace = new Vector2(localSpace.x / MapBounds.size.x + 0.5f, localSpace.z / MapBounds.size.z + 0.5f);
            if (!normalizedUV)
            {
                scaledSpace *= PixelDimensions;
            }

            return scaledSpace;
        }
    }
}
