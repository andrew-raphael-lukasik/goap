using UnityEngine;
using System.Collections.Generic;

public sealed class GoapAgent : MonoBehaviour
{

	FSM _stateMachine;

	FSM.FSMState _idleState;// finds something to do
	FSM.FSMState _moveToState;// moves to a target
	FSM.FSMState _performActionState;// performs an action
	HashSet<GoapAction> _availableActions;
	Queue<GoapAction> _currentActions;
	IGoap _dataProvider;// this is the implementing class that provides our world data and listens to feedback on planning
	GoapPlanner _planner;

	void Start ()
	{
		_stateMachine = new FSM();
		_availableActions = new HashSet<GoapAction>();
		_currentActions = new Queue<GoapAction>();
		_planner = new GoapPlanner();
		FindDataProvider();
		CreateIdleState();
		CreateMoveToState();
		CreatePerformActionState();
		_stateMachine.PushState( _idleState );
		LoadActions();
	}
	
	void Update () => _stateMachine.Update( this.gameObject );

	public void AddAction ( GoapAction a ) => _availableActions.Add(a);

	public GoapAction GetAction ( System.Type action )
	{
		foreach( GoapAction g in _availableActions)
			if( g.GetType().Equals(action) )
			    return g;
		return null;
	}

	public void RemoveAction ( GoapAction action ) => _availableActions.Remove( action );

	bool HasActionPlan () => _currentActions.Count>0;

	void CreateIdleState ()
	{
		_idleState = (fsm, gameObj) =>
		{
			// GOAP planning

			// get the world state and the goal we want to plan for
			var worldState = _dataProvider.GetWorldState();
			var goal = _dataProvider.CreateGoalState();

			// Plan
			Queue<GoapAction> plan = _planner.Plan( gameObject , _availableActions , worldState , goal );
			if( plan!=null )
			{
				// we have a plan, hooray!
				_currentActions = plan;
				_dataProvider.PlanFound( goal , plan );

				fsm.PopState();// move to PerformAction state
				fsm.PushState( _performActionState );

			} else {
				// ugh, we couldn't get a plan
				Debug.Log( $"<color=orange>Failed Plan:</color>{PrettyPrint(goal)}" );
				_dataProvider.PlanFailed( goal );
				fsm.PopState();// move back to IdleAction state
				fsm.PushState( _idleState );
			}

		};
	}
	
	void CreateMoveToState ()
	{
		_moveToState = (fsm, gameObj) =>
		{
			// move the game object

			GoapAction action = _currentActions.Peek();
			if( action.RequiresInRange() && action.target==null) {
				Debug.Log( "<color=red>Fatal error:</color> Action requires a target but has none. Planning failed. You did not assign the target in your Action.checkProceduralPrecondition()" );
				fsm.PopState();// move
				fsm.PopState();// perform
				fsm.PushState(_idleState);
				return;
			}

			// get the agent to move itself
			if( _dataProvider.MoveAgent(action) )
				fsm.PopState();

			/*MovableComponent movable = gameObj.GetComponent<MovableComponent>();
			if( movable==null) {
				Debug.Log( "<color=red>Fatal error:</color> Trying to move an Agent that doesn't have a MovableComponent. Please give it one." );
				fsm.popState();// move
				fsm.popState();// perform
				fsm.pushState(idleState);
				return;
			}

			float step = movable.moveSpeed * Time.deltaTime;
			gameObj.transform.position = Vector3.MoveTowards(gameObj.transform.position, action.target.transform.position, step);

			if( gameObj.transform.position.Equals(action.target.transform.position) ) {
				// we are at the target location, we are done
				action.setInRange(true);
				fsm.popState();
			}*/
		};
	}
	
	void CreatePerformActionState ()
	{
		_performActionState = (fsm, gameObj) =>
		{
			// perform the action

			if( !HasActionPlan() )
			{
				// no actions to perform
				Debug.Log( "<color=red>Done actions</color>" );
				fsm.PopState();
				fsm.PushState(_idleState);
				_dataProvider.ActionsFinished();
				return;
			}

			GoapAction action = _currentActions.Peek();
			if( action.IsDone() )
			{
				// the action is done. Remove it so we can perform the next one
				_currentActions.Dequeue();
			}

			if( HasActionPlan() )
			{
				// perform the next action
				action = _currentActions.Peek();
				bool inRange = action.RequiresInRange() ? action.IsInRange() : true;

				if( inRange )
				{
					// we are in range, so perform the action
					bool success = action.Perform( gameObj );
					if( !success )
					{
						// action failed, we need to plan again
						fsm.PopState();
						fsm.PushState(_idleState);
						_dataProvider.PlanAborted(action);
					}
				}
				else
				{
					// we need to move there first
					// push moveTo state
					fsm.PushState(_moveToState);
				}
			}
			else
			{
				// no actions left, move to Plan state
				fsm.PopState();
				fsm.PushState( _idleState );
				_dataProvider.ActionsFinished();
			}
		};
	}

	void FindDataProvider () => _dataProvider = gameObject.GetComponent<IGoap>();

	void LoadActions ()
	{
		GoapAction[] actions = gameObject.GetComponents<GoapAction>();
		foreach( GoapAction a in actions )
			_availableActions.Add (a);
		Debug.Log( $"Found actions: {PrettyPrint(actions)}" );
	}

	public static string PrettyPrint ( Dictionary<string,object> state )
	{
		var sb = new System.Text.StringBuilder();
		foreach( var kv in state )
			sb.Append( $"{kv.Key}:{kv.Value}, " );
		return sb.ToString();
	}
	public static string PrettyPrint ( Queue<GoapAction> actions )
	{
		var sb = new System.Text.StringBuilder();
		foreach( GoapAction a in actions )
			sb.Append($"{a.GetType().Name} → " );
		sb.Append( "GOAL" );
		return sb.ToString();
	}
	public static string PrettyPrint ( GoapAction[] actions )
	{
		var sb = new System.Text.StringBuilder();
		foreach( GoapAction a in actions )
			sb.Append( $"{a.GetType().Name}, " );
		return sb.ToString();
	}
	public static string PrettyPrint ( GoapAction action ) => action.GetType().Name;

}
