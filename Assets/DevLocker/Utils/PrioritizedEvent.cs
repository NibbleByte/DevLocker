using System;
using System.Collections.Generic;

namespace DevLocker.Utils
{
	/// <summary>
	/// Event that will call handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEvent : PrioritizedList<Action>
	{
		public void Invoke()
		{
			// Copy the items as they may change while iterating.
			var items = new List<Action>(m_Items.Values);

			foreach(var item in items) {
				item.Invoke();
			}
		}
	}

	/// <summary>
	/// Event that will call handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEvent<T1> : PrioritizedList<Action<T1>>
	{
		public void Invoke(T1 arg1)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Action<T1>>(m_Items.Values);

			foreach(var item in items) {
				item.Invoke(arg1);
			}
		}
	}

	/// <summary>
	/// Event that will call handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEvent<T1, T2> : PrioritizedList<Action<T1, T2>>
	{
		public void Invoke(T1 arg1, T2 arg2)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Action<T1, T2>>(m_Items.Values);

			foreach(var item in items) {
				item.Invoke(arg1, arg2);
			}
		}
	}

	/// <summary>
	/// Event that will call handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEvent<T1, T2, T3> : PrioritizedList<Action<T1, T2, T3>>
	{
		public void Invoke(T1 arg1, T2 arg2, T3 arg3)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Action<T1, T2, T3>>(m_Items.Values);

			foreach(var item in items) {
				item.Invoke(arg1, arg2, arg3);
			}
		}
	}

	/// <summary>
	/// Event that will call handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEvent<T1, T2, T3, T4> : PrioritizedList<Action<T1, T2, T3, T4>>
	{
		public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Action<T1, T2, T3, T4>>(m_Items.Values);

			foreach(var item in items) {
				item.Invoke(arg1, arg2, arg3, arg4);
			}
		}
	}

	/// <summary>
	/// Event that will call handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEvent<T1, T2, T3, T4, T5> : PrioritizedList<Action<T1, T2, T3, T4, T5>>
	{
		public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Action<T1, T2, T3, T4, T5>>(m_Items.Values);

			foreach(var item in items) {
				item.Invoke(arg1, arg2, arg3, arg4, arg5);
			}
		}
	}

	/// <summary>
	/// Event that will call handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEvent<T1, T2, T3, T4, T5, T6> : PrioritizedList<Action<T1, T2, T3, T4, T5, T6>>
	{
		public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Action<T1, T2, T3, T4, T5, T6>>(m_Items.Values);

			foreach(var item in items) {
				item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
			}
		}
	}

	/// <summary>
	/// Event that will call handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEvent<T1, T2, T3, T4, T5, T6, T7> : PrioritizedList<Action<T1, T2, T3, T4, T5, T6, T7>>
	{
		public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Action<T1, T2, T3, T4, T5, T6, T7>>(m_Items.Values);

			foreach(var item in items) {
				item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
			}
		}
	}

	/// <summary>
	/// Event that will call handlers in the order specified by the priorities.
	/// </summary>
	public class PrioritizedEvent<T1, T2, T3, T4, T5, T6, T7, T8> : PrioritizedList<Action<T1, T2, T3, T4, T5, T6, T7, T8>>
	{
		public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			// Copy the items as they may change while iterating.
			var items = new List<Action<T1, T2, T3, T4, T5, T6, T7, T8>>(m_Items.Values);

			foreach(var item in items) {
				item.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
			}
		}
	}


}