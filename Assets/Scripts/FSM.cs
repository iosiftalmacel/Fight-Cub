using System;
using System.Collections.Generic;

public struct FSMObject<T, K> 
	where T : class
{
	#region Public types
	public delegate void Function(T self, float time);
	#endregion

	#region Public members
	public T target;
	public float time;
	#endregion

	#region Protected members
	private Dictionary<K, FSMState<T, K>> states;
	private FSMState<T, K> state;
	private FSMState<T, K> prevState;
	#endregion

	#region Public properties
	public K PrevState
	{
		get
		{
			return prevState.key;
		}
	}

	public K State
	{
		get
		{
			return state.key;
		}

		set
		{
			prevState = state;
			//UnityEngine.SBSLog.Info("prevState " + prevState.key + ", " + prevState.onExit + ", " + timeSource);
			if (prevState.onExit != null && time != null)
				prevState.onExit(target, time);

			FSMState<T, K> nextState;
			if (states.TryGetValue(value, out nextState))
			{
				state = nextState;
				//UnityEngine.SBSLog.Info("state " + state.key + ", " + state.onEnter + ", " + timeSource);

				if (state.onEnter != null && time != null)
					state.onEnter(target, time);
			}
			else
			{
				state = nextState;
			}
		}
	}
	#endregion

	#region Public methods
	public void AddState(K key, Function onEnter, Function onExec, Function onExit)
	{
		FSMState<T, K> newState;

		newState.key     = key;
		newState.onEnter = onEnter;
		newState.onExec  = onExec;
		newState.onExit  = onExit;

		if (null == states)
			states = new Dictionary<K, FSMState<T, K>>();

		states.Add(key, newState);
	}

	public void RemoveState(K key)
	{
		if (null == states)
			return;

		states.Remove(key);
	}

	public void Update(float time)
	{
		if (null == state.onExec || null == time)
			return;
		this.time = time;
		state.onExec(target, time);
	}
	#endregion
}

public struct FSMState<T, K>
	where T : class
{
	public K key;
	public FSMObject<T, K>.Function onEnter;
	public FSMObject<T, K>.Function onExec;
	public FSMObject<T, K>.Function onExit;
}

