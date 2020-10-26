using System;
using UnityEngine;

namespace DevLocker.StatesManagement.StatesCommon
{
	public abstract class StateUICanvasCurtainFaderBase<TState> : MonoBehaviour, IStateSubscriber, IStateLoadingCurtainTransition where TState : struct, IComparable
	{
		public float Duration = 0.25f;
		public bool Inverse = false;    // Can be used for black outs.
		public bool TimeScaled = true;  // Should it be timeScale dependent or not.

		public bool WaitForOtherAnimations;

		[Tooltip("Skip first transition and wait for fade out directly. Useful on game start.")]
		public bool SkipFirstFadeIn = false;

		[Tooltip("Optional target canvas. If left empty, target is obtained from current gameobject.")]
		public CanvasGroup TargetCanvas;

		public TState[] SkipWhenExitingFrom;
		public TState[] SkipWhenEnteringTo;

		public abstract StateManagerBase<TState> StateManager { get; }

		// IStateVisualTransition - Wait for any visual transitions like animations, tweens, etc.
		// Override this to include any custom types to wait for.
		protected Type[] WaitForTransitionTypes = new Type[] { typeof(IStateVisualTransition) };

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
			StateManager.StateChanged += OnStateChanged;

			// SkipWhenExitingFrom is stronger than SkipFirstFadeIn.
			if (SkipFirstFadeIn && Array.IndexOf(SkipWhenExitingFrom, StateManager.CurrentState) != -1) {
				SkipFirstFadeIn = false;
			}

			if (Inverse) {
				TargetCanvas.gameObject.SetActive(!SkipFirstFadeIn);
				TargetCanvas.alpha = SkipFirstFadeIn ? 0f : 1f;
			} else {
				TargetCanvas.gameObject.SetActive(SkipFirstFadeIn);
				TargetCanvas.alpha = SkipFirstFadeIn ? 1f : 0f;
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
				StateManager.StateChanged -= OnStateChanged;
			}
		}

		protected virtual void OnTransitionStarts(StateEventArgs<TState> e) {

			if (SkipFirstFadeIn) {
				SkipFirstFadeIn = false;
				return;
			}

			if (Array.IndexOf(SkipWhenExitingFrom, e.PrevState) != -1)
				return;
			if (Array.IndexOf(SkipWhenEnteringTo, e.NextState) != -1)
				return;

			m_CurrentTransitionArgs = e;
			m_CurrentTransitionArgs.AddTransition(this);

			m_StartTime = Now;
			m_StartAlpha = (Inverse) ? 1.0f : 0.0f;
			m_EndAlpha = (Inverse) ? 0.0f : 1.0f;

			TargetCanvas.alpha = m_StartAlpha;

			TargetCanvas.gameObject.SetActive(true);
			enabled = true;
		}

		protected virtual void OnStateChanged(StateEventArgs<TState> e) {

			if (Array.IndexOf(SkipWhenExitingFrom, e.PrevState) != -1)
				return;
			if (Array.IndexOf(SkipWhenEnteringTo, e.NextState) != -1)
				return;

			m_StartTime = Now;
			m_StartAlpha = (Inverse) ? 0.0f : 1.0f;
			m_EndAlpha = (Inverse) ? 1.0f : 0.0f;

			TargetCanvas.alpha = m_StartAlpha;

			TargetCanvas.gameObject.SetActive(true);
			enabled = true;
		}

		protected virtual void Update()
		{
			if (WaitForOtherAnimations && m_CurrentTransitionArgs != null && m_CurrentTransitionArgs.HasTransitions(this, WaitForTransitionTypes)) {
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