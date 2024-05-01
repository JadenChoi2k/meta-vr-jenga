/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

namespace Meta.WitAi.Composer.Data
{
    /// <summary>
    /// This class represents a specific path within the Composer graph.
    /// It can be extended to any specific path ID.
    /// </summary>
    public abstract class ReservedContextPath : BaseReservedContextPath
    {
        protected ReservedContextPath() { }

        /// <summary>
        /// The value of the path field.
        /// </summary>
        private string _value;

        /// <returns>the string representation of the value at this reserved path.</returns>
        public string GetValue()
        {
            return _value;
        }

        /// <summary>
        /// Sets the value of the reserved path
        /// </summary>
        /// <param name="value">The value you want to set, generally transient descriptive state.</param>
        public void Set(string value)
        {
            _value = value;
            UpdateContextMap();
        }

        /// <summary>
        /// Sets the current context map's value for this reserved path.
        /// </summary>
        protected internal override void UpdateContextMap()
        {
            if (Map == null)
            {
                VLog.W($"Missing Composer map for { this }");
            }
            Map?.SetData(ReservedPath, _value);
        }

        /// <summary>
        /// Removes all context of this type
        /// </summary>
        public override void Clear()
        {
            _value = string.Empty;
            base.Clear();
        }

        public override string ToString()
        {
            return $"{ReservedPath} : {_value}";
        }
    }
}
