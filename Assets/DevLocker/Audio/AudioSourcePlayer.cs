using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;

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

		public AudioResource AudioResource {
			get => m_AudioResource;
			set {

				// Conductor may be setting audio resource to be played.
				if (!IsPlayingOrPaused) {
					m_AudioAsset = null;
				}
				m_AudioResource = value;

				if (m_AudioSource) m_AudioSource.resource = value;
			}
		}

		public AudioPlayerAsset AudioAsset
		{
			get => m_AudioAsset;
			set {
				m_AudioAsset = value;
				m_AudioResource = null;

				if (m_AudioSource) m_AudioSource.resource = null;
			}
		}

		/// <summary>
		/// Object used by <see cref="AudioPlayerAsset"/> filters as context.
		/// Works great with <see cref="Conductors.DictionaryContext"/>, but you can have your custom implementation of <see cref="Conductors.IValuesContainer"/>.
		/// </summary>
		public object ConductorsFilterContext;

		/// <summary>
		/// Used by conductors to persist data per player between usages. For example: don't repeat last clip.
		/// Try to use unique key names.
		/// </summary>
		public Dictionary<string, object> ConductorsStorage = new Dictionary<string, object>();

		/// <summary>
		/// Used by conductors to persist data between usages globally. For example: don't repeat last clip.
		/// Try to use unique key names.
		/// </summary>
		public static Dictionary<string, object> GlobalConductorsStorage = new Dictionary<string, object>();

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
		private AudioResource m_AudioResource;

		[Tooltip("Custom audio assets provide more options on how to play your audio")]
		[SerializeField]
		private AudioPlayerAsset m_AudioAsset;


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
		private Coroutine m_ConductorCoroutine;

		private float m_NextPlayTime;
		private bool m_LastIsPlaying;
		private bool m_ShouldPlayRepeating;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ClearStaticsCache()
		{
			GlobalConductorsStorage = new Dictionary<string, object>();
		}

		protected virtual void OnEnable()
		{
			m_ActivePlayersRegister.Add(this);

			AudioSource.enabled = true;
			if (PlayOnEnable && (AudioResource || AudioAsset)) {
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

		public bool IsPlaying => m_AudioSource && (m_AudioSource.isPlaying || (m_ShouldPlayRepeating && m_RepeatPattern == RepeatPatternType.RepeatInterval));
		public bool IsPaused { get; private set; }

		// IsPlaying is false when paused.
		public bool IsPlayingOrPaused => IsPaused || IsPlaying;

		[ContextMenu("Play")]
		public virtual void Play()
		{
			PlayImpl(0f);
		}

		public virtual void PlayDelayed(float delaySeconds)
		{
			PlayImpl(delaySeconds);
		}

		private void PlayImpl(float delay)
		{
			m_ShouldPlayRepeating = true;
			IsPaused = false;

			StopVolumeCrt();
			StopConductorCrt();

			if (m_AudioAsset != null) {
				m_ConductorCoroutine = StartCoroutine(StartAudioAsset(AudioAsset, delay));
				PlayStarted?.Invoke(this);
			} else {
				if (delay <= 0f) {
					AudioSource.Play();
				} else {
					AudioSource.PlayDelayed(delay);
				}
				PlayStarted?.Invoke(this);
			}
		}

		public virtual void PlayOneShot(AudioClip clip)
		{
			StopVolumeCrt();
			StopConductorCrt();

			AudioSource.PlayOneShot(clip);
		}

		public virtual void PlayOnGamepad(int playerIndex)
		{
#if UNITY_EDITOR
			m_ShouldPlayRepeating = true;
			IsPaused = false;

			StopVolumeCrt();
			StopConductorCrt();

			AudioSource.PlayOnGamepad(playerIndex);	// This is not available for every platform (e.g. PC doesn't have it).

			PlayStarted?.Invoke(this);
#endif
		}

		[ContextMenu("Stop")]
		public virtual void Stop()
		{
			// Prevent multiple calls as it will reset the coroutine every time.
			if (!m_ShouldPlayRepeating)
				return;

			m_ShouldPlayRepeating = false;
			IsPaused = false;

			if (InterruptionFadeDuration > 0f) {
				// Just kill the coroutine and resume from where it left off.
				if (m_VolumeCoroutine != null) {
					StopCoroutine(m_VolumeCoroutine);
				}
				m_VolumeCoroutine = StartCoroutine(FadeVolumeCrt(InterruptionFadeDuration, false, AudioSource.Stop));
			} else {
				StopVolumeCrt();
				StopConductorCrt();

				AudioSource.Stop();
			}

			PlayStopped?.Invoke(this);
		}

		[ContextMenu("Pause")]
		public virtual void Pause()
		{
			// Prevent multiple calls as it will reset the coroutine every time.
			if (!m_ShouldPlayRepeating)
				return;

			m_ShouldPlayRepeating = false;
			IsPaused = true;

			if (InterruptionFadeDuration > 0f) {
				// Just kill the coroutine and resume from where it left off.
				if (m_VolumeCoroutine != null) {
					StopCoroutine(m_VolumeCoroutine);
				}
				m_VolumeCoroutine = StartCoroutine(FadeVolumeCrt(InterruptionFadeDuration, false, AudioSource.Pause));

			} else {
				StopVolumeCrt();
				// Audio assets keep going and wait for the player to get unpaused.

				AudioSource.Pause();
			}

			PlayPaused?.Invoke(this);
		}

		[ContextMenu("UnPause")]
		public virtual void UnPause()
		{
			// Prevent multiple calls as it will reset the coroutine every time.
			if (m_ShouldPlayRepeating)
				return;

			m_ShouldPlayRepeating = true;
			IsPaused = false;

			if (InterruptionFadeDuration > 0f) {
				// Just kill the coroutine and resume from where it left off.
				if (m_VolumeCoroutine != null) {
					StopCoroutine(m_VolumeCoroutine);
				}
				m_VolumeCoroutine = StartCoroutine(FadeVolumeCrt(InterruptionFadeDuration, true));
				AudioSource.UnPause();

			} else {
				StopVolumeCrt();
				// Audio assets keep updating while paused.

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

				PlayStopped?.Invoke(this);

			} else {
				StopVolumeCrt();
				StopConductorCrt();

				AudioSource.Stop();

				PlayStopped?.Invoke(this);
				destroyAction();
			}
		}

		public T GetConductorsStorageValue<T>(string keyName, T defaultValue)
		{
			if (ConductorsStorage.TryGetValue(keyName, out object objValue) && objValue is T value) {
				return value;
			} else {
				return defaultValue;
			}
		}

		public void SetConductorsStorageValue(string keyName, object value)
		{
			ConductorsStorage[keyName] = value;
		}

		public static T GetGlobalConductorsStorageValue<T>(string keyName, T defaultValue)
		{
			if (GlobalConductorsStorage.TryGetValue(keyName, out object objValue) && objValue is T value) {
				return value;
			} else {
				return defaultValue;
			}
		}

		public static void SetGlobalConductorsStorageValue(string keyName, object value)
		{
			GlobalConductorsStorage[keyName] = value;
		}

		private IEnumerator StartAudioAsset(AudioPlayerAsset audioAsset, float delay)
		{
			if (delay > 0f) {
				float waitTime = 0f;
				while (waitTime < delay) {
					yield return null;

					if (!IsPaused) {
						waitTime += Time.deltaTime;
					}
				}
			}

			yield return audioAsset.Play(this, ConductorsFilterContext);
		}

		private void StopVolumeCrt()
		{
			if (m_VolumeCoroutine != null) {
				AudioSource.volume = Volume;

				StopCoroutine(m_VolumeCoroutine);
				m_VolumeCoroutine = null;
			}
		}

		private void StopConductorCrt()
		{
			if (m_ConductorCoroutine != null) {
				StopCoroutine(m_ConductorCoroutine);
				m_ConductorCoroutine = null;
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
			if (!fadeIn && !IsPaused) {
				StopConductorCrt();
			}

			callbackOnFinish?.Invoke();
		}

		protected virtual void Update()
		{
			if (m_ShouldPlayRepeating && m_ConductorCoroutine == null && m_RepeatPattern == RepeatPatternType.RepeatInterval && m_AudioSource && m_AudioSource.resource) {
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
			m_AudioSource.resource = m_AudioResource;
			m_AudioSource.outputAudioMixerGroup = m_Output ?? m_AudioSource.outputAudioMixerGroup;
			m_AudioSource.loop = m_RepeatPattern == RepeatPatternType.Loop;
			m_AudioSource.volume = m_Volume;

			if (m_Template && m_Template.gameObject != gameObject) {
				CopyAudioSourceDetails(m_AudioSource, m_Template);
			}
		}

		public static void CopyAudioSource(AudioSource destination, AudioSource source)
		{
			destination.playOnAwake = source.playOnAwake;
			destination.resource = source.resource;
			destination.outputAudioMixerGroup = source.outputAudioMixerGroup;
			destination.loop = source.loop;
			destination.volume = source.volume;

			CopyAudioSource(destination, source);
		}

		public static void CopyAudioSourceDetails(AudioSource destination, AudioSource source)
		{
			destination.bypassEffects = source.bypassEffects;
			destination.bypassListenerEffects = source.bypassListenerEffects;
			destination.bypassReverbZones = source.bypassReverbZones;
			destination.priority = source.priority;
			destination.pitch = source.pitch;
			destination.panStereo = source.panStereo;
			destination.spatialBlend = source.spatialBlend;
			destination.spatializePostEffects = source.spatializePostEffects;
			destination.reverbZoneMix = source.reverbZoneMix;

			destination.dopplerLevel = source.dopplerLevel;
			destination.minDistance = source.minDistance;
			destination.maxDistance = source.maxDistance;
			destination.SetCustomCurve(AudioSourceCurveType.CustomRolloff, source.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
			destination.SetCustomCurve(AudioSourceCurveType.SpatialBlend, source.GetCustomCurve(AudioSourceCurveType.SpatialBlend));
			destination.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, source.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix));
			destination.SetCustomCurve(AudioSourceCurveType.Spread, source.GetCustomCurve(AudioSourceCurveType.Spread));
			destination.rolloffMode = source.rolloffMode; // Because changing the curve changes this property to custom.
		}
	}
}
