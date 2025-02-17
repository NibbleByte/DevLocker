#if UNITY_2023_2_OR_NEWER
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace DevLocker.Audio
{
	/// <summary>
	/// Use to add sounds to your UI buttons and other Selectable elements.
	/// Instead of copying audio references for multiple buttons, use a shared template prefab.
	/// The audio references here can be used to override the template ones.
	///
	/// NOTE: If audio reference is not supplied it won't consume the event itself.
	/// </summary>
	public class UIAudioEffects : MonoBehaviour
	{
		public enum InteractableModeType
		{
			AlwaysPlay,
			PlayWhenInteractable,
			PlayWhenNonInteractable,
		}

		[Tooltip("When should audio be played in relation to the Selectable attached to.")]
		public InteractableModeType InteractableMode = InteractableModeType.PlayWhenInteractable;

		[Tooltip("Optional template to share audio setup.\nIt must be a prefab with the template component and AudioSource to copy settings from.\nThis way you can easily set common mixer output etc.")]
		public UIAudioTemplate Template;

		[Header("Template Overrides")]

		[Tooltip("Submit is called on pressing <Enter> or gamepad <A>, but NOT on pointer clicks.")]
		public AudioResource SubmitAudio;
		[Tooltip("OnClick is called on only for pointer clicks, NOT on pressing <Enter> or gamepad <A>.")]
		public AudioResource PointerClickAudio;

		public AudioResource PointerDownAudio;
		public AudioResource PointerUpAudio;
		public AudioResource PointerEnterAudio;
		public AudioResource PointerExitAudio;

		public AudioResource SelectAudio;
		public AudioResource DeselectAudio;

		public delegate void UIAudioEffectsEventHandler(UIAudioEffects uiAudioEffects, AudioResource playedResource);
		public static event UIAudioEffectsEventHandler PlayedAudio;

		public AudioSource AudioSource { get; private set; }
		private Selectable m_Selectable;

		void Awake()
		{
			if (Template) {

				var templateSource = Template.GetComponent<AudioSource>();
				AudioSource = gameObject.AddComponent<AudioSource>();

				AudioSource.playOnAwake = false;
				AudioSource.loop = false;
				AudioSource.outputAudioMixerGroup = templateSource.outputAudioMixerGroup;
				AudioSource.volume = templateSource.volume;

				AudioSourcePlayer.CopyAudioSourceDetails(AudioSource, templateSource);

				SubmitAudio = SubmitAudio ?? Template.SubmitAudio;
				PointerClickAudio = PointerClickAudio ?? Template.PointerClickAudio;
				PointerDownAudio = PointerDownAudio ?? Template.PointerDownAudio;
				PointerUpAudio = PointerUpAudio ?? Template.PointerUpAudio;
				PointerEnterAudio = PointerEnterAudio ?? Template.PointerEnterAudio;
				PointerExitAudio = PointerExitAudio ?? Template.PointerExitAudio;
				SelectAudio = SelectAudio ?? Template.SelectAudio;
				DeselectAudio = DeselectAudio ?? Template.DeselectAudio;

			} else {
				AudioSource = GetComponent<AudioSource>(); // Can use this as template

				if (AudioSource == null) {
					AudioSource = gameObject.AddComponent<AudioSource>();
					AudioSource.spatialBlend = 0f;	// Make it 2D
				}
			}

			// Add handler components instead of listening to ourselves.
			// If we did, we would have to implement all those interfaces, but only use a few.
			// Implementing the interface will consume the event, so child objects might not hear it.
			SetupHandler<UIAudioEffects_Submit>(SubmitAudio);
			SetupHandler<UIAudioEffects_PointerClick>(PointerClickAudio);

			SetupHandler<UIAudioEffects_PointerDown>(PointerDownAudio);
			SetupHandler<UIAudioEffects_PointerUp>(PointerUpAudio);
			SetupHandler<UIAudioEffects_PointerEnter>(PointerEnterAudio);
			SetupHandler<UIAudioEffects_PointerExit>(PointerExitAudio);
			SetupHandler<UIAudioEffects_Select>(SelectAudio);
			SetupHandler<UIAudioEffects_Deselect>(DeselectAudio);
		}

		private void PlayAudio(AudioResource audioResource)
		{
			if (audioResource == null)
				return;

			if (InteractableMode != InteractableModeType.AlwaysPlay) {
				if (m_Selectable == null) {
					m_Selectable = GetComponentInParent<Selectable>();
				}

				bool playWhenInteractable = InteractableMode == InteractableModeType.PlayWhenInteractable;
				if (m_Selectable && m_Selectable.IsInteractable() != playWhenInteractable) {
					return;
				}
			}

			AudioSource.resource = audioResource;
			AudioSource.Play();

			PlayedAudio?.Invoke(this, audioResource);
		}

		#region Helper behaviours

		private void SetupHandler<T>(AudioResource audioResource) where T : UIAudioEffects_EventHandler
		{
			if (audioResource == null)
				return;

			var handler = gameObject.AddComponent<T>();
			handler.Owner = this;
			handler.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
		}

		private class UIAudioEffects_EventHandler : MonoBehaviour
		{
			public UIAudioEffects Owner;
		}



		private class UIAudioEffects_Submit : UIAudioEffects_EventHandler, ISubmitHandler
		{
			// Same as OnClick, or else pressing Enter on selected button won't play.
			public void OnSubmit(BaseEventData eventData) => Owner.PlayAudio(Owner.SubmitAudio);
		}

		private class UIAudioEffects_PointerClick : UIAudioEffects_EventHandler, IPointerClickHandler
		{
			public void OnPointerClick(PointerEventData eventData) => Owner.PlayAudio(Owner.PointerClickAudio);
		}

		private class UIAudioEffects_PointerDown : UIAudioEffects_EventHandler, IPointerDownHandler
		{
			public void OnPointerDown(PointerEventData eventData) => Owner.PlayAudio(Owner.PointerDownAudio);
		}

		private class UIAudioEffects_PointerUp : UIAudioEffects_EventHandler, IPointerUpHandler
		{
			public void OnPointerUp(PointerEventData eventData) => Owner.PlayAudio(Owner.PointerUpAudio);
		}

		private class UIAudioEffects_PointerEnter : UIAudioEffects_EventHandler, IPointerEnterHandler
		{
			public void OnPointerEnter(PointerEventData eventData) => Owner.PlayAudio(Owner.PointerEnterAudio);
		}

		private class UIAudioEffects_PointerExit : UIAudioEffects_EventHandler, IPointerExitHandler
		{
			public void OnPointerExit(PointerEventData eventData) => Owner.PlayAudio(Owner.PointerExitAudio);
		}

		private class UIAudioEffects_Select : UIAudioEffects_EventHandler, ISelectHandler
		{
			public void OnSelect(BaseEventData eventData) => Owner.PlayAudio(Owner.SelectAudio);
		}

		private class UIAudioEffects_Deselect : UIAudioEffects_EventHandler, IDeselectHandler
		{
			public void OnDeselect(BaseEventData eventData) => Owner.PlayAudio(Owner.DeselectAudio);
		}

		#endregion
	}

}
#endif
