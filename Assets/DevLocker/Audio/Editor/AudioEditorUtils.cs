// from RTyper forum post that hooks up unitys AudioUtil library

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_2023_2_OR_NEWER
using UnityEngine.Audio;
#endif

namespace DevLocker.Audio.Editor
{
	/// <summary>
	/// Audio utilities.
	/// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Audio/Bindings/AudioUtil.bindings.cs
	/// </summary>
	public static class AudioEditorUtils
	{
		private static GUIStyle s_PlayStopButtonStyle;
		private static GUIContent s_PlayIconContent;
		private static GUIContent s_StopIconContent;

		public static GUIStyle PlayStopButtonStyle {
			get {
				if (s_PlayStopButtonStyle == null) {
					s_PlayStopButtonStyle = new GUIStyle(EditorStyles.miniButton);
					s_PlayStopButtonStyle.padding = new RectOffset();
					s_PlayStopButtonStyle.margin = new RectOffset();
				}
				return s_PlayStopButtonStyle;
			}
		}

		public static GUIContent PlayIconContent => s_PlayIconContent ?? (s_PlayIconContent = new GUIContent(EditorGUIUtility.FindTexture("PlayButton"), "Play the sound"));
		public static GUIContent StopIconContent => s_StopIconContent ?? (new GUIContent(EditorGUIUtility.FindTexture("PreMatQuad"), "Stop the playing sound"));

		// Play AudioClip in the editor (without AudioSource and GameObject).
		public static void PlayPreviewClip(AudioClip clip , int startSample = 0 , bool loop = false) {
			ExecuteAudioUtilMethod("PlayPreviewClip", clip, startSample, loop);
		}

#if UNITY_2023_2_OR_NEWER
		// Play AudioClip in the editor (without AudioSource and GameObject).
		public static void PlayPreviewClip(AudioResource resource , int startSample = 0 , bool loop = false) {
			if (resource is AudioClip clip) {
				PlayPreviewClip(clip, startSample, loop);

			} else {
				// NOTE: This is still very fresh API and may change in the near future.
				// Assume AudioRandomContainer type, which is internal.
				var elementsField = resource.GetType().GetProperty("elements", BindingFlags.NonPublic | BindingFlags.Instance);
				var elements = elementsField?.GetValue(resource) as IList;
				if (elements != null && elements.Count > 0) {
					var element = elements[UnityEngine.Random.Range(0, elements.Count)];
					var elementField = element.GetType().GetProperty("audioClip", BindingFlags.NonPublic | BindingFlags.Instance);
					clip = elementField?.GetValue(element) as AudioClip;
					if (clip != null) {
						PlayPreviewClip(clip, startSample, loop);
					}
				}
			}
		}
#endif

		public static void PausePreviewClip() {
			ExecuteAudioUtilMethod("PausePreviewClip");
		}

		public static void ResumePreviewClip() {
			ExecuteAudioUtilMethod("ResumePreviewClip");
		}

		public static void LoopPreviewClip(bool on) {
			ExecuteAudioUtilMethod("LoopPreviewClip", on);
		}

		public static bool IsPreviewClipPlaying() {
			return (bool) ExecuteAudioUtilMethod("IsPreviewClipPlaying");
		}

		public static void StopAllPreviewClips() {
			ExecuteAudioUtilMethod("StopAllPreviewClips");
		}

		public static float GetPreviewClipPosition() {
			return (float) ExecuteAudioUtilMethod("GetPreviewClipPosition");
		}

		public static int GetPreviewClipSamplePosition() {
			return (int) ExecuteAudioUtilMethod("GetPreviewClipSamplePosition");
		}

		public static void SetPreviewClipSamplePosition(AudioClip clip , int iSamplePosition) {
			ExecuteAudioUtilMethod("SetPreviewClipSamplePosition", clip, iSamplePosition);
		}

		public static int GetSampleCount(AudioClip clip) {
			return (int) ExecuteAudioUtilMethod("GetSampleCount", clip);
		}

		public static int GetChannelCount(AudioClip clip) {
			return (int) ExecuteAudioUtilMethod("GetChannelCount", clip);
		}

		public static int GetBitRate(AudioClip clip) {
			return (int) ExecuteAudioUtilMethod("GetBitRate", clip);
		}

		public static int GetBitsPerSample(AudioClip clip) {
			return (int) ExecuteAudioUtilMethod("GetBitsPerSample", clip);
		}

		public static int GetFrequency(AudioClip clip) {
			return (int) ExecuteAudioUtilMethod("GetFrequency", clip);
		}

		public static int GetSoundSize(AudioClip clip) {
			return (int) ExecuteAudioUtilMethod("GetSoundSize", clip);
		}

		public static AudioCompressionFormat GetSoundCompressionFormat(AudioClip clip) {
			return (AudioCompressionFormat) ExecuteAudioUtilMethod("GetSoundCompressionFormat", clip);
		}

		public static AudioCompressionFormat GetTargetPlatformSoundCompressionFormat(AudioClip clip) {
			return (AudioCompressionFormat) ExecuteAudioUtilMethod("GetTargetPlatformSoundCompressionFormat", clip);
		}

		public static string[] GetAmbisonicDecoderPluginNames() {
			return (string[]) ExecuteAudioUtilMethod("GetAmbisonicDecoderPluginNames");
		}

		public static bool HasPreview(AudioClip clip) {
			return (bool) ExecuteAudioUtilMethod("HasPreview", clip);
		}

		public static AudioImporter GetImporterFromClip(AudioClip clip) {
			return (AudioImporter) ExecuteAudioUtilMethod("GetImporterFromClip", clip);
		}

		public static float[] GetMinMaxData(AudioImporter importer) {
			return (float[]) ExecuteAudioUtilMethod("GetMinMaxData", importer);
		}

		public static double GetDuration(AudioClip clip) {
			return (double) ExecuteAudioUtilMethod("GetDuration", clip);
		}

		private static object ExecuteAudioUtilMethod(string methodName, params object[] args)
		{
			Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
			Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
			Type[] paramTypes = args.Select(a => a.GetType()).ToArray();
			MethodInfo method = audioUtilClass.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public, null, paramTypes, null);

			return method.Invoke(null, args);
		}
	}

}
