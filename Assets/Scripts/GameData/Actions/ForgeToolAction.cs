
using UnityEngine;

public class ForgeToolAction : GoapAction
{
	bool _forged = false;
	ForgeComponent _targetForge;// where we forge tools
	
	float _startTime = 0;
	public float forgeDuration = 2;// seconds
	
	public ForgeToolAction ()
	{
		AddPrecondition( "hasOre" , true );
		AddEffect( "hasNewTools" , true );
	}
	
	public override void Reset ()
	{
		_forged = false;
		_targetForge = null;
		_startTime = 0;
	}
	
	public override bool IsDone () => _forged;
	
	public override bool RequiresInRange () => true;// yes we need to be near a forge
	
	public override bool CheckProceduralPrecondition ( GameObject agent )
	{
		// find the nearest forge
		ForgeComponent[] forges = FindObjectsOfType<ForgeComponent>();
		ForgeComponent closest = null;
		float closestDist = 0;
		
		foreach (ForgeComponent forge in forges) {
			if( closest==null) {
				// first one, so choose it for now
				closest = forge;
				closestDist = (forge.gameObject.transform.position - agent.transform.position).magnitude;
			} else {
				// is this one closer than the last?
				float dist = (forge.gameObject.transform.position - agent.transform.position).magnitude;
				if( dist < closestDist) {
					// we found a closer one, use it
					closest = forge;
					closestDist = dist;
				}
			}
		}
		if( closest==null)
			return false;

		_targetForge = closest;
		target = _targetForge.gameObject;
		
		return closest!=null;
	}
	
	public override bool Perform ( GameObject agent )
	{
		if( _startTime==0 )
			_startTime = Time.time;
		
		if( (Time.time-_startTime)>forgeDuration )
		{
			// finished forging a tool
			BackpackComponent backpack = agent.GetComponent<BackpackComponent>();
			backpack.numOre = 0;
			_forged = true;
		}
		return true;
	}
	
}
