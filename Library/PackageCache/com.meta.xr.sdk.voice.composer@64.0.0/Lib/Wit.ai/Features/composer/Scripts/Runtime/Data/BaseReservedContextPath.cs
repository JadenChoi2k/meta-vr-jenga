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
    /// This class represents a specific path within the Composer graph which
    /// has specific meaning -- it is reserved for a specific use.
    /// This class is intended to be extended so as to represent specific path IDs.
    ///
    /// Note also that it is a IContextMapReservedPathExtension, so it will be
    /// automatically registered with the composer's context map.
    /// </summary>
    public abstract class BaseReservedContextPath : IContextMapReservedPathExtension
    {
        protected ComposerContextMap Map => _composer.CurrentContextMap;
        protected abstract string ReservedPath { get; }

        private ComposerService _composer;

        /// <summary>
        /// Should only be true if this extension has been successfully integrated
        /// with the composer service.
        /// </summary>
        public bool IsInitialized;

        protected BaseReservedContextPath() { }

        /// <summary>
        /// Ensures that the data in this object is present to the Composer
        /// Context Map.
        /// </summary>
        protected internal abstract void UpdateContextMap();


        /// <summary>
        /// Does whatever setup is required for this plugin, in case it was instantiated
        /// without appropriate links.
        /// </summary>
        /// <param name="composer">the composer object to to which this collection
        /// is related.</param>
        public virtual void Initialize(ComposerService composer)
        {
            _composer = composer;
            ComposerContextMap.ReservedPaths.Add(ReservedPath);

            IsInitialized = true;
        }

        /// <summary>
        /// Removes all context of this type
        /// </summary>
        public virtual void Clear()
        {
            Map?.ClearData(ReservedPath);
        }
    }
}
