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

// @lint-ignore-every LICENSELINT

using System;
using UnityEngine;

namespace Oculus.Haptics
{
    /// <summary>
    /// The controller to play haptics on.
    /// </summary>
    public enum Controller
    {
        Left,
        Right,
        Both
    }

    internal static class Utils
    {
        /// <summary>
        /// Converts Controller type to Ffi.Controller type.
        /// </summary>
        ///
        /// <param name="controller">Controller type.</param>
        /// <exception cref="ArgumentException">If an invalid Controller type was passed in.</exception>
        internal static Ffi.Controller ControllerToFfiController(Controller controller)
        {
            return controller switch
            {
                Controller.Left => Ffi.Controller.Left,
                Controller.Right => Ffi.Controller.Right,
                Controller.Both => Ffi.Controller.Both,
                _ => throw new ArgumentException($"Invalid controller selected: {controller}.")
            };
        }

        /// <summary>
        /// Linearly scale a value from its "position" in a source range to an equivalent value in a target range.
        /// Does not impose clamping within the range, that is, when <c>input</c> is outside the input range it
        /// will map to an appropriate value outside the output range.
        /// </summary>
        ///
        /// <param name="input">The value to be scaled.</param>
        /// <param name="inMin">The lower limit of the source range.</param>
        /// <param name="inMax">The upper limit of the source range.</param>
        /// <param name="outMin">The lower limit of the target range.</param>
        /// <param name="outMax">The upper limit of the target range.</param>
        internal static float Map(int input, int inMin, int inMax, int outMin, int outMax)
        {
            float numerator = (input - inMin) * (outMax - outMin);
            float denominator = inMax - inMin;
            return (numerator / denominator) + outMin;
        }
    }
}
