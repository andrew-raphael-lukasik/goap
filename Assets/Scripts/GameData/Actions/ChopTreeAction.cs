using UnityEngine;

public class ChopTreeAction : GoapAction
{
	bool _chopped = false;
	TreeComponent _targetTree;// where we get the logs from
	
	float _startTime = 0;
	public float workDuration = 2;// seconds
	
	public ChopTreeAction ()
	{
		AddPrecondition( "hasTool" , true );// we need a tool to do this
		AddPrecondition( "hasLogs" , false );// if we have logs we don't want more
		AddEffect( "hasLogs" , true );
	}
	
	public override void Reset ()
	{
		_chopped = false;
		_targetTree = null;
		_startTime = 0;
	}
	
	public override bool IsDone () => _chopped;
	
	public override bool RequiresInRange () => true; // yes we need to be near a tree
	
	public override bool CheckProceduralPrecondition ( GameObject agent )
	{
		// find the nearest tree that we can chop
		TreeComponent[] trees = FindObjectsOfType<TreeComponent>();
		TreeComponent closest = null;
		float closestDist = 0;
		
		foreach( TreeComponent tree in trees )
		{
			if( closest==null )
			{
				// first one, so choose it for now
				closest = tree;
				closestDist = (tree.gameObject.transform.position - agent.transform.position).magnitude;
			}
			else
			{
				// is this one closer than the last?
				float dist = (tree.gameObject.transform.position - agent.transform.position).magnitude;
				if( dist<closestDist )
				{
					// we found a closer one, use it
					closest = tree;
					closestDist = dist;
				}
			}
		}
		if( closest==null )
			return false;

		_targetTree = closest;
		target = _targetTree.gameObject;
		
		return closest!=null;
	}
	
	public override bool Perform ( GameObject agent )
	{
		if( _startTime==0 )
			_startTime = Time.time;
		
		if( (Time.time-_startTime)>workDuration )
		{
			// finished chopping
			BackpackComponent backpack = agent.GetComponent<BackpackComponent>();
			backpack.numLogs += 1;
			_chopped = true;
			ToolComponent tool = backpack.tool.GetComponent<ToolComponent>();
			tool.Use( 0.34f );
			if( tool.IsDestroyed() )
			{
				Destroy( backpack.tool );
				backpack.tool = null;
			}
		}
		return true;
	}
	
}
