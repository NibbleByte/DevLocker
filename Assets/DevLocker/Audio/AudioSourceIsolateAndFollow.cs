using System.Linq;
using UnityEngine;

namespace DevLocker.Audio
{
	/// <summary>
	/// If game object with <see cref="AudioSource"/> gets destroyed, the sound emitting sound will stop immediately.
	/// This may not be the desired result - for example shot sound should keep playing even when the gun is destroyed.
	///
	/// This component allows you to "isolate" and "follow" the object emitting the sound and fixes the sound interruption issue.
	/// "Isolate" means it will detach the current object from the hierarchy (attach all children to my parent and set my parent to null).
	/// "Follow" means it will move with the original parent. If the parent gets disabled or destroyed, isolated object gets destroyed too once the sound stops playing.
	/// This way references to the players remain and currently playing sounds won't get interrupted.
	/// </summary>
	[RequireComponent(typeof(AudioSourcePlayer))]
	public class AudioSourceIsolateAndFollow : MonoBehaviour
	{
		[Tooltip("If original object gets disabled, should any playing sound be \"interrupted\", so it can quickly fade out gracefully?")]
		public bool InterruptOnDisable;
		[Tooltip("If original object gets destroyed, should any playing sound be \"interrupted\", so it can quickly fade out gracefully?")]
		public bool InterruptOnDestroy;

		public AudioSourceIsolateAndFollowLink Link => m_Link;
		private AudioSourceIsolateAndFollowLink m_Link;	// Not property as it is accessed by the editor
		private bool m_LinkActive;
		private bool m_LinkDestroyInterrupted;

		private AudioSourcePlayer[] m_Players;

		private static Transform m_IsolateRoot;

		public class AudioSourceIsolateAndFollowLink : MonoBehaviour
		{
			public AudioSourceIsolateAndFollow Owner;
		}

		void Start()
		{
			m_Players = GetComponents<AudioSourcePlayer>();

			if (transform.parent == null) {
				Debug.LogError($"\"{name}\" has no parent, which is required by the {nameof(AudioSourceIsolateAndFollow)}", this);
				enabled = false;
				return;
			}


			var linkTransform = new GameObject($"{name} (Link)").transform;
			linkTransform.SetParent(transform.parent);
			linkTransform.localPosition = transform.localPosition;
			linkTransform.SetSiblingIndex(transform.GetSiblingIndex());
			m_Link = linkTransform.gameObject.AddComponent<AudioSourceIsolateAndFollowLink>();
			m_Link.Owner = this;
			m_LinkActive = m_Link.gameObject.activeInHierarchy;

			while (transform.childCount > 0) {
				transform.GetChild(0).transform.SetParent(linkTransform);
			}

			if (m_IsolateRoot == null) {
				m_IsolateRoot = new GameObject("_IsolatedAudioPlayers").transform;
				m_IsolateRoot.SetSiblingIndex(0);
			}


			transform.SetParent(m_IsolateRoot);
		}

		private void Update()
		{
			if (m_Link == null) {

				if (InterruptOnDestroy && !m_LinkDestroyInterrupted) {
					m_LinkDestroyInterrupted = true;

					foreach (var player in m_Players) {
						if (player.IsPlaying) {
							player.Stop();
						}
					}
				}

				if (!m_Players.Any(p => p.IsPlaying)) {
					Destroy(gameObject);
				}
				return;
			}

			bool prevParentActive = m_LinkActive;

			transform.position = m_Link.transform.position;
			m_LinkActive = m_Link.isActiveAndEnabled;

			if (prevParentActive != m_LinkActive) {
				if (m_LinkActive) {
					foreach (var player in m_Players) {
						if (player.PlayOnEnable && !player.IsPlaying) {
							player.Play();
						}
					}
				} else if (InterruptOnDisable) {
					foreach (var player in m_Players) {
						if (player.IsPlaying) {
							player.Stop();
						}
					}
				}
			}
		}
	}
}
