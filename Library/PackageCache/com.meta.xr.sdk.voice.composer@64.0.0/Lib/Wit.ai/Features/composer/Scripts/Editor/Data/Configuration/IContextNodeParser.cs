/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using Meta.WitAi.Json;

namespace Meta.Voice.Composer.Data
{
    /// <summary>
    /// A class which can receive a Context Node from Composer, in JSON format, and parse it.
    /// </summary>
    public interface IContextNodeParser
    {
        /// <param name="contextType"></param>
        /// <returns>true if it can parse a context node of the given type. False otherwise</returns>
        public bool HandlesType(string contextType);

        /// <summary>
        /// Processes the given node
        /// </summary>
        /// <param name="module">the Context node to parse</param>
        /// <param name="composerParser">The general composer parser through which results can be saved</param>
        public void ProcessNode(WitResponseNode module, ComposerParser composerParser);
    }
}
