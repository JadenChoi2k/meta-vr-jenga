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
using System.Text;
using System.Runtime.InteropServices;

namespace Oculus
{
    namespace Haptics
    {
        public class Ffi
        {
            private const string NativeLibName = "haptics_sdk";

            public static bool Succeeded(Result result)
            {
                return (int)result >= 0;
            }

            public static bool Failed(Result result)
            {
                return (int)result < 0;
            }

            public const int InvalidId = -1;

            public enum Result : int
            {
                Success = 0,
                Error = -1,
                InstanceInitializationFailed = -2,
                InstanceAlreadyInitialized = -3,
                InstanceAlreadyUninitialized = -4,
                InstanceNotInitialized = -5,
                InvalidUtf8 = -6,
                LoadClipFailed = -7,
                CreatePlayerFailed = -8,
                ClipIdInvalid = -9,
                PlayerIdInvalid = -10,
                PlayerInvalidAmplitude = -11,
                PlayerInvalidFrequencyShift = -12,
                PlayerInvalidPriority = -13,
                NoClipLoaded = -14
            }

            public struct SdkVersion
            {
                public ushort major;
                public ushort minor;
                public ushort patch;
            }

            public enum Controller
            {
                Left = 0,
                Right = 1,
                Both = 2,
            }

            public enum LogLevel
            {
                Trace = 0,
                Debug = 1,
                Info = 2,
                Warn = 3,
                Error = 4,
            }
            public delegate void LogCallback(LogLevel level, string message);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_version")]
            public static extern SdkVersion version();

#nullable enable

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_initialize_logging")]
            public static extern Result initialize_logging(
                [MarshalAs(UnmanagedType.FunctionPtr)] LogCallback? logCallback);

#nullable disable

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_initialize_with_ovr_plugin")]
            static extern Result initialize_with_ovr_plugin_bytes(
                [In] byte[] game_engine_name,
                [In] byte[] game_engine_version,
                [In] byte[] game_engine_haptics_sdk_version);

            public static Result initialize_with_ovr_plugin(
                string game_engine_name,
                string game_engine_version,
                string game_engine_haptics_sdk_version)
            {
                byte[] name = Encoding.UTF8.GetBytes(game_engine_name + '\0');
                byte[] version = Encoding.UTF8.GetBytes(game_engine_version + '\0');
                byte[] sdk_version = Encoding.UTF8.GetBytes(game_engine_haptics_sdk_version + '\0');

                return initialize_with_ovr_plugin_bytes(name, version, sdk_version);
            }

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_initialize_with_null_backend")]
            public static extern Result initialize_with_null_backend();

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_uninitialize")]
            public static extern Result uninitialize();

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_initialized")]
            public static extern Result initialized(out bool initialized);

            [DllImport(NativeLibName)]
            static extern IntPtr haptics_sdk_error_message();

            [DllImport(NativeLibName)]
            static extern int haptics_sdk_error_message_length();

            public static string error_message()
            {
                IntPtr message = haptics_sdk_error_message();
                if (message == IntPtr.Zero)
                {
                    throw new InvalidOperationException("No error message is available");
                }
                int message_length_bytes = haptics_sdk_error_message_length();
                byte[] message_bytes = new byte[message_length_bytes];
                Marshal.Copy(message, message_bytes, 0, message_length_bytes);
                return Encoding.UTF8.GetString(message_bytes);
            }

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_set_suspended")]
            public static extern Result set_suspended(bool suspended);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_load_clip")]
            static extern Result load_clip_bytes([In] byte[] data, uint data_length, out int clip_id_out);

            public static Result load_clip(string data, out int clip_id_out)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                return load_clip_bytes(bytes, (uint)bytes.Length, out clip_id_out);
            }

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_clip_duration")]
            public static extern Result clip_duration(int clipId, out float clip_duration);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_release_clip")]
            public static extern Result release_clip(int clipId);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_create_player")]
            public static extern Result create_player(out int player_id);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_release_player")]
            public static extern Result release_player(int playerId);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_player_set_clip")]
            public static extern Result player_set_clip(int playerId, int clipId);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_player_play")]
            public static extern Result player_play(int playerId, Controller controller);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_player_stop")]
            public static extern Result player_stop(int playerId);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_player_set_amplitude")]
            public static extern Result player_set_amplitude(int playerId, float amplitude);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_player_amplitude")]
            public static extern Result player_amplitude(int playerId, out float amplitude);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_player_set_frequency_shift")]
            public static extern Result player_set_frequency_shift(int playerId, float amount);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_player_frequency_shift")]
            public static extern Result player_frequency_shift(int playerId, out float frequency_shift);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_player_set_looping_enabled")]
            public static extern Result player_set_looping_enabled(int playerId, bool enabled);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_player_looping_enabled")]
            public static extern Result player_looping_enabled(int playerId, out bool looping_enabled);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_player_set_priority")]
            public static extern Result player_set_priority(int playerId, uint priority);

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_player_priority")]
            public static extern Result player_priority(int playerId, out uint priority);

            public struct NullBackendStatistics
            {
                public long play_call_count;
                public long samples_played;
            }

            [DllImport(NativeLibName, EntryPoint = "haptics_sdk_get_null_backend_statistics")]
            public static extern NullBackendStatistics get_null_backend_statistics();
        }
    }
}
