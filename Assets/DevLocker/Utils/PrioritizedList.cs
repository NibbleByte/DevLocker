using System.Collections;
using System.Collections.Generic;

namespace DevLocker.Utils
{
	/// <summary>
	/// List with items sorted by priorities.
	/// Can have multiple items with the same priority.
	/// Lower priority is first. Priority can be negative.
	/// </summary>
	public class PrioritizedList<T> : IEnumerable<T>, IEnumerable
	{
		private class DuplicatePriorityComparer<TKey> : IComparer<int>
		{
			public int Compare(int x, int y)
			{
				int result = x.CompareTo(y);

				// Duplicate keys are not supported by SortedList, so just lie.
				if (result == 0)
					return 1;

				return result;
			}
		}

		protected readonly SortedList<int, T> m_Items = new SortedList<int, T>(new DuplicatePriorityComparer<int>());

		public int Count => m_Items.Count;
		public int Capacity {
			get => m_Items.Capacity;
			set => m_Items.Capacity = value;
		}

		public T this[int i] => m_Items.Values[i];

		/// <summary>
		/// Insert item to the list based on the specified priority.
		/// Can have multiple items with the same priority.
		/// Lower priority is first. Priority can be negative.
		/// </summary>
		public void Add(T item, int priority) => m_Items.Add(priority, item);

		/// <summary>
		/// Insert items to the list based on specified priority.
		/// Can have multiple items with the same priority.
		/// Lower priority is first. Priority can be negative.
		/// </summary>
		public void AddRange(IEnumerable<T> items, int priority)
		{
			foreach (var item in items) {
				m_Items.Add(priority, item);
			}
		}

		public void AddRange(PrioritizedList<T> prioritizedEvent)
		{
			foreach (var pair in prioritizedEvent.m_Items) {
				m_Items.Add(pair.Key, pair.Value);
			}
		}

		/// <summary>
		/// Insert item to the list based on specified priority.
		/// Remove previous occurrences of the item before that.
		/// Can have multiple items with the same priority.
		/// Lower priority is first. Priority can be negative.
		/// </summary>
		public void AddOrReplace(T item, int priority)
		{
			Remove(item);
			Add(item, priority);
		}

		public void AddOrReplaceRange(PrioritizedList<T> prioritizedEvent)
		{
			foreach (var pair in prioritizedEvent.m_Items) {
				AddOrReplace(pair.Value, pair.Key);
			}
		}

		public bool Remove(T item)
		{
			int index = m_Items.IndexOfValue(item);

			if (index >= 0) {
				m_Items.RemoveAt(index);
			}

			return index >= 0;
		}

		public bool Contains(T item) => m_Items.Values.Contains(item);

		public void Clear() => m_Items.Clear();

		public IEnumerator<T> GetEnumerator() => m_Items.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => m_Items.GetEnumerator();
	}
}