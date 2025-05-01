using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace DevLocker.Audio
{
	/// <summary>
	/// Asset used by the <see cref="AudioSourcePlayer"/> to play sound in a specific way with given filters.
	/// </summary>
	[CreateAssetMenu(fileName = "Unknown_AudioAsset", menuName = "Audio/Audio Player Asset")]
	public class AudioPlayerAsset : ScriptableObject
	{

		/// <summary>
		/// Responsible for playing the desired audio.
		/// Inherit to have custom behaviour.
		/// </summary>
		[Serializable]
		public abstract class AudioConductor
		{
			public abstract IEnumerator Play(AudioSourcePlayer player, AudioPlayerAsset asset);

			public virtual void OnValidate(AudioPlayerAsset context) { }
		}

		/// <summary>
		/// Used as filters when choosing which conductor to play.
		/// </summary>
		[Serializable]
		public abstract class AudioPredicate
		{
			public abstract bool IsAllowed(object context, AudioSourcePlayer player, AudioPlayerAsset asset);

			public virtual void OnValidate(AudioPlayerAsset context) { }
		}

		#region Data helpers

		/// <summary>
		/// Use in conductors to show audio with volume.
		/// </summary>
		[Serializable]
		public struct ResourceWithVolume
		{
			public AudioResource Resource;

			public float Volume => AudioVolumeUtils.DecibelToFloat(VolumeDB);

			[Utils.FieldUnitDecorator("dB", "Decibels in range [-80, 0]", MinValue = -80f, MaxValue = 0f)]
			public float VolumeDB;
		}

		/// <summary>
		/// Use in conductors to show audio with volume.
		/// </summary>
		[Serializable]
		public struct ClipWithVolume
		{
			public AudioClip Clip;

			public float Volume => AudioVolumeUtils.DecibelToFloat(VolumeDB);

			[Utils.FieldUnitDecorator("dB", "Decibels in range [-80, 0]", MinValue = -80f, MaxValue = 0f)]
			public float VolumeDB;
		}

		#endregion

		public enum ConductorsStateStorageLocation
		{
			Asset,
			Player,
		}

		[Serializable]
		public struct AudioConductorBind
		{
			[Tooltip("Responsible for playing the desired audio.")]
			[SerializeReference]
			public AudioConductor Conductor;

			[Tooltip("All filters should be satisfied in order for this event to execute.")]
			[SerializeReference]
			public AudioPredicate[] Filters;
		}

		[Tooltip("Where to store conductors state (if any)?\nExample: should screams shuffle per character or per asset?")]
		public ConductorsStateStorageLocation StateStorageLocation;

		[Tooltip("Should it loop this audio asset (ignores the player settings)?")]
		public bool LoopRepeat;
		[Tooltip("Random interval to repeatedly play the audio asset.")]
		public AudioSourcePlayer.IntervalRange RepeatIntervalRange;

		public AudioConductorBind[] Conductors;

		/// <summary>
		/// Used by conductors to persist state per asset between usages. For example: don't repeat last clip.
		/// Try to use unique key names.
		/// </summary>
		public Dictionary<string, object> ConductorsStateStorage = new Dictionary<string, object>();


		public IEnumerator Play(AudioSourcePlayer player, object context)
		{
			do {
				var conductorBind = Conductors.FirstOrDefault(bind => bind.Filters.All(f => f?.IsAllowed(context, player, this) ?? true));
				if (conductorBind.Conductor != null) {
					yield return conductorBind.Conductor.Play(player, this);
				}

				if (LoopRepeat) {
					float waitTime = RepeatIntervalRange.NextValue();
					float passedTime = 0.0f;

					while(passedTime <= waitTime && LoopRepeat) {
						yield return null;

						if (!player.IsPaused && !player.AudioSource.isPlaying) {
							passedTime += Time.deltaTime;
						}
					}
				}

			} while (LoopRepeat);
		}

		void OnValidate()
		{
			Utils.SerializeReferenceValidation.ClearDuplicateReferences(this);

			RepeatIntervalRange.OnValidate(this);

			foreach (var conductorBind in Conductors) {
				conductorBind.Conductor?.OnValidate(this);

				foreach(var filter in conductorBind.Filters) {
					filter?.OnValidate(this);
				}
			}
		}

#if UNITY_EDITOR
		//
		// This is the only way to reset scriptable object's state if assembly reload is disabled. Sad!
		//
		void OnEnable()
		{
			UnityEditor.EditorApplication.playModeStateChanged += EditorStateChanged;
		}

		void OnDisable()
		{
			UnityEditor.EditorApplication.playModeStateChanged -= EditorStateChanged;
		}

		private void EditorStateChanged(UnityEditor.PlayModeStateChange state)
		{
			if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode) {
				ConductorsStateStorage = new Dictionary<string, object>();
			}
		}
#endif


		/// <summary>
		/// Get conductors storage value based on the <see cref="ConductorsStateStorage"/> setting.
		/// </summary>
		public T GetConductorsStorageValue<T>(string keyName, AudioSourcePlayer audioPlayer, T defaultValue)
		{
			object objValue;

			switch (StateStorageLocation) {

				case ConductorsStateStorageLocation.Asset:
					if (ConductorsStateStorage.TryGetValue(keyName, out objValue) && objValue is T) {
						return (T)objValue;
					}
					break;

				case ConductorsStateStorageLocation.Player:
					if (audioPlayer.ConductorsStateStorage.TryGetValue($"{keyName}_{name}_{GetInstanceID()}", out objValue) && objValue is T) {
						return (T)objValue;
					}
					break;

				default:
					break;
			}

			return defaultValue;
		}

		/// <summary>
		/// Set conductors storage value based on the <see cref="ConductorsStateStorage"/> setting.
		/// </summary>
		public void SetConductorsStorageValue(string keyName, AudioSourcePlayer audioPlayer, object value)
		{
			switch (StateStorageLocation) {

				case ConductorsStateStorageLocation.Asset:
					ConductorsStateStorage[keyName] = value;
					break;

				case ConductorsStateStorageLocation.Player:
					audioPlayer.ConductorsStateStorage[$"{keyName}_{name}_{GetInstanceID()}"] = value;
					break;

				default:
					break;
			}
		}
	}

}
