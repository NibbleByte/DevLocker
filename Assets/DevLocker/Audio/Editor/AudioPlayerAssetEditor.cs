using DevLocker.Utils;
using UnityEditor;

namespace DevLocker.Audio.Editor
{
	[CustomPropertyDrawer(typeof(AudioPlayerAsset.AudioPredicate))]
	public class AudioPredicateDrawer : SerializeReferenceCreatorDrawer<AudioPlayerAsset.AudioPredicate>
	{
	}

	[CustomPropertyDrawer(typeof(AudioPlayerAsset.AudioConductor))]
	public class AudioConductorDrawer : SerializeReferenceCreatorDrawer<AudioPlayerAsset.AudioConductor>
	{
	}
}
