/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Meta.WitAi.Composer.Data.Info;
using UnityEditor;

namespace Meta.WitAi.Windows
{
    [CustomPropertyDrawer(typeof(WitComposerData))]
    public class WitComposerInfoPropertyDrawer : WitPropertyDrawer
    {
        protected override bool FoldoutEnabled => false;    // Show only the name
    }
}
