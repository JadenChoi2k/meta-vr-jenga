/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using JetBrains.Annotations;
using Meta.WitAi.Composer.Data.Info;
using Meta.WitAi;
using Meta.WitAi.Attributes;
using Meta.WitAi.Data.Configuration;

namespace Meta.Voice.Composer.Data
{
    /// <summary>
    /// Static class that provides a method for refreshing composer data canvases
    /// </summary>
    [UsedImplicitly]
    public static class WitComposerDataRefresher
    {
        /// <summary>
        /// Refreshes all WitComposerData within a configuration
        /// </summary>
        [WitConfigurationAssetRefresh]
        [UsedImplicitly]
        public static void RefreshComposerData(WitConfiguration configuration, WitComposerData composerData)
        {
            // Ignore without server access token
            if (string.IsNullOrEmpty(configuration.GetServerAccessToken()))
            {
                return;
            }

            // Update
            WitExportRetriever.GetExport(configuration,
                (exportZip, error) =>
                {
                    if (string.IsNullOrEmpty(error))
                    {
                        composerData.canvases = new ComposerParser().ExtractComposerInfo(exportZip);
                    }
                    else
                    {
                        VLog.E($"Could not retrieve Composer data for app {configuration.GetApplicationId()}\n{error}\n");
                    }
                });
        }
    }
}
