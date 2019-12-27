using UnityEngine.Playables;
using UnityEngine;

namespace DevLocker.Animations
{
	/// <summary>
	/// Play simple animations without much hassle.
	/// This is not very useful for more complex situations where proper preview and details are needed.
	/// </summary>
	public class PlayAnimation : MonoBehaviour
	{
		public AnimationClip Clip;

		private PlayableGraph _playableGraph;

		[ContextMenu("Play")]
		public void Play()
		{
			if (!Application.isPlaying)
				return;

			var playable = _playableGraph.GetRootPlayable(0);
			playable.Play();
			playable.SetTime(0);
		}

		[ContextMenu("Pause")]
		public void Pause()
		{
			if (!Application.isPlaying)
				return;

			_playableGraph.GetRootPlayable(0).Pause();
		}

		[ContextMenu("Resume")]
		public void Resume()
		{
			if (!Application.isPlaying)
				return;

			_playableGraph.GetRootPlayable(0).Play();
		}

		void OnEnable()
		{
			var animator = GetComponent<Animator>();
			if (animator == null) {
				animator = gameObject.AddComponent<Animator>();
			}

			AnimationPlayableUtilities.PlayClip(animator, Clip, out _playableGraph);
		}

		void OnDisable()
		{
			_playableGraph.Destroy();
		}
	}

}
