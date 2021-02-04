using System;
using System.Linq;
using System.Collections.Generic;

public class TFunction
{
	// λ-closure(qi)
	public Dictionary<string, List<string>> LClosures { get; set; }

	// T table
	public Dictionary<string, Dictionary<char, List<string>>> Transitions { get; set; }

	public NFA M { get; set; }

	public TFunction(NFA m)
	{
		M = m;

		// Generates Lambda Closure
		LClosures = new Dictionary<string, List<string>>(m.States.Count());
		foreach(var state in M.States)
		{
			var states = new List<string>{state.Key};

			var i = 0;
			do
			{
				if(M.States[states[i]].Transitions.ContainsKey('λ'))
					states.AddRange(M.States[states[i]].Transitions['λ']
						.Select(s => s.Name)
						.Where(s => !states.Contains(s)));
			}while(++i < states.Count());

			states.Sort();
			LClosures.Add(state.Key, states);
		}

		// Generates T-Table
		Transitions = new Dictionary<string, Dictionary<char, List<string>>>(m.States.Count());
		foreach(var state in M.States)
		{
			var trans = new Dictionary<char, List<string>>();
			foreach(var symb in  M.Alphabet)
			{
				var states = new List<string>();
				foreach(var state2 in LClosures[state.Key])
				{
					// If transition leads to somewhere, then add the λ-closure on that destination
					if(M.States[state2].Transitions.ContainsKey(symb))
					{
						var reached = M.States[state2].Transitions[symb].Select(s => s.Name);
						states.AddRange(reached.SelectMany(s => LClosures[s]));
						
						/*foreach(var cl in LClosures[state2])
							if(M.States[cl].Transitions.ContainsKey(symb))
								states.AddRange(M.States[cl].Transitions[symb]
										.SelectMany(cll => LClosures[cll.Name]));*/
					}
				}
				trans.Add(symb, states.Distinct().OrderBy(s => s).ToList());
			}
			Transitions.Add(state.Key, trans);
		}
	}

	public DFA ConstructDFA()
	{
		var states = new Dictionary<string, TState>();
		
		// Adds Null State
		var nullState = new TState("Ø");
		nullState.Transitions = M.Alphabet.AsEnumerable()
								.ToDictionary(a => a, a => nullState);
		states.Add(nullState.Name, nullState);

		// Adds Start State
		var startState = new TState(LClosures[M.StartState.Name].Select(s => M.States[s]), true);
		states.Add(startState.Name, startState);
		Build(startState, states);

		// Convertst ot DFA
		return new DFA(M, states.Values, startState);
	}

	// Builds the states table
		// Starts by adding transitions to start state
	private void Build(TState curr, Dictionary<string, TState> states)
	{
		foreach(var symb in M.Alphabet)
		{
			// What the added states transitions into
			var next = curr.States
							.SelectMany(s => Transitions[s][symb])
							.Distinct()
							.OrderBy(s => s)
							.ToList();
			var nextS = string.Join("", next);

			// Does not transition anywhere?: null state
			if(next == null || next.Count() == 0)
			{
				curr.Transitions.Add(symb, states["Ø"]);
			}

			// Transitions into noted state
			else if(states.ContainsKey(nextS))
			{
				curr.Transitions.Add(symb, states[nextS]);
			}

			// Transition into new state: build for that state as well
			else
			{
				var nextState = new TState(next.Select(s => M.States[s]));
				states.Add(nextState.Name, nextState);
				curr.Transitions.Add(symb, nextState);
				
				Build(nextState, states);
			}
		}
	}

	public override string ToString()
	{
		var s = "q\tλ-Closure\n";
		s += string.Join("\n", LClosures.Select(lc => $"{lc.Key}\t{string.Join(",", lc.Value)}"));
		s += $"\n\nt\t{string.Join("\t", M.Alphabet)}";
		foreach(var state in Transitions)
		{
			var transList = string.Join("\t", state.Value
										.Select(ss => $"{ss.Key}:[{string.Join(",", ss.Value)}]"));
			s += $"\n{state.Key}\t{transList}";
		}

		return s;
	}
}
