using UnityEngine;

namespace DevLocker.Audio
{
	/// <summary>
	/// Helper functions to convert to and from decibels dB.
	/// </summary>
	public static class AudioVolumeUtils
	{
		/// <summary>
		/// Convert normalized 0-1 float value to decibel dB.
		/// https://discussions.unity.com/t/changing-audio-mixer-group-volume-with-ui-slider/567394/12
		///
		/// Useful when dealing with <see cref="UnityEngine.Audio.AudioMixer"/>.
		/// </summary>
		public static float FloatToDecibel(float fvalue) => Mathf.Log10(fvalue > 0.0001f ? fvalue : 0.0001f) * 20;    // 0.0001f is correct!

		/// <summary>
		/// Convert decivel dB to normalized 0-1 float value.
		/// https://discussions.unity.com/t/how-to-convert-decibel-db-number-to-audio-source-volume-number-0to1/46543/4
		/// </summary>
		public static float DecibelToFloat(float dB) => Mathf.Pow(10f, dB / 20f);

		/// <summary>
		/// Returns the current dB value or -80 db if muted.
		/// Useful in options screen.
		/// </summary>
		public static float MuteableFloatToDecibel(bool mute, float fvalue) => mute ? -80f : FloatToDecibel(fvalue);
	}
}
