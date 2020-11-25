using UnityEngine;

public class PickUpToolAction : GoapAction
{
	bool _hasTool = false;
	SupplyPileComponent _targetSupplyPile;// where we get the tool from

	public PickUpToolAction ()
	{
		AddPrecondition( "hasTool" , false );// don't get a tool if we already have one
		AddEffect( "hasTool" , true );// we now have a tool
	}

	public override void Reset ()
	{
		_hasTool = false;
		_targetSupplyPile = null;
	}
	
	public override bool IsDone () => _hasTool;

	public override bool RequiresInRange () => true;// yes we need to be near a supply pile so we can pick up the tool

	public override bool CheckProceduralPrecondition ( GameObject agent )
	{
		// find the nearest supply pile that has spare tools
		SupplyPileComponent[] supplyPiles = UnityEngine.GameObject.FindObjectsOfType<SupplyPileComponent>();
		SupplyPileComponent closest = null;
		float closestDist = 0;

		foreach( SupplyPileComponent supply in supplyPiles )
		{
			if( supply.numTools>0 )
			{
				if( closest==null)
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
		if( _targetSupplyPile.numTools>0 )
		{
			_targetSupplyPile.numTools--;
			_hasTool = true;

			// create the tool and add it to the agent

			BackpackComponent backpack = agent.GetComponent<BackpackComponent>();
			GameObject prefab = Resources.Load<GameObject>( backpack.toolType );
			GameObject tool = Instantiate( prefab , transform.position , transform.rotation );
			backpack.tool = tool;
			tool.transform.parent = transform;// attach the tool

			return true;
		}
		else
		{
			// we got there but there was no tool available! Someone got there first. Cannot perform action
			return false;
		}
	}

}
