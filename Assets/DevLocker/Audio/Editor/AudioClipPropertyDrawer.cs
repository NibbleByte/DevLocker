using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DevLocker.Audio.Editor
{
	/// <summary>
	/// Draw "P" play button next to the reference.
	/// </summary>
#if UNITY_2023_2_OR_NEWER
	[CustomPropertyDrawer(typeof(UnityEngine.Audio.AudioResource))]
	[CustomPropertyDrawer(typeof(AudioClip))]
#else
	[CustomPropertyDrawer(typeof(AudioClip))]
#endif
	public class AudioClipPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			const float PLAY_BTN_WIDTH = 20.0f;
			const float PADDING = 4.0f;

			var refRect = new Rect(position.position, new Vector2(position.width - PLAY_BTN_WIDTH - PADDING, EditorGUIUtility.singleLineHeight));
			var playBtnRect = new Rect(position.position, new Vector2(PLAY_BTN_WIDTH, EditorGUIUtility.singleLineHeight));
			playBtnRect.x += refRect.width + PADDING;

			EditorGUI.PropertyField(refRect, property);

			if (AudioEditorUtils.IsPreviewClipPlaying()) {
				if (GUI.Button(playBtnRect, AudioEditorUtils.StopIconContent, AudioEditorUtils.PlayStopButtonStyle)) {
					AudioEditorUtils.StopAllPreviewClips();
				}

				// Force repaint till sound stops playing.
				foreach (var editor in ActiveEditorTracker.sharedTracker.activeEditors) {
					if (editor.serializedObject.targetObject == property.serializedObject.targetObject) {
						editor.Repaint();
					}
				}

			} else {

				if (GUI.Button(playBtnRect, AudioEditorUtils.PlayIconContent, AudioEditorUtils.PlayStopButtonStyle)) {

#if UNITY_2023_2_OR_NEWER
					if (property.objectReferenceValue is UnityEngine.Audio.AudioResource resource) {
						AudioEditorUtils.PlayPreviewClip(resource);
					}
#else
					if (property.objectReferenceValue is AudioClip clip) {
						AudioEditorUtils.PlayPreviewClip(clip);
					}
#endif
				}
			}
		}
	}

}
