
using UnityEngine;

public class DropOffLogsAction: GoapAction
{
	bool _droppedOffLogs = false;
	SupplyPileComponent _targetSupplyPile; // where we drop off the logs
	
	public DropOffLogsAction ()
	{
		AddPrecondition( "hasLogs" , true ); // can't drop off logs if we don't already have some
		AddEffect( "hasLogs" , false ); // we now have no logs
		AddEffect( "collectLogs" , true ); // we collected logs
	}
	
	public override void Reset ()
	{
		_droppedOffLogs = false;
		_targetSupplyPile = null;
	}
	
	public override bool IsDone () => _droppedOffLogs;
	
	public override bool RequiresInRange () => true; // yes we need to be near a supply pile so we can drop off the logs
	
	public override bool CheckProceduralPrecondition ( GameObject agent )
	{
		// find the nearest supply pile
		SupplyPileComponent[] supplyPiles = FindObjectsOfType<SupplyPileComponent>();
		SupplyPileComponent closest = null;
		float closestDist = 0;
		
		foreach (SupplyPileComponent supply in supplyPiles) {
			if( closest==null) {
				// first one, so choose it for now
				closest = supply;
				closestDist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
			}
			else
			{
				// is this one closer than the last?
				float dist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
				if( dist<closestDist )
				{
					// we found a closer one, use it
					closest = supply;
					closestDist = dist;
				}
			}
		}
		if( closest==null )
			return false;

		_targetSupplyPile = closest;
		target = _targetSupplyPile.gameObject;
		
		return closest!=null;
	}
	
	public override bool Perform ( GameObject agent )
	{
		BackpackComponent backpack = agent.GetComponent<BackpackComponent>();
		_targetSupplyPile.numLogs += backpack.numLogs;
		_droppedOffLogs = true;
		backpack.numLogs = 0;
		
		return true;
	}

}
