/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

namespace Meta.WitAi.Composer.Integrations
{
    public enum WitComposerMessageType
    {
        Message
    }

    public static class WitComposerConstants
    {
        /// <summary>
        /// Path is reserved for shallow path based flows in Composer
        /// </summary>
        public static string CONTEXT_MAP_RESERVED_PATH = "path";

        // Composer Endpoints & parameters
        public const string ENDPOINT_COMPOSER_SPEECH = "converse";
        public const string ENDPOINT_COMPOSER_PARAM_SESSION = "session_id";
        public const string ENDPOINT_COMPOSER_PARAM_CONTEXT_MAP = "context_map";
        public const string ENDPOINT_COMPOSER_PARAM_DEBUG = "debug";
        public const string ENDPOINT_COMPOSER_MESSAGE = "event";
        public const string ENDPOINT_COMPOSER_MESSAGE_PARAM_MESSAGE = "message";
        public const string ENDPOINT_COMPOSER_MESSAGE_PARAM_TYPE = "type";
        public const string ENDPOINT_COMPOSER_MESSAGE_TAG = "tag";

        // Request options used for platform integration
        public const string PI_COMPOSER_ENABLE = "useComposer";
        public const string PI_COMPOSER_ENABLE_ON = "True";

        // Response parsing
        public const string RESPONSE_NODE_EXPECTS_INPUT = "expects_input";
        public const string RESPONSE_NODE_ACTION = "action";
        public const string RESPONSE_NODE_TEXT = "text";
        public const string RESPONSE_NODE_SPEECH = "speech";
        public const string RESPONSE_NODE_Q = "q";
    }
}
