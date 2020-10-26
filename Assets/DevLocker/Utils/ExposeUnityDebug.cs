using UnityEngine;

namespace DevLocker.Utils
{
	/// <summary>
	/// Exposes Unity Debug methods so they can be called by UnityEvent.
	/// </summary>
	public class ExposeUnityDebug : MonoBehaviour
	{
		public void Log(string message)
		{
			Debug.Log(message, this);
		}

		public void LogWarning(string message)
		{
			Debug.LogWarning(message, this);
		}

		public void LogError(string message)
		{
			Debug.LogError(message, this);
		}

		public void Break()
		{
			Debug.Break();
		}

		public void ClearDeveloperConsole()
		{
			Debug.ClearDeveloperConsole();
		}
	}
}