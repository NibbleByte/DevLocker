using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace DevLocker.Audio
{
	/// <summary>
	/// Wraps around the AudioSource offering API improvements and simpler interface.
	/// Also supports fading in and out sounds when interrupted (Stop, Pause, UnPause).
	/// </summary>
	public class AudioSourcePlayer : MonoBehaviour
	{
		public enum RepeatPatternType
		{
			Once = 0,
			Loop = 1,

			RepeatInterval = 4,
		}

		[Serializable]
		public struct IntervalRange
		{
			public float MinSeconds;
			public float MaxSeconds;
		}

		public delegate void PlayerEventHandler(AudioSourcePlayer player);
		public static event PlayerEventHandler PlayStarted;
		public static event PlayerEventHandler PlayPaused;
		public static event PlayerEventHandler PlayUnpaused;
		public static event PlayerEventHandler PlayStopped;

#if UNITY_2023_2_OR_NEWER
		public AudioResource AudioResource {
			get => m_AudioResource;
			set {
				m_AudioResource = value;
				if (m_AudioSource) m_AudioSource.resource = value;
			}
		}
#else
		public AudioClip AudioResource {
			get => m_AudioResource;
			set {
				m_AudioResource = value;
				if (m_AudioSource) m_AudioSource.clip = value;
			}
		}
#endif

		public AudioMixerGroup Output {
			get => m_Output;
			set {
				m_Output = value;
				if (m_AudioSource) m_AudioSource.outputAudioMixerGroup = value;
			}
		}

		public AudioSource Template {
			get => m_Template;
			set {
				m_Template = value;
				if (m_AudioSource) {
					SetupAudioSource();
				}
			}
		}

		public bool Mute {
			get => m_Mute;
			set {
				m_Mute = value;
				if (m_AudioSource) m_AudioSource.mute = value;
			}
		}

		public bool PlayOnEnable {
			get => m_PlayOnEnable;
			set {
				m_PlayOnEnable = value;
				// Handled by us.
			}
		}

		/// <summary>
		/// Short-cut for "<see cref="RepeatPattern"/> = <see cref="RepeatPatternType.Loop"/>"
		/// </summary>
		public bool Loop {
			get => RepeatPattern == RepeatPatternType.Loop;
			set => RepeatPattern = RepeatPatternType.Loop;
		}

		public RepeatPatternType RepeatPattern {
			get => m_RepeatPattern;
			set {
				m_RepeatPattern = value;
				if (m_AudioSource) m_AudioSource.loop = value == RepeatPatternType.Loop;
				if (value == RepeatPatternType.RepeatInterval) {
					m_NextPlayTime = Time.time;
					m_LastIsPlaying = IsPlaying;
				}
			}
		}

		public IntervalRange RepeatIntervalRange {
			get => m_RepeatIntervalRange;
			set {
				m_RepeatIntervalRange = value;
			}
		}

		public float Volume {
			get => m_Volume;
			set {
				m_Volume = value;
				if (m_AudioSource) m_AudioSource.volume = value;
			}
		}

		public float Pitch => AudioSource?.pitch ?? 0f;

		public float SpatialBlend => AudioSource?.spatialBlend ?? 0f;

		public AudioSource AudioSource {
			get {
				if (m_AudioSource == null) {
					SetupAudioSource();
				}

				return m_AudioSource;
			}
		}

		public static IReadOnlyList<AudioSourcePlayer> ActivePlayersRegister => m_ActivePlayersRegister.AsReadOnly();

		[SerializeField]
		[Tooltip("Resource to play")]
#if UNITY_2023_2_OR_NEWER
		private AudioResource m_AudioResource;
#else
		private AudioClip m_AudioResource;
#endif

		[SerializeField]
		[Tooltip("Resource to play. If left empty, it will copy the one of the template, if any")]
		private AudioMixerGroup m_Output;

		[SerializeField]
		[Tooltip("Prefab (or scene object) to be used as template when initializing the AudioSource properties")]
		private AudioSource m_Template;

		[SerializeField]
		[Tooltip("Mute the sound")]
		private bool m_Mute = false;

		[SerializeField]
		[Tooltip("Play automatically every time this component is enabled?")]
		private bool m_PlayOnEnable = true;

		[SerializeField]
		[Tooltip("How sound should be repeated, if needed.")]
		[UnityEngine.Serialization.FormerlySerializedAs("m_Loop")]
		private RepeatPatternType m_RepeatPattern;

		[SerializeField]
		[Tooltip("How much seconds to wait AFTER audio finished playing so it can start again. Will select random value within range.")]
		private IntervalRange m_RepeatIntervalRange;

		[Tooltip("Fade duration when sound is interrupted (Stop, Pause, Unpause)")]
		public float InterruptionFadeDuration = 0.2f;

		[Range(0f, 1f)]
		[SerializeField]
		[Tooltip("Volume of the sound")]
		private float m_Volume = 1f;

		private static readonly List<AudioSourcePlayer> m_ActivePlayersRegister = new List<AudioSourcePlayer>();

		private AudioSource m_AudioSource;

		private Coroutine m_VolumeCoroutine;

		private float m_NextPlayTime;
		private bool m_LastIsPlaying;
		private bool m_ShouldPlayRepeating;

		protected virtual void OnEnable()
		{
			m_ActivePlayersRegister.Add(this);

			AudioSource.enabled = true;
			if (PlayOnEnable) {
				Play();
			}
		}

		protected virtual void OnDisable()
		{
			m_ActivePlayersRegister.Remove(this);

			AudioSource.enabled = false;
		}

		protected virtual void OnValidate()
		{
#if UNITY_EDITOR
			if (InterruptionFadeDuration < 0f) {
				InterruptionFadeDuration = 0f;
				UnityEditor.EditorUtility.SetDirty(this);
			}

			if (m_RepeatIntervalRange.MinSeconds < 0f) {
				m_RepeatIntervalRange.MinSeconds = 0f;
				UnityEditor.EditorUtility.SetDirty(this);
			}

			if (m_RepeatIntervalRange.MaxSeconds < m_RepeatIntervalRange.MinSeconds) {
				m_RepeatIntervalRange.MaxSeconds = m_RepeatIntervalRange.MinSeconds;
				UnityEditor.EditorUtility.SetDirty(this);
			}

			if (Application.isPlaying && m_AudioSource) {
				if (m_AudioSource.mute != m_Mute) {
					m_AudioSource.mute = m_Mute;
				}
				if (m_AudioSource.loop != (m_RepeatPattern == RepeatPatternType.Loop)) {
					m_AudioSource.loop = m_RepeatPattern == RepeatPatternType.Loop;
				}
				if (m_AudioSource.volume != m_Volume) {
					m_AudioSource.volume = m_Volume;
				}
			}
#endif
		}

		public bool IsPlaying => m_AudioSource && m_AudioSource.isPlaying;

		[ContextMenu("Play")]
		public virtual void Play()
		{
			m_ShouldPlayRepeating = true;
			StopVolumeCrt();
			AudioSource.Play();

			PlayStarted?.Invoke(this);
		}

		public virtual void PlayDelayed(float delaySeconds)
		{
			m_ShouldPlayRepeating = true;
			StopVolumeCrt();
			AudioSource.PlayDelayed(delaySeconds);

			PlayStarted?.Invoke(this);
		}

		public virtual void PlayOneShot(AudioClip clip)
		{
			StopVolumeCrt();
			AudioSource.PlayOneShot(clip);
		}

		public virtual void PlayOnGamepad(int playerIndex)
		{
#if UNITY_EDITOR
			m_ShouldPlayRepeating = true;
			StopVolumeCrt();
			AudioSource.PlayOnGamepad(playerIndex);	// This is not available for every platform (e.g. PC doesn't have it).

			PlayStarted?.Invoke(this);
#endif
		}

		[ContextMenu("Stop")]
		public virtual void Stop()
		{
			m_ShouldPlayRepeating = false;

			if (InterruptionFadeDuration > 0f) {
				// Just kill the coroutine and resume from where it left off.
				if (m_VolumeCoroutine != null) {
					StopCoroutine(m_VolumeCoroutine);
				}
				m_VolumeCoroutine = StartCoroutine(FadeVolumeCrt(InterruptionFadeDuration, false, AudioSource.Stop));
			} else {
				StopVolumeCrt();
				AudioSource.Stop();
			}

			PlayStopped?.Invoke(this);
		}

		[ContextMenu("Pause")]
		public virtual void Pause()
		{
			m_ShouldPlayRepeating = false;

			if (InterruptionFadeDuration > 0f) {
				// Just kill the coroutine and resume from where it left off.
				if (m_VolumeCoroutine != null) {
					StopCoroutine(m_VolumeCoroutine);
				}
				m_VolumeCoroutine = StartCoroutine(FadeVolumeCrt(InterruptionFadeDuration, false, AudioSource.Pause));

			} else {
				StopVolumeCrt();
				AudioSource.Pause();
			}

			PlayPaused?.Invoke(this);
		}

		[ContextMenu("UnPause")]
		public virtual void UnPause()
		{
			m_ShouldPlayRepeating = true;

			if (InterruptionFadeDuration > 0f) {
				// Just kill the coroutine and resume from where it left off.
				if (m_VolumeCoroutine != null) {
					StopCoroutine(m_VolumeCoroutine);
				}
				m_VolumeCoroutine = StartCoroutine(FadeVolumeCrt(InterruptionFadeDuration, true));
				AudioSource.UnPause();

			} else {
				StopVolumeCrt();
				AudioSource.UnPause();
			}

			PlayUnpaused?.Invoke(this);
		}

		/// <summary>
		/// Destroy the component + audio source OR the whole game object.
		/// If <see cref="InterruptionFadeDuration"/> is non-zero value, will fade the sound first, then destroy it.
		/// </summary>
		public virtual void DestroyPlayer(bool destroyGameObject = false)
		{
			m_ShouldPlayRepeating = false;

			System.Action destroyAction = () => {
				if (destroyGameObject) {
					GameObject.Destroy(gameObject);
				} else {
					if (m_AudioSource) {
						GameObject.Destroy(m_AudioSource);
					}
					GameObject.Destroy(this);
				}
			};

			if (InterruptionFadeDuration > 0f) {
				// Just kill the coroutine and resume from where it left off.
				if (m_VolumeCoroutine != null) {
					StopCoroutine(m_VolumeCoroutine);
				}
				m_VolumeCoroutine = StartCoroutine(FadeVolumeCrt(InterruptionFadeDuration, false, destroyAction));

			} else {
				destroyAction();
			}

			PlayStopped?.Invoke(this);
		}

		private void StopVolumeCrt()
		{
			if (m_VolumeCoroutine != null) {
				AudioSource.volume = Volume;

				StopCoroutine(m_VolumeCoroutine);
				m_VolumeCoroutine = null;
			}
		}

		private IEnumerator FadeVolumeCrt(float fadeSeconds, bool fadeIn, System.Action callbackOnFinish = null)
		{
			float startTime = Time.time;
			float startVolume = fadeIn ? 0f : Volume;
			float endVolume = fadeIn ? Volume : 0f;

			if (m_VolumeCoroutine != null) {
				startVolume = AudioSource.volume; // Resume from where it left off.

				// Reduce the fade time to what is left.
				fadeSeconds *= Mathf.Abs(endVolume - AudioSource.volume) / Volume;
			}


			while (Time.time - startTime < fadeSeconds) {
				AudioSource.volume = Mathf.Lerp(startVolume, endVolume, (Time.time - startTime) / fadeSeconds);
				yield return null;
			}

			StopVolumeCrt();
			callbackOnFinish?.Invoke();
		}

		protected virtual void Update()
		{
			if (m_ShouldPlayRepeating && m_RepeatPattern == RepeatPatternType.RepeatInterval && m_AudioSource
#if UNITY_2023_2_OR_NEWER
				&& m_AudioSource.resource
#else
				&& m_AudioSource.clip
#endif
				) {
				if (IsPlaying != m_LastIsPlaying) {
					if (!IsPlaying) {
						m_NextPlayTime = Time.time + UnityEngine.Random.Range(m_RepeatIntervalRange.MinSeconds, m_RepeatIntervalRange.MaxSeconds);
					}
					m_LastIsPlaying = IsPlaying;
				}

				if (!IsPlaying && Time.time >= m_NextPlayTime) {
					Play();
				}
			}
		}

		private void SetupAudioSource()
		{
			if (m_AudioSource && (m_Template == null || m_Template.gameObject != gameObject)) {
				DestroyImmediate(m_AudioSource);
			}

			// If the template is component on this object, use it directly.
			if (m_Template && m_Template.gameObject == gameObject) {
				m_AudioSource = m_Template;
			} else {
				m_AudioSource = gameObject.AddComponent<AudioSource>();
			}

			m_AudioSource.playOnAwake = false; // Will be handled by us.
#if UNITY_2023_2_OR_NEWER
			m_AudioSource.resource = m_AudioResource;
#else
			m_AudioSource.clip = m_AudioResource;
#endif
			m_AudioSource.outputAudioMixerGroup = m_Output ?? m_AudioSource.outputAudioMixerGroup;
			m_AudioSource.loop = m_RepeatPattern == RepeatPatternType.Loop;
			m_AudioSource.volume = m_Volume;

			if (m_Template && m_Template.gameObject != gameObject) {
				m_AudioSource.bypassEffects = m_Template.bypassEffects;
				m_AudioSource.bypassListenerEffects = m_Template.bypassListenerEffects;
				m_AudioSource.bypassReverbZones = m_Template.bypassReverbZones;
				m_AudioSource.priority = m_Template.priority;
				m_AudioSource.pitch = m_Template.pitch;
				m_AudioSource.panStereo = m_Template.panStereo;
				m_AudioSource.spatialBlend = m_Template.spatialBlend;
				m_AudioSource.spatializePostEffects = m_Template.spatializePostEffects;
				m_AudioSource.reverbZoneMix = m_Template.reverbZoneMix;

				m_AudioSource.dopplerLevel = m_Template.dopplerLevel;
				m_AudioSource.minDistance = m_Template.minDistance;
				m_AudioSource.maxDistance = m_Template.maxDistance;
				m_AudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, m_Template.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
				m_AudioSource.SetCustomCurve(AudioSourceCurveType.SpatialBlend, m_Template.GetCustomCurve(AudioSourceCurveType.SpatialBlend));
				m_AudioSource.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, m_Template.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix));
				m_AudioSource.SetCustomCurve(AudioSourceCurveType.Spread, m_Template.GetCustomCurve(AudioSourceCurveType.Spread));
				m_AudioSource.rolloffMode = m_Template.rolloffMode;	// Because changing the curve changes this property to custom.
			}
		}
	}
}
