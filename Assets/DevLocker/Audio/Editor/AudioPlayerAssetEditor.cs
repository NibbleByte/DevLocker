using DevLocker.Utils;
using UnityEditor;
using UnityEngine;

namespace DevLocker.Audio.Editor
{
	[CustomPropertyDrawer(typeof(AudioPlayerAsset.AudioPredicate))]
	public class AudioPredicateDrawer : SerializeReferenceCreatorDrawer
	{
	}

	[CustomPropertyDrawer(typeof(AudioPlayerAsset.AudioConductor))]
	public class AudioConductorDrawer : SerializeReferenceCreatorDrawer
	{
	}

	[CustomPropertyDrawer(typeof(AudioPlayerAsset.ResourceWithVolume))]
	[CustomPropertyDrawer(typeof(AudioPlayerAsset.ClipWithVolume))]
	internal class AudioWithVolumePropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			bool isClip = property.type == nameof(AudioPlayerAsset.ClipWithVolume);

			var resourceProperty = property.FindPropertyRelative(isClip ? nameof(AudioPlayerAsset.ClipWithVolume.Clip) : nameof(AudioPlayerAsset.ResourceWithVolume.Resource));
			var volumeProperty = property.FindPropertyRelative(nameof(AudioPlayerAsset.ResourceWithVolume.VolumeDB));

			const float volumeWidth = 74f;

			var resourceRect = position;
			resourceRect.width -= volumeWidth;

			var volumeRect = position;
			volumeRect.x += volumeRect.width - volumeWidth;
			volumeRect.width = volumeWidth;

			EditorGUI.PropertyField(resourceRect, resourceProperty, new GUIContent(""), true);
			EditorGUI.PropertyField(volumeRect, volumeProperty, new GUIContent(""), true);
		}
	}
}
