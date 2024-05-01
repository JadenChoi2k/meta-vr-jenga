/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using JetBrains.Annotations;
using Meta.WitAi.Composer.Integrations;
using UnityEngine.Scripting;

namespace Meta.WitAi.Composer.Data
{
    /// <summary>
    /// The NamedPath path is the reserved path, awkwardly named 'path'.
    /// This is used within composer to choose which "named path" to use,
    /// notably when an intent isn't found.
    /// </summary>
    [UsedImplicitly]
    public class NamedPath : ReservedContextPath
    {
        protected override string ReservedPath => WitComposerConstants.CONTEXT_MAP_RESERVED_PATH;
        [Preserve]
        public NamedPath() { }

        public override string ToString()
        {
            return GetValue();
        }
    }
}
