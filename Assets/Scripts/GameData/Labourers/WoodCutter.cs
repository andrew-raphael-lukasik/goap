using System.Collections.Generic;

public class WoodCutter : Labourer
{
	/**
	 * Our only goal will ever be to chop logs.
	 * The ChopFirewoodAction will be able to fulfill this goal.
	 */
	protected override Dictionary<string,bool> OnCreateGoalState ()
		=> new Dictionary<string,bool>{ { "collectFirewood" , true } };
}
