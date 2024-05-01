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

#if USING_XR_MANAGEMENT && (USING_XR_SDK_OCULUS || USING_XR_SDK_OPENXR)
#define USING_XR_SDK
#endif

#if UNITY_2020_1_OR_NEWER
#define REQUIRES_XR_SDK
#endif

using System;
using System.Diagnostics;
using System.IO;

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MetaXRSimulatorEnabler : MonoBehaviour
{
    const string OpenXrRuntimeEnvKey = "XR_RUNTIME_JSON";
    const string PreviousOpenXrRuntimeEnvKey = "XR_RUNTIME_JSON_PREV";
    const string XrSimConfigEnvKey = "META_XRSIM_CONFIG_JSON";
    const string PreviousXrSimConfigEnvKey = "META_XRSIM_CONFIG_JSON_PREV";

    const string PackagePath = "Packages/com.meta.xr.simulator";

    private static bool unityRunningInBatchmode = false;

    static MetaXRSimulatorEnabler()
    {
        if (Environment.CommandLine.Contains("-batchmode"))
        {
            unityRunningInBatchmode = true;
        }
    }

    const string kXrSimMenu = "Oculus/Meta XR Simulator";

    private static string GetSimulatorJsonPath()
    {
        return Path.GetFullPath(PackagePath + "/MetaXRSimulator/meta_openxr_simulator.json");
    }

    private static string GetSimulatorDllPath()
    {
        return Path.GetFullPath(PackagePath + "/MetaXRSimulator/SIMULATOR.dll");
    }

    private static string GetCurrentProjectPath()
    {
        return Directory.GetParent(Application.dataPath).FullName;
    }

    private static string GetSimulatorConfigPath()
    {
        if (unityRunningInBatchmode)
        {
            return Path.GetFullPath(PackagePath + "/MetaXRSimulator/config/sim_core_configuration_ci.json");
        }
        else
        {
            return Path.GetFullPath(PackagePath + "/MetaXRSimulator/config/sim_core_configuration.json");
        }
    }

    private static bool HasSimulatorInstalled()
    {
        string simulatorJsonPath = GetSimulatorJsonPath();
        string simulatorDllPath = GetSimulatorDllPath();

        return (!string.IsNullOrEmpty(simulatorJsonPath) &&
                !string.IsNullOrEmpty(simulatorDllPath) &&
                File.Exists(simulatorJsonPath) &&
                File.Exists(simulatorDllPath));
    }

    public static bool IsSimulatorActivated()
    {
        return Environment.GetEnvironmentVariable(OpenXrRuntimeEnvKey) == GetSimulatorJsonPath();
    }

    const string kActivateSimulator = kXrSimMenu + "/Activate";

    [MenuItem(kActivateSimulator, true, 0)]
    private static bool ValidateSimulatorActivated()
    {
        bool checkMenuItem = HasSimulatorInstalled() && IsSimulatorActivated();
        Menu.SetChecked(kActivateSimulator, checkMenuItem);
        return true;
    }

    [MenuItem(kActivateSimulator, false, 0)]
    private static void ActivateSimulatorMenuItem()
    {
        ActivateSimulator(false);
    }

    // force hide dialog if activating simulator from tests
    public static void ActivateSimulator(bool forceHideDialog)
    {
#if META_XR_SDK_CORE_SUPPORTS_TELEMETRY
        using var marker = new OVRTelemetryMarker(OVRTelemetryConstants.XRSim.MarkerId.ToggleState);
        marker.AddAnnotation(OVRTelemetryConstants.XRSim.AnnotationType.IsActive, true.ToString());
#endif

        if (!HasSimulatorInstalled())
        {
            DisplayDialogOrError("Meta XR Simulator Not Found",
                "SIMULATOR.json is not found. Please enable OVRPlugin through Oculus/Tools/OVR Utilities Plugin/Set OVRPlugin To OpenXR", forceHideDialog);
#if META_XR_SDK_CORE_SUPPORTS_TELEMETRY
            marker.SetResult(OVRPlugin.Qpl.ResultType.Fail);
#endif
            return;
        }

        if (IsSimulatorActivated())
        {
            ReportInfo("Meta XR Simulator", "Meta XR Simulator is already activated.");
            return;
        }

        Environment.SetEnvironmentVariable(PreviousOpenXrRuntimeEnvKey,
            Environment.GetEnvironmentVariable(OpenXrRuntimeEnvKey));
        Environment.SetEnvironmentVariable(OpenXrRuntimeEnvKey, GetSimulatorJsonPath());

        Environment.SetEnvironmentVariable(PreviousXrSimConfigEnvKey,
            Environment.GetEnvironmentVariable(XrSimConfigEnvKey));
        Environment.SetEnvironmentVariable(XrSimConfigEnvKey, GetSimulatorConfigPath());

        ReportInfo("Meta XR Simulator is activated",
            String.Format("{0} is set to {1}\n{2} is set to {3}", OpenXrRuntimeEnvKey, Environment.GetEnvironmentVariable(OpenXrRuntimeEnvKey), XrSimConfigEnvKey, Environment.GetEnvironmentVariable(XrSimConfigEnvKey)));
    }

    const string kDeactivateSimulator = kXrSimMenu + "/Deactivate";

    [MenuItem(kDeactivateSimulator, true, 1)]
    private static bool ValidateSimulatorDeactivated()
    {
        bool checkMenuItem = !HasSimulatorInstalled() || !IsSimulatorActivated();
        Menu.SetChecked(kDeactivateSimulator, checkMenuItem);
        return true;
    }

    [MenuItem(kDeactivateSimulator, false, 1)]
    private static void DeactivateSimulatorMenuItem()
    {
        DeactivateSimulator(false);
    }

    public static void DeactivateSimulator(bool forceHideDialog)
    {
#if META_XR_SDK_CORE_SUPPORTS_TELEMETRY
        using var marker = new OVRTelemetryMarker(OVRTelemetryConstants.XRSim.MarkerId.ToggleState);
        marker.AddAnnotation(OVRTelemetryConstants.XRSim.AnnotationType.IsActive, false.ToString());
#endif

        if (!HasSimulatorInstalled())
        {
            DisplayDialogOrError("Meta XR Simulator", "SIMULATOR.json is not found. Please enable OVRPlugin through Oculus/Tools/OVR Utilities Plugin/Set OVRPlugin To OpenXR", forceHideDialog);
#if META_XR_SDK_CORE_SUPPORTS_TELEMETRY
            marker.SetResult(OVRPlugin.Qpl.ResultType.Fail);
#endif
        }

        if (!IsSimulatorActivated())
        {
            ReportInfo("Meta XR Simulator", "Meta XR Simulator is not activated.");
#if META_XR_SDK_CORE_SUPPORTS_TELEMETRY
            marker.SetResult(OVRPlugin.Qpl.ResultType.Fail);
#endif
            return;
        }

        Environment.SetEnvironmentVariable(OpenXrRuntimeEnvKey,
            Environment.GetEnvironmentVariable(PreviousOpenXrRuntimeEnvKey));
        Environment.SetEnvironmentVariable(PreviousOpenXrRuntimeEnvKey, "");

        Environment.SetEnvironmentVariable(XrSimConfigEnvKey,
            Environment.GetEnvironmentVariable(PreviousXrSimConfigEnvKey));
        Environment.SetEnvironmentVariable(PreviousXrSimConfigEnvKey, "");

        ReportInfo("Meta XR Simulator is deactivated",
            String.Format("{0} is set to {1}\n{2} is set to {3}", OpenXrRuntimeEnvKey, Environment.GetEnvironmentVariable(OpenXrRuntimeEnvKey), XrSimConfigEnvKey, Environment.GetEnvironmentVariable(XrSimConfigEnvKey)));
    }

    private static void DisplayDialogOrError(string title, string body, bool forceHideDialog = false)
    {
        if (!forceHideDialog && !unityRunningInBatchmode)
        {
            EditorUtility.DisplayDialog(title, body, "Ok");
        }
        else
        {
            ReportError(title, body);
        }
    }

    private static void DisplayDialogOrLog(string title, string body, bool forceHideDialog = false)
    {
        if (!forceHideDialog && !unityRunningInBatchmode)
        {
            EditorUtility.DisplayDialog(title, body, "Ok");
        }
        else
        {
            ReportInfo(title, body);
        }
    }

    private static void ReportInfo(string title, string body)
    {
        UnityEngine.Debug.Log(String.Format("[{0}] {1}", title, body));
    }

    private static void ReportError(string title, string body)
    {
        UnityEngine.Debug.LogError(String.Format("[{0}] {1}", title, body));
    }

    // SES functionality
    const string kSynthEnvServer = "Synthetic Environment Server";
    const string kSynthEnvServerMenu = kXrSimMenu + "/" + kSynthEnvServer;
    const string kSynthEnvServerPort = "33792";

    private static void LaunchEnvironment(string environmentName)
    {
#if META_XR_SDK_CORE_SUPPORTS_TELEMETRY
        using var marker = new OVRTelemetryMarker(OVRTelemetryConstants.XRSim.MarkerId.SESInteraction);
        marker.AddAnnotation(OVRTelemetryConstants.XRSim.AnnotationType.Action, environmentName);
#endif

        ReportInfo(kSynthEnvServer, "Launching " + environmentName);

        string binaryPath = GetSynthEnvServerPath();
        if (!File.Exists(binaryPath))
        {
            DisplayDialogOrError(kSynthEnvServer, "failed to find " + binaryPath);
#if META_XR_SDK_CORE_SUPPORTS_TELEMETRY
            marker.SetResult(OVRPlugin.Qpl.ResultType.Fail);
#endif
            return;
        }

        ProcessPort existingProcess = GetProcessStatusFromPort(kSynthEnvServerPort);
        if (existingProcess != null)
        {
            bool replace = EditorUtility.DisplayDialog(
                kSynthEnvServer,
                "A synthetic environment server is already running. " +
                "Do you want to terminate it before opening the new scene?",
                "Yes", "No");
            if (!replace)
            {
                return;
            }
            StopServer();
        }

        // launch the binary
        LaunchProcess(binaryPath, environmentName, kLocalSharingServer);
    }

    [MenuItem(kSynthEnvServerMenu + "/Launch Game Room")]
    public static void LaunchGameRoom()
    {
        LaunchEnvironment("GameRoom");
        LaunchLocalSharingServer();
    }

    [MenuItem(kSynthEnvServerMenu + "/Launch Living Room")]
    public static void LaunchLivingRoom()
    {
        LaunchEnvironment("LivingRoom");
        LaunchLocalSharingServer();
    }

    [MenuItem(kSynthEnvServerMenu + "/Launch Bedroom")]
    public static void LaunchBedroom()
    {
        LaunchEnvironment("Bedroom");
        LaunchLocalSharingServer();
    }

    [MenuItem(kSynthEnvServerMenu + "/Stop Server")]
    public static void StopServer()
    {
#if META_XR_SDK_CORE_SUPPORTS_TELEMETRY
        using var marker = new OVRTelemetryMarker(OVRTelemetryConstants.XRSim.MarkerId.SESInteraction);
        marker.AddAnnotation(OVRTelemetryConstants.XRSim.AnnotationType.Action, "stop");
#endif

        StopProcess(kSynthEnvServerPort, kSynthEnvServer);
        StopLocalSharingServer();
    }

    public static string GetSynthEnvServerPath()
    {
        return Path.GetFullPath(PackagePath + "/MetaXRSimulator/.synth_env_server/synth_env_server.exe");
    }

    // LSS functionality
    const string kLocalSharingServer = "Local Sharing Server";
    const string kLocalSharingServerMenu = kXrSimMenu + "/" + kLocalSharingServer;
    const string kLocalSharingServerPort = "33793";

    [MenuItem(kLocalSharingServerMenu + "/Launch Sharing Server")]
    public static void LaunchLocalSharingServer()
    {
        ReportInfo(kLocalSharingServer, "Launching local sharing server");

        string binaryPath = GetLocalSharingServerPath();
        if (!File.Exists(binaryPath))
        {
            DisplayDialogOrError(kLocalSharingServer, "failed to find " + binaryPath);
            return;
        }

        // Always force restart LSS
        ProcessPort existingProcess = GetProcessStatusFromPort(kLocalSharingServerPort);
        if (existingProcess != null)
        {
            StopServer();
        }

        // launch the binary
        LaunchProcess(binaryPath, "", kLocalSharingServer);
    }

    [MenuItem(kLocalSharingServerMenu + "/Stop Sharing Server")]
    public static void StopLocalSharingServer()
    {
        StopProcess(kLocalSharingServerPort, kLocalSharingServer);
    }

    public static string GetLocalSharingServerPath()
    {
        return Path.GetFullPath(PackagePath + "/MetaXRSimulator/.local_sharing_server/local_sharing_server.exe");
    }

    public static ProcessPort GetProcessStatusFromPort(string port)
    {
        var existingPorts = ProcessPort.GetProcessesByPort(port);
        return existingPorts.Count > 0 ? existingPorts[0] : null;
    }

    public static bool IsProcessRunning(string port)
    {
        var existingProcess = GetProcessStatusFromPort(port);
        return existingProcess != null;
    }

    public static void LaunchProcess(string binaryPath, string arguments, string logContext)
    {
        ReportInfo(logContext, "Launching " + binaryPath);
        Process sesProcess = new Process();
        sesProcess.StartInfo.FileName = binaryPath;
        sesProcess.StartInfo.Arguments = arguments;
        if (!sesProcess.Start())
        {
            DisplayDialogOrError(logContext, "failed to launch " + binaryPath);
        }
    }

    public static void StopProcess(string processPort, string logContext)
    {
        ProcessPort existingProcess = GetProcessStatusFromPort(processPort);
        if (existingProcess == null)
        {
            return;
        }
        ReportInfo(logContext, "Stopping " + existingProcess);

        Process p = Process.GetProcessById(existingProcess.processId);
        p.Kill();
        p.WaitForExit();
    }
}
