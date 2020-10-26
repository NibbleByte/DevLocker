using System;
using UnityEngine;

namespace DevLocker.StatesManagement.StatesCommon
{
	public abstract class StateUICanvasFadeInBase<TState> : MonoBehaviour, IStateSubscriber, IStateVisualTransition where TState : struct, IComparable
	{
		public TState[] ActiveStates;

		public float Duration = 0.25f;
		public bool Inverse = false;    // Can be used to hide if active state.
		public bool TimeScaled = true;  // Should it be timeScale dependent or not.

		[Tooltip("Skip first transition right. Useful on game start.")]
		public bool SkipFirstFade = true;

		[Tooltip("Optional target canvas. If left empty, target is obtained from current gameobject.")]
		public CanvasGroup TargetCanvas;

		public abstract StateManagerBase<TState> StateManager { get; }

		// Override this to include any custom types to wait for.
		protected Type[] WaitForTransitionTypes = new Type[] { };

		protected bool m_Subscribed { get; private set; }

		private StateEventArgs<TState> m_CurrentTransitionArgs;

		private float m_StartTime;
		private float m_StartAlpha;
		private float m_EndAlpha;

		private float Now {
			get { return (TimeScaled) ? Time.time : Time.unscaledTime; }
		}

		void Awake() {
			SubscribeState();
		}

		void OnDestroy() {
			UnsubscribeState();
		}

		public virtual void SubscribeState()
		{
			if (m_Subscribed)
				return;

			m_Subscribed = true;

			if (TargetCanvas == null) {
				TargetCanvas = GetComponent<CanvasGroup>();
			}

			StateManager.TransitionStarts += OnTransitionStarts;

			bool isActive = Array.IndexOf(ActiveStates, StateManager.CurrentOrLastEventArgs.NextState) != -1;

			if (Inverse) {
				TargetCanvas.gameObject.SetActive(!isActive);
				TargetCanvas.alpha = isActive ? 0f : 1f;
			} else {
				TargetCanvas.gameObject.SetActive(isActive);
				TargetCanvas.alpha = isActive ? 1f : 0f;
			}

			enabled = false;
		}

		public virtual void UnsubscribeState()
		{
			if (!m_Subscribed)
				return;

			m_Subscribed = false;

			if (StateManager != null) {
				StateManager.TransitionStarts -= OnTransitionStarts;
			}
		}

		protected virtual void OnTransitionStarts(StateEventArgs<TState> e) {

			bool prevIsActive = Array.IndexOf(ActiveStates, e.PrevState) != -1;
			bool nextIsActive = Array.IndexOf(ActiveStates, e.NextState) != -1;

			if (SkipFirstFade) {
				SkipFirstFade = false;

				if (Inverse) {
					TargetCanvas.alpha = (nextIsActive) ? 0.0f : 1.0f;
					TargetCanvas.gameObject.SetActive(!nextIsActive);
				} else {
					TargetCanvas.alpha = (nextIsActive) ? 1.0f : 0.0f;
					TargetCanvas.gameObject.SetActive(nextIsActive);
				}
				return;
			}

			if (prevIsActive == nextIsActive)
				return;

			m_CurrentTransitionArgs = e;
			m_CurrentTransitionArgs.AddTransition(this);

			m_StartTime = Now;
			if (Inverse) {
				m_StartAlpha = (nextIsActive) ? 1.0f : 0.0f;
				m_EndAlpha = (nextIsActive) ? 0.0f : 1.0f;
			} else {
				m_StartAlpha = (nextIsActive) ? 0.0f : 1.0f;
				m_EndAlpha = (nextIsActive) ? 1.0f : 0.0f;
			}

			TargetCanvas.alpha = m_StartAlpha;

			TargetCanvas.gameObject.SetActive(true);
			enabled = true;
		}

		protected virtual void Update()
		{
			if (m_CurrentTransitionArgs != null && m_CurrentTransitionArgs.HasTransitions(this, WaitForTransitionTypes)) {
				m_StartTime = Now;
				return;
			}

			float progress = (Now - m_StartTime) / Duration;

			progress = Mathf.Clamp01(progress);

			TargetCanvas.alpha = Mathf.Lerp(m_StartAlpha, m_EndAlpha, progress);

			if (progress >= 1.0f) {
				if (m_EndAlpha == 0.0f) {
					TargetCanvas.gameObject.SetActive(false);
				}
				enabled = false;

				if (m_CurrentTransitionArgs != null) {
					m_CurrentTransitionArgs.RemoveTransition(this);
					m_CurrentTransitionArgs = null;
				}
			}
		}

	}

}