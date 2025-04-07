using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace DevLocker.Audio.Conductors
{
	public class PlayAudioConductor : AudioPlayerAsset.AudioConductor
	{
		public AudioResource AudioResource;

		[Range(0f, 1f)]
		public float Volume = 1.0f;

		public override IEnumerator Play(AudioSourcePlayer player)
		{
			player.AudioResource = AudioResource;
			player.Volume = Volume;
			player.Play();

			yield break;
		}
	}

	public class PlayAudioWithVFXConductor : AudioPlayerAsset.AudioConductor
	{
		public AudioResource AudioResource;
		[Range(0f, 1f)]
		public float Volume = 1.0f;

		public GameObject VisualEffectsPrefab;

		public bool ParentToSource;
		public bool RotateAsSource;
		public Vector3 Offset;

		public override IEnumerator Play(AudioSourcePlayer player)
		{
			player.AudioResource = AudioResource;
			player.Volume = Volume;
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

	public class PlayPitchSequenceConductor : AudioPlayerAsset.AudioConductor
	{
		public AudioClip AudioClip;

		[Range(0f, 1f)]
		public float Volume = 1.0f;

		[Tooltip("Should it start all over the pitch sequence on reacing the end, or sohuld it use the last pitch?")]
		public bool ResetOnSequenceEnd = false;

		[Tooltip("Reset pitch index after this many seconds idle. Set to 0 to never reset so you can do it manually.")]
		public float ResetAfterSeconds = 3f;

		[Tooltip("If true, the sequence represents semitone integer numbers that will be converted to the desired pitch float value.")]
		public bool UseSemitones = true;

		// To comply with music theory, the size of pitch difference should match a single semitone.
		// Semitone is the smallest musical step (white-to-black keys on piano).
		// Each octave is 12 semitones. To move a frequency up one octave you multiply by 2. So to move a frequency up one semitone you multiply by 2^(1/12)= 1.059463
		// Read more here: https://www.reddit.com/r/Unity3D/comments/18ycc02/sharing_a_really_basic_but_useful_tip_if_theres_a/
		public const float SemitonePitchSize = 1.059463f;

		public float[] PitchSequence;

		private const string PitchIndex_StorageKey = "PitchIndex_" + nameof(PlayPitchSequenceConductor);
		private const string LastPlayTime_StorageKey = "LastPlayTime_" + nameof(PlayPitchSequenceConductor);

#if UNITY_EDITOR
		public override void OnValidate(AudioPlayerAsset context)
		{
			base.OnValidate(context);

			bool hasChanged = false;

			if (UseSemitones) {

				for(int i = 0; i <  PitchSequence.Length; i++) {
					float pitch = PitchSequence[i];
					float semitone = Mathf.Max(0, Mathf.Round(pitch));
					if (pitch != semitone) {
						PitchSequence[i] = semitone;
						hasChanged= true;
					}
				}

			} else {

				for (int i = 0; i < PitchSequence.Length; i++) {
					if (PitchSequence[i] < 0.0001f) {
						PitchSequence[i] = 0.0001f;
						hasChanged = true;
					}
				}
			}

			if (hasChanged) {
				UnityEditor.EditorUtility.SetDirty(context);
			}
		}
#endif

		public override IEnumerator Play(AudioSourcePlayer player)
		{
			int pitchIndex = player.GetConductorsStorageValue(PitchIndex_StorageKey, 0);
			float lastPlayTime = player.GetConductorsStorageValue(LastPlayTime_StorageKey, -1f);

			if (ResetAfterSeconds > 0 && lastPlayTime + ResetAfterSeconds < Time.time) {
				pitchIndex = 0;
			}

			player.AudioSource.pitch = UseSemitones
				? Mathf.Pow(SemitonePitchSize, PitchSequence[pitchIndex])
				: PitchSequence[pitchIndex]
				;

			player.Volume = Volume;
			player.AudioSource.PlayOneShot(AudioClip);

			pitchIndex = ResetOnSequenceEnd
				? (pitchIndex + 1) % PitchSequence.Length
				: Mathf.Min(pitchIndex + 1, PitchSequence.Length - 1)
				;

			player.SetConductorsStorageValue(PitchIndex_StorageKey, pitchIndex);
			player.SetConductorsStorageValue(LastPlayTime_StorageKey, Time.time);

			yield break;
		}

		public static void ResetPitchIndex(AudioSourcePlayer player)
		{
			if (player == null)
				return;

			player.SetConductorsStorageValue(PitchIndex_StorageKey, 0);
		}
	}

}
