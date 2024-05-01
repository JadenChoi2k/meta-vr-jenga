/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Meta.Voice.Hub.Attributes;
using Meta.Voice.Hub.Interfaces;
using UnityEngine;

namespace Meta.Voice.Hub.Content
{
    [MetaHubPageScriptableObject]
    public class SamplesPage : ScriptableObject, IPageInfo
    {
        [Tooltip("The name to be shown to the user in the samples display")]
        [SerializeField] private string _displayName;
        
        [Tooltip("The hierarchy location in the left-hand menu bar where this page will appear. If left empty, it will be a top-level item. Note that the prefix must end in a forward slash ( / )")]
        [SerializeField] private string _prefix;
        
        [Tooltip("A reference to the main tab in which this page will be shown")]
        [SerializeField] private MetaHubContext _context;
        
        [Tooltip("The ordering priority of this page")]
        [SerializeField] private int _priority = 0;
        
        [Tooltip("The unique identifier for this page. It is used by individual samples to denote in which page they'll be shown.")]
        [SerializeField] private string _sampleSetId;

        public string SampleSetId => _sampleSetId;
        public string Name => _displayName ?? name;
        public string Context => _context.Name;
        public int Priority => _priority;
        public string Prefix => _prefix;
    }
}
