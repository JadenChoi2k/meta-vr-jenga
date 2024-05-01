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

/************************************************************************************
 * Filename    :   MetaXRAudioSource.cs
 * Content     :   Interface into the Meta XR Audio Plugin
 ***********************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

/// \brief Component to extend the built-in AudioSource with Meta-specific features.
///
/// Adding this component to any game object that also has an AudioSource component allows specification of
/// Meta-specific parameters to control how that audio source is rendered.
///
/// The public member variables of this class directly control the spatializer plug-in assigned to the accompaning
/// audio source. The spatializer plug-ins values are updated to the member variable values every call to
/// MetaXRAudioSource::Update().
[RequireComponent(typeof(AudioSource))]
public class MetaXRAudioSource : MonoBehaviour
{
    private AudioSource source_;
    private bool wasPlaying_ = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        Debug.Log($"Setting spatial voice limit: {MetaXRAudioSettings.Instance.voiceLimit}");
        MetaXRAudio_SetGlobalVoiceLimit(MetaXRAudioSettings.Instance.voiceLimit);
    }

    [SerializeField]
    [Tooltip("Enables HRTF Spatialization.")]
    private bool enableSpatialization = true;
    /// \brief Enable/disable HRTF Spatialization. Default = true.
    ///
    /// If false, audio will not be spatialized (i.e. it won't pass through HRTF convolution or be sent
    /// to the reverb bus or have early reflections). This effectively is an override for the "Spatialize" parameter of
    /// Unity's built-in AudioSource component.
    public  bool EnableSpatialization
    {
        get
        {
            return enableSpatialization;
        }
        set
        {
            enableSpatialization = value;
        }
    }

    [SerializeField]
    [Tooltip("Additional gain beyond 0dB")]
    [Range(0.0f, 20.0f)]
    private float gainBoostDb = 0.0f;
    /// \brief Gain the AudioSource's signal by an optional amount. [0dB, 20dB], Default = 0dB.
    ///
    /// The gain will be applied to the direct sound of only this audio source, and (if enabled and again, only for this audio source) its sends to the early reflections and late reverberation busses.
    ///
    /// \see ReverbSendDb to control only the late reverberation send amount.
    /// \see MetaXRAudioSourceExperimentalFeatures#EarlyReflectionsSendDb to control the early reflections send amount.
    public  float GainBoostDb
    {
        get
        {
            return gainBoostDb;
        }
        set
        {
            gainBoostDb = Mathf.Clamp(value, 0.0f, 20.0f);
        }
    }

    [SerializeField]
    [Tooltip("Enables room acoustics simulation (early reflections and reverberation) for this audio source only")]
    private bool enableAcoustics = true;
    /// \brief Enable/disable room acoustics simulation for this AudioSource. Default = true.
    ///
    /// If enabled, this audio source will have room acoustics simulated for it, including early reflections
    /// and reverberation in addition to rendering it's direct sound. If disabled, only the direct sound will be
    /// rendered.
    public  bool EnableAcoustics
    {
        get
        {
            return enableAcoustics;
        }
        set
        {
            enableAcoustics = value;
        }
    }

    [SerializeField]
    [Tooltip("Additional gain applied to reverb send for this audio source only")]
    [Range(-60.0f, 20.0f)]
    private float reverbSendDb = 0.0f;
    /// \brief Additional gain to apply to the signal before sending it to the reverb bus. [-60.0dB, 20.0dB] Default = 0dB
    ///
    /// This gain will be applie only before sending the signal to the reverb bus. The early reflections and direct
    /// sound are not affected by this gain.
    ///
    /// \see GainBoostDb to control the gain of all aspects of an audio source (direct sound, early reflections, and late reverberation)
    /// \see MetaXRAudioSourceExperimentalFeatures#EarlyReflectionsSendDb to control the early reflections send amount.
    public float ReverbSendDb
    {
        get
        {
            return reverbSendDb;
        }
        set
        {
            reverbSendDb = Mathf.Clamp(value, -60.0f, 20.0f);
        }
    }

    void Awake()
    {
        source_ = GetComponent<AudioSource>();
        UpdateParameters();
    }

    void Update()
    {
        if (source_ == null)
        {
            source_ = GetComponent<AudioSource>();
            if (source_ == null)
            {
                return;
            }
        }

        UpdateParameters();

        wasPlaying_ = source_.isPlaying;
    }

    /// \brief Enumeration of the Meta's native spatialializer plug-in for Unity.
    ///
    /// \see UpdateParameters to see where this enumeration is used.
    public enum NativeParameterIndex : int
    {
        /// \brief Overall gain applied to this audio source's direct sound, early reflections and late reverberation. [0, +20dB] Default = 0dB
        ///
        /// \see GainBoostDb for further description and a more convenient control of this parameter.
        P_GAIN,
        /// \brief [UNUSED] Enable/disable inverse-law distance attenuation. Default: false
        ///
        /// If true (i.e 1.0), then the library will apply an inverse-square law distance attenutation to the direct sound sound. If false, the direct sound will not be attenuated in any way based on distance by the plug-in.
        P_USEINVSQR,
        /// \brief [UNUSED) Attenuation distance minimum, in meters. [0, 10000] Default: 1m
        ///
        /// Only used when P_USEINVSQR is true.
        P_NEAR,
        /// \brief [UNUSED] Attenuation distance maximum, in meters. [0, 10000] Default: 400
        ///
        /// Only used when P_USEINVSQR is true.
        P_FAR,
        /// \brief Controls the volumetric radius of the sound source. [0, +Inf) Default: 0
        ///
        /// \see MetaXRAudioSourceExperimentalFeatures#VolumetricRadius for a more convenient control of this parameter.
        P_RADIUS,
        /// \brief Enable/disable acoustics for this audio source only. Default: false
        ///
        /// \see EnableAcoustics for further description and a more convenient control of this parameter.
        P_DISABLE_RFL,
        /// \brief [READONLY] Ambisonic stream status.
        ///
        /// Caches the status of the ambisonic stream. Mostly for internal/debugging use.
        /// Unitialized = -1, not enabled = 0, success = 1, stream error = 2, process error =3.
        P_AMBISTAT,
        /// \brief [READONLY] Parameter to query whether the global reflections are enabled.
        ///
        /// This parameter is controlled with the "Early Reflections Enabled" parameter of the "Meta XR Audio Reflection" audio effect plugin.
        /// \see MetaXRAudioNativeInterface#NativeInterface#SetEnabled to enable or disable reflections globally in C#, but it is better controlled with the "Meta XR Audio Reflection" plug-in parameter.
        P_READONLY_GLOBAL_RFL_ENABLED,
        /// \brief [READONLY] Parameter to query for the total number of voices that are active in the engine.
        ///
        /// Using this parameter, you can see how many voices are currently active in the engine.
        ///
        /// \see MetaXRAudioSettings#voiceLimit to set the maximum number of voices the engine supports.
        P_READONLY_NUM_VOICES,
        /// \brief HRTF intensity. [0, 1] Default: 1.0
        ///
        /// \see MetaXRAudioSourceExperimentalFeatures#HrtfIntensity
        P_HRTF_INTENSITY,
        /// \brief Gain applied to the early reflections send. [-60.0dB, 20.0dB] Default: 0dB
        ///
        /// \see MetaXRAudioSourceExperimentalFeatures#EarlyReflectionsSendDb for further description and a more convenient control of this parameter.
        P_REFLECTIONS_SEND,
        /// \brief Gain applied to the late reverberation send. [-60.0dB, 20.0dB] Default: 0dB
        ///
        /// \see MetaXRAudioSource#ReverbSendDb for further description and a more convenient control of this parameter.
        P_REVERB_SEND,
        /// \brief Enables/disabled directivty for this audio source only. Default: false
        ///
        /// \see MetaXRAudioSourceExperimentalFeatures#DirectivityPattern for choosing which pattern is applied, and
        /// enabling/disabling this parameter with a more convenient control.
        /// \see MetaXRAudioSourceExperimentalFeatures#DirectivityIntensity for controlling the intensity of the directivity pattern applied.
        P_DIRECTIVITY_ENABLED,
        /// \brief Directivity intensity [0.0, 1.0] Default: 1.0
        ///
        /// \see MetaXRAudioSourceExperimentalFeatures#DirectivityIntensity for a more convenient control of this parameter.
        /// \see MetaXRAudioSourceExperimentalFeatures#DirectivityPattern for choosing which pattern is applied.
        P_DIRECTIVITY_INTENSITY,
        /// \brief [Internal] Force the direct sound of an object to be rendered via an ambisonic HRTF. Default: false
        ///
        /// When true, the source will be rendered using the same ambisonic HRTFs that are used for the early reflections
        /// and late reverberation which is more computationally efficient but spatialized less accurately. If false, the
        /// direct sound will be rendered by convolving with HRTF filters directly, which is more computationaly expensive
        /// but results in more accurate spatialization.
        P_AMBI_DIRECT_ENABLED,
        /// \brief Control the "reverb reach" of a sound source. [0.0, 1.0f] Default: 0.5
        ///
        /// When set to 0.0, the late reverberation will decrease with distance using the same distance-based attenuation
        /// applied to the direct sound. When 1.0, the late reverberation will decay at a rate slower than the direct sound.
        /// A settings of 0.5 is the most natural.
        /// \see MetaXRAudioSourceExperimentalFeatures#ReverbReach for further description and a more convenient control of this parameter.
        P_REVERB_REACH,
        /// \brief Enable/disable the direct sound and early reflections. Default: true
        ///
        /// \see MetaXRAudioSourceExperimentalFeatures#DirectSoundEnabled for further description and a more convenient control of this parameter.
        P_DIRECT_ENABLED,
        /// \brief Utility to easily query how many parameters the plugin has.
        P_NUM
    };

    /// \brief Sync all the member variables of this class with the spatializer plug-in instance associated with this sound source.
    ///
    /// This function should be called during every call to Update and there should be no need to call this
    /// explicitly unless you want to force sync the spatializer instance's parameters with this component.
    public void UpdateParameters()
    {
        source_.spatialize = enableSpatialization;
        source_.SetSpatializerFloat((int)NativeParameterIndex.P_GAIN, gainBoostDb);
        source_.SetSpatializerFloat((int)NativeParameterIndex.P_DISABLE_RFL, enableAcoustics ? 0.0f : 1.0f);
        source_.SetSpatializerFloat((int)NativeParameterIndex.P_REVERB_SEND, reverbSendDb);
    }

    [System.Runtime.InteropServices.DllImport(MetaXRAudioNativeInterface.UnityNativeInterface.binaryName)]
    private static extern int MetaXRAudio_SetGlobalVoiceLimit(int VoiceLimit);
}
