using DevLocker.Utils;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

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
			public abstract IEnumerator Play(AudioSourcePlayer player);

			public virtual void OnValidate(AudioPlayerAsset context) { }
		}

		/// <summary>
		/// Used as filters when choosing which conductor to play.
		/// </summary>
		[Serializable]
		public abstract class AudioPredicate
		{
			public abstract bool IsAllowed(object context);

			public virtual void OnValidate(AudioPlayerAsset context) { }
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

		public AudioConductorBind[] Conductors;

		public IEnumerator Play(AudioSourcePlayer player, object context)
		{
			var conductorBind = Conductors.FirstOrDefault(bind => bind.Filters.All(f => f?.IsAllowed(context) ?? true));
			if (conductorBind.Conductor != null) {
				yield return conductorBind.Conductor.Play(player);
			}
		}


		void OnValidate()
		{
			SerializeReferenceValidation.ClearDuplicateReferences(this);

			foreach (var conductorBind in Conductors) {
				conductorBind.Conductor?.OnValidate(this);

				foreach(var filter in conductorBind.Filters) {
					filter?.OnValidate(this);
				}
			}
		}



#if UNITY_EDITOR
		// These are public so they can be inherited if needed.

		[UnityEditor.CustomPropertyDrawer(typeof(AudioConductor))]
		public class __AudioConductorDrawer : SerializeReferenceCreatorDrawer<AudioConductor>
		{
		}

		[UnityEditor.CustomPropertyDrawer(typeof(AudioPredicate))]
		public class __AudioPredicateDrawer : SerializeReferenceCreatorDrawer<AudioPredicate>
		{
		}
#endif

	}

}
