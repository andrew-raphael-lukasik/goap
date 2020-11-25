
using UnityEngine;

public class PickUpOreAction : GoapAction
{
	bool _hasOre = false;
	SupplyPileComponent _targetSupplyPile; // where we get the ore from
	
	public PickUpOreAction ()
	{
		AddPrecondition( "hasOre" , false ); // don't get a ore if we already have one
		AddEffect( "hasOre" , true ); // we now have a ore
	}
	
	public override void Reset ()
	{
		_hasOre = false;
		_targetSupplyPile = null;
	}
	
	public override bool IsDone () => _hasOre;
	
	public override bool RequiresInRange () => true; // yes we need to be near a supply pile so we can pick up the ore
	
	public override bool CheckProceduralPrecondition ( GameObject agent )
	{
		// find the nearest supply pile that has spare ores
		SupplyPileComponent[] supplyPiles = FindObjectsOfType<SupplyPileComponent>();
		SupplyPileComponent closest = null;
		float closestDist = 0;
		
		foreach( SupplyPileComponent supply in supplyPiles )
		{
			if( supply.numOre>=3 )// we need to take 3 ore
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
		if( _targetSupplyPile.numOre>=3 )
		{
			_targetSupplyPile.numOre -= 3;
			_hasOre = true;
			//TODO play effect, change actor icon
			BackpackComponent backpack = agent.GetComponent<BackpackComponent>();
			backpack.numOre += 3;
			
			return true;
		} else {
			// we got there but there was no ore available! Someone got there first. Cannot perform action
			return false;
		}
	}
	
}
