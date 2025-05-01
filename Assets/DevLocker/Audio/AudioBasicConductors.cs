using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace DevLocker.Audio.Conductors
{
	public class PlayAudioConductor : AudioPlayerAsset.AudioConductor
	{
		public const string StopPlayingSoundTooltip = "Should it stop (interrupt) the currently playing sound?\n\nWhen disabled will use AudioSource PlayOneShot() instead of normal Play().";

		[Tooltip(StopPlayingSoundTooltip)]
		public bool StopPlayingSound = false;

		public AudioPlayerAsset.ClipWithVolume AudioClip;

		public override IEnumerator Play(AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			player.PlayDirectClip(AudioClip, playAsOneShot: !StopPlayingSound);

			yield break;
		}
	}

	public class PlayCollectionAudioConductor : AudioPlayerAsset.AudioConductor
	{
		public enum PlaybackMode
		{
			Sequential,
			Shuffle,
			Random,
		}

		private const string SequentialIndex_StorageKey = "SequentialIndex_" + nameof(PlayCollectionAudioConductor);
		private const string ShuffleIndices_StorageKey = "ShuffleList_" + nameof(PlayCollectionAudioConductor);
		private const string RandomLastIndices_StorageKey = "RandomLastIndices" + nameof(PlayCollectionAudioConductor);

		[Tooltip(PlayAudioConductor.StopPlayingSoundTooltip)]
		public bool StopPlayingSound = false;

		[Tooltip("How clips from the collection will be selected on play.\n" +
			" > Sequential - select in sequential order from the list\n" +
			" > Shuffle - shuffles the list before playback. Each clip is played once before the list is reshuffled and repeated.\n" +
			" > Random - select randomly each time from the list (can duplicate)."
			)]
		public PlaybackMode Mode = PlaybackMode.Random;

		[Tooltip("Avoid repeating the last n clips. Used with Random mode. Limited to the number of clips in the collection.")]
		public int AvoidRepeatingLast = 0;

		public AudioPlayerAsset.ClipWithVolume[] AudioClips;

#if UNITY_EDITOR
		public override void OnValidate(AudioPlayerAsset context)
		{
			base.OnValidate(context);

			if (AvoidRepeatingLast < 0) {
				AvoidRepeatingLast = 0;
				UnityEditor.EditorUtility.SetDirty(context);
			}

			if (AvoidRepeatingLast > AudioClips.Length - 1) {
				AvoidRepeatingLast = Mathf.Max(AudioClips.Length - 1, 0);
				UnityEditor.EditorUtility.SetDirty(context);
			}
		}
#endif

		public override IEnumerator Play(AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			if (AudioClips.Length == 0)
				yield break;

			switch (Mode) {
				case PlaybackMode.Sequential:
					int sequentialIndex = asset.GetConductorsStorageValue(SequentialIndex_StorageKey, player, 0);

					player.PlayDirectClip(AudioClips[sequentialIndex % AudioClips.Length /* Clamp just in case */], playAsOneShot: !StopPlayingSound);

					sequentialIndex = (sequentialIndex + 1) % AudioClips.Length;

					asset.SetConductorsStorageValue(SequentialIndex_StorageKey, player, sequentialIndex);

					break;

				case PlaybackMode.Shuffle:
					var shuffleIndices = asset.GetConductorsStorageValue<List<int>>(ShuffleIndices_StorageKey, player, null);
					if (shuffleIndices == null || shuffleIndices.Count == 0) {
						shuffleIndices = Enumerable.Range(0, AudioClips.Length).ToList();
						Shuffle(shuffleIndices);
					}

					player.PlayDirectClip(AudioClips[shuffleIndices.LastOrDefault()], playAsOneShot: !StopPlayingSound);

					shuffleIndices.RemoveAt(shuffleIndices.Count - 1);

					asset.SetConductorsStorageValue(ShuffleIndices_StorageKey, player, shuffleIndices);

					break;

				case PlaybackMode.Random:
					var randomLastIndices = asset.GetConductorsStorageValue<Queue<int>>(RandomLastIndices_StorageKey, player, null);
					if (randomLastIndices == null) {
						randomLastIndices = new Queue<int>();
					}

					var clipIndexPairs = AudioClips
						.Select((c, i) => KeyValuePair.Create(c, i))
						.Where(pair => !randomLastIndices.Contains(pair.Value))
						.ToList();

					if (clipIndexPairs.Count == 0) {
						var lastIndex = randomLastIndices.Dequeue();
						clipIndexPairs.Add(KeyValuePair.Create(AudioClips[lastIndex], lastIndex));
					}

					var clipIndexPair = clipIndexPairs[UnityEngine.Random.Range(0, clipIndexPairs.Count)];

					player.PlayDirectClip(clipIndexPair.Key, playAsOneShot: !StopPlayingSound);

					randomLastIndices.Enqueue(clipIndexPair.Value);

					while(randomLastIndices.Count > Mathf.Max(AvoidRepeatingLast, 0)) {
						randomLastIndices.Dequeue();
					}

					asset.SetConductorsStorageValue(RandomLastIndices_StorageKey, player, randomLastIndices);


					break;

				default:
					throw new NotSupportedException(Mode.ToString());
			}



			yield break;
		}

		private static System.Random s_ShuffleRandom = new System.Random();

		// Fisher-Yates shuffle
		// https://xlinux.nist.gov/dads/HTML/fisherYatesShuffle.html
		// https://stackoverflow.com/questions/273313/randomize-a-listt
		private static void Shuffle<T>(IList<T> list)
		{
			int n = list.Count;
			while (n > 1) {
				n--;
				int k = s_ShuffleRandom.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}

	public class PlayAudioWithVFXConductor : PlayAudioConductor
	{
		[Header("VFX")]
		public bool ParentToSource;
		public bool RotateAsSource;
		public Vector3 Offset;

		public GameObject VisualEffectsPrefab;

		public override IEnumerator Play(AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			var baseIt = base.Play(player, asset);
			while (baseIt.MoveNext()) {
				// Assume base method is instant - no yields. If we yield it, the code will resume the next frame.
			};

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

	public class PlayCollectionAudioWithVFXConductor : PlayCollectionAudioConductor
	{
		[Header("VFX")]
		public bool ParentToSource;
		public bool RotateAsSource;
		public Vector3 Offset;

		public GameObject[] VisualEffectsPrefabs;

		public override IEnumerator Play(AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			var baseIt = base.Play(player, asset);
			while (baseIt.MoveNext()) {
				// Assume base method is instant - no yields. If we yield it, the code will resume the next frame.
			};

			if (VisualEffectsPrefabs.Length > 0) {
				Quaternion rotation = RotateAsSource ? player.transform.rotation : Quaternion.identity;

				var effectsPrefab = VisualEffectsPrefabs[UnityEngine.Random.Range(0, VisualEffectsPrefabs.Length)];

				if (ParentToSource) {
					GameObject.Instantiate(effectsPrefab, player.transform.position + Offset, rotation);
				} else {
					GameObject.Instantiate(effectsPrefab, player.transform.position + Offset, rotation, player.transform);
				}
			}

			yield break;
		}
	}

	public class IntroThenLoopConductor : AudioPlayerAsset.AudioConductor
	{
		public AudioClip Intro;
		public AudioResource Looped;

		[Range(0, 1)]
		public float Volume = 1.0f;

		public float Overlap = 0f;

		public override IEnumerator Play(AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			player.PlayDirectClip(Intro, playAsOneShot: true, Volume);

			player.AudioSource.volume = Volume * player.Volume;	// OneShot is volume is passed as argument, not changing the source.

			player.AudioSource.resource = Looped;
			player.Loop = true;

			double introLengthDouble = (double)Intro.samples / (double)Intro.frequency; // This is more accurate than clip.float.
			player.AudioSource.PlayScheduled(AudioSettings.dspTime + introLengthDouble - Overlap);

			yield break;
		}
	}

	public class PlayPitchSequenceConductor : AudioPlayerAsset.AudioConductor
	{
		public AudioPlayerAsset.ClipWithVolume AudioClip;

		[Tooltip("Should it start all over the pitch sequence on reacing the end, or should it use the last pitch?")]
		public bool ResetOnSequenceEnd = false;

		[Tooltip("Reset pitch index after this many seconds idle. Set to 0 to never reset so you can do it manually.")]
		public float ResetAfterSeconds = 3f;

		// To comply with music theory, the size of pitch difference should use semitones or cents.
		// One octave corresponding to a doubling of frequency. For example, the frequency one octave above 40 Hz is 80 Hz. In other words - power of two.
		// Semitone is the smallest musical step (white-to-black keys on piano distance).
		// Each octave is 12 semitones. To move a frequency up one octave you multiply by 2. So to move a frequency up one semitone you multiply by 2^(1/12)= 1.059463
		// Each semitone has 100 cent units. So to move a frequency up one cent you multiply by 2^(1/1200)= 1.0005777895065548592967925757932
		// Read more here: https://www.reddit.com/r/Unity3D/comments/18ycc02/sharing_a_really_basic_but_useful_tip_if_theres_a/
		// We use cents, because Unity uses cents in their AudioRandomContainer.
		public const float CentPitchSize = 1.0005777895065548592967925757932f;

		[Tooltip("Pitch sequence in cents. One semitone has 100 cents. One octave has 12 semitones.\nPrefer using semitone pitches, e.g. 100, 200, 400, etc.\n0 means no pitch change.")]
		[Utils.FieldUnitDecorator("ct", "Cents")]
		public int[] PitchSequence;

		private const string PitchIndex_StorageKey = "PitchIndex_" + nameof(PlayPitchSequenceConductor);
		private const string LastPlayTime_StorageKey = "LastPlayTime_" + nameof(PlayPitchSequenceConductor);

#if UNITY_EDITOR
		public override void OnValidate(AudioPlayerAsset context)
		{
			base.OnValidate(context);

			bool hasChanged = false;

			for (int i = 0; i < PitchSequence.Length; i++) {
				if (PitchSequence[i] < 0) {
					PitchSequence[i] = 0;
					hasChanged = true;
				}
			}

			if (hasChanged) {
				UnityEditor.EditorUtility.SetDirty(context);
			}
		}
#endif

		public override IEnumerator Play(AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			int pitchIndex = asset.GetConductorsStorageValue(PitchIndex_StorageKey, player, 0);
			float lastPlayTime = asset.GetConductorsStorageValue(LastPlayTime_StorageKey, player, - 1f);

			if (ResetAfterSeconds > 0 && lastPlayTime + ResetAfterSeconds < Time.time) {
				pitchIndex = 0;
			}

			player.AudioSource.pitch = Mathf.Pow(CentPitchSize, PitchSequence[pitchIndex]);

			player.PlayDirectClip(AudioClip, playAsOneShot: true);

			pitchIndex = ResetOnSequenceEnd
				? (pitchIndex + 1) % PitchSequence.Length
				: Mathf.Min(pitchIndex + 1, PitchSequence.Length - 1)
				;

			asset.SetConductorsStorageValue(PitchIndex_StorageKey, player, pitchIndex);
			asset.SetConductorsStorageValue(LastPlayTime_StorageKey, player, Time.time);

			yield break;
		}

		/// <summary>
		/// Helper function to reset the pitch sequence. Useful if <see cref="ResetAfterSeconds"/> is set to 0.
		/// </summary>
		public static void ResetPitchIndex(AudioSourcePlayer player)
		{
			if (player == null)
				return;

			player.AudioAsset.SetConductorsStorageValue(PitchIndex_StorageKey, player, 0);

		}
	}

	/// <summary>
	/// Play multiple sounds in a sequence, each one overlapping the last a bit.
	/// Helps create continues loop that doesn't feel like one.
	/// </summary>
	public class LoopSequenceOverlappingConductor : AudioPlayerAsset.AudioConductor
	{
		public AudioClip[] Clips;

		[Range(0f, 1f)]
		public float Volume = 1.0f;

		[Tooltip("How much time should the sequence be looped? Set to -1 to loop endlessly.")]
		public float Duration = -1f;
		public float Overlap = 0.1f;
		public bool RandomizeSequence = false;

		public override IEnumerator Play(AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			AudioClip clip = Clips.First();

			float startTime = float.MinValue;   // Fake it to start immediately.
			float totalPlayTime = 0f;

			while (true) {

				if (player.IsPaused)
					yield return null;

				totalPlayTime += Time.deltaTime;
				if (Duration >= 0f && totalPlayTime > Duration)
					yield break;	// The last clip will continue playing as OneShot.

				if (Time.time - startTime > clip.length - Overlap) {
					int nextIndex = RandomizeSequence
						? UnityEngine.Random.Range(0, Clips.Length)
						: (Array.IndexOf(Clips, clip) + 1) % Clips.Length;
						;

					clip = Clips[nextIndex];

					startTime = Time.time;

					player.PlayDirectClip(clip, playAsOneShot: true, Volume);
				}

				yield return null;
			}
		}
	}

}
