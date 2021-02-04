using System;
using System.Linq;
using System.Collections.Generic;

public class NFAState : TransitionState
{
	public Dictionary<char, List<NFAState>> Transitions { get; set; }

	public NFAState(string name, bool isEndState = false, bool isStartState = false)
	{
		Name = name;
		IsEndState = isEndState;
		IsStartState = isStartState;
		Transitions = new Dictionary<char, List<NFAState>>();
	}

	public NFAState(NFAState copy)
	{
		Name = copy.Name;
		IsEndState = copy.IsEndState;
		IsStartState = copy.IsStartState;
		Transitions = new Dictionary<char, List<NFAState>>();
	}

	public void CopyTransitions(Dictionary<char, List<NFAState>> copyTrans,
								Dictionary<string, NFAState> newStates)
	{
		Transitions = copyTrans.ToDictionary(s => s.Key, 
											s => s.Value.Select(ss => newStates[ss.Name]).ToList());
	}

	// Generates table row representation for the Transition State
	public override string ToString()
	{
		var symb = $"{(IsStartState?">":"")}{(IsEndState?"*":"")}";
		var transS = Transitions.Select(t => $"{t.Key}:[{string.Join(", ", t.Value.Select(tt => tt.Name))}]");
		return $"{symb}{Name}\t" + string.Join("\t", transS);
	}
}
