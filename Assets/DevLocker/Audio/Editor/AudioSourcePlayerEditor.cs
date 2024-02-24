using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace DevLocker.Audio.Editor
{
	[CustomEditor(typeof(AudioSourcePlayer), true)]
	[CanEditMultipleObjects]
	public class AudioSourcePlayerEditor : UnityEditor.Editor
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

			var repeatPattern = (AudioSourcePlayer.RepeatPatternType)serializedObject.FindProperty("m_RepeatPattern").intValue;

			// Will draw any child properties without [HideInInspector] attribute.
			if (repeatPattern == AudioSourcePlayer.RepeatPatternType.RepeatInterval) {
				DrawPropertiesExcluding(serializedObject, "m_Script");
			} else {
				DrawPropertiesExcluding(serializedObject, "m_Script", "m_RepeatIntervalRange");
			}

			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
			}

			EditorGUILayout.BeginHorizontal();

			var player = serializedObject.targetObject as AudioSourcePlayer;
			bool isPlaying = player?.IsPlaying ?? false;
			Color prevColor = GUI.color;
			if (isPlaying) {
				GUI.color = Color.green;
			}
			EditorGUILayout.LabelField(" ", isPlaying ? "Playing" : "Not Playing", EditorStyles.helpBox, GUILayout.Width(63f));
			GUI.color = prevColor;

			if (GUILayout.Button("Open Audio Monitor", GUILayout.ExpandWidth(false))) {
				AudioSourcePlayerMonitorWindow.ShowMonitor();
			}

			EditorGUILayout.EndHorizontal();
		}
	}

}
