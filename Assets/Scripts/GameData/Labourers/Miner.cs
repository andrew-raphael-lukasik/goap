using System.Collections.Generic;

public class Miner : Labourer
{
	/**
	 * Our only goal will ever be to mine ore.
	 * The MineOreAction will be able to fulfill this goal.
	 */
	protected override Dictionary<string,bool> OnCreateGoalState ()
		=> new Dictionary<string,bool>{ { "collectOre" , true } };

}
