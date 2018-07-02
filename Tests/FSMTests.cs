using NUnit.Framework;

namespace DUCK.FSM.Editor.Tests
{
	[TestFixture]
	public class FSMTests
	{
		private const string LOAD_COMMAND = "load";
		private const string CANCEL_COMMAND = "cancel";
		private const string ERROR_COMMAND = "error";
		private const string COMPLETE_COMMAND = "complete";
		private const string RETRY_COMMAND = "retry";

		private enum States
		{
			Idle,
			Loading,
			Valid,
			Invalid
		}

		private bool cooldown;
		private FiniteStateMachine<States> fsm;

		[SetUp]
		public void SetUp()
		{
			cooldown = false;
			fsm = FiniteStateMachine<States>.FromEnum()
				.AddTransition(States.Idle, States.Loading, LOAD_COMMAND)
				.AddTransition(States.Loading, States.Idle, CANCEL_COMMAND)
				.AddTransition(States.Loading, States.Invalid, ERROR_COMMAND)
				.AddTransition(States.Loading, States.Valid, COMPLETE_COMMAND)
				.AddTransition(States.Invalid, States.Loading, RETRY_COMMAND, () => !cooldown);
		}

		[TearDown]
		public void TearDown()
		{
			fsm = null;
		}

		[Test]
		public void ExpectValidCommandToInvokeTransition()
		{
			fsm.Begin(States.Idle);
			fsm.IssueCommand(LOAD_COMMAND);
			Assert.AreEqual(fsm.CurrentState, States.Loading);
		}

		[Test]
		public void ExpectInValidCommandToDoNothing()
		{
			fsm.Begin(States.Idle);
			fsm.IssueCommand(ERROR_COMMAND);
			Assert.AreEqual(fsm.CurrentState, States.Idle);
		}

		[Test]
		public void ExpectValidToInvokeTransitionIfConditionIsMet()
		{
			cooldown = true;
			fsm.Begin(States.Invalid);
			fsm.IssueCommand(RETRY_COMMAND);
			Assert.AreEqual(fsm.CurrentState, States.Invalid);
		}

		[Test]
		public void ExpectValidToDoNothingIfConditionIsNotMetCommand()
		{
			cooldown = false;
			fsm.Begin(States.Invalid);
			fsm.IssueCommand(RETRY_COMMAND);
			Assert.AreEqual(fsm.CurrentState, States.Loading);
		}

		[Test]
		public void ExpectOnEnterHandlerToBeCalled()
		{
			var handlerWasCalled = false;

			fsm.OnEnter(States.Loading, () => handlerWasCalled = true);

			fsm.Begin(States.Idle);

			fsm.IssueCommand(LOAD_COMMAND);

			Assert.IsTrue(handlerWasCalled);
		}

		[Test]
		public void ExpectOnExitHandlerToBeCalled()
		{
			var handlerWasCalled = false;

			fsm.OnExit(States.Idle, () => handlerWasCalled = true);

			fsm.Begin(States.Idle);

			fsm.IssueCommand(LOAD_COMMAND);

			Assert.IsTrue(handlerWasCalled);
		}

		[Test]
		public void ExpectOnChangeHandlerToBeCalledWithCorrectResults()
		{
			var handlerWasCalled = false;
			object fromStateResult = null;
			object toStateResult = null;

			fsm.OnChange((fromState, toState) =>
			{
				handlerWasCalled = true;
				fromStateResult = fromState;
				toStateResult = toState;
			});

			fsm.Begin(States.Idle);

			fsm.IssueCommand(LOAD_COMMAND);

			Assert.IsTrue(handlerWasCalled);
			Assert.AreEqual(fromStateResult, States.Idle);
			Assert.AreEqual(toStateResult, States.Loading);
		}

		[Test]
		public void ExpectSpecificOnChangeHandlerToBeCalledWithCorrectResults()
		{
			var handlerWasCalled = false;

			fsm.OnChange(States.Idle, States.Loading, () =>
			{
				handlerWasCalled = true;
			});

			fsm.Begin(States.Idle);

			fsm.IssueCommand(LOAD_COMMAND);

			Assert.IsTrue(handlerWasCalled);
		}
	}
}