using DevLocker.Utils;
using UnityEditor;
using UnityEngine;

namespace DevLocker.Audio.Editor
{
	[CustomEditor(typeof(AudioPlayerAsset))]
	[CanEditMultipleObjects]
	public class AudioPlayerAssetEditor : UnityEditor.Editor
	{
		protected void DrawScriptProperty()
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
			EditorGUI.EndDisabledGroup();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawScriptProperty();

			EditorGUI.BeginChangeCheck();

			var loopRepeatProperty = serializedObject.FindProperty(nameof(AudioPlayerAsset.LoopRepeat));

			// Will draw any child properties without [HideInInspector] attribute.
			if (loopRepeatProperty.boolValue) {
				DrawPropertiesExcluding(serializedObject, "m_Script");
			} else {
				DrawPropertiesExcluding(serializedObject, "m_Script", nameof(AudioPlayerAsset.RepeatIntervalRange));
			}

			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
			}
		}
	}

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
