using System;
using UnityEngine;
using UnityEngine.Events;

namespace DevLocker.Animations
{
	/// <summary>
	/// Use this to control how animation is played in the Animator.
	/// Very useful for transitions that need to play back and forth (instead of doing two separate animations).
	/// API provides easy control from UGUI events.
	///
	/// This works by setting Animator state parameter that is fed to the "Motion Time" state parameter which controls the animation progress.
	/// NOTE: if Animator is not in a state, that the animation is controlled by this component via the parameter, effects won't be visible.
	///		  Best use this in an Animator with a single state.
	/// </summary>
	public class PlayAnimatorMotionTime : MonoBehaviour
	{
		public enum LoopBehaviourType
		{
			PauseOnFinish,
			Repeat,
			Clamp,			// Keep playing forever...
			PingPong,		// Play back and forth forever
		}

		public Animator Animator;

		[Tooltip("Animator float parameter name that is fed to the playing animator state as \"Motion Time\".\nWill be used to control how the animation is played.")]
		public string MotionParamName;

		[Tooltip("What should happen when animation progress reaches the end?")]
		public LoopBehaviourType LoopBehaviour = LoopBehaviourType.Clamp;

		[Tooltip("Will the animation advance in the desired direction with every Update().")]
		public bool Play = true;

		[Tooltip("Direction to advance the animation when playing.")]
		public bool Forward = true;

		[Tooltip("Use Time.unscaledDeltaTime instead of Time.deltaTime (for UI animations during paused game)")]
		public bool UnscaledTime;

		[Tooltip("Speed to advance the animation with.")]
		public float Speed = 1;

		[Range(0f, 1f)]
		[Tooltip("Progress of the animation that is set to the Animator float parameter.")]
		public float Progress;

		[Tooltip("Called whenever animation finishes when used with PauseOnFinish")]
		public UnityEvent Finished;

		private string m_MotionParamNameUsed;
		private int m_MotionParamHashUsed;

		void Reset()
		{
			Animator = GetComponent<Animator>();
		}

		void OnEnable()
		{
			// Ensure parameter is set from the start, even when not playing initially.
			if (!string.IsNullOrWhiteSpace(MotionParamName) && Animator) {
				Animator.SetFloat(MotionParamName, Progress);
			}
		}

		void OnValidate()
		{
			if (Application.isPlaying && !Play) {
				// Changing runtime? Most likely changing the Progress - apply it to the Animator.
				OnEnable();
			}
		}

		[ContextMenu("Play Forward")]
		public void PlayForward()
		{
			if (!Application.isPlaying)
				return;

			Forward = true;
			ResetAndPlay();
		}

		[ContextMenu("Play Backward")]
		public void PlayBackward()
		{
			if (!Application.isPlaying)
				return;


			Forward = false;
			ResetAndPlay();
		}

		[ContextMenu("Pause")]
		public void Pause()
		{
			if (!Application.isPlaying)
				return;

			Play = false;
		}

		[ContextMenu("Resume")]
		public void Resume()
		{
			if (!Application.isPlaying)
				return;

			Play = true;
		}

		[ContextMenu("Resume Forward")]
		public void ResumeForward()
		{
			if (!Application.isPlaying)
				return;

			Play = true;
			Forward = true;
		}

		[ContextMenu("Resume Backward")]
		public void ResumeBackward()
		{
			if (!Application.isPlaying)
				return;

			Play = true;
			Forward = false;
		}

		[ContextMenu("Reset And Pause")]
		public void ResetAndPause()
		{
			if (!Application.isPlaying)
				return;

			Progress = Forward ? 0f : 1f;

			Update();

			// Make sure is after Update() to apply the parameter first.
			Play = false;
		}

		[ContextMenu("Reset And Play")]
		public void ResetAndPlay()
		{
			if (!Application.isPlaying)
				return;

			Play = true;
			Progress = Forward ? 0f : 1f;

			Update();
		}

		[ContextMenu("Finish Playback")]
		public void FinishPlayback()
		{
			if (!Application.isPlaying)
				return;

			Play = true;
			Progress = Forward ? 1f : 0f;

			Update();
		}

		public void TogglePlay(bool play)
		{
			Play = play;
		}

		public void TogglePlayReversed(bool pause)
		{
			Play = !pause;
		}

		public void ToggleDirection(bool forward)
		{
			Forward = forward;
		}

		public void ToggleDirectionReversed(bool backward)
		{
			Forward = !backward;
		}

		void Update()
		{
			if (Animator == null)
				return;

			if (!ReferenceEquals(MotionParamName, m_MotionParamNameUsed)) {
				if (string.IsNullOrWhiteSpace(MotionParamName)) {
					MotionParamName = m_MotionParamNameUsed = null;
				} else {
					m_MotionParamNameUsed = MotionParamName;
					m_MotionParamHashUsed = Animator.StringToHash(m_MotionParamNameUsed);
				}
			}

			if (Play && MotionParamName != null) {
				float deltaTime = UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
				float nextProgress = Progress + (Forward ? 1 : -1) * deltaTime * Speed;

				if (nextProgress > 1f || nextProgress < 0f) {
					switch (LoopBehaviour) {

						case LoopBehaviourType.PauseOnFinish:
							Progress = Mathf.Clamp01(nextProgress);
							Play = false;
							Finished.Invoke();
							break;

						case LoopBehaviourType.Repeat:
							Progress = Forward ? 0 : 1;
							break;

						case LoopBehaviourType.Clamp:
							Progress = Mathf.Clamp01(nextProgress);
							break;

						case LoopBehaviourType.PingPong:
							float diff = Mathf.Abs(nextProgress - Progress);
							Progress = Forward ? 1 - diff : 0 + diff;
							Forward = !Forward;
							break;
						default:
							throw new NotImplementedException(LoopBehaviour.ToString());
					}

				} else {
					Progress = nextProgress;
				}

				Animator.SetFloat(m_MotionParamHashUsed, Progress);
			}
		}
	}

}
