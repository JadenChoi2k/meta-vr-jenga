/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using JetBrains.Annotations;
using Meta.WitAi.Composer.Data.Info;
using Meta.WitAi.Data.Info;
namespace Meta.WitAi.Data.Configuration.Tabs
{

    [UsedImplicitly]
    public class WitConfigurationComposerTab : WitConfigurationEditorTab
    {
        public override Type DataType => typeof(WitComposerData);
        public override int TabOrder { get; } = 5;
        public override string TabID { get; } = "composer";
        public override string TabLabel { get; } = WitTexts.Texts.ConfigurationComposerTabLabel;
        public override string MissingLabel { get; } = WitTexts.Texts.ConfigurationComposerMissingLabel;
        public override string GetPropertyName(string tabID) => "canvases";


        public override bool ShouldTabShow(WitAppInfo appInfo) => false;

        public override bool ShouldTabShow(WitConfiguration configuration)
        {
            var composerData = configuration.Composer();
            return composerData != null && composerData.canvases?.Length>0;
        }
        public override string GetTabText(bool titleLabel)
        {
            return titleLabel ? WitTexts.Texts.ConfigurationComposerTabLabel : WitTexts.Texts.ConfigurationComposerMissingLabel;
        }
    }
}
