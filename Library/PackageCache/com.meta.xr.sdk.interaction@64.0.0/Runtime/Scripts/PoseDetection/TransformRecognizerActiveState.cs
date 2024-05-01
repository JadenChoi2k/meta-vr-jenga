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

using Oculus.Interaction.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Oculus.Interaction.PoseDetection
{
    [Serializable]
    public class TransformFeatureConfigList
    {
        [SerializeField]
        private List<TransformFeatureConfig> _values;

        public List<TransformFeatureConfig> Values => _values;
    }

    [Serializable]
    public class TransformFeatureConfig : FeatureConfigBase<TransformFeature>
    {
    }

    /// <summary>
    /// Used in hand pose detection to get the current state of the hand's transforms and compares it to the required transforms. If both match, the state is active.
    /// </summary>
    public class TransformRecognizerActiveState : MonoBehaviour, IActiveState
    {
        /// <summary>
        /// The hand to read for transform state data.
        /// </summary>
        [SerializeField, Interface(typeof(IHand))]
        private UnityEngine.Object _hand;
        public IHand Hand { get; private set; }
        /// <summary>
        /// An <cref="ITransformFeatureStateProvider" />, which provides the current state of the tracked hand's transforms.
        /// </summary>
        [SerializeField, Interface(typeof(ITransformFeatureStateProvider))]
        private UnityEngine.Object _transformFeatureStateProvider;

        protected ITransformFeatureStateProvider TransformFeatureStateProvider;
        /// <summary>
        /// A list of required transforms that the tracked hand must match for the pose to become active (assuming all shapes are also active).
        /// Each transform is an orientation and a boolean (ex. PalmTowardsFace is True.)
        /// </summary>
        [SerializeField]
        private TransformFeatureConfigList _transformFeatureConfigs;

        /// <summary>
        /// Influences state transitions computed via <cref="TransformFeatureStateProvider" />. It becomes active whenever all of the listed transform states are active.
        /// State provider uses this to determine the state of features during real time, so edit at runtime at your own risk.
        /// </summary>
        [SerializeField]
        [Tooltip("State provider uses this to determine the state of features during real time, so" +
            " edit at runtime at your own risk.")]
        private TransformConfig _transformConfig;

        public IReadOnlyList<TransformFeatureConfig> FeatureConfigs => _transformFeatureConfigs.Values;

        public TransformConfig TransformConfig => _transformConfig;

        protected bool _started = false;

        protected virtual void Awake()
        {
            Hand = _hand as IHand;
            TransformFeatureStateProvider =
                _transformFeatureStateProvider as ITransformFeatureStateProvider;
        }

        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            this.AssertField(Hand, nameof(Hand));
            this.AssertField(TransformFeatureStateProvider, nameof(TransformFeatureStateProvider));

            this.AssertField(_transformFeatureConfigs, nameof(_transformFeatureConfigs));
            this.AssertField(_transformConfig, nameof(_transformConfig));

            _transformConfig.InstanceId = GetInstanceID();
            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                TransformFeatureStateProvider.RegisterConfig(_transformConfig);

                // Warm up the proactive evaluation
                InitStateProvider();
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                TransformFeatureStateProvider.UnRegisterConfig(_transformConfig);
            }
        }

        private void InitStateProvider()
        {
            foreach(var featureConfig in FeatureConfigs)
            {
                TransformFeatureStateProvider.GetCurrentState(_transformConfig, featureConfig.Feature, out _);
            }
        }

        public void GetFeatureVectorAndWristPos(TransformFeature feature, bool isHandVector,
            ref Vector3? featureVec, ref Vector3? wristPos)
        {
            TransformFeatureStateProvider.GetFeatureVectorAndWristPos(
                TransformConfig, feature, isHandVector, ref featureVec, ref wristPos);
        }

        public bool Active
        {
            get
            {
                if (!isActiveAndEnabled)
                {
                    return false;
                }
                foreach(var featureConfig in FeatureConfigs)
                {
                    if (! TransformFeatureStateProvider.IsStateActive(
                        _transformConfig,
                        featureConfig.Feature,
                        featureConfig.Mode,
                        featureConfig.State))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        #region Inject

        public void InjectAllTransformRecognizerActiveState(IHand hand,
            ITransformFeatureStateProvider transformFeatureStateProvider,
            TransformFeatureConfigList transformFeatureList,
            TransformConfig transformConfig)
        {
            InjectHand(hand);
            InjectTransformFeatureStateProvider(transformFeatureStateProvider);
            InjectTransformFeatureList(transformFeatureList);
            InjectTransformConfig(transformConfig);
        }

        public void InjectHand(IHand hand)
        {
            _hand = hand as UnityEngine.Object;
            Hand = hand;
        }

        public void InjectTransformFeatureStateProvider(ITransformFeatureStateProvider transformFeatureStateProvider)
        {
            TransformFeatureStateProvider = transformFeatureStateProvider;
            _transformFeatureStateProvider = transformFeatureStateProvider as UnityEngine.Object;
        }

        public void InjectTransformFeatureList(TransformFeatureConfigList transformFeatureList)
        {
            _transformFeatureConfigs = transformFeatureList;
        }

        public void InjectTransformConfig(TransformConfig transformConfig)
        {
            _transformConfig = transformConfig;
        }
        #endregion
    }
}
