/* 
	Apolinar Ortega
	Final Project Assigment

	ERROR: kleene star and plus does not work very well; it captures all elements left of it
*/
using System;
using System.Linq;

public class Program
{
	public static void Main(string[] args)
	{
		if(args.Length >= 1)
		{
			var args2 = args.Skip(1).ToArray();
			if(args[0] == "regex")
				RunRegexParser(args2);

			else if(args[0] == "NFA")
				RunNFAToDFAConverter(args2);

			else if(args[0] == "DFA")
				RunDFAMinimization(args2);

			else
				Console.WriteLine("In the first argument, please enter \"regex\" " 
									+ "for the regex parser, \"NFA\" for the NFA "
									+ "to DFA converter, or \"DFA\" for the DFA "
									+ "minimizer.");
		}
		else
		{
			DemoNFA("./demos/NFA/Exam3", "aaba", "baab", "babab");
			Console.WriteLine("Press enter to continue:");
			Console.ReadLine();
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			RunRegexParser("(a*)(b*)(c*)", "aabc", "bc", "ccb");
		}
	}

	public static void RunRegexParser(string[] args)
	{
		if(args.Length == 0)
		{
			RunRegexParser("a*b*c*");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			RunRegexParser("(b*)a((b*)a(b*)a(b*))*", "ababa", "aa");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			RunRegexParser("aa(b)+");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			RunRegexParser("(a|b)c");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			RunRegexParser("a*|b+");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			RunRegexParser("a+|b+");
		}
		else
			RunRegexParser(args[0], args.Skip(1).ToArray());

	}


	public static void RunRegexParser(string pattern, params string[] tests)
	{
		var regex = new RegularExpression(pattern);

		if(!regex.IsValid)
		{
			Console.WriteLine($"{pattern} Is invalid.");
		}
		else
		{
			Console.WriteLine($"Pattern ({pattern}): {regex}");

			Console.WriteLine("\nNFA:");
			Console.WriteLine(regex.NFA);

			Console.WriteLine("\nDFA:");
			var dm = new TFunction(regex.NFA).ConstructDFA();
			dm.Rename();
			Console.WriteLine(dm);

			Console.WriteLine("\nMinimized DFA:");
			var diff = dm.GetDistinguishableElements();
			var minimized = dm.Minimize(diff);
			minimized.Rename();
			Console.WriteLine(minimized);

			Console.WriteLine("\nInput Tests:");
			foreach(var test in tests)
				minimized.Run(test);
		}
	}

	public static void RunNFAToDFAConverter(string[] args)
	{
		if(args.Length == 0)
		{
			Console.WriteLine("NFA file not specified. Running all demos.");
			DemoNFA("./demos/NFA/HW5_40");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			DemoNFA("./demos/NFA/HW5_36");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			DemoNFA("./demos/NFA/NFA1");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			DemoNFA("./demos/NFA/Exam3");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			DemoNFA("./demos/NFA/Exam4");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");

		}
		else
			DemoNFA(args[0], args.Skip(1).ToArray());
	}


	public static void DemoNFA(string file, params string[] tests)
	{
		Console.WriteLine(file);
		Console.WriteLine("NFA:");
		var m = MachineUtilities.ParseFileToNFA(file);
		Console.WriteLine(m);

		Console.WriteLine("\nλ-Closure & t-Table:");
		var tTrans = new TFunction(m);
		Console.WriteLine(tTrans);

		Console.WriteLine("\nDFA:");
		var mDFA = tTrans.ConstructDFA();
		Console.WriteLine(mDFA);

		Console.WriteLine("\nRenamed:");
		mDFA.Rename();
		Console.WriteLine(mDFA);

		Console.WriteLine("\nMinimized:");
		var diff = mDFA.GetDistinguishableElements();
		var minimized = mDFA.Minimize(diff);
		Console.WriteLine(minimized);

		Console.WriteLine("\nInput Tests:");
		foreach(var test in tests)
			minimized.Run(test);
	}

	public static void RunDFAMinimization(string[] args)
	{
		if(args.Length == 0)
		{
			Console.WriteLine("DFA file not specified. Running all demos.");
			DemoProgram("demos/DFA/input.txt", "aaa", "aba");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			DemoProgram("demos/DFA/HMU Page 155 Fig 4_8.txt", "011", "111");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			DemoProgram("demos/DFA/HMU 4_4_1.txt", "010", "111");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			DemoProgram("demos/DFA/HMU 4_4_2.txt", "01", "111");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			DemoProgram("demos/DFA/Sudkamp CH5 45a.txt", "abb", "aa");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			DemoProgram("demos/DFA/Sudkamp CH5 45b.txt", "aa", "aaa");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");
			DemoProgram("demos/DFA/Sudkamp CH5 45c.txt", "aa", "aaa");
			Console.WriteLine("•••••••••••••••");
			Console.WriteLine("•••••••••••••••");

		}
		else
			DemoProgram(args[0], args.Skip(1).ToArray());
	}

	// Minimizes the DFA in the specifies file
		// then it runs the DFA using the testStrings as arguments
	public static void DemoProgram(string file, params string[] testStrings)
	{
		Console.WriteLine(file);

		Console.WriteLine("DFA:");
		DFA machine = MachineUtilities.ParseFileToDFA(file);
		Console.WriteLine(machine);

		Console.WriteLine("\nDistinguishable (0-No):");
		var diff = machine.GetDistinguishableElements();
		machine.PrintDiff(diff);


		Console.WriteLine("\nMinimized DFA:");
		var minimized = machine.Minimize(diff);
		Console.WriteLine(minimized);

		Console.WriteLine("\nInput Tests:");
		foreach(var testString in testStrings)
		{
			Console.Write($"M(\"{testString}\"):\t");
			machine.Run(testString);

			Console.Write($"DM(\"{testString}\"):\t");
			minimized.Run(testString);
			Console.WriteLine();
		}
	}
}
