/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

namespace Meta.WitAi.Composer.Interfaces
{
    public interface IComposerActionHandler
    {
        /// <summary>
        /// Performs the specified action id
        /// </summary>
        /// <param name="sessionData">Specified composer, context data and response data</param>
        void PerformAction(ComposerSessionData sessionData);

        /// <summary>
        /// Whether the specific session data action is still occuring
        /// </summary>
        /// <param name="sessionData">Specified composer, context data and response data</param>
        bool IsPerformingAction(ComposerSessionData sessionData);
    }
}
