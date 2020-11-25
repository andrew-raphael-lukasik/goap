
using UnityEngine;

public class DropOffOreAction : GoapAction
{
	bool _droppedOffOre = false;
	SupplyPileComponent _targetSupplyPile;// where we drop off the ore
	
	public DropOffOreAction ()
	{
		AddPrecondition( "hasOre" , true );// can't drop off ore if we don't already have some
		AddEffect( "hasOre" , false );// we now have no ore
		AddEffect( "collectOre" , true );// we collected ore
	}
	
	public override void Reset ()
	{
		_droppedOffOre = false;
		_targetSupplyPile = null;
	}
	
	public override bool IsDone () => _droppedOffOre;
	
	public override bool RequiresInRange () => true;// yes we need to be near a supply pile so we can drop off the ore
	
	public override bool CheckProceduralPrecondition (GameObject agent)
	{
		// find the nearest supply pile that has spare ore
		SupplyPileComponent[] supplyPiles = FindObjectsOfType<SupplyPileComponent>();
		SupplyPileComponent closest = null;
		float closestDist = 0;
		
		foreach (SupplyPileComponent supply in supplyPiles) {
			if( closest==null) {
				// first one, so choose it for now
				closest = supply;
				closestDist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
			} else {
				// is this one closer than the last?
				float dist = (supply.gameObject.transform.position - agent.transform.position).magnitude;
				if( dist < closestDist) {
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
		_targetSupplyPile.numOre += backpack.numOre;
		_droppedOffOre = true;
		backpack.numOre = 0;
		//TODO play effect, change actor icon
		
		return true;
	}

}
