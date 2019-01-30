using System;

namespace DUCK.FSM
{
	/// <summary>
	/// Controls a transition from a FromState to a ToState.
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	public abstract class Transition<TState> where TState : IComparable
	{
		public TState FromState { get; private set; }
		public TState ToState { get; private set; }

		private readonly Func<bool> testConditionFunc;

		public event Action OnComplete;

		protected Transition(TState from, TState to, Func<bool> testConditionFunction = null)
		{
			FromState = from;
			ToState = to;
			testConditionFunc = testConditionFunction;
		}

		protected void Complete()
		{
			if (OnComplete != null) OnComplete();
		}

		public abstract void Begin();

		public bool TestCondition()
		{
			return testConditionFunc == null || testConditionFunc();
		}
	}

	public class DefaultStateTransition<TState> : Transition<TState> where TState : IComparable
	{
		public DefaultStateTransition(TState from, TState to, Func<bool> testConditionFunction = null) 
			: base(from, to, testConditionFunction)
		{
		}

		public override void Begin()
		{
			Complete();
		}
	}
}