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
using UnityEngine.Events;
using Unity.AI.Navigation;
using UnityEngine.AI;
using System.Collections.Generic;

namespace Meta.XR.MRUtilityKit
{
    public class SceneNavigation : MonoBehaviour
    {
        [Header("Custom Nav Mesh Settings")]
        [Tooltip("Used for specifying the type of geometry to collect when building a NavMesh")]
        public NavMeshCollectGeometry CollectGeometry = NavMeshCollectGeometry.PhysicsColliders;

        [Tooltip("Used for specifying the type of objects to include when building a NavMesh")]
        public CollectObjects CollectObjects = CollectObjects.Children;

        [Tooltip("The minimum distance to the walls where the navigation mesh can exist.")]
        public float AgentRadius = 0.2f;

        [Tooltip("How much vertical clearance space must exist.")]
        public float AgentHeight = 0.5f;

        [Tooltip("The height of discontinuities in the level the agent can climb over (i.e. steps and stairs).")]
        public float AgentClimb = 0.04f;

        [Tooltip("Maximum slope the agent can walk up.")]
        public float AgentMaxSlope = 5.5f;

        [Tooltip("The agents that will be assigned to the NavMesh generated with the scene data.")]
        public List<NavMeshAgent> Agents;

        [Header("Scene Settings")]
        [Tooltip("The scene objects that will contribute to the creation of the NavMesh.")]
        public MRUKAnchor.SceneLabels SceneObjectsToInclude;
        public LayerMask Layers;

        [Space(10)]
        public UnityEvent OnNavMeshInitialized = new();
        private NavMeshSurface _navMeshSurface;
        private float _minimumNavMeshSurfaceArea = 0;
        private EffectMesh _effectMesh;
        private MRUKAnchor.SceneLabels _sceneLabels;

        private void Awake()
        {
            _navMeshSurface = gameObject.GetComponent<NavMeshSurface>();
            _sceneLabels = SceneObjectsToInclude;
        }

        /// <summary>
        /// Toggles the use of global mesh for navigation.
        /// </summary>
        /// <param name="agentTypeID">The agent type ID to use for creating the scene nav mesh,
        /// if not specified, a new agent will be created.</param>
        public void ToggleGlobalMeshNavigation(bool useGlobalMesh, int agentTypeID = -1)
        {
            if (MRUK.Instance.GetCurrentRoom().GlobalMeshAnchor == null)
            {
                Debug.LogWarning("[MRUK] No Global Mesh anchor was found in the scene.");
                return;
            }
            _sceneLabels = useGlobalMesh ? MRUKAnchor.SceneLabels.GLOBAL_MESH : SceneObjectsToInclude;
            CreateEffectMesh();
            RemoveNavMeshData();
            if (agentTypeID == -1)
            {
                BuildSceneNavMesh();
            }
            else
            {
                BuildSceneNavMeshFromExistingAgent(agentTypeID);
            }
        }

        /// <summary>
        /// Creates a navigation mesh for the scene.
        /// </summary>
        /// <remarks>
        /// This method creates a navigation mesh by collecting geometry from the scene,
        /// building the navigation mesh data, and adding it to the NavMesh.
        /// Currently Unity does not allow the creation of custom NavMeshAgents at runtime.
        /// It also assigns the created navigation mesh to all NavMeshAgents in the scene.
        /// </remarks>
        public void BuildSceneNavMesh()
        {
            if (!_navMeshSurface)
                CreateNavMeshSurface(); // in case no NavMeshSurface component was found, create a new one
            RemoveNavMeshData(); // clean up previous data
            var navMeshBounds = ResizeNavMeshFromRoomBounds(ref _navMeshSurface); // resize the nav mesh to fit the room
            var navMeshBuildSettings = CreateNavMeshBuildSettings(AgentRadius, AgentHeight, AgentMaxSlope, AgentClimb);

            NavMeshData data = CreateNavMeshData(navMeshBounds, navMeshBuildSettings);
            _navMeshSurface.navMeshData = data;
            _navMeshSurface.agentTypeID = navMeshBuildSettings.agentTypeID;
            _navMeshSurface.AddData();

            InitializeNavMesh(navMeshBuildSettings.agentTypeID);
        }

        /// <summary>
        /// Creates a navigation mesh from an existing NavMeshAgent.
        /// </summary>
        /// <param name="agentIndex">The index of the NavMeshAgent to create the navigation mesh from.</param>
        public void BuildSceneNavMeshFromExistingAgent(int agentIndex)
        {
            if (!_navMeshSurface)
                CreateNavMeshSurface();
            RemoveNavMeshData(); // clean up previous data
            var navMeshBuildSettings = NavMesh.GetSettingsByID(agentIndex);
            _navMeshSurface.agentTypeID = navMeshBuildSettings.agentTypeID;
            _navMeshSurface.BuildNavMesh();
            InitializeNavMesh(navMeshBuildSettings.agentTypeID);
        }

        /// <summary>
        /// Creates a new NavMeshBuildSettings object with the specified agent properties.
        /// </summary>
        /// <param name="agentRadius">The minimum distance to the walls where the navigation mesh can exist.</param>
        /// <param name="agentHeight">How much vertical clearance space must exist.</param>
        /// <param name="agentMaxSlope">Maximum slope the agent can walk up.</param>
        /// <param name="agentClimb">The height of discontinuities in the level the agent can climb over (i.e. steps and stairs).</param>
        /// <returns>A new NavMeshBuildSettings object with the specified agent properties.</returns>
        public NavMeshBuildSettings CreateNavMeshBuildSettings(float agentRadius, float agentHeight, float agentMaxSlope, float agentClimb)
        {
            var settings = NavMesh.CreateSettings();
            settings.agentRadius = agentRadius;
            settings.agentHeight = agentHeight;
            settings.agentSlope = agentMaxSlope;
            settings.agentClimb = agentClimb;
            return settings;
        }

        /// <summary>
        /// Creates a NavMeshSurface component and sets its properties.
        /// </summary>
        public void CreateNavMeshSurface()
        {
            _navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            ResizeNavMeshFromRoomBounds(ref _navMeshSurface);
            _navMeshSurface.collectObjects = CollectObjects;
            _navMeshSurface.useGeometry = CollectGeometry;
            _navMeshSurface.layerMask = Layers;
        }

        /// <summary>
        /// Removes the NavMeshData from the NavMeshSurface component.
        /// </summary>
        public void RemoveNavMeshData()
        {
            if (!_navMeshSurface)
                return;
            _navMeshSurface.navMeshData = null;
            _navMeshSurface.RemoveData();
        }

        /// <summary>
        /// Resizes the NavMeshSurface to fit the room bounds.
        /// </summary>
        /// <param name="surface">The NavMeshSurface to resize.</param>
        /// <returns>The bounds of the resized NavMeshSurface.</returns>
        public Bounds ResizeNavMeshFromRoomBounds(ref NavMeshSurface surface)
        {
            var mapBounds = MRUK.Instance.GetCurrentRoom().GetRoomBounds();
            var mapCenter = new Vector3(mapBounds.center.x, mapBounds.min.y, mapBounds.center.z);
            surface.center = mapCenter;

            var mapScale = new Vector3(mapBounds.size.x, 2f, mapBounds.size.z);
            surface.size = mapScale;
            var bounds = new Bounds(surface.center, Abs(surface.size));
            return bounds;
        }

        /// <summary>
        /// Initializes the navigation mesh with the given agent type ID.
        /// </summary>
        /// <param name="agentTypeID">The agent type ID to initialize the navigation mesh with.</param>
        private void InitializeNavMesh(int agentTypeID)
        {
            if (_navMeshSurface.navMeshData.sourceBounds.extents.x * _navMeshSurface.navMeshData.sourceBounds.extents.z >
                _minimumNavMeshSurfaceArea)
            {
                if (Agents != null)
                {
                    foreach (var agent in Agents)
                    {
                        agent.agentTypeID = agentTypeID;
                    }
                }
                OnNavMeshInitialized?.Invoke();
            }
            else
            {
                Debug.LogWarning("Failed to generate a nav mesh, this may be because the room is too small" +
                    " or the AgentType settings are to strict");
            }
        }

        /// <summary>
        /// Creates NavMeshData for the given bounds and build settings.
        /// </summary>
        /// <param name="navMeshBounds">The bounds for the NavMesh.</param>
        /// <param name="navMeshBuildSettings">The build settings for the NavMesh.</param>
        /// <returns>The created NavMeshData.</returns>
        private NavMeshData CreateNavMeshData(Bounds navMeshBounds, NavMeshBuildSettings navMeshBuildSettings)
        {
            List<NavMeshBuildSource> sources = new();
            if (_navMeshSurface.collectObjects == CollectObjects.Volume)
            {
                NavMeshBuilder.CollectSources(navMeshBounds, _navMeshSurface.layerMask,
                    _navMeshSurface.useGeometry, 0, new List<NavMeshBuildMarkup>(), sources);
            }
            else
            {
                CreateEffectMesh();
                NavMeshBuilder.CollectSources(transform, _navMeshSurface.layerMask,
                    _navMeshSurface.useGeometry, 0, new List<NavMeshBuildMarkup>(), sources);
            }
            var data = NavMeshBuilder.BuildNavMeshData(navMeshBuildSettings, sources, navMeshBounds,
                transform.position, transform.rotation);
            return data;
        }

        /// <summary>
        /// Creates an EffectMesh component and sets its properties using the SceneLabels defined on the script.
        /// The effect mesh will create the colliders needed to build the NavMesh
        /// </summary>
        private void CreateEffectMesh()
        {
            if (!_effectMesh)
            {
                _effectMesh = gameObject.AddComponent<EffectMesh>();
                _effectMesh.HideMesh = true;
                _effectMesh.Colliders = true;
                _effectMesh.BorderSize = 0.2f;
            }
            _effectMesh.DestroyMesh();
            _effectMesh.Labels = _sceneLabels;
            _effectMesh.CreateMesh();
            _effectMesh.SetEffectObjectsParent(transform);
            _navMeshSurface.collectObjects = CollectObjects.Children;
        }

        static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        private void OnDestroy()
        {
            OnNavMeshInitialized.RemoveAllListeners();
        }
    }
}
