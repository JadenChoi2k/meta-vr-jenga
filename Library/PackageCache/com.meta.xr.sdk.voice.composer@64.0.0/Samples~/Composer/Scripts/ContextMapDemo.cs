/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEngine;
using UnityEngine.UI;

namespace Meta.WitAi.Composer.Samples
{
    public class ContextMapDemo : MonoBehaviour
    {
        private Text _contextMapText;
        public ComposerService composer;
        public string contextData;

        void Awake()
        {
            _contextMapText = GetComponent<Text>();
            UpdateContextMapGui();
        }

        public void HandleResponse(ComposerSessionData sessionData)
        {
            // Check context map
            if (sessionData.contextMap == null || sessionData.contextMap.Data == null)
            {
                VLog.E("No Context Map");
                return;
            }

            UpdateContextMapGui();

            sessionData.contextMap.SetData(
                "other_color", $"black");
        }

        private void UpdateContextMapGui()
        {
            _contextMapText.text = "Context map = " + composer.CurrentContextMap.GetJson();
        }

        public void SetContextMap()
        {
            composer.CurrentContextMap.SetData("updated_info",
                string.IsNullOrEmpty(contextData) ? "a button has been pushed" : contextData);
            UpdateContextMapGui();
        }

        public void ClearContextMap()
        {
            composer.CurrentContextMap.ClearAllNonReservedData();
            UpdateContextMapGui();
        }

        public void SendContextMap()
        {
            composer.SendContextMapEvent();
        }

        public void OnContextMapChange()
        {
            Debug.Log("Context map has changed to:" + composer.CurrentContextMap.GetJson());
        }

    }
}
