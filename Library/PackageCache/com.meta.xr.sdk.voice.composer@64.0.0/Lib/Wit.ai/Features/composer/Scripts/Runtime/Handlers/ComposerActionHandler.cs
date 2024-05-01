/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Meta.WitAi.Composer.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.WitAi.Composer.Handlers
{
    [Serializable]
    public class ComposerActionEvent : UnityEvent<ComposerSessionData> {}

    [Serializable]
    public struct ComposerActionEventData
    {
        public string actionID;
        public ComposerActionEvent actionEvent;
    }

    public class ComposerActionHandler : MonoBehaviour, IComposerActionHandler
    {
        /// <summary>
        /// Action events are called when the specified action is called by composer
        /// </summary>
#if UNITY_2021_3_2 || UNITY_2021_3_3 || UNITY_2021_3_4 || UNITY_2021_3_5
        [NonReorderable]
#endif
        [SerializeField] private ComposerActionEventData[] _actionEvents;
        private int _highestIndex = 0;
        public ComposerActionEventData[] ActionEvents => _actionEvents;

        protected virtual void Start()
        {
            // assume any empty slots are there for a purpose
            _highestIndex = Math.Max(0,_actionEvents.Length-1);
        }

        public void AddEvent(ComposerActionEventData actionEvent)
        {
            if (_highestIndex >= _actionEvents.Length -1)
            {
                Array.Resize(ref _actionEvents,1+ _actionEvents.Length*2);
            }
            _actionEvents[_highestIndex++] = actionEvent;

        }
        /// <summary>
        /// Action event that will force the composer to wait until the
        /// the coroutine has completed before continuing
        /// </summary>
        public Func<ComposerSessionData, IEnumerator> HandleActionAsync;
        // The currently running coroutines
        private Dictionary<ComposerSessionData, bool> _actionCoroutines = new Dictionary<ComposerSessionData, bool>();

        /// <summary>
        /// Called when the composer requests a specific action id.
        /// Calls all action events, action attributes and action coroutines
        /// </summary>
        /// <param name="sessionData">Composer data including the action id</param>
        public void PerformAction(ComposerSessionData sessionData)
        {
            // Get action id
            string actionID = sessionData.responseData.actionID;

            // Get and perform all action events
            int eventIndex = GetActionEventIndex(actionID);
            if (eventIndex != -1)
            {
                _actionEvents[eventIndex].actionEvent?.Invoke(sessionData);
            }

            // TODO: Invoke all action attribute methods via conduit

            // Perform all coroutines
            if (HandleActionAsync != null)
            {
                StartCoroutine(PerformActionAsync(sessionData, HandleActionAsync));
            }
        }

        // Performs action coroutines in order and then removes from placeholder
        private IEnumerator PerformActionAsync(ComposerSessionData sessionData, Func<ComposerSessionData, IEnumerator> actionAsync)
        {
            // Began
            _actionCoroutines[sessionData] = true;
            // Perform all actions
            foreach (Func<ComposerSessionData, IEnumerator> actionDelegate in actionAsync.GetInvocationList())
            {
                yield return actionDelegate(sessionData);
            }
            // Completed
            if (_actionCoroutines.ContainsKey(sessionData))
            {
                _actionCoroutines.Remove(sessionData);
            }
        }

        // Return true while action coroutine is performing
        public bool IsPerformingAction(ComposerSessionData sessionData)
        {
            return _actionCoroutines != null && _actionCoroutines.ContainsKey(sessionData);
        }

        /// <summary>
        /// Searches the action events for a specific action event
        /// </summary>
        /// <param name="actionID">Specified action id</param>
        /// <returns>Returns the specified event index</returns>
        public int GetActionEventIndex(string actionID)
        {
            if (_actionEvents != null)
            {
                return Array.FindIndex(_actionEvents,
                    (a) => string.Equals(a.actionID, actionID, StringComparison.CurrentCultureIgnoreCase));
            }
            return -1;
        }
    }
}
