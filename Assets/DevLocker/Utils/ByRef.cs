namespace DevLocker.Utils
{
	/// <summary>
	/// Use any value type as reference one.
	/// </summary>
	public class ByRef<T> where T : struct
	{
		public T Value;

		public ByRef(T value = default(T))
		{
			Value = value;
		}
	}
}