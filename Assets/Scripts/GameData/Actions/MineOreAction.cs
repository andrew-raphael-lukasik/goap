
using UnityEngine;

public class MineOreAction : GoapAction
{
	bool _mined = false;
	IronRockComponent _targetRock; // where we get the ore from

	float _startTime = 0;
	[SerializeField][UnityEngine.Serialization.FormerlySerializedAs( "miningDuration" )] float _miningDuration = 2;// seconds

	public MineOreAction ()
	{
		AddPrecondition( "hasTool" , true );// we need a tool to do this
		AddPrecondition( "hasOre" , false );// if we have ore we don't want more
		AddEffect( "hasOre" , true );
	}
	
	public override void Reset ()
	{
		_mined = false;
		_targetRock = null;
		_startTime = 0;
	}
	
	public override bool IsDone () => _mined;
	
	public override bool RequiresInRange () => true;// yes we need to be near a rock
	
	public override bool CheckProceduralPrecondition (GameObject agent)
	{
		// find the nearest rock that we can mine
		IronRockComponent[] rocks = FindObjectsOfType<IronRockComponent>();
		IronRockComponent closest = null;
		float closestDist = 0;
		
		foreach( IronRockComponent rock in rocks )
		{
			if( closest==null )
			{
				// first one, so choose it for now
				closest = rock;
				closestDist = ( rock.gameObject.transform.position - agent.transform.position ).magnitude;
			}
			else
			{
				// is this one closer than the last?
				float dist = ( rock.gameObject.transform.position - agent.transform.position ).magnitude;
				if( dist<closestDist )
				{
					// we found a closer one, use it
					closest = rock;
					closestDist = dist;
				}
			}
		}
		_targetRock = closest;
		target = _targetRock.gameObject;
		
		return closest!=null;
	}
	
	public override bool Perform ( GameObject agent )
	{
		if( _startTime==0 )
			_startTime = Time.time;

		if( (Time.time-_startTime)>_miningDuration )
		{
			// finished mining
			BackpackComponent backpack = agent.GetComponent<BackpackComponent>();
			backpack.numOre += 2;
			_mined = true;
			ToolComponent tool = backpack.tool.GetComponent<ToolComponent>();
			tool.Use( 0.5f );
			if( tool.IsDestroyed() )
			{
				Destroy(backpack.tool);
				backpack.tool = null;
			}
		}
		return true;
	}
	
}
