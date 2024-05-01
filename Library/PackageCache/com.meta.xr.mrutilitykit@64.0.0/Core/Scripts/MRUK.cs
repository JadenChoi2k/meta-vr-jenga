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
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

namespace Meta.XR.MRUtilityKit
{

    /// <summary>
    /// This class contains convenience functions that allow you to
    /// query your scene.
    ///
    /// Use together with <seealso cref="MRUKLoader"/> to
    /// load data via link, fake data, or on-device data.
    /// </summary>
    public class MRUK : MonoBehaviour
    {
        // when interacting specifically with tops of volumes, this can be used to
        // specify where the return position should be aligned on the surface
        // e.g. some apps might want a position right in the center of the table (chess)
        // for others, the edge may be more important (piano or pong)
        public enum PositioningMethod
        {
            DEFAULT,
            CENTER,
            EDGE
        }

        /// <summary>
        /// Specify the source of the scene data.
        /// </summary>
        public enum SceneDataSource
        {
            /// <summary>
            /// Load scene data from the device.
            /// </summary>
            Device,
            /// <summary>
            /// Load scene data from prefabs.
            /// </summary>
            Prefab,
            /// <summary>
            /// First try to load data from the device and if none can be found
            /// fall back to loading from a prefab.
            /// </summary>
            DeviceWithPrefabFallback,
            /// <summary>
            /// Load Scene from Json String
            /// </summary>
            Json,
        }

        public enum RoomFilter
        {
            None,
            CurrentRoomOnly,
            AllRooms,
        };

        /// <summary>
        /// Return value from the call to LoadSceneFromDevice
        /// </summary>
        public enum LoadDeviceResult
        {
            /// <summary>
            /// Scene data loaded successfully.
            /// </summary>
            Success,
            /// <summary>
            /// User did not grant scene permissions.
            /// </summary>
            NoScenePermission,
            /// <summary>
            /// No rooms were found (e.g. User did not go through space setup)
            /// </summary>
            NoRoomsFound,
        };

        [Flags]
        public enum SurfaceType
        {
            FACING_UP = 1 << 0,
            FACING_DOWN = 1 << 1,
            VERTICAL = 1 << 2,
        };

        [Flags]
        private enum AnchorRepresentation
        {
            PLANE = 1 << 0,
            VOLUME = 1 << 1,
        }

        public bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Event that is triggered when the scene is loaded.
        /// </summary>
        public UnityEvent SceneLoadedEvent = new();

        /// <summary>
        /// Event that is triggered when a room is created.
        /// </summary>
        public UnityEvent<MRUKRoom> RoomCreatedEvent = new();

        /// <summary>
        /// Event that is triggered when a room is updated.
        /// </summary>
        public UnityEvent<MRUKRoom> RoomUpdatedEvent = new();

        /// <summary>
        /// Event that is triggered when a room is removed.
        /// </summary>
        public UnityEvent<MRUKRoom> RoomRemovedEvent = new();
        /// <summary>
        /// When world locking is enabled the position of the camera rig will be adjusted each frame to ensure
        /// the room anchors are where they should be relative to the camera position.This is necessary to
        /// ensure the position of the virtual objects in the world do not get out of sync with the real world.
        /// </summary>
        public bool EnableWorldLock = true;

        private OVRCameraRig _cameraRig;
        private bool _worldLockWasEnabled = false;
        private bool _loadSceneCalled = false;
        private Pose? _prevTrackingSpacePose = default;


        /// <summary>
        /// This is the final event that tells developer code that Scene API and MR Utility Kit have been initialized, and that the room can be queried.
        /// </summary>
        void InitializeScene()
        {
            try
            {
                SceneLoadedEvent.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            IsInitialized = true;
        }

        /// <summary>
        /// Register to receive a callback when the scene is loaded. If the scene is already loaded
        /// at the time this is called, the callback will be invoked immediatly.
        /// </summary>
        /// <param name="callback"></param>
        public void RegisterSceneLoadedCallback(UnityAction callback)
        {
            SceneLoadedEvent.AddListener(callback);
            if (IsInitialized)
            {
                callback();
            }
        }

        /// <summary>
        /// Register to receive a callback when a new room has been created from scene capture.
        /// </summary>
        /// <param name="callback">
        /// - `MRUKRoom` The created room object.
        /// </param>
        public void RegisterRoomCreatedCallback(UnityAction<MRUKRoom> callback)
        {
            RoomCreatedEvent.AddListener(callback);
        }

        /// <summary>
        /// Register to receive a callback when a room has been updated from scene capture.
        /// </summary>
        /// <param name="callback">
        /// - `MRUKRoom` The updated room object.
        /// </param>
        public void RegisterRoomUpdatedCallback(UnityAction<MRUKRoom> callback)
        {
            RoomUpdatedEvent.AddListener(callback);
        }
        /// <summary>
        /// Registers a callback function to be called before the room is removed.
        /// </summary>
        /// <param name="callback">The function to be called when the room is removed. It takes one parameter:
        /// - `MRUKRoom` The removed room object.
        ///</param>
        public void RegisterRoomRemovedCallback(UnityAction<MRUKRoom> callback)
        {
            RoomRemovedEvent.AddListener(callback);
        }

        /// <summary>
        /// Get a list of all the rooms in the scene.
        /// </summary>
        [Obsolete("Use Rooms property instead")]
        public List<MRUKRoom> GetRooms() => Rooms;

        /// <summary>
        /// Get a flat list of all Anchors in the scene
        /// </summary>
        [Obsolete("Use GetCurrentRoom().Anchors instead")]
        public List<MRUKAnchor> GetAnchors() => GetCurrentRoom().Anchors;


        /// <summary>
        /// Returns the current room the headset is in. If the headset is not in any given room
        /// then it will return the room the headset was last in when this function was called.
        /// If the headset hasn't been in a valid room yet then return the first room in the list.
        /// If no rooms have been loaded yet then return null.
        /// </summary>
        public MRUKRoom GetCurrentRoom()
        {
            // This is a rather expensive operation, we should only do it at most once per frame.
            if (_cachedCurrentRoomFrame != Time.frameCount)
            {
                if (_cameraRig?.centerEyeAnchor.position is Vector3 eyePos)
                {
                    foreach (var room in Rooms)
                    {
                        if (room.IsPositionInRoom(eyePos, false))
                        {
                            _cachedCurrentRoom = room;
                            _cachedCurrentRoomFrame = Time.frameCount;
                            return room;
                        }
                    }
                }
            }

            if (_cachedCurrentRoom != null)
            {
                return _cachedCurrentRoom;
            }

            if (Rooms.Count > 0)
            {
                return Rooms[0];
            }

            return null;
        }

        /// <summary>
        /// Checks whether any anchors can be loaded.
        /// </summary>
        /// <returns>Returns a task-based bool, which is true if
        /// there are any scene anchors in the system, and false
        /// otherwise. If false is returned, then either
        /// the scene permission needs to be set, or the user
        /// has to run Scene Capture.</returns>
        public static async Task<bool> HasSceneModel()
        {
            var rooms = new List<OVRAnchor>();
            if (!await OVRAnchor.FetchAnchorsAsync<OVRRoomLayout>(rooms))
                return false;
            return rooms.Count > 0;
        }

        [Serializable]
        public class MRUKSettings
        {
            [SerializeField, Tooltip("Where to load the data from.")]
            public SceneDataSource DataSource = SceneDataSource.Device;
            [SerializeField, Tooltip("Which room to use; -1 is random.")]
            public int RoomIndex = -1;
            [SerializeField, Tooltip("The list of prefab rooms to use.")]
            public GameObject[] RoomPrefabs;
            [SerializeField, Tooltip("Trigger a scene load on startup.")]
            public bool LoadSceneOnStartup = true;
            [SerializeField, Tooltip("The width of a seat. Use to calculate seat positions")]
            public float SeatWidth = 0.6f;
            [SerializeField, Tooltip("The Scene Json to use")]
            public string SceneJson;
        }

        [Tooltip("Contains all the information regarding data loading.")]
        public MRUKSettings SceneSettings;

        MRUKRoom _cachedCurrentRoom = null;
        int _cachedCurrentRoomFrame = 0;


        /// <summary>
        /// List of all the rooms in the scene.
        /// </summary>
        public List<MRUKRoom> Rooms { get; } = new();

        public static MRUK Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.Assert(false, "There should be only one instance of MRUK!");
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            if (SceneSettings == null) return;

            if (SceneSettings.LoadSceneOnStartup)
            {
                // We can't await for the LoadScene result because Awake is not async, silence the warning
#pragma warning disable CS4014
#if !UNITY_EDITOR && UNITY_ANDROID
                // If we are going to load from device we need to ensure we have permissions first
                if ((SceneSettings.DataSource == SceneDataSource.Device || SceneSettings.DataSource == SceneDataSource.DeviceWithPrefabFallback) &&
                    !Permission.HasUserAuthorizedPermission(OVRPermissionsRequester.ScenePermission))
                {
                    var callbacks = new PermissionCallbacks();
                    callbacks.PermissionDenied += permissionId =>
                    {
                        Debug.LogWarning("User denied permissions to use scene data");
                        // Permissions denied, if data source is using prefab fallback let's load the prefab scene instead
                        if (SceneSettings.DataSource == SceneDataSource.DeviceWithPrefabFallback)
                        {
                            LoadScene(SceneDataSource.Prefab);
                        }
                    };
                    callbacks.PermissionGranted += permissionId =>
                    {
                        // Permissions are now granted and it is safe to try load the scene now
                        LoadScene(SceneSettings.DataSource);
                    };
                    // Note: If the permission request dialog is already active then this call will silently fail
                    // and we won't receive the callbacks. So as a work-around there is a code in Update() to mitigate
                    // this problem.
                    Permission.RequestUserPermission(OVRPermissionsRequester.ScenePermission, callbacks);
                }
                else
#endif
                {
                    LoadScene(SceneSettings.DataSource);
                }
#pragma warning restore CS4014
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                RoomCreatedEvent.RemoveAllListeners();
                RoomRemovedEvent.RemoveAllListeners();
                RoomUpdatedEvent.RemoveAllListeners();
                SceneLoadedEvent.RemoveAllListeners();
            }
        }

        private void Start()
        {
            if (!_cameraRig)
            {
                _cameraRig = FindObjectOfType<OVRCameraRig>();
            }

        }


        private void Update()
        {
            if (SceneSettings.LoadSceneOnStartup && !_loadSceneCalled)
            {
#if !UNITY_EDITOR && UNITY_ANDROID
                // This is to cope with the case where the permissions dialog was already opened before we called
                // Permission.RequestUserPermission in Awake() and we don't get the PermissionGranted callback
                if (Permission.HasUserAuthorizedPermission(OVRPermissionsRequester.ScenePermission))
                {
                    // We can't await for the LoadScene result because Awake is not async, silence the warning
#pragma warning disable CS4014
                    LoadScene(SceneSettings.DataSource);
#pragma warning restore CS4014

                }
#endif
            }
            if (_cameraRig)
            {
                if (EnableWorldLock)
                {
                    var room = GetCurrentRoom();
                    if (room)
                    {
                        if (room.UpdateWorldLock(out Vector3 position, out Quaternion rotation))
                        {
                            if (_prevTrackingSpacePose is Pose pose && (_cameraRig.trackingSpace.position != pose.position || _cameraRig.trackingSpace.rotation != pose.rotation))
                            {
                                Debug.LogWarning("MRUK EnableWorldLock is enabled and is controlling the tracking space position.\n" +
                                    $"Tracking position was set to {_cameraRig.trackingSpace.position} and rotation to {_cameraRig.trackingSpace.rotation}, this is being overridden by MRUK.");
                            }
                            _cameraRig.trackingSpace.SetPositionAndRotation(position, rotation);
                            _prevTrackingSpacePose = new(position, rotation);
                        }
                    }
                }
                else if (_worldLockWasEnabled)
                {
                    // Reset the tracking space when disabling world lock
                    _cameraRig.trackingSpace.localPosition = Vector3.zero;
                    _cameraRig.trackingSpace.localRotation = Quaternion.identity;
                    _prevTrackingSpacePose = null;
                }
                _worldLockWasEnabled = EnableWorldLock;
            }
        }

        /// <summary>
        /// Load the scene asynchronously from the specified data source
        /// </summary>
        async Task LoadScene(SceneDataSource dataSource)
        {
            _loadSceneCalled = true;
            try
            {
                if (dataSource == SceneDataSource.Device ||
                    dataSource == SceneDataSource.DeviceWithPrefabFallback)
                {
                    await LoadSceneFromDevice();
                }
                if (dataSource == SceneDataSource.Prefab ||
                    (dataSource == SceneDataSource.DeviceWithPrefabFallback && Rooms.Count == 0))
                {
                    if (SceneSettings.RoomPrefabs.Length == 0)
                    {
                        Debug.LogWarning($"Failed to load room from prefab because prefabs list is empty");
                        return;
                    }

                    // Clone the roomPrefab, but essentially replace all its content
                    // if -1 or out of range, use a random one
                    var roomIndex = SceneSettings.RoomIndex;
                    if (roomIndex == -1)
                        roomIndex = UnityEngine.Random.Range(0, SceneSettings.RoomPrefabs.Length);

                    Debug.Log($"Loading prefab room {roomIndex}");

                    GameObject roomPrefab = SceneSettings.RoomPrefabs[roomIndex];
                    LoadSceneFromPrefab(roomPrefab);
                }

                if (dataSource == SceneDataSource.Json)
                {
                    if (SceneSettings.SceneJson == "")
                    {
                        Debug.LogWarning($"Empty SceneJson string provided");
                    }
                    else
                    {
                        LoadSceneFromJsonString(SceneSettings.SceneJson);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw;
            }
        }

        /// <summary>
        /// Called when the room is destroyed
        /// </summary>
        /// <remarks>
        /// This is used to keep the list of active rooms up to date.
        /// So there should never be any null entries in the list.
        /// </remarks>
        /// <param name="room"></param>
        internal void OnRoomDestroyed(MRUKRoom room)
        {
            Rooms.Remove(room);
            if (_cachedCurrentRoom == room)
            {
                _cachedCurrentRoom = null;
            }
        }

        /// <summary>
        /// Destroys the rooms and all children
        /// </summary>
        public void ClearScene()
        {
            foreach (var room in Rooms)
            {
                foreach (Transform child in room.transform)
                    Destroy(child.gameObject);
                Destroy(room.gameObject);
            }
            Rooms.Clear();
            _cachedCurrentRoom = null;
        }

        /// <summary>
        /// Loads the scene from the data stored on the device.
        /// </summary>
        /// <remarks>
        /// The user must have granted ScenePermissions or this will fail.
        ///
        /// In order to check if the user has granted permissions use the following call:
        /// Permission.HasUserAuthorizedPermission(OVRPermissionsRequester.ScenePermission)
        ///
        /// In order to request permissions from the user, use the following call:
        /// Permission.RequestUserPermission(OVRPermissionsRequester.ScenePermission, callbacks);
        /// </remarks>
        /// <param name="requestSceneCaptureIfNoDataFound">If true and no rooms are found when loading from device,
        /// the request space setup flow will be started.</param>
        /// <returns>An enum indicating whether loading was successful or not.</returns>
        public async Task<LoadDeviceResult> LoadSceneFromDevice(bool requestSceneCaptureIfNoDataFound = true)
        {
            var newSceneData = await CreateSceneDataFromDevice();

            if (newSceneData.Rooms.Count == 0)
            {
#if !UNITY_EDITOR && UNITY_ANDROID
                // If no rooms were loaded it could be due to missing scene permissions, check for this and print a warning
                // if that is the issue.
                if (!Permission.HasUserAuthorizedPermission(OVRPermissionsRequester.ScenePermission))
                {
                    Debug.LogWarning($"MRUK couldn't load any scene data. The app does not have permissions for {OVRPermissionsRequester.ScenePermission}.");
                    return LoadDeviceResult.NoScenePermission;
                }
#endif
                // There could be 0 rooms because the user has not setup their space on the device.
                // Trigger the space setup flow and try to load again if that is the case.
                if (requestSceneCaptureIfNoDataFound)
                {
                    if (await OVRScene.RequestSpaceSetup())
                    {
                        // Try again but this time don't request a space setup again if there are no rooms to avoid
                        // the user getting stuck in an infinite loop.
                        return await LoadSceneFromDevice(false);
                    }
                }
                return LoadDeviceResult.NoRoomsFound;
            }

            UpdateScene(newSceneData);

            InitializeScene();

            return LoadDeviceResult.Success;
        }

        /// <summary>
        /// Attempts to create scene data from the device.
        /// </summary>
        /// <returns>A tuple containing a boolean indicating whether the operation was successful, the created scene data, and a list of OVRAnchors.</returns>
        private async Task<Data.SceneData> CreateSceneDataFromDevice()
        {
            var sceneData = new Data.SceneData()
            {
                CoordinateSystem = SerializationHelpers.CoordinateSystem.Unity,
                Rooms = new List<Data.RoomData>()
            };

            var rooms = new List<OVRAnchor>();
            bool success = await OVRAnchor.FetchAnchorsAsync<OVRRoomLayout>(rooms);


#if UNITY_EDITOR
            OVRTelemetry.Start(TelemetryConstants.MarkerId.LoadSceneFromDevice)
                .AddAnnotation(TelemetryConstants.AnnotationType.NumRooms, rooms.Count.ToString())
                .SetResult(success ? OVRPlugin.Qpl.ResultType.Success : OVRPlugin.Qpl.ResultType.Fail)
                .Send();
#endif

            foreach (var roomAnchor in rooms)
            {
                var layout = roomAnchor.GetComponent<OVRRoomLayout>();
                if (!layout.TryGetRoomLayout(out var ceiling, out var floor, out var walls))
                {
                    Debug.LogWarning($"Failed to get room layout");
                }

                var room = roomAnchor.GetComponent<OVRAnchorContainer>();
                var childAnchors = new List<OVRAnchor>();
                await room.FetchChildrenAsync(childAnchors);

                var roomData = new Data.RoomData()
                {
                    Anchor = roomAnchor,
                    Anchors = new List<Data.AnchorData>(),
                    RoomLayout = new Data.RoomLayoutData()
                    {
                        FloorUuid = floor,
                        CeilingUuid = ceiling,
                        WallsUuid = new List<Guid>(walls?.Length ?? 0),
                    }
                };


                // Make sure order of the walls is preserved
                foreach (var wall in walls)
                {
                    roomData.RoomLayout.WallsUuid.Add(wall);
                }

                // Enable locatable on all the child anchors, we do this before accessing all the positions
                // so that when we actually go to access them they are already enabled and we can get all
                // the position on the same frame. Or as close as possible to the same frame.
                var tasks = new List<OVRTask<bool>>();
                foreach (var child in childAnchors)
                {
                    if (!child.TryGetComponent<OVRLocatable>(out var locatable))
                        continue;

                    tasks.Add(locatable.SetEnabledAsync(true));
                }
                await OVRTask.WhenAll(tasks);

                foreach (var child in childAnchors)
                {
                    var anchorData = new Data.AnchorData
                    {
                        Anchor = child,
                    };
                    var splitLabels = new List<string>();
                    if (child.TryGetComponent(out OVRSemanticLabels labels) && labels.IsEnabled)
                    {
                        splitLabels.AddRange(labels.Labels.Split(','));
                    }
                    anchorData.SemanticClassifications = splitLabels;
                    if (child.TryGetComponent(out OVRBounded2D bounds2) && bounds2.IsEnabled)
                    {
                        anchorData.PlaneBounds = new Data.PlaneBoundsData()
                        {
                            Min = bounds2.BoundingBox.min,
                            Max = bounds2.BoundingBox.max,
                        };

                        if (bounds2.TryGetBoundaryPointsCount(out var counts))
                        {
                            using var boundary = new NativeArray<Vector2>(counts, Allocator.Temp);
                            if (bounds2.TryGetBoundaryPoints(boundary))
                            {
                                anchorData.PlaneBoundary2D = new List<Vector2>();
                                for (int i = 0; i < counts; i++)
                                {
                                    anchorData.PlaneBoundary2D.Add(boundary[i]);
                                }
                            }
                        }
                    }
                    if (child.TryGetComponent(out OVRBounded3D bounds3) && bounds3.IsEnabled)
                    {
                        anchorData.VolumeBounds = new Data.VolumeBoundsData()
                        {
                            Min = bounds3.BoundingBox.min,
                            Max = bounds3.BoundingBox.max,
                        };
                    }

                    anchorData.Transform.Scale = Vector3.one;
                    bool gotLocation = false;
                    if (child.TryGetComponent(out OVRLocatable locatable) && locatable.IsEnabled)
                    {
                        if (locatable.TryGetSceneAnchorPose(out var pose))
                        {
                            var position = pose.ComputeWorldPosition(Camera.main);
                            var rotation = pose.ComputeWorldRotation(Camera.main);
                            if (rotation.HasValue && rotation.HasValue)
                            {
                                anchorData.Transform.Translation = position.Value;
                                anchorData.Transform.Rotation = rotation.Value.eulerAngles;
                                gotLocation = true;
                            }
                        }
                    }
                    if (!gotLocation)
                    {
                        Debug.LogWarning($"Failed to get location of anchor with UUID: {anchorData.Anchor.Uuid}");
                    }

                    roomData.Anchors.Add(anchorData);
                }
                sceneData.Rooms.Add(roomData);
            }
            return sceneData;
        }

        private void FindAllObjects(GameObject roomPrefab, out List<GameObject> walls, out List<GameObject> volumes,
            out List<GameObject> planes)
        {
            walls = new List<GameObject>();
            volumes = new List<GameObject>();
            planes = new List<GameObject>();
            FindObjects(MRUKAnchor.SceneLabels.WALL_FACE.ToString(), roomPrefab.transform, ref walls);
            FindObjects(MRUKAnchor.SceneLabels.INVISIBLE_WALL_FACE.ToString(), roomPrefab.transform, ref walls);
            FindObjects(MRUKAnchor.SceneLabels.OTHER.ToString(), roomPrefab.transform, ref volumes);
            FindObjects(MRUKAnchor.SceneLabels.TABLE.ToString(), roomPrefab.transform, ref volumes);
            FindObjects(MRUKAnchor.SceneLabels.COUCH.ToString(), roomPrefab.transform, ref volumes);
            FindObjects(MRUKAnchor.SceneLabels.WINDOW_FRAME.ToString(), roomPrefab.transform, ref planes);
            FindObjects(MRUKAnchor.SceneLabels.DOOR_FRAME.ToString(), roomPrefab.transform, ref planes);
            FindObjects(MRUKAnchor.SceneLabels.WALL_ART.ToString(), roomPrefab.transform, ref planes);
            FindObjects(MRUKAnchor.SceneLabels.PLANT.ToString(), roomPrefab.transform, ref volumes);
            FindObjects(MRUKAnchor.SceneLabels.SCREEN.ToString(), roomPrefab.transform, ref volumes);
            FindObjects(MRUKAnchor.SceneLabels.BED.ToString(), roomPrefab.transform, ref volumes);
            FindObjects(MRUKAnchor.SceneLabels.LAMP.ToString(), roomPrefab.transform, ref volumes);
            FindObjects(MRUKAnchor.SceneLabels.STORAGE.ToString(), roomPrefab.transform, ref volumes);
        }

        /// <summary>
        /// This simulates the creation of a scene in the Editor, using transforms and names from our prefab rooms.
        /// </summary>
        public void LoadSceneFromPrefab(GameObject scenePrefab, bool clearSceneFirst = true)
        {
#if UNITY_EDITOR
            OVRTelemetry.Start(TelemetryConstants.MarkerId.LoadSceneFromPrefab)
                .AddAnnotation(TelemetryConstants.AnnotationType.SceneName, scenePrefab.name)
                .Send();
#endif

            if (clearSceneFirst)
            {
                ClearScene();
            }

            // first, examine prefab to determine if it's a single room or collection of rooms
            // if the hierarchy is more than two levels deep, consider it a home
            if (scenePrefab.transform.childCount > 0 && scenePrefab.transform.GetChild(0).childCount > 0)
            {
                foreach (Transform room in scenePrefab.transform)
                {
                    LoadRoomFromPrefab(room.gameObject);
                }
            }
            else
            {
                LoadRoomFromPrefab(scenePrefab);
            }

            InitializeScene();
        }

        private void LoadRoomFromPrefab(GameObject roomPrefab)
        {
            FindAllObjects(roomPrefab, out var walls, out var volumes, out var planes);

            GameObject sceneRoom = new GameObject(roomPrefab.name);
            MRUKRoom roomInfo = sceneRoom.AddComponent<MRUKRoom>();
            roomInfo.Anchor = new OVRAnchor(0, Guid.NewGuid());

            // walls ordered sequentially, CW when viewed top-down
            List<MRUKAnchor> orderedWalls = new List<MRUKAnchor>();

            List<MRUKAnchor> unorderedWalls = new List<MRUKAnchor>();
            List<Vector3> floorCorners = new List<Vector3>();

            float wallHeight = 0.0f;

            for (int i = 0; i < walls.Count; i++)
            {
                if (i == 0)
                {
                    wallHeight = walls[i].transform.localScale.y;
                }

                MRUKAnchor objData = CreateAnchorFromRoomObject(walls[i].transform, walls[i].transform.localScale, AnchorRepresentation.PLANE);
                objData.Room = roomInfo;

                // if this is an INVISIBLE_WALL_FACE, it also needs the WALL_FACE label
                if (walls[i].name.Equals(MRUKAnchor.SceneLabels.INVISIBLE_WALL_FACE.ToString()))
                {
                    objData.AnchorLabels.Add(MRUKAnchor.SceneLabels.WALL_FACE.ToString());
                }

                objData.transform.parent = sceneRoom.transform;
                objData.transform.Rotate(0, 180, 0);

                unorderedWalls.Add(objData);
                roomInfo.Anchors.Add(objData);
            }

            // There may be imprecision between the prefab walls (misaligned edges)
            // so, we shift them so the edges perfectly match up:
            // bottom left corner of wall is fixed, right corner matches left corner of wall to the right
            int seedId = 0;
            for (int i = 0; i < unorderedWalls.Count; i++)
            {
                MRUKAnchor wall = GetAdjacentWall(ref seedId, unorderedWalls);
                orderedWalls.Add(wall);

                Rect wallRect = wall.PlaneRect.Value;
                Vector3 leftCorner = wall.transform.TransformPoint(new Vector3(wallRect.max.x, wallRect.min.y, 0.0f));
                floorCorners.Add(leftCorner);
            }
            for (int i = 0; i < orderedWalls.Count; i++)
            {
                Rect planeRect = orderedWalls[i].PlaneRect.Value;
                Vector3 corner1 = floorCorners[i];
                int nextID = (i == orderedWalls.Count - 1) ? 0 : i + 1;
                Vector3 corner2 = floorCorners[nextID];

                Vector3 wallRight = (corner1 - corner2);
                wallRight.y = 0.0f;
                float wallWidth = wallRight.magnitude;
                wallRight /= wallWidth;
                Vector3 wallUp = Vector3.up;
                Vector3 wallFwd = Vector3.Cross(wallRight, wallUp);
                Vector3 newPosition = (corner1 + corner2) * 0.5f + Vector3.up * (planeRect.height * 0.5f);
                Quaternion newRotation = Quaternion.LookRotation(wallFwd, wallUp);
                Rect newRect = new Rect(-0.5f * wallWidth, planeRect.y, wallWidth, planeRect.height);

                orderedWalls[i].transform.position = newPosition;
                orderedWalls[i].transform.rotation = newRotation;
                orderedWalls[i].PlaneRect = newRect;
                orderedWalls[i].PlaneBoundary2D = new List<Vector2>
                {
                    new Vector2(newRect.xMin, newRect.yMin),
                    new Vector2(newRect.xMax, newRect.yMin),
                    new Vector2(newRect.xMax, newRect.yMax),
                    new Vector2(newRect.xMin, newRect.yMax),
                };

                roomInfo.WallAnchors.Add(orderedWalls[i]);
            }

            for (int i = 0; i < volumes.Count; i++)
            {
                Vector3 cubeScale = new Vector3(volumes[i].transform.localScale.x, volumes[i].transform.localScale.z, volumes[i].transform.localScale.y);
                var representation = AnchorRepresentation.VOLUME;
                // Table and couch are special. They also have a plane attached to them.
                if (volumes[i].transform.name == MRUKAnchor.SceneLabels.TABLE.ToString() ||
                    volumes[i].transform.name == MRUKAnchor.SceneLabels.COUCH.ToString())
                {
                    representation |= AnchorRepresentation.PLANE;
                }
                MRUKAnchor objData = CreateAnchorFromRoomObject(volumes[i].transform, cubeScale, representation);
                objData.transform.parent = sceneRoom.transform;
                objData.Room = roomInfo;

                // in the prefab rooms, the cubes are more Unity-like and default: Y is up, pivot is centered
                // this needs to be converted to Scene format, in which the pivot is on top of the cube and Z is up
                objData.transform.position += cubeScale.z * 0.5f * Vector3.up;
                objData.transform.Rotate(new Vector3(-90, 0, 0), Space.Self);
                roomInfo.Anchors.Add(objData);
            }
            for (int i = 0; i < planes.Count; i++)
            {
                MRUKAnchor objData = CreateAnchorFromRoomObject(planes[i].transform, planes[i].transform.localScale, AnchorRepresentation.PLANE);
                objData.transform.parent = sceneRoom.transform;
                objData.Room = roomInfo;

                // Unity quads have a surface normal facing the opposite direction
                // Rather than have "backwards" walls in the room prefab, we just rotate them here
                objData.transform.Rotate(0, 180, 0);
                roomInfo.Anchors.Add(objData);
            }

            // mimic OVRSceneManager: floor/ceiling anchor aligns with longest wall, scaled to room size
            MRUKAnchor longestWall = null;
            float longestWidth = 0.0f;
            foreach (var wall in orderedWalls)
            {
                float wallWidth = wall.PlaneRect.Value.size.x;
                if (wallWidth > longestWidth)
                {
                    longestWidth = wallWidth;
                    longestWall = wall;
                }
            }

            // calculate the room bounds, relative to the longest wall
            float zMin = 0.0f;
            float zMax = 0.0f;
            float xMin = 0.0f;
            float xMax = 0.0f;
            for (int i = 0; i < floorCorners.Count; i++)
            {
                Vector3 localPos = longestWall.transform.InverseTransformPoint(floorCorners[i]);

                zMin = i == 0 ? localPos.z : Mathf.Min(zMin, localPos.z);
                zMax = i == 0 ? localPos.z : Mathf.Max(zMax, localPos.z);
                xMin = i == 0 ? localPos.x : Mathf.Min(xMin, localPos.x);
                xMax = i == 0 ? localPos.x : Mathf.Max(xMax, localPos.x);
            }
            Vector3 localRoomCenter = new Vector3((xMin + xMax) * 0.5f, 0, (zMin + zMax) * 0.5f);
            Vector3 roomCenter = longestWall.transform.TransformPoint(localRoomCenter);
            roomCenter -= Vector3.up * wallHeight * 0.5f;
            Vector3 floorScale = new Vector3(zMax - zMin, xMax - xMin, 1);

            for (int i = 0; i < 2; i++)
            {
                string anchorName = (i == 0 ? "FLOOR" : "CEILING");

                var position = roomCenter + Vector3.up * wallHeight * i;
                float anchorFlip = i == 0 ? 1 : -1;
                var rotation = Quaternion.LookRotation(longestWall.transform.up * anchorFlip, longestWall.transform.right);
                MRUKAnchor objData = CreateAnchor(anchorName, position, rotation, floorScale, AnchorRepresentation.PLANE);
                objData.transform.parent = sceneRoom.transform;
                objData.Room = roomInfo;

                objData.PlaneBoundary2D = new(floorCorners.Count);
                foreach (var corner in floorCorners)
                {
                    var localCorner = objData.transform.InverseTransformPoint(corner);
                    objData.PlaneBoundary2D.Add(new Vector2(localCorner.x, localCorner.y));
                }

                if (i == 1)
                {
                    objData.PlaneBoundary2D.Reverse();
                }
                roomInfo.Anchors.Add(objData);
                if (i == 0)
                {
                    roomInfo.FloorAnchor = objData;
                }
                else
                {
                    roomInfo.CeilingAnchor = objData;
                }
            }

            // after everything, we need to let the room computation run
            roomInfo.ComputeRoomInfo();
            Rooms.Add(roomInfo);
        }

        /// <summary>
        /// Serializes the current scene into a JSON string using the specified coordinate system for serialization.
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system to be used for serialization (Unity/Unreal).</param>
        /// <returns>A JSON string representing the serialized scene data.</returns>
        public string SaveSceneToJsonString(SerializationHelpers.CoordinateSystem coordinateSystem)
        {
            return SerializationHelpers.Serialize(coordinateSystem);
        }

        /// <summary>
        /// Loads the scene from a JSON string representing the scene data.
        /// </summary>
        /// <param name="jsonString">The JSON string containing the serialized scene data.</param>
        public void LoadSceneFromJsonString(string jsonString)
        {
            var newSceneData = SerializationHelpers.Deserialize(jsonString);

            UpdateScene(newSceneData);

#if UNITY_EDITOR
            OVRTelemetry.Start(TelemetryConstants.MarkerId.LoadSceneFromJson)
                .AddAnnotation(TelemetryConstants.AnnotationType.NumRooms, Rooms.Count.ToString())
                .Send();
#endif

            InitializeScene();
        }

        private MRUKAnchor CreateAnchorFromRoomObject(Transform refObject, Vector3 objScale, AnchorRepresentation representation)
        {
            return CreateAnchor(refObject.name, refObject.position, refObject.rotation, objScale, representation);
        }

        /// <summary>
        /// Creates an anchor with the specified properties.
        /// </summary>
        /// <param name="name">The name of the anchor.</param>
        /// <param name="position">The position of the anchor.</param>
        /// <param name="rotation">The rotation of the anchor.</param>
        /// <param name="objScale">The scale of the anchor.</param>
        /// <param name="representation">The representation of the anchor (plane or volume).</param>
        /// <returns>The created anchor.</returns>
        private MRUKAnchor CreateAnchor(string name, Vector3 position, Quaternion rotation, Vector3 objScale, AnchorRepresentation representation)
        {
            var realAnchor = new GameObject(name);
            realAnchor.transform.position = position;
            realAnchor.transform.rotation = rotation;
            MRUKAnchor objData = realAnchor.AddComponent<MRUKAnchor>();
            objData.AnchorLabels.Add(realAnchor.name);
            if ((representation & AnchorRepresentation.PLANE) != 0)
            {
                var size2d = new Vector2(objScale.x, objScale.y);
                var rect = new Rect(-0.5f * size2d, size2d);
                objData.PlaneRect = rect;
                objData.PlaneBoundary2D = new List<Vector2>
                {
                    new Vector2(rect.xMin, rect.yMin),
                    new Vector2(rect.xMax, rect.yMin),
                    new Vector2(rect.xMax, rect.yMax),
                    new Vector2(rect.xMin, rect.yMax),
                };
            }
            if ((representation & AnchorRepresentation.VOLUME) != 0)
            {
                Vector3 offsetCenter = new Vector3(0, 0, -objScale.z * 0.5f);
                objData.VolumeBounds = new Bounds(offsetCenter, objScale);
            }
            objData.Anchor = new OVRAnchor(0, Guid.NewGuid());
            return objData;
        }

        void FindObjects(string objName, Transform rootTransform, ref List<GameObject> objList)
        {
            if (rootTransform.name.Equals(objName))
            {
                objList.Add(rootTransform.gameObject);
            }

            foreach (Transform child in rootTransform)
            {
                FindObjects(objName, child, ref objList);
            }
        }

        MRUKAnchor GetAdjacentWall(ref int thisID, List<MRUKAnchor> randomWalls)
        {
            Vector2 thisWallScale = randomWalls[thisID].PlaneRect.Value.size;

            Vector3 halfScale = thisWallScale * 0.5f;
            Vector3 bottomRight = randomWalls[thisID].transform.position - randomWalls[thisID].transform.up * halfScale.y - randomWalls[thisID].transform.right * halfScale.x;
            float closestCornerDistance = Mathf.Infinity;
            // When searching for a matching corner, the correct one should match positions. If they don't, assume there's a crack in the room.
            // This should be an impossible scenario and likely means broken data from Room Setup.
            int rightWallID = 0;
            for (int i = 0; i < randomWalls.Count; i++)
            {
                // compare to bottom left point of other walls
                if (i != thisID)
                {
                    Vector2 testWallHalfScale = randomWalls[i].PlaneRect.Value.size * 0.5f;
                    Vector3 bottomLeft = randomWalls[i].transform.position - randomWalls[i].transform.up * testWallHalfScale.y + randomWalls[i].transform.right * testWallHalfScale.x;
                    float thisCornerDistance = Vector3.Distance(bottomLeft, bottomRight);
                    if (thisCornerDistance < closestCornerDistance)
                    {
                        closestCornerDistance = thisCornerDistance;
                        rightWallID = i;
                    }
                }
            }
            thisID = rightWallID;
            return randomWalls[thisID];
        }

        /// <summary>
        /// Manages the scene by creating, updating, or deleting rooms and anchors based on new scene data.
        /// </summary>
        /// <param name="newSceneData">The new scene data.</param>
        /// <returns>A list of managed MRUKRoom objects.</returns>
        private void UpdateScene(Data.SceneData newSceneData)
        {
            List<Data.RoomData> newRoomsToCreate = new();
            List<MRUKRoom> rooms = new();

            //the existing rooms will get removed from this list and updated separately
            newRoomsToCreate.AddRange(newSceneData.Rooms);

            List<MRUKRoom> roomsToRemove = new();

            //check old rooms to see if a new received room match and then perform update on room,
            //update,delete or create on anchors.
            foreach (var oldRoom in Rooms)
            {
                bool foundRoom = false;
                foreach (var newRoomData in newSceneData.Rooms)
                {
                    if (oldRoom.IsIdenticalRoom(newRoomData))
                    {
                        foundRoom = true;
                    }
                    else if (oldRoom.IsSameRoom(newRoomData))
                    {
                        foundRoom = true;

                        // Found the same room but we need to update the anchors within it
                        oldRoom.Anchor = newRoomData.Anchor;
                        oldRoom.UpdateRoomLabel(newRoomData);

                        List<Tuple<MRUKAnchor, Data.AnchorData>> anchorsToUpdate = new();
                        List<Data.AnchorData> newAnchorsToCreate = new();
                        List<MRUKAnchor> anchorsToRemove = new();

                        // Find anchors to create and update
                        foreach (var anchorData in newRoomData.Anchors)
                        {
                            bool foundMatch = false;
                            foreach (var oldAnchor in oldRoom.Anchors)
                            {
                                if (oldAnchor.Equals(anchorData))
                                {
                                    foundMatch = true;
                                    break;
                                }

                                if (oldAnchor.Anchor == anchorData.Anchor)
                                {
                                    anchorsToUpdate.Add(
                                        new Tuple<MRUKAnchor, Data.AnchorData>(oldAnchor, anchorData));

                                    foundMatch = true;
                                    break;
                                }
                            }

                            if (!foundMatch)
                            {
                                newAnchorsToCreate.Add(anchorData);
                            }
                        }

                        // Find anchors to remove
                        foreach (var oldAnchor in oldRoom.Anchors)
                        {
                            bool foundAnchor = false;
                            foreach (var anchorData in newRoomData.Anchors)
                            {
                                if (oldAnchor.Anchor == anchorData.Anchor)
                                {
                                    foundAnchor = true;
                                    break;
                                }
                            }

                            if (!foundAnchor)
                            {
                                anchorsToRemove.Add(oldAnchor);
                            }
                        }

                        foreach (var anchor in newAnchorsToCreate)
                        {
                            oldRoom.CreateAnchor(anchor);
                        }

                        foreach (var anchor in anchorsToRemove)
                        {
                            oldRoom.AnchorRemovedEvent?.Invoke(anchor);
                            oldRoom.RemoveAndDestroyAnchor(anchor);
                        }

                        foreach (var anchorToUpdate in anchorsToUpdate)
                        {
                            var oldAnchor = anchorToUpdate.Item1;
                            var newAnchorData = anchorToUpdate.Item2;
                            oldAnchor.UpdateAnchor(newAnchorData);
                            oldRoom.AnchorUpdatedEvent?.Invoke(oldAnchor);
                        }

                        oldRoom.UpdateRoomLayout(newRoomData.RoomLayout);
                        RoomUpdatedEvent?.Invoke(oldRoom);
                    }

                    if (foundRoom)
                    {
                        rooms.Add(oldRoom);
                        for (int i = 0; i < newRoomsToCreate.Count; ++i)
                        {
                            if (newRoomsToCreate[i].Anchor == newRoomData.Anchor)
                            {
                                newRoomsToCreate.RemoveAt(i);
                                break;
                            }
                        }
                        break;
                    }
                }

                if (!foundRoom)
                {
                    roomsToRemove.Add(oldRoom);
                }
            }

            foreach (var oldRoom in roomsToRemove)
            {
                for (int j = oldRoom.Anchors.Count - 1; j >= 0; j--)
                {
                    var anchor = oldRoom.Anchors[j];
                    oldRoom.Anchors.Remove(anchor);
                    oldRoom.AnchorRemovedEvent?.Invoke(anchor);
                    Utilities.DestroyGameObjectAndChildren(anchor.gameObject);
                }

                Rooms.Remove(oldRoom);
                RoomRemovedEvent?.Invoke(oldRoom);

                Utilities.DestroyGameObjectAndChildren(oldRoom.gameObject);

            }

            foreach (var newRoomData in newRoomsToCreate)
            {
                //create room and throw events for room and anchors
                var room = CreateRoom(newRoomData);
                rooms.Add(room);
                RoomCreatedEvent?.Invoke(room);
            }

            Rooms.Clear();
            Rooms.AddRange(rooms);

            foreach (var room in Rooms)
            {
                // after everything, we need to let the room computation run
                room.ComputeRoomInfo();
            }
        }

        /// <summary>
        /// Creates a new room with the specified parameters.
        /// </summary>
        /// <param name="roomData">The data for the new room.</param>
        /// <returns>The created MRUKRoom object.</returns>
        private MRUKRoom CreateRoom(Data.RoomData roomData)
        {
            GameObject sceneRoom = new GameObject();
            MRUKRoom roomInfo = sceneRoom.AddComponent<MRUKRoom>();

            roomInfo.Anchor = roomData.Anchor;

            foreach (Data.AnchorData anchorData in roomData.Anchors)
            {
                roomInfo.CreateAnchor(anchorData);
            }

            roomInfo.UpdateRoomLabel(roomData);
            roomInfo.UpdateRoomLayout(roomData.RoomLayout);

            return roomInfo;
        }
    }
}
