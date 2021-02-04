# Regex/NFA/DFA Converter
## Apolinar Ortega

## About

This program was developed using C# with the mono compiler. This program takes in arguments from the command line. The first argument is the file with the DFA in it. The next optional arguments would be what strings to run the DFA's with.

## Notes
* Note that when running the regex parser, the alphabet is implicitly defined through the unique characters found in the regex pattern.
	* For example regex 'a+ba' has an alphabet of {a, b}
	* The program fails if you enter characters out of the alphabet for testing. For example, testing 'abc' on the above fails since 'c' is not in the defined alphabet
	* This has the same restriction with the NFA and DFA conversion

## Running The Program

Make sure that when compiling and running the program that you are in the same directory as this README. If you do not have mono, you may either have to download it or run the _csc_ compile command if on Windows. Run any of the following to compile the program:

	make
	make program
	mcs src/program.cs src/DFA.cs src/TransitionState.cs -out:program

Once compiled, you can run the program with:

	mono program $DFA_FILE $OPTIONAL_STRINGS

Where $DFA_FILE is replaced with the file containing the DFA and $OPTIONAL_STRINGS can be left empty or can be replaced with a set of strings to test the machines with.

The program also contains a series of demoes that can be run with the following:

	make run


## DFA File Format

The demos are located in the _/demos_ sub-directory. The first line is a string of characters that will be the alphabet.

The second line lists the names of the states. The starting state is preceded by a _>_ and the end states are preceeded by a _*_.

The next set of rows consists of transitions for each of those states. The line starts with the state name, then a _|_, and then a comma-separated list. Each item in that list contains a symbol in the alphabet, then a _:_, and finally the state in which it transitions to.

See _input.txt_ as a simple example of this.


## Possible future notes
* Allow the usage of wildcard '.'
* When running the regex program, allow test input with characters outside of the dictionary