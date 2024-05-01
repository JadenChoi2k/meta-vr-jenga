/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Meta.WitAi.Data.Configuration;

namespace Meta.WitAi.Composer.Data.Info
{
    /// <summary>
    /// Object to load and store action and context map path data for reuse.
    /// </summary>
    [Serializable]
    public class ComposerCanvasData
    {
        private List<string> _actions;
        private List<string> _contextMapPaths;

        public List<string> Actions => _actions;
        public List<string> ContextMapPaths => _contextMapPaths;

        /// <summary>
        /// Inspects the deployed Composer graph and loads in available actions and available
        /// context map entities.
        /// </summary>
        /// <param name="config">the config containing the relevant composer info</param>
        public void LoadCanvasData(WitConfiguration config)
        {
            //NB we only use the deployed ( 'Live' ) canvas as that's all the client can access when in play mode.
            ComposerGraph liveCanvas = config.Composer().canvases.First(can => can.canvasName.Equals("Live"));
            AddActionsFromCanvas(liveCanvas);
            AddContextMapPathsFromCanvas(liveCanvas);
        }

        /// <summary>
        /// Goes through all the context maps
        /// </summary>
        private void AddContextMapPathsFromCanvas(ComposerGraph liveCanvas)
        {
            _contextMapPaths = new List<string>();
            _contextMapPaths.AddRange((from val in liveCanvas.contextMap.server select val.path).ToArray());
            _contextMapPaths.AddRange((from val in liveCanvas.contextMap.shared select val.path).ToArray());
        }

        /// <summary>
        /// Adds all the available actions in the canvas to the ActionSystem
        /// </summary>
        private void AddActionsFromCanvas(ComposerGraph liveCanvas)
        {
            _actions = liveCanvas.actions.Select(a => a.Replace(".", "/")).ToList();
        }
    }
}
