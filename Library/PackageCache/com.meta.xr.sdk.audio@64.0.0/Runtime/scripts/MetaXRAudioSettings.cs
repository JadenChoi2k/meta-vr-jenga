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
using System.Collections;

/// \brief Class used to manage the saving and loading of global plug-in settings.
#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
public sealed class MetaXRAudioSettings : ScriptableObject
{
    /// \brief Maximum number of mono or ambisonic sources available for use. [1,+Inf)
    ///
    /// If the application creates more objects than are available, the oldest voice (i.e. the one that has been
    /// playing the longest) is silenced and the channel is reused for the next voice.
    ///
    /// This values is used in MetaXRAudioSource#OnBeforeSceneLoadRuntimeMethod via the MetaXRAudioSettings#Instance
    /// field. As such, in order that the value you set persists across you should only ever set this value on that
    /// static instance, or the object serialized at Assets/Resources/MetaXRAudioSettings.asset.
    [SerializeField]
    public int voiceLimit = 64;

    private static MetaXRAudioSettings instance;
    /// \brief The global plug-in settings as loaded from disk.
    ///
    /// This value is null until first access. On first access, it is read from Assets/Resources/MetaXRAudioSettings.asset.
    ///
    /// If that file doesn't exist, it will be created and populated with defaults.
    ///
    /// You can create the asset in the content browser manually and as long as it is named correctly and at the correct path, the settings in that asset are the ones used/applied.
    public static MetaXRAudioSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<MetaXRAudioSettings>("MetaXRAudioSettings");

                // This can happen if the developer never input their App Id into the Unity Editor
                // and therefore never created the OculusPlatformSettings.asset file
                // Use a dummy object with defaults for the getters so we don't have a null pointer exception
                if (instance == null)
                {
                    instance = ScriptableObject.CreateInstance<MetaXRAudioSettings>();

#if UNITY_EDITOR
                    // Only in the editor should we save it to disk
                    string properPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "Resources");
                    if (!System.IO.Directory.Exists(properPath))
                    {
                        UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                    }

                    string fullPath = System.IO.Path.Combine(
                        System.IO.Path.Combine("Assets", "Resources"),
                        "MetaXRAudioSettings.asset");
                    UnityEditor.AssetDatabase.CreateAsset(instance, fullPath);
#endif
                }
            }

            return instance;
        }
    }
}
