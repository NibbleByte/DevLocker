using UnityEngine;

namespace DevLocker.Utils
{
	/// <summary>
	/// Validation routines.
	/// </summary>
	public static class Validation
	{
		/// <summary>
		/// Check if UnityObject has been destroyed and log error if it is.
		/// </summary>
		/// <returns></returns>
		public static bool ValidateMissingObject(Object source, Object objValue, string fieldName = null)
		{
			if (!ReferenceEquals(objValue, null) && objValue == null) {
				if (!string.IsNullOrEmpty(fieldName)) {
					fieldName = $".{fieldName}";
				}
				Debug.LogError($"\"{source.name}\" of {source.GetType().Name}{fieldName} references missing / deleted object.", source);

				return false;
			}

			return true;
		}
	}
}
