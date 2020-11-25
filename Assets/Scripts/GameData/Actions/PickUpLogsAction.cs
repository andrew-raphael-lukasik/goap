
using UnityEngine;

public class PickUpLogsAction : GoapAction
{
	bool _hasLogs = false;
	SupplyPileComponent _targetSupplyPile; // where we get the logs from
	
	public PickUpLogsAction ()
	{
		AddPrecondition( "hasLogs" , false ); // don't get a logs if we already have one
		AddEffect( "hasLogs" , true ); // we now have a logs
	}
	
	public override void Reset ()
	{
		_hasLogs = false;
		_targetSupplyPile = null;
	}
	
	public override bool IsDone () => _hasLogs;
	
	public override bool RequiresInRange () => true; // yes we need to be near a supply pile so we can pick up the logs
	
	public override bool CheckProceduralPrecondition ( GameObject agent )
	{
		// find the nearest supply pile that has spare logs
		SupplyPileComponent[] supplyPiles = FindObjectsOfType<SupplyPileComponent>();
		SupplyPileComponent closest = null;
		float closestDist = 0;
		
		foreach( SupplyPileComponent supply in supplyPiles )
		{
			if( supply.numLogs>0 )
			{
				if( closest==null )
				{
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
		}
		if( closest==null )
			return false;

		_targetSupplyPile = closest;
		target = _targetSupplyPile.gameObject;
		
		return closest!=null;
	}
	
	public override bool Perform ( GameObject agent )
	{
		if( _targetSupplyPile.numLogs>0 )
		{
			_targetSupplyPile.numLogs -= 1;
			_hasLogs = true;
			//TODO play effect, change actor icon
			BackpackComponent backpack = agent.GetComponent<BackpackComponent>();
			backpack.numLogs = 1;
			
			return true;
		}
		else
		{
			// we got there but there was no logs available! Someone got there first. Cannot perform action
			return false;
		}
	}

}
