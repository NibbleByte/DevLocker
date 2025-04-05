using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace DevLocker.Audio.Conductors
{
	public class PlayAudioConductor : AudioPlayerAsset.AudioConductor
	{
		public AudioResource AudioResource;

		public override IEnumerator Play(AudioSourcePlayer player)
		{
			player.AudioResource = AudioResource;
			player.Play();

			yield break;
		}
	}

	public class PlayAudioWithVFXConductor : AudioPlayerAsset.AudioConductor
	{
		public AudioResource AudioResource;
		public GameObject VisualEffectsPrefab;

		public bool ParentToSource;
		public bool RotateAsSource;
		public Vector3 Offset;

		public override IEnumerator Play(AudioSourcePlayer player)
		{
			player.AudioResource = AudioResource;
			player.Play();

			if (VisualEffectsPrefab) {
				Quaternion rotation = RotateAsSource ? player.transform.rotation : Quaternion.identity;

				if (ParentToSource) {
					GameObject.Instantiate(VisualEffectsPrefab, player.transform.position + Offset, rotation);
				} else {
					GameObject.Instantiate(VisualEffectsPrefab, player.transform.position + Offset, rotation, player.transform);
				}
			}

			yield break;
		}
	}

	public class IntroThenLoopConductor : AudioPlayerAsset.AudioConductor
	{
		public AudioClip Intro;
		public AudioResource Looped;
		public float Overlap = 0f;

		public override IEnumerator Play(AudioSourcePlayer player)
		{
			player.AudioSource.PlayOneShot(Intro);

			player.AudioResource = Looped;
			player.Loop = true;

			double introLengthDouble = (double)Intro.samples / (double)Intro.frequency; // This is more accurate than clip.float.
			player.AudioSource.PlayScheduled(AudioSettings.dspTime + introLengthDouble - Overlap);

			yield break;
		}
	}

}
