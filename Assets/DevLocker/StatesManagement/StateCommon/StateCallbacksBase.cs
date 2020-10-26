using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using System.Collections;

namespace DevLocker.StatesManagement.StatesCommon
{
	/// <summary>
	/// Calls UnityEvents when active state is on.
	/// </summary>
	public abstract class StateCallbacksBase<TState> : MonoBehaviour, IStateSubscriber, IStateVisualTransition where TState : struct, IComparable
	{
		public TState[] ActiveStates;
		[Tooltip("Should a callback be called when transitioning from one active state (from the list above) to another active one.")]
		public bool AlwaysTriggerCallback;
		[Tooltip("(Optional) How much time the transition should last (if needed).")]
		public float TransitionDuration;
		public bool Invert = false;

		public UnityEvent OnActivateStateStartTransition;
		public UnityEvent OnDeactivateStateStartTransition;

		public UnityEvent OnActivateStateChanged;
		public UnityEvent OnDeactivateStateChanged;

		public abstract StateManagerBase<TState> StateManager { get; }

		protected bool m_Subscribed { get; private set; }

		public virtual void SubscribeState()
		{
			if (m_Subscribed)
				return;

			m_Subscribed = true;

			StateManager.TransitionStarts += OnStateTransitionStarts;
			StateManager.StateChanged += OnStateChanged;

			OnStateChanged(StateManager.CurrentOrLastEventArgs);
		}

		public virtual void UnsubscribeState()
		{
			if (!m_Subscribed)
				return;

			m_Subscribed = false;

			if (StateManager != null) {
				StateManager.TransitionStarts -= OnStateTransitionStarts;
				StateManager.StateChanged -= OnStateChanged;
			}
		}

		protected virtual void OnStateTransitionStarts(StateEventArgs<TState> e) {
			bool prevIsActive = Array.IndexOf(ActiveStates, e.PrevState) != -1;
			bool nextIsActive = Array.IndexOf(ActiveStates, e.NextState) != -1;

			if (prevIsActive == nextIsActive && !AlwaysTriggerCallback)
				return;

			if (Invert) {
				if (nextIsActive) {
					OnDeactivateStateStartTransition.Invoke();
				}
				if (prevIsActive) {
					OnActivateStateStartTransition.Invoke();
				}

			} else {
				if (nextIsActive) {
					OnActivateStateStartTransition.Invoke();
				}
				if (prevIsActive) {
					OnDeactivateStateStartTransition.Invoke();
				}
			}

			if (TransitionDuration > 0f) {
				StartCoroutine(StartTransitionDelay(e));
			}
		}

		protected virtual void OnStateChanged(StateEventArgs<TState> e) {
			bool prevIsActive = Array.IndexOf(ActiveStates, e.PrevState) != -1;
			bool nextIsActive = Array.IndexOf(ActiveStates, e.NextState) != -1;

			if (prevIsActive == nextIsActive && !AlwaysTriggerCallback)
				return;

			if (Invert) {
				if (nextIsActive) {
					OnDeactivateStateChanged.Invoke();
				}
				if (prevIsActive) {
					OnActivateStateChanged.Invoke();
				}

			} else {
				if (nextIsActive) {
					OnActivateStateChanged.Invoke();
				}
				if (prevIsActive) {
					OnDeactivateStateChanged.Invoke();
				}
			}
		}

		private IEnumerator StartTransitionDelay(StateEventArgs<TState> args)
		{
			using (args.GetTransitionScope(this)) {
				yield return new WaitForSeconds(TransitionDuration);
			}
		}
	}

}