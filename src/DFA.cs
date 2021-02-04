using System;
using System.Linq;
using System.Collections.Generic;

public class DFA
{
#region Properties
	public char[] Alphabet { get; set; }
	public Dictionary<string, DFAState> States { get; set; }
	public Dictionary<string, DFAState> FinalStates { get; set; }
	public DFAState StartState { get; set; }
	private Dictionary<string, int> StateIndex { get; set; }
#endregion

#region Constructors
	// Create DFA from string inputs
	public DFA(IEnumerable<string> states, char[] alphabet, string startState, 
			IEnumerable<string> endStates, Dictionary<string, Dictionary<char, string>> transitions)
	{
		States = states
					.ToDictionary(state => state, 
						state => new DFAState(state, endStates.Contains(state), state == startState));
		FinalStates = States
						.Where(state => state.Value.IsEndState)
						.ToDictionary(state => state.Key, state => state.Value);
		StartState = States[startState];
		Alphabet = alphabet;

		int i = 0;
		StateIndex = States.ToDictionary(state => state.Key, state => i++);

		// Reads the transitions for each state
		foreach(var state in transitions)
		{
			States[state.Key].Transitions = state.Value
											.ToDictionary(s => s.Key, s => States[s.Value]);
		}
	}

	public DFA(NFA M, IEnumerable<TState> states, TState startState)
	{
		Alphabet = M.Alphabet;
		States = states.ToDictionary(s => s.Name, s => s.ToDFAState());
		FinalStates = States
						.Where(s => s.Value.IsEndState)
						.ToDictionary(s => s.Key, s => s.Value);
		StartState = States[startState.Name];

		int i = 0;
		StateIndex = States.ToDictionary(state => state.Key, state => i++);

		foreach(var tState in states)
			States[tState.Name].Transitions = tState.ConvertTransitionsToDFA(States);
	}

	// Creates DFA from other DFA and a distinguishability table
	public DFA(Dictionary<string, DFAState> states, char[] alphabet, int[,] diff)
	{
		Alphabet = alphabet;

		// Notes the equivalent states
		var newDFA = new Dictionary<string, List<string>>();
		for(int i = 0; i < states.Count; i++)
		{
			newDFA.Add(states.Keys.ElementAt(i), new List<string>{states.Keys.ElementAt(i)});
			for(int j = 0; j < i; j++)
			{
				if(diff[i,j] == 0)
				{
					newDFA[states.Keys.ElementAt(i)].Add(states.Keys.ElementAt(j));
					newDFA[states.Keys.ElementAt(j)].Add(states.Keys.ElementAt(i));
				}
			}
		}

		// Creates new, combined states
		States = new Dictionary<string, DFAState>();
		foreach(var state in newDFA)
		{
			var stateName = String.Join(",", state.Value.OrderBy(val => val));
			if(!States.ContainsKey(stateName))
				States.Add(stateName, new DFAState(stateName, 
							state.Value.Any(s => states[s].IsEndState), 
							state.Value.Any(s => states[s].IsStartState)));
		}

		// Gets the transitions for each state
		foreach(var state in newDFA)
		{
			var stateName = String.Join(",", state.Value.OrderBy(val => val));
			if(States[stateName].Transitions.Count == 0)
			{
				foreach(var trans in states[state.Key].Transitions)
				{
					var originalTransName = trans.Value.Name;
					var newTransName = String.Join(",", newDFA[originalTransName].OrderBy(x => x));
					States[stateName].Transitions.Add(trans.Key, States[newTransName]);
				}
			}
		}
		
		FinalStates = States
						.Where(state => state.Value.IsEndState)
						.ToDictionary(state => state.Key, state => state.Value);
		StartState = States.Where(state => state.Value.IsStartState).First().Value;
		int k = 0;
		StateIndex = States.ToDictionary(state => state.Key, state => k++);
	}
#endregion

#region ToString, Run, & Minimization
	// For each element, goes through the renaming process
	public void Rename(string[] newNames = null)
	{
		if(newNames == null || newNames.Length != States.Count)
			newNames = Enumerable
							.Range(0, States.Count)
							.Select(n => $"q{n}")
							.ToArray();

		var newStates = new Dictionary<string, DFAState>(States.Count);
		var oldNames = States.Keys.ToArray();
		for(int i = 0; i < States.Count; i++)
		{
			var curr = States[oldNames[i]];
			curr.Name = newNames[i];
			newStates.Add(curr.Name, curr);
		}
		States = newStates;

		// Retrieves final states and redoe indexing
		FinalStates = States
						.Where(state => state.Value.IsEndState)
						.ToDictionary(state => state.Key, state => state.Value);
		int j = 0;
		StateIndex = States.ToDictionary(state => state.Key, state => j++);
	}

	// Returns a table representation of the DFA
	public override string ToString()
	{
		return $"δ\t{string.Join("\t", Alphabet)}\n"
				+ string.Join("\n", States.Select(state => state.Value.ToString()));
	}

	// Runs an input string through the DFA
	public void Run(string test)
	{
		var curr = StartState;
		for(int i = 0; i < test.Length; i++)
		{
			Console.Write($"{curr.ToTransitionString(test.Substring(i))} –> ");
			curr = curr.Transitions[test[i]];
		}

		Console.Write(curr.ToTransitionString(null));
		Console.WriteLine($"\t{curr.IsEndState?"ACCEPT":"REJECT"}");
	}

	// Returns the distinguishibility of states in the DFA
	public int[,] GetDistinguishableElements()
	{
		// For diff[x][y], is x distinguishable from y?
		var diff = new int[States.Count,States.Count];

		// Builds the initial diff table
		for(int i = 0; i < States.Count-1; i++)
		{
			for(int j = 1; j < States.Count; j++)
			{
				var a = States.Values.ElementAt(i).IsEndState;
				var b = States.Values.ElementAt(j).IsEndState;
				if(a != b && (a || b))
					diff[i,j] = diff[j,i] = 1;
			}
		}

		// Continues to build the diff table
		int diffCount;
		int stage = 2;
		do
		{
			diffCount = 0;
			for(int i = 0; i < States.Count-1; i++)
			{
				for(int j = 1; j < States.Count; j++)
				{
					foreach(var symbol in Alphabet)
					{
						// State after transition on symbol
						var stateA = States.Values.ElementAt(i).Transitions[symbol];
						var stateB = States.Values.ElementAt(j).Transitions[symbol];
						var x = StateIndex[stateA.Name];
						var y = StateIndex[stateB.Name];

						// Are the two states different?
						if(diff[x, y] > 0 && diff[i, j] == 0 && diff[x,y] != stage)
						{
							diff[i,j] = diff[j,i] = stage;
							diffCount++;
						}
					}
				}
			}
			stage++;
		}while(diffCount > 0);
		
		return diff;
	}

	// Returns a minimized version of the DFA
			// Takes in a table of distinguishability
	public DFA Minimize(int[,] diff)
	{
		return new DFA(States, Alphabet, diff);
	}

	// Prints the table of distinguishability
	public void PrintDiff(int[,] diff)
	{
		for(int i = 1; i < States.Count; i++)
		{
			Console.Write(States.Keys.ElementAt(i));
			for(int j = 0; j < i; j++)
				Console.Write($"\t{diff[i,j]}");
			Console.WriteLine();
		}

		Console.Write("∂");
		for(int i = 0; i < States.Count-1; i++)
			Console.Write($"\t{States.Keys.ElementAt(i)}");
		Console.WriteLine();
	}
#endregion
}
