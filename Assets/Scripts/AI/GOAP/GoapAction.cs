
using UnityEngine;
using System.Collections.Generic;

public abstract class GoapAction : MonoBehaviour
{

	Dictionary<string,object> _preconditions;
	public Dictionary<string,object> Preconditions => _preconditions;

	Dictionary<string,object> _effects;
	public Dictionary<string,object> Effects => _effects;

	bool _inRange = false;

	/* The cost of performing the action. 
	 * Figure out a weight that suits the action. 
	 * Changing it will affect what actions are chosen during planning.*/
	[SerializeField][UnityEngine.Serialization.FormerlySerializedAs("cost")] float _cost = 1f;
	public float cost => _cost;

	/**
	 * An action often has to perform on an object. This is that object. Can be null. */
	public GameObject target;

	public GoapAction ()
	{
		_preconditions = new Dictionary<string,object>();
		_effects = new Dictionary<string,object>();
	}

	public void DoReset ()
	{
		_inRange = false;
		target = null;
		Reset();
	}

	/**
	 * Reset any variables that need to be reset before planning happens again.
	 */
	public abstract void Reset ();

	/**
	 * Is the action done?
	 */
	public abstract bool IsDone ();

	/**
	 * Procedurally check if this action can run. Not all actions
	 * will need this, but some might.
	 */
	public abstract bool CheckProceduralPrecondition ( GameObject agent );

	/**
	 * Run the action.
	 * Returns True if the action performed successfully or false
	 * if something happened and it can no longer perform. In this case
	 * the action queue should clear out and the goal cannot be reached.
	 */
	public abstract bool Perform ( GameObject agent );

	/**
	 * Does this action need to be within range of a target game object?
	 * If not then the moveTo state will not need to run for this action.
	 */
	public abstract bool RequiresInRange ();
	
	/**
	 * Are we in range of the target?
	 * The MoveTo state will set this and it gets reset each time this action is performed.
	 */
	public bool IsInRange () => _inRange;
	
	public void SetInRange ( bool inRange ) => this._inRange = inRange;

	public void AddPrecondition ( string key , object value ) => _preconditions.Add( key,value );

	public void RemovePrecondition ( string key )
	{
		if( _preconditions.ContainsKey(key) )
			_preconditions.Remove( key );
	}

	public void AddEffect ( string key , object value ) => _effects.Add( key , value );

	public void RemoveEffect ( string key )
	{
		if( _effects.ContainsKey(key) )
			_effects.Remove( key );
	}

}
