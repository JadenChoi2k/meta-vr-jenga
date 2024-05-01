/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Meta.WitAi.Composer.Interfaces;

namespace Meta.WitAi.Composer.Integrations
{
    public class WitComposerService : ComposerService
    {
        // Use default handler
        protected override IComposerRequestHandler GetRequestHandler() => new WitComposerRequestHandler();

        // Get required event parameters
        protected override string[] GetRequiredEventParams() => new string[] { WitComposerConstants.ENDPOINT_COMPOSER_MESSAGE_PARAM_TYPE };
    }
}
