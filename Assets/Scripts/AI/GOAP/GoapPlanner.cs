using System.Collections.Generic;
using UnityEngine;

/**
 * Plans what actions can be completed in order to fulfill a goal state.
 */
public class GoapPlanner
{

	/**
	 * Plan what sequence of actions can fulfill the goal.
	 * Returns null if a plan could not be found, or a list of the actions
	 * that must be performed, in order, to fulfill the goal.
	 */
	public Queue<GoapAction> Plan (
		GameObject agent ,
		HashSet<GoapAction> availableActions ,
		Dictionary<string,object> worldState ,
		Dictionary<string,object> goal) 
	{
		// reset the actions so we can start fresh with them
		foreach( GoapAction action in availableActions )
			action.DoReset();

		// check what actions can run using their checkProceduralPrecondition
		HashSet<GoapAction> usableActions = new HashSet<GoapAction>();
		foreach( GoapAction action in availableActions )
			if( action.CheckProceduralPrecondition(agent) )
				usableActions.Add(action);
		
		// we now have all actions that can run, stored in usableActions

		// build up the tree and record the leaf nodes that provide a solution to the goal.
		List<Node> leaves = new List<Node>();

		// build graph
		Node start = new Node( null , 0 , worldState , null );
		bool success = BuildGraph( start , leaves , usableActions , goal );

		if( !success )
		{
			// oh no, we didn't get a plan
			Debug.Log( "NO PLAN" );
			return null;
		}

		// get the cheapest leaf
		Node cheapest = null;
		foreach( Node leaf in leaves )
		{
			if( cheapest==null )
				cheapest = leaf;
			else
			{
				if( leaf.runningCost<cheapest.runningCost )
					cheapest = leaf;
			}
		}

		// get its node and work back through the parents
		List<GoapAction> result = new List<GoapAction>();
		Node n = cheapest;
		while( n!=null )
		{
			if( n.action!=null )
				result.Insert( 0 , n.action ); // insert the action in the front
			n = n.parent;
		}
		// we now have this action list in correct order

		Queue<GoapAction> queue = new Queue<GoapAction>();
		foreach (GoapAction a in result)
			queue.Enqueue(a);

		// hooray we have a plan!
		return queue;
	}

	/**
	 * Returns true if at least one solution was found.
	 * The possible paths are stored in the leaves list. Each leaf has a
	 * 'runningCost' value where the lowest cost will be the best action
	 * sequence.
	 */
	bool BuildGraph (
		Node parent ,
		List<Node> leaves ,
		HashSet<GoapAction> usableActions ,
		Dictionary<string,object> goal
	)
	{
		bool foundOne = false;

		// go through each action available at this node and see if we can use it here
		foreach( GoapAction action in usableActions )
		{
			// if the parent state has the conditions for this action's preconditions, we can use it here
			if( InState(action.Preconditions,parent.state) )
			{
				// apply the action's effects to the parent state
				Dictionary<string,object> currentState = PopulateState( parent.state , action.Effects );
				//Debug.Log(GoapAgent.prettyPrint(currentState));
				Node node = new Node( parent , parent.runningCost+action.cost , currentState , action );

				if( InState(goal,currentState) )
				{
					// we found a solution!
					leaves.Add( node );
					foundOne = true;
				}
				else
				{
					// not at a solution yet, so test all the remaining actions and branch out the tree
					HashSet<GoapAction> subset = ActionSubset( usableActions , action );
					bool found = BuildGraph( node , leaves , subset , goal );
					if( found )
						foundOne = true;
				}
			}
		}
		return foundOne;
	}

	/**
	 * Create a subset of the actions excluding the removeMe one. Creates a new set.
	 */
	HashSet<GoapAction> ActionSubset ( HashSet<GoapAction> actions , GoapAction removeMe )
	{
		HashSet<GoapAction> subset = new HashSet<GoapAction>();
		foreach( GoapAction a in actions )
			if( !a.Equals(removeMe) )
				subset.Add(a);
		return subset;
	}

	/**
	 * Check that all items in 'test' are in 'state'. If just one does not match or is not there
	 * then this returns false.
	 */
	bool InState ( Dictionary<string,object> test , Dictionary<string,object> state )
	{
		bool allMatch = true;
		foreach( var t in test )
		{
			bool match = false;
			foreach( var s in state )
			{
				if( s.Equals(t) )
				{
					match = true;
					break;
				}
			}
			if( !match )
				allMatch = false;
		}
		return allMatch;
	}
	
	/**
	 * Apply the stateChange to the currentState
	 */
	Dictionary<string,object> PopulateState ( Dictionary<string,object> currentState , Dictionary<string,object> stateChange )
	{
		Dictionary<string,object> state = new Dictionary<string,object>();
		// copy the KVPs over as new objects
		foreach( var kv in currentState )
			state.Add( kv.Key , kv.Value );

		foreach( var change in stateChange )
		{
			// if the key exists in the current state, update the Value, create kv otherwise
			if( state.ContainsKey(change.Key) )
				state[ change.Key ] = change.Value;
			else
				state.Add( change.Key , change.Value );
		}
		return state;
	}

	/**
	 * Used for building up the graph and holding the running costs of actions.
	 */
	class Node
	{
		public Node parent;
		public float runningCost;
		public Dictionary<string,object> state;
		public GoapAction action;
		public Node (
			Node parent ,
			float runningCost ,
			Dictionary<string,object> state ,
			GoapAction action
		)
		{
			this.parent = parent;
			this.runningCost = runningCost;
			this.state = state;
			this.action = action;
		}
	}

}
