using System;
using System.Linq;
using System.Collections.Generic;

public class TState : TransitionState
{
	public Dictionary<char, TState> Transitions { get; set; }
	public List<string> States { get; set; }

	public TState(IEnumerable<NFAState> states, bool isStartState = false)
	{
		States = states.Select(s => s.Name).ToList();
		Name = string.Join("", States);
		IsEndState = states.Any(s => s.IsEndState);
		IsStartState = isStartState;
		Transitions = new Dictionary<char, TState>();
	}

	public TState(string name, bool isEndState = false, bool isStartState = false)
	{
		States = new List<string>{name};
		Name = name;
		IsEndState = isEndState;
		IsStartState = isStartState;
		Transitions = new Dictionary<char, TState>();
	}

	public DFAState ToDFAState()
	{
		return new DFAState(Name, IsEndState, IsStartState);
	}

	public Dictionary<char, DFAState> ConvertTransitionsToDFA(Dictionary<string, DFAState> states)
	{
		return Transitions.ToDictionary(s => s.Key, s => states[s.Value.Name]);
	}
}
