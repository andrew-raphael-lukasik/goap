using System.Collections.Generic;

public class Blacksmith : Labourer
{
	/**
	 * Our only goal will ever be to make tools.
	 * The ForgeTooldAction will be able to fulfill this goal.
	 */
	protected override Dictionary<string,bool> OnCreateGoalState ()
		=> new Dictionary<string,bool>{ { "collectTools" , true } };
}
