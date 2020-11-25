
using UnityEngine;

public class ChopFirewoodAction : GoapAction
{
	bool _chopped = false;
	ChoppingBlockComponent _targetChoppingBlock;// where we chop the firewood
	
	float _startTime = 0;
	public float workDuration = 2;// seconds
	
	public ChopFirewoodAction ()
	{
		AddPrecondition( "hasTool" , true );// we need a tool to do this
		AddPrecondition( "hasFirewood" , false );// if we have firewood we don't want more
		AddEffect( "hasFirewood" , true );
	}
	
	public override void Reset ()
	{
		_chopped = false;
		_targetChoppingBlock = null;
		_startTime = 0;
	}
	
	public override bool IsDone () => _chopped;
	
	public override bool RequiresInRange () => true;// yes we need to be near a chopping block
	
	public override bool CheckProceduralPrecondition ( GameObject agent )
	{
		// find the nearest chopping block that we can chop our wood at
		ChoppingBlockComponent[] blocks = FindObjectsOfType<ChoppingBlockComponent>();
		ChoppingBlockComponent closest = null;
		float closestDist = 0;
		
		foreach( ChoppingBlockComponent block in blocks )
		{
			if( closest==null )
			{
				// first one, so choose it for now
				closest = block;
				closestDist = (block.gameObject.transform.position - agent.transform.position).magnitude;
			}
			else
			{
				// is this one closer than the last?
				float dist = (block.gameObject.transform.position - agent.transform.position).magnitude;
				if( dist<closestDist )
				{
					// we found a closer one, use it
					closest = block;
					closestDist = dist;
				}
			}
		}
		if( closest==null )
			return false;

		_targetChoppingBlock = closest;
		target = _targetChoppingBlock.gameObject;
		
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
			backpack.numFirewood += 5;
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
