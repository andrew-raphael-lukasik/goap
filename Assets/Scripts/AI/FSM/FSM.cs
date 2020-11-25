using System.Collections.Generic;
using UnityEngine;

/**
 * Stack-based Finite State Machine.
 * Push and pop states to the FSM.
 * 
 * States should push other states onto the stack 
 * and pop themselves off.
 */
public class FSM
{
	public delegate void FSMState ( FSM fsm , GameObject gameObject );
	Stack<FSMState> _stateStack = new Stack<FSMState>();

	public void Update ( GameObject gameObject )
	{
		if( _stateStack.Peek()!=null )
			_stateStack.Peek().Invoke( this , gameObject );
	}

	public void PushState ( FSMState state ) => _stateStack.Push( state );
	public void PopState () => _stateStack.Pop();

}
