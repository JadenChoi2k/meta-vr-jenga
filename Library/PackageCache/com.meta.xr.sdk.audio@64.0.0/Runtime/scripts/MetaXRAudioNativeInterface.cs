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
using System.Runtime.InteropServices;
using UnityEngine;
using Meta.XR.Audio;

namespace Meta
{
    namespace XR
    {
        namespace Audio
        {
            /***********************************************************************************/
            // ENUMS and STRUCTS
            /***********************************************************************************/
            [Flags]
            /// \brief Enumeration of features that can be enabled/disabled.
            ///
            /// \see MetaXRAudioNativeInterface#NativeInterface#SetEnabled
            public enum EnableFlag : uint
            {
                NONE = 0, ///< Everything disabled.
                SIMPLE_ROOM_MODELING = 2, ///< Enable/disable simple room modeling globally. Default: disabled
                LATE_REVERBERATION = 3, ///< Enable/disable late reverbervation, requires simple room modeling enabled. Default: disabled
                RANDOMIZE_REVERB = 4, ///< Randomize reverb parameters to diminish artifacts. Output sounds perceptually better when enabled but can complicate unit testing. Default: enabled
                PERFORMANCE_COUNTERS = 5, ///< Enable performance profiling (increases CPU cost slightly when enabled). Default: disabled
            }
        }
    }
}

/// \brief Parent class that functions as a scope for all the code that wraps binary interfaces for the Unity, Wwise and FMOD plug-ins.
public class MetaXRAudioNativeInterface
{
    static NativeInterface CachedInterface;

    /// \brief The current interface being used by the system.
    ///
    /// The first access of this field will trigger a scan for any Meta XR Audio plugin binaries, which are searched for in the following order (taking the first one found and halting any further search):
    /// 1. Meta XR Audio Plugin for Wwise
    /// 2. Meta XR Audio Plugin for FMOD
    /// 3. Meta XR Audio Plugin for Unity
    ///
    /// If none are found, Unity is assumed, but attempting to call any of the functions of the returned `NativeInterface` class result in undefined behavior.
    ///
    /// \see NativeInterface
    /// \see UnityNativeInterface
    /// \see WwisePluginInterface
    /// \see FMODPluginInterface
    public static NativeInterface Interface { get { if (CachedInterface == null) CachedInterface = FindInterface(); return CachedInterface; } }

    static NativeInterface FindInterface()
    {
        IntPtr temp;
        try
        {
            temp = WwisePluginInterface.getOrCreateGlobalOvrAudioContext();
            Debug.Log("Meta XR Audio Native Interface initialized with Wwise plugin");
            return new WwisePluginInterface();
        }
        catch(System.DllNotFoundException)
        {
            // this is fine
        }
        try
        {
            FMODPluginInterface.ovrAudio_GetPluginContext(out temp, ClientType.OVRA_CLIENT_TYPE_FMOD);
            Debug.Log("Meta XR Audio Native Interface initialized with FMOD plugin");
            return new FMODPluginInterface();
        }
        catch (System.DllNotFoundException)
        {
            // this is fine
        }

        Debug.Log("Meta XR Audio Native Interface initialized with Unity plugin");
        return new UnityNativeInterface();
    }

    /// \brief Enumeration of all the data types that could be used in the binary's interface.
    public enum ovrAudioScalarType : uint
    {
        Int8, ///< 8-bit signed integer [-128, 127]
        UInt8, ///< 8-bit unsigned integer [0, 255]
        Int16, ///< 16-bit signed integer [-32768, 32767]
        UInt16, ///< 16-bit unsigned integer [0, 65535]
        Int32, ///< 32-bit signed integer [-2147483648, 21474836]
        UInt32, ///< 32-bit unsigned integer [0, 4294967295]
        Int64, ///< 64-bit signed integer [-9223372036854775808, 9223372036854775807]
        UInt64, ///< 64-bit unsigned integer [0, 184467440737095516]
        Float16, ///< 16-bit floating point number with 1 bit sign, 10 bits exponent, and 15 bits mantissa
        Float32, ///< 32-bit floating point number with 1 bit sign, 23 bits exponent, and 23 bits mantissa
        Float64 ///< 64-bit floating point number with 1 bit sign, 52 bits exponent, and 52 bits mantissa
    }

    /// \brief A colection of constant integers used as an enumeration to specify the what type of client/binary a MetaXRAudioNativeInterface class wraps.
    public class ClientType
    {
        // Copied from AudioSDK\OVRAudio\OVR_Audio_Internal.h
        public const uint OVRA_CLIENT_TYPE_NATIVE = 0; ///< [DEPRECATED] Native, raw binary client type or unknown
        public const uint OVRA_CLIENT_TYPE_WWISE_2016 = 1; ///< [DEPRECATED] Wwise 2016 client
        public const uint OVRA_CLIENT_TYPE_WWISE_2017_1 = 2; ///< [DEPRECATED] Wwise 2017.1 client
        public const uint OVRA_CLIENT_TYPE_WWISE_2017_2 = 3; ///< [DEPRECATED] Wwise 2017.2 client
        public const uint OVRA_CLIENT_TYPE_WWISE_2018_1 = 4; ///< [DEPRECATED] Wwise 2018.1 client
        public const uint OVRA_CLIENT_TYPE_FMOD = 5; ///< FMOD client
        public const uint OVRA_CLIENT_TYPE_UNITY = 6; ///< Unity client
        public const uint OVRA_CLIENT_TYPE_UE4 = 7; ///< [DEPRECATED] UE4 client
        public const uint OVRA_CLIENT_TYPE_VST = 8; ///< [DEPRECATED] UE4 client
        public const uint OVRA_CLIENT_TYPE_AAX = 9; ///< [DEPRECATED] AAX client
        public const uint OVRA_CLIENT_TYPE_TEST = 10; ///< [DEPRECATED] Test client
        public const uint OVRA_CLIENT_TYPE_OTHER = 11; ///< [DEPRECATED] Other/Unknown client
        public const uint OVRA_CLIENT_TYPE_WWISE_UNKNOWN = 12; ///< [DEPRECATED] Unknown version of the Wwise client.
    }

    /// \brief Abstract parent class for all classes that wrap a binary's interface.
    /// \see UnityNativeInterface
    /// \see WwisePluginInterface
    /// \see FMODPluginInterface
    public interface NativeInterface
    {

        /***********************************************************************************/
        // Shoebox Reflections API
        /***********************************************************************************/

        /// \brief Set global shoebox room reverberation parameters.
        ///
        /// These parameters are used for reverberation/early reflections if
        /// SIMPLE_ROOM_MODELING is enabled. If SIMPLE_ROOM_MODELING is disabled, these values can still be specified and
        /// will be cached. Upon enabling room modelling, the cached values will be applied.
        ///
        /// Note, it's easiest to the MetaXRAudioRoomAcousticProperties MonoBehavior instead of calling this function directly.
        ///
        /// \param[in] width, height, depth Dimensions of shoebox room in meters. Should be > 0.
        /// \param[in] lockToListenerPosition If true, room is centered on listener. If false, room center is specified by
        /// RoomPosition coordinates
        /// \param[in] position Desired position of room's origin which is in the center of the room. Ignored if \p lockToListenerPosition is true.
        /// \param[in] wallMaterials Reflection coefficients (in the range of [0.0, 1.0f]) for room materials for each wall. It is a packed array the refletion coefficients for the right, left, ceiling, floor, front and back walls in that order, each wall having 4 frequency-dependent reflection coefficients in order of increasing frequency.
        /// \return ovrResult indicating success (0) or failure (1)
        ///
        /// \see MetaXRAudioRoomAcousticProperties
        int SetAdvancedBoxRoomParameters(float width, float height, float depth,
            bool lockToListenerPosition, Vector3 position, float[] wallMaterials);

        /// \brief Set the level of the global reverb.
        ///
        /// Sets the linear gain applied to the output of the reverberation. Lower values effectively make all objects drier.
        ///
        /// \param[in] linearLevel Linear gain to be applied to the output of the reverberator. In the range of [0,+Inf)
        /// \return ovrResult indicating success (0) or failure (1)
        ///
        /// \see MetaXRAudioNativeInterface#NativeInterface#SetEnabled
        int SetSharedReverbWetLevel(float linearLevel);

        /// \brief Enable/disable options in the audio context.
        ///
        /// \param[in] feature Meta#XR#Audio#EnableFlag specifying which feature to enable/disable.
        /// \param[in] enabled If true, enable the specified feature. If false, disable the specified feature.
        /// \return Returns an ovrResult indicating success or failure
        ///
        /// \see Meta#XR#Audio#EnableFlag
        int SetEnabled(int feature, bool enabled);

        /// \brief Sets the clutter factor for the global room simulation.
        ///
        /// This parameter is meant to simulate effect furniture and other objects have on reverberation. Higher
        /// values simulate more furniture and other objects in the room and reduce the T60 of the reverberator
        /// (moreso in the higher frequencies than the low). Lower values simulate an emptier room.
        ///
        /// \param[in] clutterFactor An array of clutter values for each audio band, [0,1]. 0.0 represents an empty room for that frequency and 1.0 is the maximum T60 reduction for that band.
        /// \return Returns an ovrResult indicating success or failure
        int SetRoomClutterFactor(float[] clutterFactor);

        /// \brief Set the number of dynamic rays cast per second.
        ///
        /// More rays mean more accurate and responsive modelling but at the expense of increased CPU usage.
        ///
        /// \param[in] RaysPerSecond The number of rays cast per second. [0,8192], Default = 256
        /// \return Returns an ovrResult indicating success or failure
        ///
        /// \see SetDynamicRoomInterpSpeed
        /// \see SetDynamicRoomMaxWallDistance
        /// \see SetDynamicRoomRaysRayCacheSize
        int SetDynamicRoomRaysPerSecond(int RaysPerSecond);

        /// \brief Set the interpolation speed of the dynamic room model.
        ///
        /// Higher values will cause the reverb to update more quickly but less smoothly.
        ///
        /// \param[in] InterpSpeed The speed with which to interpolate between the old and new dynamic room model. [0, 1.0], Default = 0.9
        /// \return Returns an ovrResult indicating success or failure.
        int SetDynamicRoomInterpSpeed(float InterpSpeed);

        /// \brief Set the maximum distance a cast ray can travel in a dynamic simulation.
        ///
        /// Larger values allows one to simulate larger rooms but unnecessarily increases CPU usage for smaller rooms.
        /// \param[in] MaxWallDistance Max distance, in meters, a project ray can travel before being ignored. [0, 250], Default = 50
        /// \return Returns an ovrResult indicating success or failure.
        ///
        /// \see SetDynamicRoomRaysPerSecond
        /// \see SetDynamicRoomInterpSpeed
        /// \see SetDynamicRoomRaysRayCacheSize
        int SetDynamicRoomMaxWallDistance(float MaxWallDistance);

        /// \brief Set the size of the cache that holds the history of cast rays.
        ///
        /// A larger value will mean reverb parameters are calculated using more rays/samples (and thus the simulation will be more
        /// stable) at the expense of making the reverb less responsive to geometry changes and increasing runtime memory usage.
        ///
        /// \param[in] RayCacheSize The number of rays held in the cache. [64,8192], Default = 512
        /// \return Returns an ovrResult indicating success or failure.
        ///
        /// \see SetDynamicRoomRaysPerSecond
        /// \see SetDynamicRoomInterpSpeed
        /// \see SetDynamicRoomMaxWallDistance
        int SetDynamicRoomRaysRayCacheSize(int RayCacheSize);

        /// \brief Get the current dimensions, reflection coefficients and position of the shoebox room.
        ///
        /// Note, it's easiest to the MetaXRAudioRoomAcousticProperties MonoBehavior instead of calling this function directly.
        ///
        /// \param[out] roomDimensions The dimensions of the shoebox room in meters. [Width, Height, Depth]
        /// \param[out] reflectionsCoefs The reflection coefficients of the shoebox room. [Left, Right, Up, Down, Front Back], each being 4-vector of frequency-dependent reflection coefficients from lowest to highest frequency
        /// \param[out] position The position of the shoebox room in Unity world space coordinates. [X,Y,Z]
        /// \return Returns an ovrResult indicating success or failure.
        ///
        /// \see MetaXRAudioRoomAcousticProperties
        int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);

        /// \brief Retrieves the cache of ray cast hits that are being used to estimate the room
        ///
        /// This is useful for debugging/visualization.
        ///
        /// \param[out] points Array of 3D positions where rays were found to intersect scene geoemtry. The length of this array will be equal to the size of the cache.
        /// \param[out] normals Array of 3D normal vectors corresponding to the intersections listed in \p points
        /// \param[out] length Corresponding length of the rays that originate at the listener and terminate at the intersections listed in \p points.
        /// \return Returns an ovrResult indicating success or failure
        ///
        /// \see SetDynamicRoomRaysRayCacheSize
        int GetRaycastHits(Vector3[] points, Vector3[] normals, int length);
    }

    /***********************************************************************************/
    // UNITY NATIVE
    /***********************************************************************************/
    public class UnityNativeInterface : NativeInterface
    {
        /// \brief Name of the binary this interface wraps.
        ///
        /// This value can be used in  `[DllImport(binaryName)]` decorators and tells Unity what the binary name is for the Unity plug-in.
        public const string binaryName = "MetaXRAudioUnity";

        /***********************************************************************************/
        // Context API: Required to create internal context if it does not exist yet
        IntPtr context_ = IntPtr.Zero;
        IntPtr context { get { if (context_ == IntPtr.Zero) { ovrAudio_GetPluginContext(out context_, ClientType.OVRA_CLIENT_TYPE_UNITY); } return context_; } }

        /// \brief Get the handle to the current context, creating one if necessary.
        ///
        /// Note that Unity's editor, player, and standalone builds will have different contexts.
        ///
        /// \param[out] context The returned handle to the context.
        /// \param[in] clientType Unused/deprecated. You can ignore this parameter.
        /// \return Returns an ovrResult indicating success or failure.
        [DllImport(binaryName)]
        public static extern int ovrAudio_GetPluginContext(out IntPtr context, uint clientType);

        /***********************************************************************************/
        // Shoebox Reflections API
        [DllImport(binaryName)]
        private static extern int ovrAudio_SetAdvancedBoxRoomParametersUnity(IntPtr context, float width, float height, float depth,
            bool lockToListenerPosition, float positionX, float positionY, float positionZ,
            float[] wallMaterials);
        public int SetAdvancedBoxRoomParameters(float width, float height, float depth,
            bool lockToListenerPosition, Vector3 position, float[] wallMaterials)
        {
            return ovrAudio_SetAdvancedBoxRoomParametersUnity(context, width, height, depth,
                lockToListenerPosition, position.x, position.y, -position.z, wallMaterials);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetRoomClutterFactor(IntPtr context, float[] clutterFactor);
        public int SetRoomClutterFactor(float[] clutterFactor)
        {
            return ovrAudio_SetRoomClutterFactor(context, clutterFactor);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetSharedReverbWetLevel(IntPtr context, float linearLevel);
        public int SetSharedReverbWetLevel(float linearLevel)
        {
            return ovrAudio_SetSharedReverbWetLevel(context, linearLevel);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);
        public int SetEnabled(int feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, EnableFlag what, int enable);
        public int SetEnabled(EnableFlag feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysPerSecond(IntPtr context, int RaysPerSecond);
        public int SetDynamicRoomRaysPerSecond(int RaysPerSecond)
        {
            return ovrAudio_SetDynamicRoomRaysPerSecond(context, RaysPerSecond);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomInterpSpeed(IntPtr context, float InterpSpeed);
        public int SetDynamicRoomInterpSpeed(float InterpSpeed)
        {
            return ovrAudio_SetDynamicRoomInterpSpeed(context, InterpSpeed);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomMaxWallDistance(IntPtr context, float MaxWallDistance);
        public int SetDynamicRoomMaxWallDistance(float MaxWallDistance)
        {
            return ovrAudio_SetDynamicRoomMaxWallDistance(context, MaxWallDistance);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysRayCacheSize(IntPtr context, int RayCacheSize);
        public int SetDynamicRoomRaysRayCacheSize(int RayCacheSize)
        {
            return ovrAudio_SetDynamicRoomRaysRayCacheSize(context, RayCacheSize);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRoomDimensions(IntPtr context, float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);
        public int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position)
        {
            return ovrAudio_GetRoomDimensions(context, roomDimensions, reflectionsCoefs, out position);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRaycastHits(IntPtr context, Vector3[] points, Vector3[] normals, int length);
        public int GetRaycastHits(Vector3[] points, Vector3[] normals, int length)
        {
            return ovrAudio_GetRaycastHits(context, points, normals, length);
        }
    }

    /***********************************************************************************/
    // WWISE
    /***********************************************************************************/
    public class WwisePluginInterface : NativeInterface
    {
        /// \brief Name of the binary this interface wraps.
        ///
        /// This value can be used in  `[DllImport(binaryName)]` decorators and tells Unity what the binary name is for the Wwise plug-in.
        public const string binaryName = "MetaXRAudioWwise";
        /***********************************************************************************/
        // Context API: Required to create internal context if it does not exist yet
        IntPtr context_ = IntPtr.Zero;

        IntPtr context
        {
            get
            {
                if (context_ == IntPtr.Zero)
                {
                    context_ = getOrCreateGlobalOvrAudioContext();
                }
                return context_;
            }
        }

        /// \brief Get the handle to the current context, creating one if necessary.
        ///
        /// Note that Unity's editor, player, and standalone builds will have different contexts.
        ///
        /// \return The returned handle to the context.
        [DllImport(binaryName)]
        public static extern IntPtr getOrCreateGlobalOvrAudioContext();

        /***********************************************************************************/
        // Shoebox Reflections API
        [DllImport(binaryName)]
        private static extern int ovrAudio_SetAdvancedBoxRoomParametersUnity(IntPtr context, float width, float height, float depth,
            bool lockToListenerPosition, float positionX, float positionY, float positionZ,
            float[] wallMaterials);
        public int SetAdvancedBoxRoomParameters(float width, float height, float depth,
            bool lockToListenerPosition, Vector3 position, float[] wallMaterials)
        {
            return ovrAudio_SetAdvancedBoxRoomParametersUnity(context, width, height, depth,
                lockToListenerPosition, position.x, position.y, -position.z, wallMaterials);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetRoomClutterFactor(IntPtr context, float[] clutterFactor);
        public int SetRoomClutterFactor(float[] clutterFactor)
        {
            return ovrAudio_SetRoomClutterFactor(context, clutterFactor);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetSharedReverbWetLevel(IntPtr context, float linearLevel);
        public int SetSharedReverbWetLevel(float linearLevel)
        {
            return ovrAudio_SetSharedReverbWetLevel(context, linearLevel);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);
        public int SetEnabled(int feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, EnableFlag what, int enable);
        public int SetEnabled(EnableFlag feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysPerSecond(IntPtr context, int RaysPerSecond);
        public int SetDynamicRoomRaysPerSecond(int RaysPerSecond)
        {
            return ovrAudio_SetDynamicRoomRaysPerSecond(context, RaysPerSecond);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomInterpSpeed(IntPtr context, float InterpSpeed);
        public int SetDynamicRoomInterpSpeed(float InterpSpeed)
        {
            return ovrAudio_SetDynamicRoomInterpSpeed(context, InterpSpeed);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomMaxWallDistance(IntPtr context, float MaxWallDistance);
        public int SetDynamicRoomMaxWallDistance(float MaxWallDistance)
        {
            return ovrAudio_SetDynamicRoomMaxWallDistance(context, MaxWallDistance);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysRayCacheSize(IntPtr context, int RayCacheSize);
        public int SetDynamicRoomRaysRayCacheSize(int RayCacheSize)
        {
            return ovrAudio_SetDynamicRoomRaysRayCacheSize(context, RayCacheSize);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRoomDimensions(IntPtr context, float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);
        public int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position)
        {
            return ovrAudio_GetRoomDimensions(context, roomDimensions, reflectionsCoefs, out position);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRaycastHits(IntPtr context, Vector3[] points, Vector3[] normals, int length);
        public int GetRaycastHits(Vector3[] points, Vector3[] normals, int length)
        {
            return ovrAudio_GetRaycastHits(context, points, normals, length);
        }
    }

    /***********************************************************************************/
    // FMOD
    /***********************************************************************************/
    public class FMODPluginInterface : NativeInterface
    {
        /// \brief Name of the binary this interface wraps.
        ///
        /// This value can be used in  `[DllImport(binaryName)]` decorators and tells Unity what the binary name is for the FMOD plug-in.
        public const string binaryName = "MetaXRAudioFMOD";

        /***********************************************************************************/
        // Context API: Required to create internal context if it does not exist yet
        IntPtr context_ = IntPtr.Zero;
        IntPtr context { get { if (context_ == IntPtr.Zero) { ovrAudio_GetPluginContext(out context_, ClientType.OVRA_CLIENT_TYPE_FMOD); } return context_; } }

        /// \brief Get the handle to the current context, creating one if necessary.
        ///
        /// Note that Unity's editor, player, and standalone builds will have different contexts.
        ///
        /// \param[out] context The returned handle to the context.
        /// \param[in] clientType Unused/deprecated. You can ignore this parameter.
        /// \return Returns an ovrResult indicating success or failure.
        [DllImport(binaryName)]
        public static extern int ovrAudio_GetPluginContext(out IntPtr context, uint clientType);

        /***********************************************************************************/
        // Shoebox Reflections API
        [DllImport(binaryName)]
        private static extern int ovrAudio_SetAdvancedBoxRoomParametersUnity(IntPtr context, float width, float height, float depth,
            bool lockToListenerPosition, float positionX, float positionY, float positionZ,
            float[] wallMaterials);
        public int SetAdvancedBoxRoomParameters(float width, float height, float depth,
            bool lockToListenerPosition, Vector3 position, float[] wallMaterials)
        {
            return ovrAudio_SetAdvancedBoxRoomParametersUnity(context, width, height, depth,
                lockToListenerPosition, position.x, position.y, -position.z, wallMaterials);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetRoomClutterFactor(IntPtr context, float[] clutterFactor);
        public int SetRoomClutterFactor(float[] clutterFactor)
        {
            return ovrAudio_SetRoomClutterFactor(context, clutterFactor);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetSharedReverbWetLevel(IntPtr context, float linearLevel);
        public int SetSharedReverbWetLevel(float linearLevel)
        {
            return ovrAudio_SetSharedReverbWetLevel(context, linearLevel);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, int what, int enable);
        public int SetEnabled(int feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_Enable(IntPtr context, EnableFlag what, int enable);
        public int SetEnabled(EnableFlag feature, bool enabled)
        {
            return ovrAudio_Enable(context, feature, enabled ? 1 : 0);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysPerSecond(IntPtr context, int RaysPerSecond);
        public int SetDynamicRoomRaysPerSecond(int RaysPerSecond)
        {
            return ovrAudio_SetDynamicRoomRaysPerSecond(context, RaysPerSecond);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomInterpSpeed(IntPtr context, float InterpSpeed);
        public int SetDynamicRoomInterpSpeed(float InterpSpeed)
        {
            return ovrAudio_SetDynamicRoomInterpSpeed(context, InterpSpeed);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomMaxWallDistance(IntPtr context, float MaxWallDistance);
        public int SetDynamicRoomMaxWallDistance(float MaxWallDistance)
        {
            return ovrAudio_SetDynamicRoomMaxWallDistance(context, MaxWallDistance);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_SetDynamicRoomRaysRayCacheSize(IntPtr context, int RayCacheSize);
        public int SetDynamicRoomRaysRayCacheSize(int RayCacheSize)
        {
            return ovrAudio_SetDynamicRoomRaysRayCacheSize(context, RayCacheSize);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRoomDimensions(IntPtr context, float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position);
        public int GetRoomDimensions(float[] roomDimensions, float[] reflectionsCoefs, out Vector3 position)
        {
            return ovrAudio_GetRoomDimensions(context, roomDimensions, reflectionsCoefs, out position);
        }

        [DllImport(binaryName)]
        private static extern int ovrAudio_GetRaycastHits(IntPtr context, Vector3[] points, Vector3[] normals, int length);
        public int GetRaycastHits(Vector3[] points, Vector3[] normals, int length)
        {
            return ovrAudio_GetRaycastHits(context, points, normals, length);
        }
    }
}
