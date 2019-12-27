using UnityEngine;
using UnityEngine.Events;

namespace DevLocker.Animations.AnimatorExpositor
{
	/// <summary>
	/// Monitors (on LateUpdate) if the Animator has entered or exited the specified state.
	/// Exposes UnityEvents to trigger requested action.
	/// </summary>
	public class AnimStateMonitor : MonoBehaviour, IAnimatorProvider
	{
		public Animator Animator;
		public AnimatorState AnimState;

		public UnityEvent OnStarted;
		public UnityEvent OnFinished;

		private Animator _animator;

		private int _lastFrameState;

		void Awake()
		{
			_animator = GetAnimator();

			if (!_animator) {
				Debug.LogError("Cannot read animator state. Animator component is missing or disabled.", this);
				return;
			}

			if (_animator.runtimeAnimatorController == null) {
				Debug.LogError("No controller assigned to the Animator.", this);
				return;
			}
		}

		void LateUpdate()
		{
			var currentStateHash = _animator.GetCurrentAnimatorStateInfo(AnimState.Layer).shortNameHash;

			if (currentStateHash == AnimState.State && currentStateHash != _lastFrameState) {
				OnStarted.Invoke();
			} else if (currentStateHash != AnimState.State && AnimState.State == _lastFrameState) {
				OnFinished.Invoke();
			}

			_lastFrameState = currentStateHash;
		}

		public Animator GetAnimator()
		{
			return Animator ? Animator : GetComponent<Animator>();
		}
	}

}
