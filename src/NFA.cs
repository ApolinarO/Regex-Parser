using System;
using System.Linq;
using System.Collections.Generic;

public class NFA
{
#region Properties
	public char[] Alphabet { get; private set; }
	public Dictionary<string, NFAState> States { get; set; }
	public Dictionary<string, NFAState> FinalStates { get; set; }
	public NFAState StartState { get; set; }
	private Dictionary<string, int> StateIndex { get; set; }
#endregion

#region Constructors
	public NFA(IEnumerable<string> states, char[] alphabet, string startState, IEnumerable<string> endStates, 
				Dictionary<string, Dictionary<char, IEnumerable<string>>> transitions)
	{
		States = states
					.ToDictionary(state => state, 
						state => new NFAState(state, endStates.Contains(state), state == startState));
		StartState = States[startState];
		Alphabet = alphabet;

		SetFinalStates();

		// Reads the transitions for each state
		foreach(var state in transitions)
		{
			States[state.Key].Transitions = state.Value
											.ToDictionary(s => s.Key, 
												s => s.Value.Select(ss => States[ss]).ToList());
		}
	}
	public NFA(NFA m)
	{
		States = m.States.ToDictionary(s => s.Key, s => new NFAState(s.Value));
		StartState = States[m.StartState.Name];
		Alphabet = m.Alphabet;
		SetFinalStates();

		foreach(var state in m.States)
			States[state.Key].CopyTransitions(state.Value.Transitions, States);
	}

	private void SetFinalStates()
	{
		FinalStates = States
						.Where(state => state.Value.IsEndState)
						.ToDictionary(state => state.Key, state => state.Value);

		int i = 0;
		StateIndex = States.ToDictionary(state => state.Key, state => i++);
	}
#endregion

#region NFACreators
	public static void Test()
	{
		//var file = "./demos/NFA/HW5_40";
		//var file = "./demos/NFA/Exam4";
		//var file = "./demos/NFA/HW5_36";

		/*var m = MachineUtilities.ParseFileToNFA(file);
		var m2 = NFA.KleeneStar(m);

		Console.WriteLine("\n" + m);
		Console.WriteLine("\n" + m2);*/

		var alph = "abcde".ToCharArray();
		var m = NFA.GenerateFromSet("abc", alph);
		var m2 = NFA.GenerateFromString("abc", alph);
		Console.WriteLine(m + "\n");
		Console.WriteLine(m2 + "\n");

		var m3 = NFA.Plus(m);
		Console.WriteLine(m3 + "\n");

		var m4 = NFA.Plus(m2);
		Console.WriteLine(m4 + "\n");

		var dm1 = new TFunction(m3).ConstructDFA();
		Console.WriteLine(dm1 + "\n");

		var dm1m = dm1.Minimize(dm1.GetDistinguishableElements());
		dm1m.Rename();
		Console.WriteLine(dm1m + "\n");
	}


	public static NFA GenerateFromString(string set, char[] alphabet)
	{
		var chars = set.ToCharArray();
		var states = Enumerable.Range(0, chars.Count() + 1).Select(n => $"q{n}").ToArray();

		// Transitions
		var trans = new Dictionary<string, Dictionary<char, IEnumerable<string>>>(states.Length);
		for(int i = 0; i < states.Length-1; i++)
		{
			var trans2 = new Dictionary<char, IEnumerable<string>>{
				{
					chars[i], 
					new string[]{states[i+1]}.AsEnumerable()
				}
			};
			trans.Add(states[i], trans2);
		}
		var trans3 = new Dictionary<char, IEnumerable<string>>{
			{
				'λ', 
				new string[]{states[states.Length-1]}.AsEnumerable()
			}
		};
		trans.Add(states[states.Length-1], trans3);

		return new NFA(states, alphabet, states[0], new string[]{states[states.Length-1]}, trans);
	}

	public static NFA GenerateFromSet(string set, char[] alphabet)
	{
		var chars = set.ToCharArray().Distinct();
		var states = new string[]{"q0", "q1"};

		// Transitions
		var trans = new Dictionary<string, Dictionary<char, IEnumerable<string>>>(2);

		// Any symbol in chars leads to end sstate
		trans.Add(states[0], chars.ToDictionary(c => c, c => new string[]{states[1]}.AsEnumerable()));

		// λ-transition to self
		var trans2 = new Dictionary<char, IEnumerable<string>>(1);
		trans2.Add('λ', new string[]{states[1]}.AsEnumerable());
		trans.Add(states[1], trans2);

		return new NFA(states, alphabet, states[0], new string[]{states[1]}, trans);
	}

	public static NFA Union(NFA m1, NFA m2)
	{
		var ret = new NFA(m1);
		m1.MakeSingleEndState();

		var ret2 = new NFA(m2);
		m1.MakeSingleEndState();


		// Renames elements before combining them
		var newNames = Enumerable.Range(0, ret.States.Count+ret2.States.Count)
									.Select(s => $"q{s}");
		ret.Rename(newNames.Take(ret.States.Count).ToArray());
		ret2.Rename(newNames.Skip(ret.States.Count).Take(ret2.States.Count).ToArray());

		// Combines the tow states
		ret.States = ret.States.Concat(ret2.States).ToDictionary(s => s.Key, s => s.Value);

		// Sets new start state
		ret.StartState.IsStartState = false;
		ret2.StartState.IsStartState = false;
		var startState = new NFAState(ret.GetUniqueName(), false, true);
		startState.Transitions['λ'] = new List<NFAState>{ret.StartState, ret2.StartState};
		
		ret.StartState = startState;
		ret.States.Add(startState.Name, startState);

		ret.SetFinalStates();
		ret.MakeSingleEndState();
		ret.Rename();
		return ret;
	}

	public static NFA Concat(NFA m1, NFA m2)
	{
		var ret = new NFA(m1);
		var endState = ret.MakeSingleEndState();

		var ret2 = new NFA(m2);

		// Renames elements before combining them
		var newNames = Enumerable.Range(0, ret.States.Count+ret2.States.Count)
									.Select(s => $"q{s}");
		ret.Rename(newNames.Take(ret.States.Count).ToArray());
		ret2.Rename(newNames.Skip(ret.States.Count).Take(ret2.States.Count).ToArray());

		// λ Transition from r1.End to r2.Start
		endState.IsEndState = false;
		ret2.StartState.IsStartState = false;
		if(endState.Transitions.ContainsKey('λ'))
		{
			endState.Transitions['λ'] = endState
											.Transitions['λ']
											.Concat(new List<NFAState>{ret2.StartState})
											.Distinct()
											.ToList();
		}
		else
			endState.Transitions['λ'] = new List<NFAState>{ret2.StartState};

		// Combines the two State lists
		ret.States = ret.States.Concat(ret2.States).ToDictionary(s => s.Key, s => s.Value);
		ret.SetFinalStates();
		return ret;
	}

	public static NFA KleeneStar(NFA m)
	{
		var ret = new NFA(m);
		var endState = ret.MakeSingleEndState();

		// λ-transition from end to start
		if(endState.Transitions.ContainsKey('λ'))
		{
			endState.Transitions['λ'] = endState
											.Transitions['λ']
											.Concat(new List<NFAState>{ret.StartState})
											.Distinct()
											.ToList();
		}
		else
			endState.Transitions['λ'] = new List<NFAState>{ret.StartState};

		// λ-transition from start to end
		if(ret.StartState.Transitions.ContainsKey('λ'))
		{
			ret.StartState.Transitions['λ'] = ret.StartState
											.Transitions['λ']
											.Concat(new List<NFAState>{endState})
											.Distinct()
											.ToList();
		}
		else
			ret.StartState.Transitions['λ'] = new List<NFAState>{endState};

		return ret;
	}

	public static NFA Plus(NFA m)
	{
		var ret = NFA.Concat(m, NFA.KleeneStar(m));
		return ret;
	}

	// Makes sure that there's a single end state
	public NFAState MakeSingleEndState()
	{
		var endState = FinalStates.Values.First();

		if(FinalStates.Count > 1)
		{
			endState = new NFAState(GetUniqueName(), true, false);
			endState.Transitions['λ'] = new List<NFAState>{endState};
			States.Add(endState.Name, endState);

			foreach(var state in FinalStates)
			{
				state.Value.IsEndState = false;

				if(state.Value.Transitions.ContainsKey('λ'))
					state.Value.Transitions['λ'].Add(endState);
				else
					state.Value.Transitions['λ'] = new List<NFAState>{endState};
			}
			SetFinalStates();
		}
		return endState;
	}

	public string GetUniqueName()
	{
		for(int i = 0; i <= States.Count; i++)
		{
			if(!States.ContainsKey($"q{i}"))
				return $"q{i}";
		}
		return $"q{States.Count}";
	}
#endregion

#region Public Methods
	// For each element, goes through the renaming process
	public void Rename(string[] newNames = null)
	{
		if(newNames == null || newNames.Length != States.Count)
			newNames = Enumerable
							.Range(0, States.Count)
							.Select(n => $"q{n}")
							.ToArray();

		var newStates = new Dictionary<string, NFAState>(States.Count);
		var oldNames = States.Keys.ToArray();
		for(int i = 0; i < States.Count; i++)
		{
			var curr = States[oldNames[i]];
			curr.Name = newNames[i];
			newStates.Add(curr.Name, curr);
		}
		States = newStates;

		SetFinalStates();
	}

	// Returns a table representation of the DFA
	public override string ToString()
	{
		return $"δ\t{string.Join("\t", Alphabet)}\n"
				+ string.Join("\n", States.Select(state => state.Value.ToString()));
	}
#endregion
}
