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

#if UNITY_ANDROID && !UNITY_EDITOR
#define ANDROID_PLATFORM_INTEGRATION
#endif

using System;
using System.Collections;
using System.Globalization;
using Meta.Voice;
using Meta.WitAi;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using Oculus.Voice.Bindings.Android;
using Oculus.Voice.Core.Bindings.Android.PlatformLogger;
using Oculus.Voice.Core.Bindings.Interfaces;
using Oculus.VoiceSDK.Utilities;
using UnityEngine;

namespace Oculus.Voice
{
    [HelpURL("https://developer.oculus.com/experimental/voice-sdk/tutorial-overview/")]
    public class AppVoiceExperience : VoiceService, IWitRuntimeConfigProvider, IWitConfigurationProvider
    {
        [SerializeField] private WitRuntimeConfiguration witRuntimeConfiguration;
        [Tooltip("Uses platform services to access wit.ai instead of accessing wit directly from within the application.")]
        [SerializeField] private bool usePlatformServices;

        [Tooltip("Enables logs related to the interaction to be displayed on console")]
        [SerializeField] private bool enableConsoleLogging;

        [Tooltip("If true, the OnFullTranscriptionEvent events will be triggered when calling Activate(string)")]
        [SerializeField] private bool sendTranscriptionEventsForMessages = false;

        public WitRuntimeConfiguration RuntimeConfiguration
        {
            get => witRuntimeConfiguration;
            set
            {
                witRuntimeConfiguration = value;

                if (voiceServiceImpl is IWitRuntimeConfigSetter configProvider)
                {
                    configProvider.RuntimeConfiguration = witRuntimeConfiguration;
                }
            }
        }

        public WitConfiguration Configuration => witRuntimeConfiguration?.witConfiguration;

        private IVoiceService voiceServiceImpl;
        private IVoiceSDKLogger voiceSDKLoggerImpl;

        private static string PACKAGE_VERSION => VoiceSDKConstants.SdkVersion;

        private bool Initialized => null != voiceServiceImpl;

        public event Action OnInitialized;

        #region Voice Service Properties
        public override bool Active => base.Active || (null != voiceServiceImpl && voiceServiceImpl.Active);
        public override bool IsRequestActive => base.IsRequestActive || (null != voiceServiceImpl && voiceServiceImpl.IsRequestActive);
        public override ITranscriptionProvider TranscriptionProvider
        {
            get => voiceServiceImpl?.TranscriptionProvider;
            set
            {
                if (voiceServiceImpl != null)
                {
                    voiceServiceImpl.TranscriptionProvider = value;
                }
            }
        }
        public override bool MicActive => null != voiceServiceImpl && voiceServiceImpl.MicActive;
        protected override bool ShouldSendMicData => witRuntimeConfiguration.sendAudioToWit ||
                                                  null == TranscriptionProvider;
        #endregion

        #if ANDROID_PLATFORM_INTEGRATION
        public bool HasPlatformIntegrations => usePlatformServices && voiceServiceImpl is VoiceSDKImpl;
        #else
        public bool HasPlatformIntegrations => false;
        #endif

        public bool EnableConsoleLogging => enableConsoleLogging;

        public override bool UsePlatformIntegrations
        {
            get => usePlatformServices;
            set
            {
                // If we're trying to turn on platform services and they're not currently active we
                // will forcibly reinit and try to set the state.
                if (usePlatformServices != value || HasPlatformIntegrations != value)
                {
                    usePlatformServices = value;
#if ANDROID_PLATFORM_INTEGRATION
                    Debug.Log($"{(usePlatformServices ? "Enabling" : "Disabling")} platform integration.");
                    InitVoiceSDK();
#endif
                }
            }
        }

        #region Voice Service Text Methods
        public override bool CanSend()
        {
            return base.CanSend() && null != voiceServiceImpl && voiceServiceImpl.CanSend();
        }

        public override VoiceServiceRequest Activate(string text, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
        {
            if (CanSend())
            {
                SetupRequestParameters(ref requestOptions, ref requestEvents);
                var request =  voiceServiceImpl.Activate(text, requestOptions, requestEvents);
                if (sendTranscriptionEventsForMessages && !string.IsNullOrEmpty(text))
                {
                    request.Events.OnFullResponse.AddListener(r =>
                    {
                        if (string.IsNullOrEmpty(r.GetTranscription()))
                        {
                            r[WitResultUtilities.WIT_KEY_TRANSCRIPTION] = text;
                        }
                    });
                    request.Events.OnSend.AddListener(r =>
                    {
                        request.Events?.OnFullTranscription?.Invoke(text);
                        VoiceEvents.OnFullTranscription?.Invoke(text);
                    });
                }
            }
            return null;
        }
        #endregion

        #region Voice Service Audio Methods
        public override bool CanActivateAudio()
        {
            return base.CanActivateAudio() && null != voiceServiceImpl && voiceServiceImpl.CanActivateAudio();
        }

        public override string GetActivateAudioError()
        {
            if (!HasPlatformIntegrations && !AudioBuffer.Instance.IsInputAvailable)
            {
                return "No Microphone(s)/recording devices found.  You will be unable to capture audio on this device.";
            }
            return base.GetActivateAudioError();
        }

        public override VoiceServiceRequest Activate(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
        {
            SetupRequestParameters(ref requestOptions, ref requestEvents);

            if (CanActivateAudio() && CanSend())
            {
                return voiceServiceImpl.Activate(requestOptions, requestEvents);
            }

            if(null == voiceServiceImpl)
            {
                VLog.D("Voice is not initialized. Attempting to initialize before activating.");
                InitVoiceSDK();

                if (CanActivateAudio() && CanSend()) return voiceServiceImpl?.Activate(requestOptions, requestEvents);
            }

            VLog.W($"Cannot currently activate\nAudio Activation Error: {GetActivateAudioError()}\nSend Error: {GetSendError()}");
            return null;
        }

        public override VoiceServiceRequest ActivateImmediately(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
        {
            SetupRequestParameters(ref requestOptions, ref requestEvents);

            if (CanActivateAudio() && CanSend())
            {
                return voiceServiceImpl.ActivateImmediately(requestOptions, requestEvents);
            }

            if(null == voiceServiceImpl)
            {
                VLog.D("Voice is not initialized. Attempting to initialize before immediate activation");
                InitVoiceSDK();

                if (CanActivateAudio() && CanSend())
                {
                    return voiceServiceImpl?.ActivateImmediately(requestOptions, requestEvents);
                }
            }

            VLog.W($"Cannot currently activate\nAudio Activation Error: {GetActivateAudioError()}\nSend Error: {GetSendError()}");
            return null;
        }

        public override void Deactivate()
        {
            voiceServiceImpl?.Deactivate();
        }

        public override void DeactivateAndAbortRequest()
        {
            voiceServiceImpl?.DeactivateAndAbortRequest();
        }

        #endregion

        private void InitVoiceSDK()
        {
            // Check voice sdk version
            if (string.IsNullOrEmpty(PACKAGE_VERSION))
            {
                VLog.E("No SDK Version Set");
            }

            // Clean up if we're switching to native C# wit impl
            if (!UsePlatformIntegrations)
            {
                if (voiceServiceImpl is VoiceSDKImpl)
                {
                    ((VoiceSDKImpl) voiceServiceImpl).Disconnect();
                }
                if (voiceSDKLoggerImpl is VoiceSDKPlatformLoggerImpl)
                {
                    try
                    {
                        ((VoiceSDKPlatformLoggerImpl)voiceSDKLoggerImpl).Disconnect();
                    }
                    catch (Exception e)
                    {
                        VLog.E($"Disconnection error: {e.Message}");
                    }
                }
            }

            // Attempt to use MonoBehaviour if applicable
            bool hasImpl = voiceServiceImpl != null;
            if (!hasImpl)
            {
                voiceServiceImpl = gameObject.GetComponent<IPlatformIntegrationOverride>();
                hasImpl = voiceServiceImpl != null;
                if (hasImpl)
                {
                    VLog.I($"Using PI override\nClass: {voiceServiceImpl.GetType()}");
                    UsePlatformIntegrations = false;
                }
            }

#if ANDROID_PLATFORM_INTEGRATION
            var loggerImpl = new VoiceSDKPlatformLoggerImpl();
            loggerImpl.Connect(PACKAGE_VERSION);
            voiceSDKLoggerImpl = loggerImpl;
            if (UsePlatformIntegrations)
            {
                VLog.I("Checking platform capabilities...");
                var platformImpl = new VoiceSDKImpl(this);
                platformImpl.OnServiceNotAvailableEvent += () => RevertToWitUnity();
                platformImpl.Connect(PACKAGE_VERSION);
                platformImpl.SetRuntimeConfiguration(RuntimeConfiguration);
                if (platformImpl.PlatformSupportsWit)
                {
                    voiceServiceImpl = platformImpl;

                    if (voiceServiceImpl is Wit wit)
                    {
                        wit.RuntimeConfiguration = witRuntimeConfiguration;
                    }

                    voiceServiceImpl.VoiceEvents = VoiceEvents;
                    voiceServiceImpl.TelemetryEvents = TelemetryEvents;
                    voiceSDKLoggerImpl.IsUsingPlatformIntegration = true;
                    hasImpl = true;
                }
                else
                {
                    VLog.W("Platform registration indicated platform support is not currently available.");
                }
            }
#else
            voiceSDKLoggerImpl = new VoiceSDKConsoleLoggerImpl();
#endif

            // Generate voice service impl
            if (!hasImpl)
            {
                RevertToWitUnity();
            }
            // Setup voice service impl
            if (voiceServiceImpl is IWitRuntimeConfigSetter configProvider)
            {
                configProvider.RuntimeConfiguration = witRuntimeConfiguration;
            }
            voiceServiceImpl.VoiceEvents = VoiceEvents;
            voiceServiceImpl.TelemetryEvents = TelemetryEvents;

            // Setup voice service logger
            voiceSDKLoggerImpl.IsUsingPlatformIntegration = UsePlatformIntegrations;
            voiceSDKLoggerImpl.WitApplication = RuntimeConfiguration?.witConfiguration?.GetLoggerAppId();
            voiceSDKLoggerImpl.ShouldLogToConsole = EnableConsoleLogging;

            // Perform initialized callback
            OnInitialized?.Invoke();
        }

        private void RevertToWitUnity()
        {
            VLog.I("Initializing Wit Unity...");
            Wit w = GetComponent<Wit>();
            if (null == w)
            {
                w = gameObject.AddComponent<Wit>();
                w.hideFlags = HideFlags.HideInInspector;
            }
            w.ShouldWrap = false; // Don't wrap requests
            voiceServiceImpl = w;
            UsePlatformIntegrations = false;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (MicPermissionsManager.HasMicPermission())
            {
                InitVoiceSDK();
            }
            else
            {
                MicPermissionsManager.RequestMicPermission();
            }

            // Listeners
            VoiceEvents.OnMinimumWakeThresholdHit?.AddListener(OnMinimumWakeThresholdHit);
            VoiceEvents.OnMicDataSent?.AddListener(OnMicDataSent);
            VoiceEvents.OnStoppedListeningDueToTimeout?.AddListener(OnStoppedListeningDueToTimeout);
            VoiceEvents.OnStoppedListeningDueToInactivity?.AddListener(OnStoppedListeningDueToInactivity);
            VoiceEvents.OnStoppedListeningDueToDeactivation?.AddListener(OnStoppedListeningDueToDeactivation);
            TelemetryEvents.OnAudioTrackerFinished?.AddListener(OnAudioDurationTrackerFinished);

            StartCoroutine(RetryInit());
        }

        private IEnumerator RetryInit()
        {
            var waitSeconds = 1;
            while (null == voiceServiceImpl)
            {
                VLog.W($"Voice Service still not initialized yet. Retrying in {waitSeconds} seconds.");
                yield return new WaitForSeconds(waitSeconds);
                if (null != voiceServiceImpl) break;
                InitVoiceSDK();
                waitSeconds++;
                if (waitSeconds == 10) waitSeconds = 1;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            #if UNITY_ANDROID
            if (voiceServiceImpl is VoiceSDKImpl platformImpl)
            {
                platformImpl.Disconnect();
            }

            if (voiceSDKLoggerImpl is VoiceSDKPlatformLoggerImpl loggerImpl)
            {
                loggerImpl.Disconnect();
            }
            #endif
            voiceServiceImpl = null;
            voiceSDKLoggerImpl = null;

            // Listeners
            VoiceEvents.OnMinimumWakeThresholdHit?.RemoveListener(OnMinimumWakeThresholdHit);
            VoiceEvents.OnMicDataSent?.RemoveListener(OnMicDataSent);
            VoiceEvents.OnStoppedListeningDueToTimeout?.RemoveListener(OnStoppedListeningDueToTimeout);
            VoiceEvents.OnStoppedListeningDueToInactivity?.RemoveListener(OnStoppedListeningDueToInactivity);
            VoiceEvents.OnStoppedListeningDueToDeactivation?.RemoveListener(OnStoppedListeningDueToDeactivation);
            TelemetryEvents.OnAudioTrackerFinished?.RemoveListener(OnAudioDurationTrackerFinished);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (enabled && hasFocus && !Initialized)
            {
                if (MicPermissionsManager.HasMicPermission())
                {
                    InitVoiceSDK();
                }
            }
        }

        #region Event listeners for logging
        protected override void OnRequestInit(VoiceServiceRequest request)
        {
            base.OnRequestInit(request);

            voiceSDKLoggerImpl.LogInteractionStart(request.Options?.RequestId, request.InputType == NLPRequestInputType.Text ? "message" : "speech");

#if ANDROID_PLATFORM_INTEGRATION
            voiceSDKLoggerImpl.LogAnnotation("clientSDKVersion", PACKAGE_VERSION);
#endif
            voiceSDKLoggerImpl.LogAnnotation("minWakeThreshold",
                RuntimeConfiguration?.soundWakeThreshold.ToString(CultureInfo.InvariantCulture));
            voiceSDKLoggerImpl.LogAnnotation("minKeepAliveTimeSec",
                RuntimeConfiguration?.minKeepAliveTimeInSeconds.ToString(CultureInfo.InvariantCulture));
            voiceSDKLoggerImpl.LogAnnotation("minTranscriptionKeepAliveTimeSec",
                RuntimeConfiguration?.minTranscriptionKeepAliveTimeInSeconds.ToString(CultureInfo.InvariantCulture));
            voiceSDKLoggerImpl.LogAnnotation("maxRecordingTime",
                RuntimeConfiguration?.maxRecordingTime.ToString(CultureInfo.InvariantCulture));
        }

        protected override void OnRequestStartListening(VoiceServiceRequest request)
        {
            base.OnRequestStartListening(request);
            voiceSDKLoggerImpl.LogInteractionPoint("startedListening");
        }

        protected override void OnRequestStopListening(VoiceServiceRequest request)
        {
            base.OnRequestStopListening(request);
            voiceSDKLoggerImpl.LogInteractionPoint("stoppedListening");
        }

        protected override void OnRequestSend(VoiceServiceRequest request)
        {
            base.OnRequestSend(request);
            voiceSDKLoggerImpl.LogInteractionPoint("witRequestCreated");
            if (request != null)
            {
                voiceSDKLoggerImpl.LogAnnotation("requestIdOverride", request.Options?.RequestId);
            }
        }

        protected override void OnRequestPartialTranscription(VoiceServiceRequest request)
        {
            base.OnRequestPartialTranscription(request);
            voiceSDKLoggerImpl.LogFirstTranscriptionTime();
        }

        protected override void OnRequestFullTranscription(VoiceServiceRequest request)
        {
            base.OnRequestFullTranscription(request);
            voiceSDKLoggerImpl.LogInteractionPoint("fullTranscriptionTime");
        }

        void OnMinimumWakeThresholdHit()
        {
            voiceSDKLoggerImpl.LogInteractionPoint("minWakeThresholdHit");
        }

        void OnStoppedListeningDueToTimeout()
        {
            voiceSDKLoggerImpl.LogInteractionPoint("stoppedListeningTimeout");
        }

        void OnStoppedListeningDueToInactivity()
        {
            voiceSDKLoggerImpl.LogInteractionPoint("stoppedListeningInactivity");
        }

        void OnStoppedListeningDueToDeactivation()
        {
            voiceSDKLoggerImpl.LogInteractionPoint("stoppedListeningDeactivate");
        }

        void OnMicDataSent()
        {
            voiceSDKLoggerImpl.LogInteractionPoint("micDataSent");
        }

        void OnAudioDurationTrackerFinished(long timestamp, double audioDuration)
        {
            voiceSDKLoggerImpl.LogAnnotation("adt_duration", audioDuration.ToString(CultureInfo.InvariantCulture));
            voiceSDKLoggerImpl.LogAnnotation("adt_finished", timestamp.ToString());
        }

        protected override void OnRequestSuccess(VoiceServiceRequest request)
        {
            base.OnRequestSuccess(request);
            WitResponseNode responseNode = request?.ResponseData;
            var tokens = responseNode?["speech"]?["tokens"];
            if (tokens != null)
            {
                int speechTokensLength = tokens.Count;
                string speechLength = tokens[speechTokensLength - 1]?["end"]?.Value;
                voiceSDKLoggerImpl.LogAnnotation("audioLength", speechLength);
            }
        }

        protected override void OnRequestComplete(VoiceServiceRequest request)
        {
            base.OnRequestComplete(request);

            if (voiceSDKLoggerImpl == null)
            {
                VLog.W("voiceSDKLoggerImpl is null");
                return;
            }

            if (request.State == VoiceRequestState.Failed)
            {
                voiceSDKLoggerImpl.LogInteractionEndFailure(request.Results.Message);
            }
            else if (request.State == VoiceRequestState.Canceled)
            {
                voiceSDKLoggerImpl.LogInteractionEndFailure("aborted");
            }
            else
            {
                voiceSDKLoggerImpl.LogInteractionEndSuccess();
            }
        }
        #endregion
    }
}
