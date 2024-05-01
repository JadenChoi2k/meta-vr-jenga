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
    /// <c>HapticClipPlayer</c> provides controls for playing a <c>HapticClip</c>.
    /// </summary>
    ///
    /// <remarks>
    /// It only plays valid <c>HapticClip</c>s. A <c>HapticClip</c> assigned to a
    /// <c>HapticClipPlayer</c> can be played and stopped as often as required.
    /// </remarks>
    public class HapticClipPlayer : IDisposable
    {
        /// <summary>
        /// The internal ID of the <c>HapticClip</c> associated with the <c>HapticClipPlayer</c>.
        /// </summary>
        private int _clipId = Ffi.InvalidId;

        /// <summary>
        /// The internal ID of the <c>HapticClipPlayer</c>. As long as the player has an ID it is
        /// in a valid state. It loses its ID if explicitly disposed.
        /// </summary>
        private int _playerId = Ffi.InvalidId;

        /// <summary>
        /// The implementation of <c>Haptics</c> for <c>HapticClipPlayer</c> to use.
        /// </summary>
        protected Haptics _haptics;

        /// <summary>
        /// Creates a <c>HapticClipPlayer</c> object for a given <c>HapticClip</c>.
        /// </summary>
        ///
        /// <param name="clip">The <c>HapticClip</c> to be played by this <c>HapticClipPlayer</c>.
        /// Providing invalid clip data will cause an exception.</param>
        public HapticClipPlayer(HapticClip clip)
        {
            SetHaptics();

            // Load clip from JSON data and check if that succeeded.
            int clipReturnValue = _haptics.LoadClip(clip.json);

            if (Ffi.InvalidId != clipReturnValue)
            {
                _clipId = clipReturnValue;
            }

            // Create player check if that succeeded.
            int playerReturnValue = _haptics.CreateHapticPlayer();

            if (Ffi.InvalidId != playerReturnValue)
            {
                _playerId = playerReturnValue;
            }

            // Assign loaded haptic clip to player
            _haptics.SetHapticPlayerClip(_playerId, _clipId);
        }

        /// <summary>
        /// Sets the <c>Haptics</c> implementation that <c>HapticClipPlayer</c> will call
        /// into for all haptics operations. This function is protected to allow derived
        /// classes to provide a custom implementation.
        /// </summary>
        protected virtual void SetHaptics()
        {
            _haptics = Haptics.Instance;
        }

        /// <summary>
        /// Starts playback on the specified controller.
        /// </summary>
        ///
        /// <param name="controller">The controller to play back on.</param>
        public void Play(Controller controller)
        {
            _haptics.PlayHapticPlayer(_playerId, controller);
        }

        /// <summary>
        /// Stops playback of the HapticClipPlayer.
        /// </summary>
        ///
        public void Stop()
        {
            _haptics.StopHapticPlayer(_playerId);
        }

        /// <summary>
        /// Whether the <c>HapticClipPlayer</c> is looping or not.
        /// </summary>
        ///
        /// <value><c>true</c> if <c>HapticClipPlayer</c> is looping.</value>
        public bool isLooping
        {
            get => _haptics.IsHapticPlayerLooping(_playerId);
            set => _haptics.LoopHapticPlayer(_playerId, value);
        }

        /// <summary>
        /// Get the duration of the loaded haptic clip of this <c>HapticClipPlayer</c>'s instance.
        /// </summary>
        ///
        /// <value>The duration in seconds of the haptic clip.</value>
        public float clipDuration => _haptics.GetClipDuration(_clipId);

        /// <summary>
        /// Sets/gets the <c>HapticClipPlayer</c>'s amplitude.
        ///
        /// During playback, the individual amplitudes in the clip will be multiplied by the player's amplitude.
        /// This changes how strong the vibration is. Amplitude values in a clip range from 0.0 to 1.0,
        /// and the result after applying the amplitude scale will be clipped to that range.
        ///
        /// An `amplitude` of 0.0 means that no vibration will be triggered, and an `amplitude` of 0.5 will
        /// result in the clip being played back at half of its amplitude.
        ///
        /// Example: if you apply amplitude of 5.0 to a haptic clip and the following amplitudes are in the
        /// clip: [0.2, 0.5, 0.1], the initial amplitude calculation would produce these values: [1.0, 2.5, 0.5]
        /// which will then be clamped like this: [1.0, 1.0, 0.5]
        ///
        /// This method can be called during active playback, in which case the amplitude is applied
        /// immediately, with a small delay in the tens of milliseconds.
        /// </summary>
        ///
        /// <value>A value between zero and one (maximum) for the amplitude of the clip player.
        /// Values greater than one will be clipped. Negative values will cause an exception.</value>
        public float amplitude
        {
            get => _haptics.GetAmplitudeHapticPlayer(_playerId);
            set => _haptics.SetAmplitudeHapticPlayer(_playerId, value);
        }

        /// <summary>
        /// Sets/gets the <c>HapticClipPlayer</c>'s frequency shift.
        ///
        /// The frequencies in a haptic clip are in the range zero to one. This property shifts the
        /// individual frequencies up or down. The acceptable range of values is -1.0 to 1.0 inclusive.
        ///
        /// Once the frequencies in a clip have been shifted, they will be clamped to the playable
        /// range of frequency values, i.e. zero to one.
        ///
        /// Setting this property to 0.0 means that the frequencies will not be changed.
        ///
        /// Example: if you apply a frequency shift of 0.8 to a haptic clip and the following frequencies
        /// are in the clip: [0.1, 0.5, 0.0], the initial frequency shift calculation will produce these
        /// frequencies: [0.9, 1.3, 0.8] which will then be clamped like this: [0.9, 1.0, 0.8]
        ///
        /// This method can be called during active playback, in which case the frequency shift is applied
        /// immediately, with a small delay in the tens of milliseconds.
        /// </summary>
        ///
        /// <value>A value between -1.0 and 1.0. Values outside this range will cause an exception.</value>
        public float frequencyShift
        {
            get => _haptics.GetFrequencyShiftHapticPlayer(_playerId);
            set => _haptics.SetFrequencyShiftHapticPlayer(_playerId, value);
        }

        /// <summary>
        /// Sets/gets the <c>HapticClipPlayer</c>'s playback priority.
        ///
        /// The playback engine of the Haptics SDK only ever renders the vibrations of a single <c>HapticClipPlayer</c>
        /// clip on the same controller at the same time. Meaning, haptic clips are not "mixed" when played back.
        /// If you have multiple players playing on the same controller at the same time, then only the player with the
        /// highest priority will trigger vibrations.
        ///
        /// Given the same priority value, the engine will always play the most recent clip that was started. All other
        /// players are muted, but continue tracking playback on their respective timeline (i.e. they are not paused).
        /// If a clip finishes (or is stopped), the engine will resume playback of the second most recent clip with an
        /// equal or higher priority level and so on.
        ///
        /// Example: Setting priority can be helpful if some haptic clips are more important than others, and allow us to
        /// design a hierarchy of haptic feedback based on context or the overall importance. For example, we could want
        /// a user to always receive a distinct haptic feedback if they are hit. Setting this "hit" clips priority higher
        /// compared to other haptic clips will ensure that the user always receives this haptic feedback.
        ///
        /// Priority values can be on the range of 0 (high priority) to 255 (low priority).
        /// By default, the priority value is set to 128 for every <c>HapticClipPlayer</c>.
        /// The player's priority can be changed before and during playback.
        /// </summary>
        ///
        /// <value>An integer value on the range 0 to 255 (inclusive).
        /// Values outside this range will cause an exception.</value>
        public uint priority
        {
            get => _haptics.GetPriorityHapticPlayer(_playerId);
            set => _haptics.SetPriorityHapticPlayer(_playerId, value);
        }

        /// <summary>
        /// Sets the <c>HapticClipPlayer</c>'s current haptic clip.
        ///
        /// This feature allows you to change the clip loaded in a clip player.
        /// If the player is currently playing it will be stopped. All other properties like amplitude, frequency
        /// shift, looping and priority are kept.
        /// </summary>
        ///
        /// <value>A valid, JSON formatted, UTF-8 encoded haptic clip.
        /// Providing invalid clip data will cause an exception.</value>
        public HapticClip clip
        {
            set
            {
                int returnValue = _haptics.LoadClip(value.json);

                if (Ffi.InvalidId != returnValue)
                {
                    _haptics.SetHapticPlayerClip(_playerId, returnValue);
                    _haptics.ReleaseClip(_clipId);
                    _clipId = returnValue;
                }
            }
        }

        /// <summary>
        /// Call this method to explicitly/deterministically release a <c>HapticClipPlayer</c> object, otherwise
        /// the garbage collector will release it. Of course, any calls to a disposed <c>HapticClipPlayer</c>
        /// will result in runtime errors.
        /// </summary>
        ///
        /// <remarks>
        /// A given <c>HapticClip</c> will not be freed until all <c>HapticClipPlayer</c>s to which
        /// it is assigned have also been freed.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the assigned clip and clip player from memory.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_playerId != Ffi.InvalidId)
            {
                if (!_haptics.ReleaseClip(_clipId) & _haptics.ReleaseHapticPlayer(_playerId))
                {
                    Debug.LogError($"Error: HapticClipPlayer or HapticClip could not be released");
                }

                _clipId = Ffi.InvalidId;
                _playerId = Ffi.InvalidId;
            }
        }

        ~HapticClipPlayer()
        {
            Dispose(false);
        }
    }
}
