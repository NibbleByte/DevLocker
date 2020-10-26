using UnityEngine;
using System;

namespace DevLocker.StatesManagement.StatesCommon
{
	/// <summary>
	/// Simply activate/deactivate on specific state.
	/// </summary>
	public abstract class StateActivatorBase<TState> : MonoBehaviour, IStateSubscriber where TState : struct, IComparable
	{

		public TState[] ActiveStates;
		public bool Invert = false;

		public abstract StateManagerBase<TState> StateManager { get; }

		protected bool m_Subscribed { get; private set; }

		public virtual void SubscribeState()
		{
			if (m_Subscribed)
				return;

			m_Subscribed = true;

			StateManager.TransitionStarts += OnTransitionStarts;
			StateManager.StateChanged += OnStateChanged;

			bool nextIsActive = Array.IndexOf(ActiveStates, StateManager.CurrentOrLastEventArgs.NextState) != -1;
			if (nextIsActive) {
				gameObject.SetActive(!Invert);
			} else {
				gameObject.SetActive(Invert);
			}
		}

		public virtual void UnsubscribeState()
		{
			if (!m_Subscribed)
				return;

			m_Subscribed = false;

			if (StateManager != null) {
				StateManager.TransitionStarts -= OnTransitionStarts;
				StateManager.StateChanged -= OnStateChanged;
			}
		}

		protected virtual void OnTransitionStarts(StateEventArgs<TState> e) {
			bool prevIsActive = Array.IndexOf(ActiveStates, e.PrevState) != -1;
			bool nextIsActive = Array.IndexOf(ActiveStates, e.NextState) != -1;

			if (prevIsActive == nextIsActive)
				return;

			if (!Invert && !nextIsActive) {
				gameObject.SetActive(false);
			}
			if (Invert && nextIsActive) {
				gameObject.SetActive(false);
			}
		}

		protected virtual void OnStateChanged(StateEventArgs<TState> e) {
			bool prevIsActive = Array.IndexOf(ActiveStates, e.PrevState) != -1;
			bool nextIsActive = Array.IndexOf(ActiveStates, e.NextState) != -1;

			if (prevIsActive == nextIsActive)
				return;

			if (!Invert && nextIsActive) {
				gameObject.SetActive(true);
			}
			if (Invert && !nextIsActive) {
				gameObject.SetActive(true);
			}
		}
	}

}