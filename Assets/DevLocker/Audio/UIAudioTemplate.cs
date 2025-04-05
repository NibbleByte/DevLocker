using UnityEngine;
using UnityEngine.Audio;

namespace DevLocker.Audio
{
	/// <summary>
	/// Template to be used by <see cref="UIAudioEffects"/> as a way of sharing settings and audio references.
	/// Must be placed next to AudioSource, preferably on a simple prefab.
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	public class UIAudioTemplate : MonoBehaviour
	{
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
	}
}
