using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

namespace DevLocker.StatesManagement
{

	public class StateEventArgs<TState> where TState : struct, IComparable {

		public readonly TState PrevState;
		public readonly object PrevParam;

		public readonly TState NextState;
		public readonly object NextParam;

		public readonly StackAction StackAction;

		public IReadOnlyCollection<object> TransitionsPending => m_TransitionsPending;

		public bool IsValid => m_StateManager != null;
		public static readonly StateEventArgs<TState> Invalid = new StateEventArgs<TState>(default, null, default, null, StackAction.ClearAndPush, null);

		private List<object> m_TransitionsPending = new List<object>();

		private StateManagerBase<TState> m_StateManager;

		/// <summary>
		/// Called by subscribers indicating that they want to do some transition that may take time.
		/// The StateManager will wait for all transitions to finish.
		/// NOTE: Transitions are allowed to be added only during TransitionStarts event!
		/// </summary>
		public void AddTransition(object transitionExecutor)
		{
			if (m_StateManager == null) {
				UnityEngine.Debug.LogError($"Trying to start a transition \"{transitionExecutor}\", with invalid arguments. Transitions can be started only during TransitionStarts events with the provided arguments.");
			}

			if (m_StateManager.CurrentEventArgs != this) {
				UnityEngine.Debug.LogError($"Trying to start a transition \"{transitionExecutor}\", while state finished changing. Transitions can be started only during TransitionStarts events.");
				return;
			}

			m_TransitionsPending.Add(transitionExecutor);
		}

		/// <summary>
		/// Called by subscribers indicating that they finished their transition.
		/// Once all transitions have finished, the state will be changed.
		/// </summary>
		public void RemoveTransition(object transitionExecutor)
		{
			bool success = m_TransitionsPending.Remove(transitionExecutor);
			if (!success) {
				UnityEngine.Debug.LogError($"Trying to remove unknown transition executor \"{transitionExecutor}\". Transitions left {m_TransitionsPending.Count}.");
				return;
			}

			if (m_TransitionsPending.Count == 0) {
				m_StateManager.OnTransitionFinished();
			}
		}

		#region Transition Scopes

		/// <summary>
		/// Use this with "using" scope to safely initiate idle timeouts block and releasing it.
		/// </summary>
		public class TransitionScope : IDisposable
		{
			private StateEventArgs<TState> m_TransitionArgs;
			private object m_Source;

			public TransitionScope(StateEventArgs<TState> transitionArgs, object source)
			{
				m_TransitionArgs = transitionArgs;
				m_Source = source;

				m_TransitionArgs.AddTransition(source);
			}

			public void Dispose()
			{
				m_TransitionArgs.RemoveTransition(m_Source);
			}
		}

		/// <summary>
		/// Check BlockIdleTimeoutsFor() for more info.
		/// </summary>
		public TransitionScope GetTransitionScope(object source)
		{
			return new TransitionScope(this, source);
		}

		#endregion

		#region Wait For Transitions API

		public IEnumerator WaitForTransitions<T1>(object excludeTransition = null) => WaitForTransitions(excludeTransition, typeof(T1));
		public IEnumerator WaitForTransitions<T1, T2>(object excludeTransition = null) => WaitForTransitions(excludeTransition, typeof(T1), typeof(T2));
		public IEnumerator WaitForTransitions<T1, T2, T3>(object excludeTransition = null) => WaitForTransitions(excludeTransition, typeof(T1), typeof(T2), typeof(T3));
		public IEnumerator WaitForTransitions<T1, T2, T3, T4>(object excludeTransition = null) => WaitForTransitions(excludeTransition, typeof(T1), typeof(T2), typeof(T3), typeof(T4));

		public IEnumerator WaitForTransitions(object excludeTransition, params Type[] types)
		{
			if (types.Length == 0)
				yield break;

			// Wait a frame for all transitions to kick-in so we can check if we should wait for some of them (order of execution).
			yield return null;

			while (HasTransitions(excludeTransition, types))
				yield return null;
		}


		public bool HasTransitions<T1>(object excludeTransition = null) => HasTransitions(excludeTransition, typeof(T1));
		public bool HasTransitions<T1, T2>(object excludeTransition = null) => HasTransitions(excludeTransition, typeof(T1), typeof(T2));
		public bool HasTransitions<T1, T2, T3>(object excludeTransition = null) => HasTransitions(excludeTransition, typeof(T1), typeof(T2), typeof(T3));
		public bool HasTransitions<T1, T2, T3, T4>(object excludeTransition = null) => HasTransitions(excludeTransition, typeof(T1), typeof(T2), typeof(T3), typeof(T4));

		public bool HasTransitions(object excludeTransition, params Type[] types)
		{
			if (excludeTransition is Type[])
				throw new ArgumentException("Exclude transition argument missing!");

			for (int i = 0; i < m_TransitionsPending.Count; ++i) {
				var obj = m_TransitionsPending[i];

				// Useful for excluding the requesting transition itself.
				if (obj == excludeTransition)
					continue;

				for (int j = 0; j < types.Length; ++j) {
					if (types[j].IsAssignableFrom(obj.GetType()))
						return true;
				}
			}

			return false;
		}

		#endregion


		internal StateEventArgs(TState prevState, object prevParam, TState nextState, object nextParam, StackAction stackAction, StateManagerBase<TState> stateManager)
		{
			PrevState = prevState;
			PrevParam = prevParam;
			NextState = nextState;
			NextParam = nextParam;
			StackAction = stackAction;

			m_StateManager = stateManager;
		}

	}

	public enum StackAction {
		ClearAndPush,
		Push,
		ReplaceTop,
	}


	public class StateManagerBase<TState> where TState : struct, IComparable
	{
		// NOTE: These might not be the same as the top of the stack (Pop & Reenter tricks).
		public TState CurrentState { get; private set; }
		public object CurrentStateParam { get; private set; }

		public int StackedStatesCount => m_StackedStates.Count;

		public StateEventArgs<TState> CurrentEventArgs { get; private set; }
		public StateEventArgs<TState> LastEventArgs { get; private set; }
		public StateEventArgs<TState> CurrentOrLastEventArgs => CurrentEventArgs ?? LastEventArgs;
		public bool IsInTransition => CurrentEventArgs != null;


		public delegate void StateChangeEventHandler(StateEventArgs<TState> e);
		public event StateChangeEventHandler TransitionStarts;		// State is about to change, right before transition starts.
		public event StateChangeEventHandler StateChanged;			// State change and all transitions finished.

		// Easy-on subscription for specific event.
		public void SubscribeTransitionStarts(TState state, Action handler) =>	m_TransitionStartsSubscribers[state] += handler;
		public void UnsubscribeTransitionStarts(TState state, Action handler) => m_TransitionStartsSubscribers[state] -= handler;
		public void SubscribeStateChanged(TState state, Action handler) =>	m_StateChangedSubscribers[state] += handler;
		public void UnsubscribeStateChanged(TState state, Action handler) => m_StateChangedSubscribers[state] -= handler;

		private Stack<StateEventArgs<TState>> m_StackedStates = new Stack<StateEventArgs<TState>>();

		private Dictionary<TState, Action> m_TransitionStartsSubscribers = new Dictionary<TState, Action>();
		private Dictionary<TState, Action> m_StateChangedSubscribers = new Dictionary<TState, Action>();


		// Used when state is changed inside another state change event.
		private bool m_ChangingStates = false;
		private Queue<StateEventArgs<TState>> m_PendingStateChanges = new Queue<StateEventArgs<TState>>();

		private bool m_ExecutingTransitionEffect = false;


		public StateManagerBase() {
			foreach (TState value in Enum.GetValues(typeof(TState))) {
				m_TransitionStartsSubscribers[value] = null;
				m_StateChangedSubscribers[value] = null;
			}

			LastEventArgs = StateEventArgs<TState>.Invalid;
		}

		public virtual void Dispose()
		{
			foreach (TState value in Enum.GetValues(typeof(TState))) {
				m_TransitionStartsSubscribers[value] = null;
				m_StateChangedSubscribers[value] = null;
			}

			CurrentState = default(TState);
			CurrentStateParam = null;

			m_StackedStates.Clear();

			m_ChangingStates = false;
			m_PendingStateChanges.Clear();

			LastEventArgs = null;
			CurrentEventArgs = null;

			TransitionStarts = null;
			StateChanged = null;
		}

		/// <summary>
		/// Push state to the top of the state stack. Can pop it out to the previous state later on.
		/// </summary>
		public void PushState(TState state, object param = null) {
			ChangeState(state, param, StackAction.Push);
		}

		/// <summary>
		/// Clears the state stack of any other states and pushes the provided one.
		/// </summary>
		public void SetState(TState state, object param = null) {
			ChangeState(state, param, StackAction.ClearAndPush);
		}

		/// <summary>
		/// Pop a single state from the state stack.
		/// </summary>
		public void PopState() {
			PopStates(1);
		}

		/// <summary>
		/// Pops multiple states from the state stack.
		/// </summary>
		public void PopStates(int count) {

			// TODO: Should we support empty stack?

			count = Math.Max(1, count);

			if (StackedStatesCount < count) {
				UnityEngine.Debug.LogError("Trying to pop states while there aren't any stacked ones.");
				return;
			}

			if (IsInTransition) {
				UnityEngine.Debug.LogError("Popping out states while in transition is not supported!");
				return;
			}

			for(int i = 0; i < count; ++i) {
				m_StackedStates.Pop();
			}

			ReenterCurrentState();
		}

		/// <summary>
		/// Pop and push back the state at the top. Will trigger changing state events.
		/// </summary>
		public void ReenterCurrentState()
		{
			if (IsInTransition) {
				UnityEngine.Debug.LogError("Reentering state while in transition is not supported!");
				return;
			}

			// Re-insert the top state to trigger changing events.
			var stateArgs = m_StackedStates.Pop();
			ChangeState(stateArgs.NextState, stateArgs.NextParam, StackAction.Push);
		}

		/// <summary>
		/// This will clear the state stack and the current state by setting it to the enum default value.
		/// It will still trigger the changing state events. Enum default value better be "None" or similar not representing real state.
		/// Basically resets the StateManager to its initial state. Use with caution!!!
		/// </summary>
		public void ClearStackAndState()
		{
			if (IsInTransition) {
				UnityEngine.Debug.LogError("Reentering state while in transition is not supported!");
				return;
			}

			ChangeState(default(TState), null, StackAction.Push);
			m_StackedStates.Clear();
		}

		/// <summary>
		/// Change the current state of the state manager. Add it to the state stack.
		/// Will trigger state changing events and transitions.
		/// Any additional state changes that happened in the meantime will be queued and executed after the current change finishes.
		/// </summary>
		public void ChangeState(TState state, object param, StackAction stackAction) {

			// Already in this state, do nothing.
			// Only if change is the same.
			if (CurrentState.CompareTo(state) == 0 && Equals(CurrentStateParam, param)) {
				return;
			}

			// Sanity check.
			if (m_StackedStates.Count > 7 && stackAction == StackAction.Push) {
				UnityEngine.Debug.LogWarning($"You're stacking too many states down. Are you sure? Stacked state: {state}.");
			}

			// TODO: Validate State Event. Give chance users to reject the next state.

			if (m_ChangingStates) {
				m_PendingStateChanges.Enqueue(new StateEventArgs<TState>(default(TState), null, state, param, stackAction, this));
				return;

			} else {
				m_ChangingStates = true;
			}


			TState prevState = CurrentState;
			object prevStateParam = CurrentStateParam;

			// Can access while inside event handler.
			CurrentEventArgs = new StateEventArgs<TState>(prevState, prevStateParam, state, param, stackAction, this);

			// HACK: User may start and finish transition while on this call. Prevent him from finishing the transition, I'll do it here after that.
			m_ExecutingTransitionEffect = true;

			TransitionStarts?.Invoke(CurrentEventArgs);
			m_TransitionStartsSubscribers[state]?.Invoke();

			m_ExecutingTransitionEffect = false;

			if (CurrentEventArgs.TransitionsPending.Count == 0) {
				OnTransitionFinished();
			}
		}

		internal void OnTransitionFinished()
		{
			if (m_ExecutingTransitionEffect)
				return;

			if (CurrentEventArgs == null) {
				UnityEngine.Debug.LogError($"No transition in progress! Abort changing event!");
				return;
			}

			if (CurrentEventArgs.StackAction == StackAction.ClearAndPush) {
				m_StackedStates.Clear();
			}

			CurrentState = CurrentEventArgs.NextState;
			CurrentStateParam = CurrentEventArgs.NextParam;

			if (CurrentEventArgs.StackAction == StackAction.ReplaceTop && m_StackedStates.Count > 0) {
				m_StackedStates.Pop();
			}

			m_StackedStates.Push(CurrentEventArgs);

			StateChanged?.Invoke(CurrentEventArgs);
			m_StateChangedSubscribers[CurrentEventArgs.NextState]?.Invoke();

			if (CurrentEventArgs.TransitionsPending.Count > 0) {
				UnityEngine.Debug.LogError($"Transitions are not allowed at StateChanged events. Only at TransitionStarts events! Pending {CurrentEventArgs.TransitionsPending.Count} transitions will NOT be executed!");
			}

			LastEventArgs = CurrentEventArgs;
			CurrentEventArgs = null;

			m_ChangingStates = false;

			// Execute the pending states...
			if (m_PendingStateChanges.Count > 0) {
				var stateArgs = m_PendingStateChanges.Dequeue();

				ChangeState(stateArgs.NextState, stateArgs.NextParam, stateArgs.StackAction);
			}
		}
	}

}