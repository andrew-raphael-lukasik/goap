using UnityEngine;
using System.Collections.Generic;

/**
 * A general labourer class.
 * You should subclass this for specific Labourer classes and implement
 * the createGoalState() method that will populate the goal for the GOAP
 * planner.
 */
public abstract class Labourer : MonoBehaviour, IGoap
{

	[SerializeField] BackpackComponent _backpack;
	[SerializeField] float _moveSpeed = 1;

	string _planPreview;

	void Start ()
	{
		if( _backpack==null )
			_backpack = gameObject.AddComponent<BackpackComponent>();
		if( _backpack.tool==null )
		{
			GameObject prefab = Resources.Load<GameObject>( _backpack.toolType );
			GameObject tool = Instantiate (prefab, transform.position, transform.rotation) as GameObject;
			_backpack.tool = tool;
			tool.transform.parent = transform;// attach the tool
		}
	}

	#if UNITY_EDITOR
	void OnDrawGizmos ()
	{
		UnityEditor.Handles.Label( transform.position , _planPreview );
	}
	#endif

	/**
	 * Key-Value data that will feed the GOAP actions and system while planning.
	 */
	Dictionary<string,object> IGoap.GetWorldState ()
	{
		return new Dictionary<string,object>{
			{ "hasOre" , _backpack.numOre>0 } ,
			{ "hasLogs" , _backpack.numLogs>0 } ,
			{ "hasFirewood" , _backpack.numFirewood>0 } ,
			{ "hasTool" , _backpack.tool!=null }
		};
	}

	Dictionary<string,object> IGoap.CreateGoalState () => OnCreateGoalState();
	protected abstract Dictionary<string,object> OnCreateGoalState ();

	void IGoap.PlanFailed ( Dictionary<string,object> failedGoal )
	{
		// Not handling this here since we are making sure our goals will always succeed.
		// But normally you want to make sure the world state has changed before running
		// the same goal again, or else it will just fail.
	}

	void IGoap.PlanFound ( Dictionary<string,object> goal , Queue<GoapAction> actions )
	{
		// Yay we found a plan for our goal
		_planPreview = GoapAgent.PrettyPrint( actions );
		Debug.Log( $"<color=green>Plan found</color> {_planPreview}" );
	}

	void IGoap.ActionsFinished ()
	{
		// Everything is done, we completed our actions for this gool. Hooray!
		_planPreview = "completed";
		Debug.Log( "<color=blue>Actions completed</color>" );
	}

	void IGoap.PlanAborted ( GoapAction aborter )
	{
		// An action bailed out of the plan. State has been reset to plan again.
		// Take note of what happened and make sure if you run the same goal again
		// that it can succeed.
		_planPreview = "aborted";
		Debug.Log( $"<color=red>Plan Aborted</color> {GoapAgent.PrettyPrint(aborter)}" );
	}

	bool IGoap.MoveAgent ( GoapAction nextAction )
	{
		// move towards the NextAction's target
		float step = _moveSpeed * Time.deltaTime;
		gameObject.transform.position = Vector3.MoveTowards(
			gameObject.transform.position ,
			nextAction.target.transform.position ,
			step
		);
		
		if( gameObject.transform.position.Equals(nextAction.target.transform.position) )
		{
			// we are at the target location, we are done
			nextAction.SetInRange(true);
			return true;
		}
		else
			return false;
	}

}
