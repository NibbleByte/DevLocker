using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace DevLocker.Audio.Editor
{
	/// <summary>
	/// Draw "P" play button next to the reference.
	/// </summary>
#if UNITY_2023_2_OR_NEWER
	[CustomPropertyDrawer(typeof(AudioResource))]
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
				if (GUI.Button(playBtnRect, new GUIContent("S", "Stop the playing sound"))) {
					AudioEditorUtils.StopAllPreviewClips();
				}

				// Force repaint till sound stops playing.
				foreach (var editor in ActiveEditorTracker.sharedTracker.activeEditors) {
					if (editor.serializedObject.targetObject == property.serializedObject.targetObject) {
						editor.Repaint();
					}
				}

			} else {

				if (GUI.Button(playBtnRect, new GUIContent("P", "Play the sound"))) {
					if (property.objectReferenceValue is AudioClip) {
						AudioEditorUtils.PlayPreviewClip((AudioClip)property.objectReferenceValue);
					}

#if UNITY_2023_2_OR_NEWER
					// NOTE: This is still very fresh API and may change in the near future.
					// Assume AudioRandomContainer type, which is internal.
					if (property.objectReferenceValue) {
						var elementsField = property.objectReferenceValue.GetType().GetProperty("elements", BindingFlags.NonPublic | BindingFlags.Instance);
						var elements = elementsField?.GetValue(property.objectReferenceValue) as IList;
						if (elements != null && elements.Count > 0) {
							var element = elements[Random.Range(0, elements.Count)];
							var elementField = element.GetType().GetProperty("audioClip", BindingFlags.NonPublic | BindingFlags.Instance);
							var clip = elementField?.GetValue(element) as AudioClip;
							if (clip != null) {
								AudioEditorUtils.PlayPreviewClip(clip);
							}
						}
					}
#endif

				}
			}
		}
	}

}
