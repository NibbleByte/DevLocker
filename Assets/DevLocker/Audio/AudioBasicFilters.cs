using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DevLocker.Audio.Conductors
{
	#region Timing Predicates

	[Serializable]
	public class CooldownFilter : AudioPlayerAsset.AudioPredicate
	{
		[Tooltip("Minimum time interval (in seconds) from last play that we can play again.")]
		public float CooldownSeconds = 0.1f;

		public override bool IsAllowed(object context, AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			if (Time.time - player.LastPlayTime > CooldownSeconds) {
				return true;
			}

			return false;
		}
	}

	#endregion

	#region Comparing Predicades

	public enum ComparisonType
	{
		Equal,
		NotEqual,
		Less,
		LessOrEqual,
		Greater,
		GreaterOrEqual,
	}

	[Serializable]
	public class CompareInt : AudioPlayerAsset.AudioPredicate
	{
		[Tooltip("Key name to get the value from the context")]
		public string KeyName;
		public ComparisonType Comparison;
		[Tooltip("The right-hand side value")]
		public int Value;

		public override bool IsAllowed(object context, AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			var values = context as IValuesContainer;
			if (values == null) {
				Debug.LogError($"Context {context} is not of expected {nameof(IValuesContainer)}", context as UnityEngine.Object);
				return false;
			}

			if (values.TryGetInt(KeyName, out int value)) {

				return Comparison switch {
					ComparisonType.Equal => value == Value,
					ComparisonType.NotEqual => value != Value,
					ComparisonType.Less => value < Value,
					ComparisonType.LessOrEqual => value <= Value,
					ComparisonType.Greater => value > Value,
					ComparisonType.GreaterOrEqual => value >= Value,
					_ => throw new NotImplementedException(),
				};
			}

			Debug.LogWarning($"Context {context} has value \"{KeyName}\" with unexpected type.", context as UnityEngine.Object);
			return false;
		}
	}

	[Serializable]
	public class CompareFloat : AudioPlayerAsset.AudioPredicate
	{
		[Tooltip("Key name to get the value from the context")]
		public string KeyName;
		public ComparisonType Comparison;
		[Tooltip("The right-hand side value")]
		public float Value;

		public override bool IsAllowed(object context, AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			var values = context as IValuesContainer;
			if (values == null) {
				Debug.LogError($"Context {context} is not of expected {nameof(IValuesContainer)}", context as UnityEngine.Object);
				return false;
			}

			if (values.TryGetFloat(KeyName, out float value)) {

				return Comparison switch {
					ComparisonType.Equal => Mathf.Approximately(value, Value),
					ComparisonType.NotEqual => !Mathf.Approximately(value, Value),
					ComparisonType.Less => value < Value,
					ComparisonType.LessOrEqual => value < Value || Mathf.Approximately(value, Value),
					ComparisonType.Greater => value > Value,
					ComparisonType.GreaterOrEqual => value > Value || Mathf.Approximately(value, Value),
					_ => throw new NotImplementedException(),
				};
			}

			Debug.LogWarning($"Context {context} has value \"{KeyName}\" with unexpected type.", context as UnityEngine.Object);
			return false;
		}
	}

	[Serializable]
	public class CompareBool : AudioPlayerAsset.AudioPredicate
	{
		[Tooltip("Key name to get the value from the context")]
		public string KeyName;
		[Tooltip("Check if the boolean context value equals this one")]
		public bool Value;

		public override bool IsAllowed(object context, AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			var values = context as IValuesContainer;
			if (values == null) {
				Debug.LogError($"Context {context} is not of expected {nameof(IValuesContainer)}", context as UnityEngine.Object);
				return false;
			}

			if (values.TryGetBool(KeyName, out bool value)) {
				return value == Value;
			}

			Debug.LogWarning($"Context {context} has value \"{KeyName}\" with unexpected type.", context as UnityEngine.Object);
			return false;
		}
	}

	[Serializable]
	public class CompareString : AudioPlayerAsset.AudioPredicate
	{
		public enum StringCompareType
		{
			ExactMatch,
			Contains,
			ContainsAnyWord,
			ContainsAllWords,
		}

		[Tooltip("Key name to get the value from the context")]
		public string KeyName;
		public StringCompareType Comparison;
		public bool CaseSensitive = false;
		[Tooltip("Check if the boolean context value equals this one")]
		public string Value;

		[Tooltip("Allow only if result is false, i.e. negative comparison.")]
		public bool NegateResult;


		public override bool IsAllowed(object context, AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			var values = context as IValuesContainer;
			if (values == null) {
				Debug.LogError($"Context {context} is not of expected {nameof(IValuesContainer)}", context as UnityEngine.Object);
				return false;
			}

			if (values.TryGetString(KeyName, out string value)) {
				if (value == null)
					return false;

				var stringComparison = CaseSensitive ? System.StringComparison.Ordinal : System.StringComparison.OrdinalIgnoreCase;

				bool result = Comparison switch {
					StringCompareType.ExactMatch => value.Equals(Value, stringComparison),
					StringCompareType.Contains => value.Contains(Value, stringComparison),
					StringCompareType.ContainsAnyWord => Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(word => value.Contains(word, stringComparison)),
					StringCompareType.ContainsAllWords => Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).All(word => value.Contains(word, stringComparison)),
					_ => throw new NotImplementedException(),
				};

				return NegateResult ? !result : result;
			}

			Debug.LogWarning($"Context {context} has value \"{KeyName}\" with unexpected type.", context as UnityEngine.Object);
			return false;
		}
	}

	[Serializable]
	public class CompareAssetReferences : AudioPlayerAsset.AudioPredicate
	{
		[Tooltip("Key name to get the value from the context")]
		public string KeyName;
		[Tooltip("Check if the reference context value is any one from the list")]
		public List<UnityEngine.Object> Reference;

		[Tooltip("Allow only if result is false, i.e. negative comparison.")]
		public bool NegateResult;

		public override bool IsAllowed(object context, AudioSourcePlayer player, AudioPlayerAsset asset)
		{
			var values = context as IValuesContainer;
			if (values == null) {
				Debug.LogError($"Context {context} is not of expected {nameof(IValuesContainer)}", context as UnityEngine.Object);
				return false;
			}

			if (values.TryGetValue(KeyName, out object objValue) && objValue is UnityEngine.Object reference) {
				bool result = Reference.Contains(reference);
				return NegateResult ? !result : result;
			}

			Debug.LogWarning($"Context {context} has value \"{KeyName}\" with unexpected type.", context as UnityEngine.Object);
			return false;
		}
	}

	#endregion

	#region Logical Predicates

	[Serializable]
	public class Logical_OR : AudioPlayerAsset.AudioPredicate
	{
		[SerializeReference]
		public List<AudioPlayerAsset.AudioPredicate> OrFilters;

		public override bool IsAllowed(object context, AudioSourcePlayer player, AudioPlayerAsset asset) => OrFilters.Any(filter => filter.IsAllowed(context, player, asset));
	}

	[Serializable]
	public class Logical_AND : AudioPlayerAsset.AudioPredicate
	{
		[SerializeReference]
		public List<AudioPlayerAsset.AudioPredicate> AndFilters;

		public override bool IsAllowed(object context, AudioSourcePlayer player, AudioPlayerAsset asset) => AndFilters.All(filter => filter.IsAllowed(context, player, asset));
	}

	[Serializable]
	public class Logical_NOT : AudioPlayerAsset.AudioPredicate
	{
		[SerializeReference]
		public AudioPlayerAsset.AudioPredicate NotFilter;

		public override bool IsAllowed(object context, AudioSourcePlayer player, AudioPlayerAsset asset) => !NotFilter.IsAllowed(context, player, asset);
	}

	#endregion

	#region Basic Context

	/// <summary>
	/// Your audio context can implement this to be compatible with some of the basic filters.
	/// </summary>
	public interface IValuesContainer
	{
		bool TryGetValue(string key, out object value);
	}

	/// <summary>
	/// Use this as your audio context so it is compatible with some of the basic filters.
	/// </summary>
	public class DictionaryContext : Dictionary<string, object>, IValuesContainer
	{
		#region Helpers

		public static DictionaryContext Create(string key1, object value1) => new DictionaryContext {
			{ key1, value1 }
		};

		public static DictionaryContext Create(string key1, object value1, string key2, object value2) => new DictionaryContext {
			{ key1, value1 },
			{ key2, value2 },
		};

		public static DictionaryContext Create(string key1, object value1, string key2, object value2, string key3, object value3) => new DictionaryContext {
			{ key1, value1 },
			{ key2, value2 },
			{ key3, value3 },
		};

		public static DictionaryContext Create(string key1, object value1, string key2, object value2, string key3, object value3, string key4, object value4) => new DictionaryContext {
			{ key1, value1 },
			{ key2, value2 },
			{ key3, value3 },
			{ key4, value4 },
		};

		public static DictionaryContext Create(string key1, object value1, string key2, object value2, string key3, object value3, string key4, object value4, string key5, object value5) => new DictionaryContext {
			{ key1, value1 },
			{ key2, value2 },
			{ key3, value3 },
			{ key4, value4 },
			{ key5, value5 },
		};

		public static DictionaryContext Create(string key1, object value1, string key2, object value2, string key3, object value3, string key4, object value4, string key5, object value5, string key6, object value6) => new DictionaryContext {
			{ key1, value1 },
			{ key2, value2 },
			{ key3, value3 },
			{ key4, value4 },
			{ key5, value5 },
			{ key6, value6 },
		};

		#endregion

	}

	public static class ValuesContainerExtensions
	{
		public static bool TryGetFloat(this IValuesContainer values, string key, out float value)
		{
			if (values.TryGetValue(key, out object objValue)) {
				value = Convert.ToSingle(objValue);
				return true;
			} else {
				value = 0f;
				return false;
			}
		}

		public static bool TryGetInt(this IValuesContainer values, string key, out int value)
		{
			if (values.TryGetValue(key, out object objValue)) {
				value = Convert.ToInt32(objValue);
				return true;
			} else {
				value = 0;
				return false;
			}
		}

		public static bool TryGetDouble(this IValuesContainer values, string key, out double value)
		{
			if (values.TryGetValue(key, out object objValue)) {
				value = Convert.ToDouble(objValue);
				return true;
			} else {
				value = 0;
				return false;
			}
		}

		public static bool TryGetString(this IValuesContainer values, string key, out string value)
		{
			if (values.TryGetValue(key, out object objValue)) {
				value = (string) objValue;
				return true;
			} else {
				value = "";
				return false;
			}
		}

		public static bool TryGetBool(this IValuesContainer values, string key, out bool value)
		{
			if (values.TryGetValue(key, out object objValue)) {
				value = Convert.ToBoolean(objValue);
				return true;
			} else {
				value = false;
				return false;
			}
		}
	}

	#endregion
}
