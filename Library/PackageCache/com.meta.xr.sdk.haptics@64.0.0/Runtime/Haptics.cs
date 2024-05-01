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
    /// Low-level API for the XR Haptics SDK runtime.
    /// </summary>
    ///
    /// <remarks>
    /// This class is provided where low-level control of the XR Haptics SDK runtime is required.
    /// Most applications probably do not require this class and <c>HapticClipPlayer</c>
    /// and <c>HapticClip</c> should be used instead.
    ///
    /// In a nutshell, it wraps the C# Native SDK bindings of the <c>Oculus.Haptics.Ffi</c> class
    /// to be used by C# abstractions. None of the methods here are thread-safe and should
    /// only be called from the main (Unity) thread. Calling these methods from a secondary
    /// thread can cause undefined behaviour and memory leaks.
    /// </remarks>
    public class Haptics : IDisposable
    {
        protected static Haptics instance;

        /// <summary>
        /// Returns the singleton instance of <c>Haptics</c>: either existing or new.
        /// </summary>
        public static Haptics Instance
        {
            get
            {
                if (!IsSupportedPlatform())
                {
                    Debug.LogError($"Error: This platform is not supported for haptics");
                    instance = null;
                    return null;
                }

                instance ??= new Haptics();

                // Ensure that the underlying runtime is initialized.
                if (!EnsureInitialized())
                {
                    instance = null;
                }

                return instance;
            }
        }

        private static bool IsSupportedPlatform()
        {
            // Standalone Quest builds and Link to Quest on Windows.
#if ((UNITY_ANDROID && !UNITY_EDITOR) || UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// The constructor is protected to ensure that a class instance cannot be created
        /// directly and all consuming code must go through the <c>Instance</c> property.
        /// </summary>
        protected Haptics()
        {
        }

        /// <summary>
        /// Ensures that the haptics runtime is initialized for supported configurations.
        /// </summary>
        ///
        /// <returns><c>true</c> if it was possible to ensure initialization; <c>false</c> otherwise.</returns>
        private static bool EnsureInitialized()
        {
            if (IsInitialized() ||
                Ffi.Succeeded(Ffi.initialize_with_ovr_plugin("Unity", Application.unityVersion, "64.0.0-mainline.0")))
                return true;

            Debug.LogError($"Error: {Ffi.error_message()}");
            return false;
        }

        private static bool IsInitialized()
        {
            if (Ffi.Failed(Ffi.initialized(out var isInitialized)))
            {
                Debug.LogError("Failed to get initialization state");
                return false;
            }

            return isInitialized;
        }

        /// <summary>
        /// Loads haptic data from a JSON string.
        /// </summary>
        ///
        /// <param name="clipJson">The UTF-8-encoded JSON string containing haptic data (.haptic format).</param>
        /// <returns>A haptic clip ID if the data is parsed successfully; Ffi.InvalidId otherwise.</returns>
        /// <exception cref="FormatException">If the format of the haptic clip data is not valid; or the data is not UTF-8 encoded.</exception>
        public int LoadClip(string clipJson)
        {
            int clipId = Ffi.InvalidId;

            switch (Ffi.load_clip(clipJson, out clipId))
            {
                case Ffi.Result.LoadClipFailed:
                    throw new FormatException($"Invalid format for clip: {clipJson}.");

                case Ffi.Result.InvalidUtf8:
                    throw new FormatException($"Invalid UTF8 encoding for clip: {clipJson}.");
            }

            return clipId;
        }

        /// <summary>
        /// Releases ownership of a loaded haptic clip.
        /// </summary>
        ///
        /// <remarks>
        /// Releasing the clip means that the API no longer maintains a handle to the clip.
        /// However resources won't be freed until all clip players that are playing the clip have
        /// also been destroyed.
        /// </remarks>
        ///
        /// <param name="clipId"> The ID of the haptic clip to be released.</param>
        /// <returns><c>true</c> if the clip was released successfully; <c>false</c> if
        /// the clip was already released or the call was unsuccessful.</returns>
        public bool ReleaseClip(int clipId)
        {
            return Ffi.Succeeded(Ffi.release_clip(clipId));
        }

        /// <summary>
        /// Creates a haptic clip player.
        /// </summary>
        ///
        /// <remarks>
        /// To play a haptic clip with a created player, the haptic clip must first be loaded using <c>LoadClip()</c>,
        /// assigned to the player using <c>SetHapticPlayerClip()</c>, and finally playback is started using
        /// <c>PlayHapticPlayer()</c>.
        /// </remarks>
        ///
        /// <returns>The player ID, if the player was created successfully; Ffi.InvalidId if something went wrong.</returns>
        public int CreateHapticPlayer()
        {
            int playerId = Ffi.InvalidId;

            Ffi.create_player(out playerId);

            return playerId;
        }

        /// <summary>
        /// Sets the clip that is used by the given player.
        ///
        /// If the player is currently playing, it will stop. Other properties like amplitude, frequency
        /// shift, looping and priority are kept.
        ///
        /// The clip must have been previously loaded by calling <c>LoadClip()</c> (see above).
        /// </summary>
        ///
        /// <param name="playerId">The ID of the clip player.</param>
        /// <param name="clipId">The ID of the haptic clip to be released.</param>
        /// <exception cref="ArgumentException">If the player ID was invalid.</exception>
        /// <exception cref="ArgumentException">If the clip ID was invalid.</exception>
        public void SetHapticPlayerClip(int playerId, int clipId)
        {
            switch (Ffi.player_set_clip(playerId, clipId))
            {
                case Ffi.Result.PlayerIdInvalid:
                    throw new ArgumentException($"Invalid player ID: {playerId}.");
                case Ffi.Result.ClipIdInvalid:
                    throw new ArgumentException($"Invalid clipId: {clipId}.");
            }
        }

        /// <summary>
        /// Starts playback on the player with the specified player ID on the specified controller.
        /// </summary>
        ///
        /// <param name="playerId">The ID of the clip player to start playback on.</param>
        /// <param name="controller">The controller to play on. Can be <c>Left</c>, <c>Right</c> or <c>Both</c> controllers.</param>
        /// <exception cref="ArgumentException">If the player ID was invalid.</exception>
        /// <exception cref="ArgumentException">If an invalid controller was selected for playback.</exception>
        /// <exception cref="InvalidOperationException">If the player has no clip loaded.</exception>
        public void PlayHapticPlayer(int playerId, Controller controller)
        {
            Ffi.Controller ffiController = Utils.ControllerToFfiController(controller);

            switch (Ffi.player_play(playerId, ffiController))
            {
                case Ffi.Result.PlayerIdInvalid:
                    throw new ArgumentException($"Invalid player ID: {playerId}.");
                case Ffi.Result.NoClipLoaded:
                    throw new InvalidOperationException($"Player with ID {playerId} has no clip loaded.");
            };
        }

        /// <summary>
        /// Stops playback that was previously started with <c>PlayHapticPlayer()</c>.
        /// </summary>
        ///
        /// <param name="playerId">The ID of the clip player to stop playback on.</param>
        /// <exception cref="ArgumentException">If the player ID was invalid.</exception>
        /// <exception cref="InvalidOperationException">If the player has no clip loaded.</exception>
        public void StopHapticPlayer(int playerId)
        {
            switch (Ffi.player_stop(playerId))
            {
                case Ffi.Result.PlayerIdInvalid:
                    throw new ArgumentException($"Invalid player ID: {playerId}.");
                case Ffi.Result.NoClipLoaded:
                    throw new InvalidOperationException($"Player with ID {playerId} has no clip loaded.");
            };
        }

        /// <summary>
        /// Returns the duration of the loaded haptic clip.
        /// </summary>
        ///
        /// <param name="clipId"> The ID of the haptic clip to be queried for its duration.</param>
        /// <returns>The duration of the haptic clip in seconds if the call was successful; 0.0 otherwise.</returns>
        /// <exception cref="ArgumentException">If the clip ID was invalid.</exception>
        public float GetClipDuration(int clipId)
        {
            float clipDuration = 0.0f;

            if (Ffi.Result.ClipIdInvalid == Ffi.clip_duration(clipId, out clipDuration))
            {
                throw new ArgumentException($"Invalid clip ID: {clipId}.");
            }

            return clipDuration;
        }

        /// <summary>
        /// Enables or disables a clip player's loop state.
        /// </summary>
        ///
        /// <param name="playerId">The ID of the clip player.</param>
        /// <param name="enabled"><c>true</c> if the clip player should loop; <c>false</c> to disable looping.</param>
        /// <exception cref="ArgumentException">If the player ID was invalid.</exception>
        public void LoopHapticPlayer(int playerId, bool enabled)
        {
            if (Ffi.Result.PlayerIdInvalid == Ffi.player_set_looping_enabled(playerId, enabled))
            {
                throw new ArgumentException($"Invalid player ID: {playerId}.");
            }
        }

        /// <summary>
        /// Gets a clip player's loop state (if it loops or not).
        /// </summary>
        ///
        /// <param name="playerId">The ID of the clip player.</param>
        /// <returns>The current loop state, if getting the state was successful, and the default value of <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">If the player ID was invalid.</exception>
        public bool IsHapticPlayerLooping(int playerId)
        {
            bool playerLoopState = false;

            if (Ffi.Result.PlayerIdInvalid == Ffi.player_looping_enabled(playerId, out playerLoopState))
            {
                throw new ArgumentException($"Invalid player ID: {playerId}.");
            }

            return playerLoopState;
        }

        /// <summary>
        /// Sets the clip player's amplitude.
        /// </summary>
        ///
        /// <param name="playerId">The ID of the clip player.</param>
        /// <param name="amplitude">A positive integer value for the amplitude of the clip player.
        /// All individual amplitudes within the clip are scaled by this value. Each individual value
        /// is clipped to one.</param>
        /// <exception cref="ArgumentException">If the player ID was invalid.</exception>
        /// <exception cref="ArgumentException">If the amplitude argument is out of range (has to be non-negative).</exception>
        public void SetAmplitudeHapticPlayer(int playerId, float amplitude)
        {
            switch (Ffi.player_set_amplitude(playerId, amplitude))
            {
                case Ffi.Result.PlayerIdInvalid:
                    throw new ArgumentException($"Invalid player ID: {playerId}.");

                case Ffi.Result.PlayerInvalidAmplitude:
                    throw new ArgumentOutOfRangeException(
                        $"Invalid amplitude: {amplitude} for player {playerId}." +
                        "Make sure the value is non-negative."
                        );
            }
        }

        /// <summary>
        /// Get the clip player's amplitude.
        /// </summary>
        ///
        /// <param name="playerId">The ID of the clip player.</param>
        /// <returns>The current player amplitude, if getting amplitude was successful; and the default value (1.0) otherwise.</returns>
        /// <exception cref="ArgumentException">If the player ID was invalid.</exception>
        public float GetAmplitudeHapticPlayer(int playerId)
        {
            float playerAmplitude = 1.0f;

            if (Ffi.Result.PlayerIdInvalid == Ffi.player_amplitude(playerId, out playerAmplitude))
            {
                throw new ArgumentException($"Invalid player ID: {playerId}.");
            }

            return playerAmplitude;
        }

        /// <summary>
        /// Sets the clip player's frequency shift.
        /// </summary>
        ///
        /// <param name="playerId">ID of the clip player.</param>
        /// <param name="amount">A value between -1.0 and 1.0 (inclusive). Values outside this range will cause an exception.</param>
        /// <exception cref="ArgumentException">If the player ID was invalid.</exception>
        /// <exception cref="ArgumentException">If the frequency shift amount is out of range.</exception>
        public void SetFrequencyShiftHapticPlayer(int playerId, float amount)
        {
            switch (Ffi.player_set_frequency_shift(playerId, amount))
            {
                case Ffi.Result.PlayerIdInvalid:
                    throw new ArgumentException($"Invalid player ID: {playerId}.");

                case Ffi.Result.PlayerInvalidFrequencyShift:
                    throw new ArgumentOutOfRangeException(
                        $"Invalid frequency shift amount: {amount} for player {playerId}." +
                        "Make sure the value is on the range -1.0 to 1.0 (inclusive)."
                        );
            }
        }

        /// <summary>
        /// Gets the clip player's current frequency shift based on it's player ID.
        /// </summary>
        ///
        /// <param name="playerId">The ID of the clip player.</param>
        /// <returns>The current player frequency shift if getting frequency shift was successful; the default value (0.0) otherwise.</returns>
        /// <exception cref="ArgumentException">If the player ID was invalid.</exception>
        public float GetFrequencyShiftHapticPlayer(int playerId)
        {
            float playerFrequencyShift = 0.0f;

            if (Ffi.Result.PlayerIdInvalid == Ffi.player_frequency_shift(playerId, out playerFrequencyShift))
            {
                throw new ArgumentException($"Invalid player ID: {playerId}.");
            }

            return playerFrequencyShift;
        }

        /// <summary>
        /// A wrapper for Utils.Map(), used specifically for scaling priority values.
        /// We make sure to catch if casting to uint overflows. This is only expected to happen when the user enters a
        /// value outside of the expected priority range (i.e. 255+), thus we throw an exception to provide guidance.
        /// </summary>
        ///
        /// <param name="input">The value to be scaled.</param>
        /// <param name="inMin">The lower limit of the source range.</param>
        /// <param name="inMax">The upper limit of the source range.</param>
        /// <param name="outMin">The lower limit of the target range.</param>
        /// <param name="outMax">The upper limit of the target range.</param>
        /// <exception cref="ArgumentException">If the input value is out of range.</exception>
        private static uint MapPriority(uint input, int inMin, int inMax, int outMin, int outMax)
        {
            try
            {
                checked
                {
                    float mappedValue = Utils.Map((int)input, inMin, inMax, outMin, outMax);
                    return (uint)Math.Round(mappedValue);
                }
            }
            catch (OverflowException)
            {
                throw new ArgumentOutOfRangeException(
                    $"Invalid priority value: {input}. " +
                    "Make sure the value is within the range 0 to 255 (inclusive)."
                    );
            }
        }

        /// <summary>
        /// Sets the clip player's current playback priority value based on its player ID.
        /// The priority values range from 0 (high priority) to 255 (low priority), with 128 being the default.
        /// </summary>
        ///
        /// <param name="playerId">ID of the clip player.</param>
        /// <param name="value">A value between 0 and 255 (inclusive). Values outside this range will cause an exception.</param>
        /// <exception cref="ArgumentException">If the player ID was invalid.</exception>
        /// <exception cref="ArgumentException">If the priority value is out of range.</exception>
        public void SetPriorityHapticPlayer(int playerId, uint value)
        {
            // The native library takes values from 0 (low priority) to 1024 (high priority),
            // while the Haptics SDK for Unity uses 0 (high priority) to 255 (low priority).
            switch (Ffi.player_set_priority(playerId, MapPriority(value, 0, 255, 1024, 0)))
            {
                case Ffi.Result.PlayerIdInvalid:
                    throw new ArgumentException($"Invalid player ID: {playerId}.");

                case Ffi.Result.PlayerInvalidPriority:
                    throw new ArgumentOutOfRangeException(
                        $"Invalid priority value: {value} for player {playerId}. " +
                        "Make sure the value is within the range 0 to 255 (inclusive)."
                        );
            }
        }

        /// <summary>
        /// Gets the clip player's current playback priority value based on its player ID.
        /// </summary>
        ///
        /// <param name="playerId">The ID of the clip player.</param>
        /// <returns>The current priority value if successful; the default value (128) otherwise.</returns>
        /// <exception cref="ArgumentException">If the player ID was invalid.</exception>
        public uint GetPriorityHapticPlayer(int playerId)
        {
            uint playerPriority = 128;

            if (Ffi.Result.PlayerIdInvalid == Ffi.player_priority(playerId, out playerPriority))
            {
                throw new ArgumentException($"Invalid player ID: {playerId}.");
            }

            // The native library takes values from 0 (low priority) to 1024 (high priority),
            // while the Haptics SDK for Unity uses 0 (high priority) to 255 (low priority).
            return MapPriority(playerPriority, 0, 1024, 255, 0);
        }

        /// <summary>
        /// Releases a clip player that was previously created with <c>CreateHapticPlayer()</c>.
        /// </summary>
        ///
        /// <param name="playerId">ID of the clip player to be released.</param>
        /// <returns><c>true</c> if release was successful; <c>false</c> if the player does not exist,
        /// was already released, or the call was unsuccessful. </returns>
        public bool ReleaseHapticPlayer(int playerId)
        {
            return Ffi.Succeeded(Ffi.release_player(playerId));
        }

        /// <summary>
        /// Call this to explicitly release the haptics runtime.
        /// </summary>
        ///
        /// <remarks>
        /// This will also result in any <c>HapticClipPlayer</c>s and <c>HapticClip</c>s loaded into
        /// the runtime getting released from memory. In general you shouldn't need to explicitly release
        /// the haptics runtime as it is intended to run for the duration of your application and to get
        /// released via <c>~Haptics</c> on shutdown. However if you do have a particular reason to release
        /// it explicitly, creating a new <c>HapticClipPlayer</c> will produce a new one.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (instance is not null)
            {
                if (IsInitialized() && Ffi.Failed(Ffi.uninitialize()))
                {
                    Debug.LogError($"Error: {Ffi.error_message()}");
                }

                instance = null;
            }
        }

        /// <summary>
        /// <c>Haptics</c> should only be garbage collected during shutdown. Relying on the garbage collector
        /// to destroy and instance of <c>Haptics</c> with the intention of creating a new one afterwards
        /// is likely to produce undefined behaviour. For this, use the <c>Dispose()</c> method instead.
        /// </summary>
        ~Haptics()
        {
            Dispose(false);
        }
    }
}
