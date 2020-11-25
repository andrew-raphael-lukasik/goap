using System.Collections.Generic;

public class Logger : Labourer
{
	/**
	 * Our only goal will ever be to chop trees.
	 * The ChopTreeAction will be able to fulfill this goal.
	 */
	protected override Dictionary<string,object> OnCreateGoalState ()
		=> new Dictionary<string,object>{ { "collectLogs" , true } };

}
