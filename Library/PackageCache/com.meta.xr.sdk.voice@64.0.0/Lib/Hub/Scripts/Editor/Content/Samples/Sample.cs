/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEditor;
using UnityEngine;

namespace Meta.Voice.Hub.Content
{
    public class Sample : ScriptableObject
    {
        [Header("Content")]
        [Tooltip("The human readable name of the sample.")]
        public string title;
        
        [TextArea]
        [Tooltip("A short description of the Sample")]
        public string description;
        [Tooltip("A 2D image representing the sample")]
        public Texture2D tileImage;

        [Tooltip("The scene file of this sample to be opened")]
        [Header("Resource Paths")]
        public SceneAsset sceneReference;
        
        [Tooltip("The name of the package in which the sample resides")]
        public string packageSampleName;
        
        [Tooltip("The grouping ID of the sample page in which this sample should be displayed.")]
        public string sampleSetId;
        
        [Tooltip("Relative ordering priority for display")]
        public float priority;
    }
}
