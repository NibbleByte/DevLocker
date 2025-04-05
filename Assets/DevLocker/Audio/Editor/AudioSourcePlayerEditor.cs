using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace DevLocker.Audio.Editor
{
	[CustomEditor(typeof(AudioSourcePlayer), true)]
	[CanEditMultipleObjects]
	public class AudioSourcePlayerEditor : UnityEditor.Editor
	{
		private Vector2 m_ContextScrollPos;

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

			var audioResourceProperty = serializedObject.FindProperty("m_AudioResource");
			var audioAssetProperty = serializedObject.FindProperty("m_AudioAsset");

			EditorGUI.BeginDisabledGroup(audioAssetProperty.objectReferenceValue);
			EditorGUILayout.PropertyField(audioResourceProperty);
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(audioResourceProperty.objectReferenceValue && audioAssetProperty.objectReferenceValue == null);
			EditorGUILayout.PropertyField(audioAssetProperty);
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();

			var repeatPattern = (AudioSourcePlayer.RepeatPatternType)serializedObject.FindProperty("m_RepeatPattern").intValue;

			// Will draw any child properties without [HideInInspector] attribute.
			if (repeatPattern == AudioSourcePlayer.RepeatPatternType.RepeatInterval) {
				DrawPropertiesExcluding(serializedObject, "m_Script", "m_AudioResource", "m_AudioAsset");
			} else {
				DrawPropertiesExcluding(serializedObject, "m_Script", "m_AudioResource", "m_AudioAsset", "m_RepeatIntervalRange");
			}

			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
			}

			EditorGUILayout.BeginHorizontal();

			var player = serializedObject.targetObject as AudioSourcePlayer;

			Color prevColor = GUI.color;
			string playingHint = "Not Playing";

			bool isPlaying = player?.IsPlaying ?? false;
			if (isPlaying) {
				GUI.color = Color.green;
				playingHint = "Playing";
			}
			bool isPaused = player?.IsPaused ?? false;
			if (isPaused) {
				GUI.color = Color.yellow;
				playingHint = "Paused";
			}

			EditorGUILayout.LabelField(" ", playingHint, EditorStyles.helpBox, GUILayout.Width(63f));
			GUI.color = prevColor;

			if (GUILayout.Button("Open Audio Monitor", GUILayout.ExpandWidth(false))) {
				AudioSourcePlayerMonitorWindow.ShowMonitor();
			}

			EditorGUILayout.EndHorizontal();

			if (Application.isPlaying && player.ConductorContext != null) {
				if (player.ConductorContext is IEnumerable<KeyValuePair<string, object>> enumerableContext) {
					EditorGUILayout.LabelField("Context", EditorStyles.boldLabel);
					EditorGUI.indentLevel++;

					m_ContextScrollPos = EditorGUILayout.BeginScrollView(m_ContextScrollPos, EditorStyles.helpBox, GUILayout.MaxHeight(100f));
					foreach(var pair in enumerableContext) {
						DrawPair(pair.Key, pair.Value);
					}
					EditorGUILayout.EndScrollView();

					EditorGUI.indentLevel--;
				}
			}
		}

		private static void DrawPair(string key, object value)
		{
			if (value is int) {
				EditorGUILayout.IntField(key, (int)value);
			}
			if (value is float || value is double) {
				EditorGUILayout.FloatField(key, (float)value);
			}
			if (value is bool) {
				EditorGUILayout.Toggle(key, (bool)value);
			}
			if (value is string) {
				EditorGUILayout.TextField(key, (string)value);
			}
			if (value is Object) {
				EditorGUILayout.ObjectField(key, (Object)value, value.GetType(), allowSceneObjects: false);
			}
		}
	}

}
