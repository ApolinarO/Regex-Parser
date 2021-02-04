using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class MachineUtilities
{
#region Parse File Contents
	public static DFA ParseFileToDFA(string file)
	{
		var fileC = ReadFileLines(file);
		return ParseToDFA(fileC.Item1, fileC.Item2, fileC.Item3);
	}

	public static NFA ParseFileToNFA(string file)
	{
		var fileC = ReadFileLines(file);
		return ParseToNFA(fileC.Item1, fileC.Item2, fileC.Item3);
	}

	private static Tuple<string, string, IEnumerable<string>> ReadFileLines(string file)
	{
		using(var reader  = new StreamReader(file))
		{
			var alphabet = reader.ReadLine();
			var states = reader.ReadLine();
			var transitions = states.Split(',').Select(s => reader.ReadLine()).ToList();
			return new Tuple<string, string, IEnumerable<string>>(alphabet, states, transitions);
		}
	}
#endregion

#region Parse To Machine
	private static DFA ParseToDFA(string alphLine, string statesLine, IEnumerable<string> transLines)
	{
		var mItems = PartialMachineParse(alphLine, statesLine);

		var transitions = new Dictionary<string, Dictionary<char, string>>(mItems.Item1.Count());
		foreach(var line in transLines)
		{
			var stateTrans = line.Split('|');
			var trans = stateTrans[1].Split(',')
								.Select(s => s.Split(':'))
								.ToDictionary(s => s[0][0], s => s[1]);
			transitions.Add(stateTrans[0], trans);
		}

		return new DFA(mItems.Item1, mItems.Item2, mItems.Item3, mItems.Item4, transitions);

	}

	private static NFA ParseToNFA(string alphLine, string statesLine, IEnumerable<string> transLines)
	{
		var mItems = PartialMachineParse(alphLine, statesLine);

		var transitions = 
			new Dictionary<string, Dictionary<char, IEnumerable<string>>>(mItems.Item1.Count());
		foreach(var line in transLines)
		{
			var stateTrans = line.Split('|');
			var trans = stateTrans[1].Split(',')
								.Select(s => s.Split(':'))
								.ToDictionary(s => s[0][0], s => s[1].Split(';').AsEnumerable());
			transitions.Add(stateTrans[0], trans);
		}
		return new NFA(mItems.Item1, mItems.Item2, mItems.Item3, mItems.Item4, transitions);

	}

	// Does a partial parse, where only Q, ∑, qi, and F are parsed
		// Does not parse ∂
	private static Tuple<IEnumerable<string>, char[], string, IEnumerable<string>>
					PartialMachineParse(string alphLine, string statesLine)
	{
		var alphabet = alphLine.ToCharArray();
		var statesArr = statesLine.Split(',');
		var states = statesArr.Select(s => ExtractName(s));
		var startState = statesArr
							.Where(s => IsStartState(s))
							.Select(s => ExtractName(s))
							.First();
		var endStates = statesArr
							.Where(s => IsEndState(s))
							.Select(s => ExtractName(s));
		return new Tuple<IEnumerable<string>, char[], string, IEnumerable<string>>
					(states, alphabet, startState, endStates);
	}
#endregion

#region Name Parsing Utilities
	private static bool IsStartState(string state)
	{
		return state[0] =='>' || (state.Length > 1 && state[1] =='>');
	}

	private static bool IsEndState(string state)
	{
		return state[0] =='*' || (state.Length > 1 && state[1] =='*');
	}

	private static string ExtractName(string state)
	{
		if(state[0] == '>' || state[0] == '*')
		{
			if(state.Length > 1 && (state[1] == '>' || state[1] == '*'))
				return state.Substring(2);

			return state.Substring(1);
		}

		return state;
	}
#endregion
}