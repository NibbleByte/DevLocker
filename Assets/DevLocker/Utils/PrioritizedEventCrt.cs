using System;
using System.Collections;
using System.Collections.Generic;

namespace DevLocker.Utils
{
	/// <summary>
	/// Event that will yield handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEventCrt : PrioritizedList<Func<IEnumerator>>
	{
		public IEnumerator Invoke()
		{
			// Copy the items as they may change while iterating.
			var items = new List<Func<IEnumerator>>(m_Items.Values);

			foreach (var item in items) {
				yield return item.Invoke();
			}
		}
	}

	/// <summary>
	/// Event that will yield handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEventCrt<T1> : PrioritizedList<Func<T1, IEnumerator>>
	{
		public IEnumerator Invoke(T1 arg1)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Func<T1, IEnumerator>>(m_Items.Values);

			foreach (var item in items) {
				yield return item.Invoke(arg1);
			}
		}
	}

	/// <summary>
	/// Event that will yield handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEventCrt<T1, T2> : PrioritizedList<Func<T1, T2, IEnumerator>>
	{
		public IEnumerator Invoke(T1 arg1, T2 arg2)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Func<T1, T2, IEnumerator>>(m_Items.Values);

			foreach (var item in items) {
				yield return item.Invoke(arg1, arg2);
			}
		}
	}

	/// <summary>
	/// Event that will yield handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEventCrt<T1, T2, T3> : PrioritizedList<Func<T1, T2, T3, IEnumerator>>
	{
		public IEnumerator Invoke(T1 arg1, T2 arg2, T3 arg3)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Func<T1, T2, T3, IEnumerator>>(m_Items.Values);

			foreach (var item in items) {
				yield return item.Invoke(arg1, arg2, arg3);
			}
		}
	}

	/// <summary>
	/// Event that will yield handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEventCrt<T1, T2, T3, T4> : PrioritizedList<Func<T1, T2, T3, T4, IEnumerator>>
	{
		public IEnumerator Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Func<T1, T2, T3, T4, IEnumerator>>(m_Items.Values);

			foreach (var item in items) {
				yield return item.Invoke(arg1, arg2, arg3, arg4);
			}
		}
	}

	/// <summary>
	/// Event that will yield handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEventCrt<T1, T2, T3, T4, T5> : PrioritizedList<Func<T1, T2, T3, T4, T5, IEnumerator>>
	{
		public IEnumerator Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Func<T1, T2, T3, T4, T5, IEnumerator>>(m_Items.Values);

			foreach (var item in items) {
				yield return item.Invoke(arg1, arg2, arg3, arg4, arg5);
			}
		}
	}

	/// <summary>
	/// Event that will yield handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEventCrt<T1, T2, T3, T4, T5, T6> : PrioritizedList<Func<T1, T2, T3, T4, T5, T6, IEnumerator>>
	{
		public IEnumerator Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Func<T1, T2, T3, T4, T5, T6, IEnumerator>>(m_Items.Values);

			foreach (var item in items) {
				yield return item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
			}
		}
	}

	/// <summary>
	/// Event that will yield handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEventCrt<T1, T2, T3, T4, T5, T6, T7> : PrioritizedList<Func<T1, T2, T3, T4, T5, T6, T7, IEnumerator>>
	{
		public IEnumerator Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Func<T1, T2, T3, T4, T5, T6, T7, IEnumerator>>(m_Items.Values);

			foreach (var item in items) {
				yield return item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			}
		}
	}

	/// <summary>
	/// Event that will yield handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEventCrt<T1, T2, T3, T4, T5, T6, T7, T8> : PrioritizedList<Func<T1, T2, T3, T4, T5, T6, T7, T8, IEnumerator>>
	{
		public IEnumerator Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Func<T1, T2, T3, T4, T5, T6, T7, T8, IEnumerator>>(m_Items.Values);

			foreach (var item in items) {
				yield return item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			}
		}
	}

}