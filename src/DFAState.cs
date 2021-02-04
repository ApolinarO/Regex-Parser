using System;
using System.Linq;
using System.Collections.Generic;

public class DFAState : TransitionState
{
	public Dictionary <char, DFAState> Transitions { get; set; }

	// Constructor for TransitionState
	public DFAState(string name, bool isEndState = false, bool isStartState = false)
	{
		Name = name;
		IsEndState = isEndState;
		IsStartState = isStartState;
		Transitions = new Dictionary<char, DFAState>();
	}

	// Generates table row representation for the Transition State
	public override string ToString()
	{
		return $"{(IsStartState?">":"")}{(IsEndState?"*":"")}{Name}\t"
			+string.Join("\t", Transitions.Select(trans => trans.Value.Name));
	}
}
