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

namespace Oculus.Interaction
{
    public class OneGrabSphereTransformer : MonoBehaviour, ITransformer
    {
        [SerializeField]
        private Transform _sphereCenter;

        [SerializeField, Range(-90, 90)]
        private float _minAngle = -90;

        [SerializeField, Range(-90, 90)]
        private float _maxAngle = 90;

        [SerializeField]
        private bool _scaleWithRadius = false;

        [SerializeField]
        private Vector3 _radiusToScaleRatio = new Vector3(1f, 1f, 1f);

        public float MinAngle {
            get => _minAngle;
            set {
                _minAngle = value;
                ClampMinMax();
            }
        }

        public float MaxAngle {
            get => _maxAngle;
            set {
                _maxAngle = value;
                ClampMinMax();
            }
        }

        void ClampMinMax()
        {
            _minAngle = Mathf.Clamp(_minAngle, -90, 90);
            _maxAngle = Mathf.Clamp(_maxAngle, _minAngle, 90);
        }

        public bool ScaleWithRadius {
            get => _scaleWithRadius;
            set { _scaleWithRadius = value; }
        }

        public Vector3 RadiusToScaleRatio {
            get => _radiusToScaleRatio;
            set {
                _radiusToScaleRatio = value;
            }
        }

        private IGrabbable _grabbable;
        private Pose _localToTransform;

        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
            ClampMinMax();
        }

        public void BeginTransform()
        {
            var grabPose = _grabbable.GrabPoints[0];

            Transform target = _grabbable.Transform;
            _localToTransform = new Pose(target.InverseTransformPoint(grabPose.position),
                                         Quaternion.Inverse(target.rotation) * grabPose.rotation);
        }

        public void UpdateTransform()
        {
            var grabPose = _grabbable.GrabPoints[0];
            Transform target = _grabbable.Transform;

            Vector3 deltaGrab = grabPose.position - _sphereCenter.position;
            float grabRadiusSq = deltaGrab.sqrMagnitude;

            float radiusSq = 1;
            float radius = 1;
            float fullRadius = 1;
            if(_scaleWithRadius)
            {
                Vector3 localScaledXY =
                    new Vector3(_localToTransform.position.x * _radiusToScaleRatio.x,
                                _localToTransform.position.y * _radiusToScaleRatio.y,
                                0);

                float zRatio = 1 - (-_localToTransform.position.z) * _radiusToScaleRatio.z;
                float zRatioSq = zRatio * zRatio;
                float lxSq = localScaledXY.x * localScaledXY.x / zRatioSq;
                float lySq = localScaledXY.y * localScaledXY.y / zRatioSq;
                radiusSq = grabRadiusSq / (1 + lxSq + lySq);
                radius = Mathf.Sqrt(radiusSq);

                fullRadius = radius / (1 - (-_localToTransform.position.z) * _radiusToScaleRatio.z);

                Vector3 parentScale = target.parent != null ? target.parent.lossyScale : Vector3.one;
                Vector3 worldTargetScale = fullRadius * _radiusToScaleRatio;
                target.localScale = new Vector3(worldTargetScale.x / parentScale.x,
                                                worldTargetScale.y / parentScale.y,
                                                worldTargetScale.z / parentScale.z);
            } else {
                Vector3 targetDeltaXY = target.TransformVector(
                    new Vector3(_localToTransform.position.x,
                                _localToTransform.position.y,
                                0));
                float oppositeSq = targetDeltaXY.sqrMagnitude;
                radiusSq = grabRadiusSq - oppositeSq;
                // Check feasability to maintain the invariant:
                // the grab may be too close to the sphere center
                if(radiusSq <= 0)
                {
                    return;
                }

                radius = Mathf.Sqrt(radiusSq);
                float z = target.TransformVector(new Vector3(0, 0, _localToTransform.position.z)).magnitude;
                if(_localToTransform.position.z < 0) z *= -1;
                fullRadius = radius - z;
            }

            float targetDeltaYSq = target.TransformVector(
                                  new Vector3(0, _localToTransform.position.y, 0)
                                ).sqrMagnitude;

            float hypotenuse = Mathf.Sqrt(targetDeltaYSq + radiusSq);

            float clamped = Mathf.Clamp(deltaGrab.y / hypotenuse, -1, 1);
            float angle = Mathf.Asin(clamped) * Mathf.Rad2Deg;

            float targetDeltaY = Mathf.Sqrt(targetDeltaYSq);
            if(_localToTransform.position.y  < 0)
            {
                targetDeltaY *= -1;
            }
            float centerAngle = Mathf.Atan2(-targetDeltaY, radius) * Mathf.Rad2Deg;
            angle += centerAngle;

            angle = Mathf.Clamp(angle, _minAngle, _maxAngle);
            Quaternion rotation = Quaternion.AngleAxis(-angle, Vector3.right);

            Pose adjacentPose = new Pose(rotation * (fullRadius * Vector3.forward), rotation);
            Vector3 worldScale = target.lossyScale;
            Vector3 scaledPos = new Vector3(worldScale.x * _localToTransform.position.x,
                                            worldScale.y * _localToTransform.position.y,
                                            worldScale.z * _localToTransform.position.z);
            Vector3 worldDelta = adjacentPose.position + adjacentPose.rotation * scaledPos;
            Vector3 worldDeltaXZ = new Vector3(worldDelta.x, 0, worldDelta.z);
            Vector3 grabPosXZ = new Vector3(deltaGrab.x, 0, deltaGrab.z);
            float angleDelta = Vector3.SignedAngle(worldDeltaXZ, grabPosXZ, Vector3.up);

            Quaternion yaw = Quaternion.AngleAxis(angleDelta, Vector3.up);
            rotation = yaw * rotation;

            target.position = _sphereCenter.position + rotation * (fullRadius * Vector3.forward);
            target.rotation = rotation;
        }

        public void EndTransform() { }
    }
}
