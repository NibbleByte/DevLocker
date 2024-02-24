using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace DevLocker.Audio.Editor
{
	/// <summary>
	/// Display any plays/stops/pauses/unpauses done by a <see cref="AudioSourcePlayer"/>
	/// </summary>
	public class AudioSourcePlayerMonitorWindow : EditorWindow
	{
		private enum ActionType
		{
			Play,
			Stop,
			Pause,
			UnPause,
		}

		[Serializable]
		private struct ActionEntry
		{
			public ActionType Type;
			public float Time;

			public AudioSourcePlayer Player;
#if UNITY_2023_2_OR_NEWER
			public AudioResource Resource;
#else
			public AudioClip Resource;
#endif
			public AudioMixerGroup MixerGroup;
			public AudioSource Template;

			public bool Mute;
			public bool PlayOnEnable;
			public AudioSourcePlayer.RepeatPatternType RepeatPattern;
			public float Volume;
			public float Pitch;
			public float SpatialBlend;
		}

		private bool m_ListenForEvents = true;
		private bool m_ClearOnPlay = true;
		private int m_EntriesLimit = 100;

		private List<ActionEntry> m_Actions = new List<ActionEntry>();

		[NonSerialized] private GUIStyle HeaderStyle;

		private Vector2 m_ScrollView;

		public static void ShowMonitor()
		{
			var window = GetWindow<AudioSourcePlayerMonitorWindow>(false, "Audio Monitor");
			window.minSize = new Vector2(750f, 200f);
		}

		void OnEnable()
		{
			AudioSourcePlayer.PlayStarted += OnPlayStarted;
			AudioSourcePlayer.PlayStopped += OnPlayStopped;
			AudioSourcePlayer.PlayPaused += OnPlayPaused;
			AudioSourcePlayer.PlayUnpaused += OnPlayUnpaused;

			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		void OnDisable()
		{
			AudioSourcePlayer.PlayStarted -= OnPlayStarted;
			AudioSourcePlayer.PlayStopped -= OnPlayStopped;
			AudioSourcePlayer.PlayPaused -= OnPlayPaused;
			AudioSourcePlayer.PlayUnpaused -= OnPlayUnpaused;
		}

		private void OnPlayModeStateChanged(PlayModeStateChange stateChange)
		{
			if (m_ClearOnPlay && stateChange == PlayModeStateChange.ExitingEditMode) {
				m_Actions.Clear();
			}
		}

		private void OnPlayStarted(AudioSourcePlayer player) => AddAction(ActionType.Play, player);

		private void OnPlayStopped(AudioSourcePlayer player) => AddAction(ActionType.Stop, player);

		private void OnPlayPaused(AudioSourcePlayer player) => AddAction(ActionType.Pause, player);

		private void OnPlayUnpaused(AudioSourcePlayer player) => AddAction(ActionType.UnPause, player);

		private void AddAction(ActionType actionType, AudioSourcePlayer player)
		{
			if (!m_ListenForEvents)
				return;

			m_Actions.Add(new ActionEntry() {
				Type = actionType,
				Time = Time.time,

				Player = player,
				Resource = player.AudioResource,
				MixerGroup = player.Output,
				Template = player.Template,

				Mute = player.Mute,
				PlayOnEnable = player.PlayOnEnable,

				RepeatPattern = player.RepeatPattern,
				Volume = player.Volume,
				Pitch = player.Pitch,
				SpatialBlend = player.SpatialBlend,

			});

			if (m_Actions.Count > m_EntriesLimit) {
				m_Actions.RemoveAt(0);
			}

			Repaint();
		}

		private void InitStyles()
		{
			HeaderStyle = new GUIStyle(EditorStyles.toolbar);
			HeaderStyle.alignment = TextAnchor.LowerCenter;
		}

		void OnGUI()
		{
			if (HeaderStyle == null) {
				InitStyles();
			}

			EditorGUILayout.BeginHorizontal();
			{
				Color prevBackgroundColor = GUI.backgroundColor;
				GUI.backgroundColor = m_ListenForEvents ? Color.green : prevBackgroundColor;

				if (GUILayout.Button("Listen", GUILayout.ExpandWidth(false))) {
					m_ListenForEvents = !m_ListenForEvents;
				}

				GUI.backgroundColor = prevBackgroundColor;

				m_EntriesLimit = Mathf.Max(0, EditorGUILayout.IntField("Limit Entries", m_EntriesLimit, GUILayout.ExpandWidth(false)));

				GUILayout.FlexibleSpace();

				float prevLabelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 80f;
				m_ClearOnPlay = EditorGUILayout.Toggle("Clear on play", m_ClearOnPlay);
				EditorGUIUtility.labelWidth = prevLabelWidth;

				if (GUILayout.Button("Clear")) {
					m_Actions.Clear();
				}
			}
			EditorGUILayout.EndHorizontal();

			// NOTE: Tried doing it smarter, but ObjectField and Toggles don't accept styles, so no way to specify margins and format columns under header.
			//		 Changing the EditorStyles didn't work out too. So I have to fake the margins manually. :(
			//		 Other option is to use the Unity TreeView.
			const float enumColumnWidth = 50f;
			const float boolColumnWidth = 35f;
			const float timeColumnWidth = 80f;
			const float floatColumnWidth = 30f;
			const float scrollViewMarginFix = 35f;
			float objectFlexibleWidth = (position.width - timeColumnWidth - 2 * enumColumnWidth - 2 * boolColumnWidth - 3 * floatColumnWidth - scrollViewMarginFix) / 4f;

			// Table Header
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Label("Action", HeaderStyle, GUILayout.Width(enumColumnWidth));
				GUILayout.Label("Time", HeaderStyle, GUILayout.Width(timeColumnWidth));

				GUILayout.Label("Player", HeaderStyle, GUILayout.Width(objectFlexibleWidth));
				GUILayout.Label("Resource", HeaderStyle, GUILayout.Width(objectFlexibleWidth));
				GUILayout.Label("Mixer Group", HeaderStyle, GUILayout.Width(objectFlexibleWidth));
				GUILayout.Label("Template", HeaderStyle, GUILayout.Width(objectFlexibleWidth));

				GUILayout.Label("Mute", HeaderStyle, GUILayout.Width(boolColumnWidth));
				GUILayout.Label("Enable", HeaderStyle, GUILayout.Width(boolColumnWidth));

				GUILayout.Label("Repeat", HeaderStyle, GUILayout.Width(enumColumnWidth + 4f));

				GUILayout.Label("Vol", HeaderStyle, GUILayout.Width(floatColumnWidth + 8f));
				GUILayout.Label("Pitch", HeaderStyle, GUILayout.Width(floatColumnWidth - 6f));
				GUILayout.Label("Spatial", HeaderStyle, GUILayout.Width(floatColumnWidth + scrollViewMarginFix));
			}
			EditorGUILayout.EndHorizontal();

			m_ScrollView = GUILayout.BeginScrollView(m_ScrollView);

#if UNITY_2023_2_OR_NEWER
			var audioType = typeof(AudioResource);
#else
			var audioType = typeof(AudioClip);
#endif

			// Table Content
			for (int i = m_Actions.Count - 1; i >= 0; i--) {
				var action = m_Actions[i];

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label(action.Type.ToString(), EditorStyles.boldLabel, GUILayout.Width(enumColumnWidth));
					EditorGUILayout.FloatField(action.Time, GUILayout.Width(timeColumnWidth));

					float objectMarginFix = 5;
					EditorGUILayout.ObjectField(action.Player, typeof(AudioSourcePlayer), true, GUILayout.Width(objectFlexibleWidth - objectMarginFix));
					EditorGUILayout.ObjectField(action.Resource, audioType, true, GUILayout.Width(objectFlexibleWidth - objectMarginFix));
					EditorGUILayout.ObjectField(action.MixerGroup, typeof(AudioMixerGroup), true, GUILayout.Width(objectFlexibleWidth - objectMarginFix));
					EditorGUILayout.ObjectField(action.Template, typeof(AudioSource), true, GUILayout.Width(objectFlexibleWidth - objectMarginFix));

					float toggleMargin = 8f;
					GUILayout.Space(toggleMargin);
					EditorGUILayout.Toggle(action.Mute, GUILayout.Width(boolColumnWidth - toggleMargin));
					GUILayout.Space(toggleMargin);
					EditorGUILayout.Toggle(action.PlayOnEnable, GUILayout.Width(boolColumnWidth - toggleMargin));

					GUILayout.Label(action.RepeatPattern.ToString().Replace("Repeat", ""), EditorStyles.boldLabel, GUILayout.Width(enumColumnWidth));

					EditorGUILayout.FloatField(action.Volume, GUILayout.Width(floatColumnWidth));
					EditorGUILayout.FloatField(action.Pitch, GUILayout.Width(floatColumnWidth));
					EditorGUILayout.FloatField(action.SpatialBlend, GUILayout.Width(floatColumnWidth));

				}
				EditorGUILayout.EndHorizontal();

			}

			EditorGUILayout.EndScrollView();
		}
	}

}
