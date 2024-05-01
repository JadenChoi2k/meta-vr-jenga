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

using Oculus.Interaction.Grab;
using UnityEngine;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.Input;
using System;

namespace Oculus.Interaction.HandGrab
{
    /// <summary>
    /// Helper class for Hand Grabbing objects.
    /// This class keeps track of the grabbing anchors and updates the target
    /// and movement during a Hand Grab interaction.
    /// </summary>
    public static class HandGrabInteraction
    {
        /// <summary>
        /// Calculates a new target. That is the point of the
        /// interactable at which the HandGrab will happen.
        /// </summary>
        /// <param name="interactable">The interactable to be HandGrabbed</param>
        /// <param name="grabTypes">The supported GrabTypes</param>
        /// <param name="anchorMode">The anchor to use for grabbing</param>
        /// <param name="handGrabResult">The a variable to store the result</param>
        /// <returns>True if a valid pose was found</returns>
        [Obsolete("Use " + nameof(CalculateBestGrab) + " instead")]
        public static bool TryCalculateBestGrab(this IHandGrabInteractor handGrabInteractor,
            IHandGrabInteractable interactable, GrabTypeFlags grabTypes,
            out HandGrabTarget.GrabAnchor anchorMode, ref HandGrabResult handGrabResult)
        {
            CalculateBestGrab(handGrabInteractor, interactable, grabTypes,
                out GrabTypeFlags activeGrabFlags, ref handGrabResult);

            if (activeGrabFlags.HasFlag(GrabTypeFlags.Pinch))
            {
                anchorMode = HandGrabTarget.GrabAnchor.Pinch;
            }
            else if (activeGrabFlags.HasFlag(GrabTypeFlags.Palm))
            {
                anchorMode = HandGrabTarget.GrabAnchor.Palm;
            }
            else
            {
                anchorMode = HandGrabTarget.GrabAnchor.Wrist;
            }

            return true;
        }

        [Obsolete]
        public static GrabTypeFlags CurrentGrabType(this IHandGrabInteractor handGrabInteractor)
        {
            return handGrabInteractor.HandGrabTarget.Anchor;
        }

        /// <summary>
        /// Calculates a new target. That is the point of the
        /// interactable at which the HandGrab will happen.
        /// </summary>
        /// <param name="handGrabInteractor">The interactor grabbing</param>
        /// <param name="interactable">The interactable to be HandGrabbed</param>
        /// <param name="grabFlags">The supported GrabTypes</param>
        /// <param name="activeGrabFlags">The anchor to use for grabbing</param>
        /// <param name="result">The a variable to store the result</param>
        public static void CalculateBestGrab(this IHandGrabInteractor handGrabInteractor,
        IHandGrabInteractable interactable, GrabTypeFlags grabFlags,
        out GrabTypeFlags activeGrabFlags, ref HandGrabResult result)
        {
            activeGrabFlags = grabFlags & interactable.SupportedGrabTypes;
            GetPoseOffset(handGrabInteractor, activeGrabFlags, out Pose handPose, out Pose grabOffset);

            interactable.CalculateBestPose(handPose, grabOffset, interactable.RelativeTo,
                handGrabInteractor.Hand.Scale, handGrabInteractor.Hand.Handedness,
               ref result);
        }

        /// <summary>
        /// Initiates the movement of the Interactable
        /// with the current HandGrabTarget
        /// </summary>
        /// <param name="handGrabInteractor">The interactor grabbing</param>
        /// <param name="interactable">The interactable to be HandGrabbed</param>
        public static IMovement GenerateMovement(this IHandGrabInteractor handGrabInteractor, IHandGrabInteractable interactable)
        {
            Pose originPose = handGrabInteractor.GetTargetGrabPose();
            Pose grabPose = handGrabInteractor.GetHandGrabPose();
            return interactable.GenerateMovement(originPose, grabPose);
        }

        /// <summary>
        /// Gets the current pose of the current grabbing point.
        /// </summary>
        /// <returns>The pose in world coordinates</returns>
        public static Pose GetHandGrabPose(this IHandGrabInteractor handGrabInteractor)
        {
            GetPoseOffset(handGrabInteractor, GrabTypeFlags.None, out Pose wristPose, out _);
            return PoseUtils.Multiply(wristPose, handGrabInteractor.WristToGrabPoseOffset);
        }

        /// <summary>
        /// Calculates the GrabPoseScore for an interactable with the
        /// given grab modes.
        /// </summary>
        /// <param name="handGrabInteractor">The interactor grabbing</param>
        /// <param name="interactable">The interactable to measure to</param>
        /// <param name="grabTypes">The supported grab types for the grab</param>
        /// <param name="result">Calculating the score requires finding the best grab pose. It is stored here.</param>
        /// <returns>The best GrabPoseScore considering the grabtypes at the interactable</returns>
        public static GrabPoseScore GetPoseScore(this IHandGrabInteractor handGrabInteractor, IHandGrabInteractable interactable,
            GrabTypeFlags grabTypes, ref HandGrabResult result)
        {
            GrabTypeFlags activeGrabFlags = grabTypes & interactable.SupportedGrabTypes;
            GetPoseOffset(handGrabInteractor, activeGrabFlags, out Pose handPose, out Pose grabOffset);

            interactable.CalculateBestPose(handPose, grabOffset, interactable.RelativeTo,
                handGrabInteractor.Hand.Scale, handGrabInteractor.Hand.Handedness,
                ref result);

            return result.Score;
        }

        /// <summary>
        /// Indicates if an interactor can interact with (hover and select) and interactable.
        /// This depends on the handedness and the valid grab types of both elements
        /// </summary>
        /// <param name="handGrabInteractor">The interactor grabbing</param>
        /// <param name="handGrabInteractable">The interactable to be grabbed</param>
        /// <returns>True if the interactor could grab the interactable</returns>
        public static bool CanInteractWith(this IHandGrabInteractor handGrabInteractor, IHandGrabInteractable handGrabInteractable)
        {
            if (!handGrabInteractable.SupportsHandedness(handGrabInteractor.Hand.Handedness))
            {
                return false;
            }

            return (handGrabInteractor.SupportedGrabTypes & handGrabInteractable.SupportedGrabTypes) != GrabTypeFlags.None;
        }

        /// <summary>
        /// Calculates the offset from the Wrist to the actual grabbing point
        /// defined by the current anchor in the interactor HandGrabTarget.
        /// </summary>
        /// <param name="handGrabInteractor">The interactor whose HandGrabTarget to inspect</param>
        /// <returns>The local offset from the wrist to the grab point</returns>
        public static Pose GetGrabOffset(this IHandGrabInteractor handGrabInteractor)
        {
            GetPoseOffset(handGrabInteractor, handGrabInteractor.HandGrabTarget.Anchor,
                out _, out Pose wristToGrabPoseOffset);
            return wristToGrabPoseOffset;

        }

        /// <summary>
        /// Calculates the strenght of the fingers of an interactor trying (or grabbing) an interactable
        /// </summary>
        /// <param name="handGrabInteractor">The interactor grabbing</param>
        /// <param name="handGrabInteractable">The interactable being grabbed</param>
        /// <param name="handGrabTypes">A filter for the grab types to calculate</param>
        /// <param name="includeSelecting">Compute also fingers that are selecting</param>
        /// <returns>The maximum strength for the grabbing fingers, normalized</returns>
        public static float ComputeHandGrabScore(IHandGrabInteractor handGrabInteractor,
            IHandGrabInteractable handGrabInteractable, out GrabTypeFlags handGrabTypes, bool includeSelecting = false)
        {
            HandGrabAPI api = handGrabInteractor.HandGrabApi;
            handGrabTypes = GrabTypeFlags.None;
            float handGrabScore = 0f;

            if (SupportsPinch(handGrabInteractor, handGrabInteractable))
            {
                float pinchStrength = api.GetHandPinchScore(handGrabInteractable.PinchGrabRules, includeSelecting);
                if (pinchStrength > handGrabScore)
                {
                    handGrabScore = pinchStrength;
                    handGrabTypes = GrabTypeFlags.Pinch;
                }
            }

            if (SupportsPalm(handGrabInteractor, handGrabInteractable))
            {
                float palmStrength = api.GetHandPalmScore(handGrabInteractable.PalmGrabRules, includeSelecting);
                if (palmStrength > handGrabScore)
                {
                    handGrabScore = palmStrength;
                    handGrabTypes = GrabTypeFlags.Palm;
                }
            }

            return handGrabScore;
        }

        public static GrabTypeFlags ComputeShouldSelect(this IHandGrabInteractor handGrabInteractor,
            IHandGrabInteractable handGrabInteractable)
        {
            if (handGrabInteractable == null)
            {
                return GrabTypeFlags.None;
            }

            HandGrabAPI api = handGrabInteractor.HandGrabApi;
            GrabTypeFlags selectingGrabTypes = GrabTypeFlags.None;
            if (SupportsPinch(handGrabInteractor, handGrabInteractable) &&
                 api.IsHandSelectPinchFingersChanged(handGrabInteractable.PinchGrabRules))
            {
                selectingGrabTypes |= GrabTypeFlags.Pinch;
            }

            if (SupportsPalm(handGrabInteractor, handGrabInteractable) &&
                 api.IsHandSelectPalmFingersChanged(handGrabInteractable.PalmGrabRules))
            {
                selectingGrabTypes |= GrabTypeFlags.Palm;
            }

            return selectingGrabTypes;
        }

        public static GrabTypeFlags ComputeShouldUnselect(this IHandGrabInteractor handGrabInteractor,
            IHandGrabInteractable handGrabInteractable)
        {
            HandGrabAPI api = handGrabInteractor.HandGrabApi;
            HandFingerFlags pinchFingers = api.HandPinchGrabbingFingers();
            HandFingerFlags palmFingers = api.HandPalmGrabbingFingers();

            if (handGrabInteractable.SupportedGrabTypes == GrabTypeFlags.None)
            {
                if (!api.IsSustainingGrab(GrabbingRule.FullGrab, pinchFingers) &&
                    !api.IsSustainingGrab(GrabbingRule.FullGrab, palmFingers))
                {
                    return GrabTypeFlags.All;
                }
                return GrabTypeFlags.None;
            }

            GrabTypeFlags unselectingGrabTypes = GrabTypeFlags.None;
            if (SupportsPinch(handGrabInteractor, handGrabInteractable.SupportedGrabTypes)
                && !api.IsSustainingGrab(handGrabInteractable.PinchGrabRules, pinchFingers)
                && api.IsHandUnselectPinchFingersChanged(handGrabInteractable.PinchGrabRules))
            {
                unselectingGrabTypes |= GrabTypeFlags.Pinch;
            }

            if (SupportsPalm(handGrabInteractor, handGrabInteractable.SupportedGrabTypes)
                && !api.IsSustainingGrab(handGrabInteractable.PalmGrabRules, palmFingers)
                && api.IsHandUnselectPalmFingersChanged(handGrabInteractable.PalmGrabRules))
            {
                unselectingGrabTypes |= GrabTypeFlags.Palm;
            }

            return unselectingGrabTypes;
        }

        public static HandFingerFlags GrabbingFingers(this IHandGrabInteractor handGrabInteractor,
            IHandGrabInteractable handGrabInteractable)
        {
            HandGrabAPI api = handGrabInteractor.HandGrabApi;
            if (handGrabInteractable == null)
            {
                return HandFingerFlags.None;
            }

            HandFingerFlags fingers = HandFingerFlags.None;

            if (SupportsPinch(handGrabInteractor, handGrabInteractable))
            {
                HandFingerFlags pinchingFingers = api.HandPinchGrabbingFingers();
                handGrabInteractable.PinchGrabRules.StripIrrelevant(ref pinchingFingers);
                fingers = fingers | pinchingFingers;
            }

            if (SupportsPalm(handGrabInteractor, handGrabInteractable))
            {
                HandFingerFlags grabbingFingers = api.HandPalmGrabbingFingers();
                handGrabInteractable.PalmGrabRules.StripIrrelevant(ref grabbingFingers);
                fingers = fingers | grabbingFingers;
            }

            return fingers;
        }

        private static bool SupportsPinch(IHandGrabInteractor handGrabInteractor,
            IHandGrabInteractable handGrabInteractable)
        {
            return SupportsPinch(handGrabInteractor, handGrabInteractable.SupportedGrabTypes);
        }

        private static bool SupportsPalm(IHandGrabInteractor handGrabInteractor,
            IHandGrabInteractable handGrabInteractable)
        {
            return SupportsPalm(handGrabInteractor, handGrabInteractable.SupportedGrabTypes);
        }

        private static bool SupportsPinch(IHandGrabInteractor handGrabInteractor,
            GrabTypeFlags grabTypes)
        {
            return (handGrabInteractor.SupportedGrabTypes & grabTypes & GrabTypeFlags.Pinch) != 0;
        }

        private static bool SupportsPalm(IHandGrabInteractor handGrabInteractor,
            GrabTypeFlags grabTypes)
        {
            return (handGrabInteractor.SupportedGrabTypes & grabTypes & GrabTypeFlags.Palm) != 0;
        }

        /// <summary>
        /// Calculates the root of a grab and the practica offset to the grabbing point
        /// </summary>
        /// <param name="handGrabInteractor">The interactor grabbing</param>
        /// <param name="anchorMode">The grab types to be used</param>
        /// <param name="pose">The root of the grab pose to use</param>
        /// <param name="offset">The offset form the root for accurate scoring</param>
        public static void GetPoseOffset(this IHandGrabInteractor handGrabInteractor, GrabTypeFlags anchorMode,
            out Pose pose, out Pose offset)
        {
            handGrabInteractor.Hand.GetRootPose(out pose);
            offset = Pose.identity;

            if (anchorMode == GrabTypeFlags.None)
            {
                return;
            }
            else if ((anchorMode & GrabTypeFlags.Pinch) != 0
                && handGrabInteractor.PinchPoint != null)
            {
                offset = PoseUtils.Delta(pose, handGrabInteractor.PinchPoint.GetPose());
            }
            else if ((anchorMode & GrabTypeFlags.Palm) != 0
                && handGrabInteractor.PalmPoint != null)
            {
                offset = PoseUtils.Delta(pose, handGrabInteractor.PalmPoint.GetPose());
            };
        }
    }
}
