# Regex/NFA/DFA Converter
## Developer(s)
* Apolinar Ortega

## About

The purpose of this program is to allow you to input in regular expressions, DFA's, and NFA's to then convert them into an executable finite-state machine, which would let you run tests against it.

## Compiling

Make sure that when compiling and running the program that you are in the same directory as this README. Make sure to have the mono compiler installed and the latest version of Java:

	make

## Possible Commands

	./program ${ARG1} ${ARG2}...${ARGN}
	./program regex '(b)+a' aa bba b

* **ARG1**: "regex", "NFA", or "DFA", depending on which machine you want to emulate
* **ARG2...ARGN**: input strings to test the regular expression with
	* Note that if the "NFA" or "DFA" option was selected, then **ARG2** is dedicated to the file containing the DFA definition with alphabet speecification
	* See files under /demos for examples of NFA and DFA definition syntax 

## Notes
* Note that when running the regex parser, the alphabet is implicitly defined through the unique characters found in the regex pattern.
	* For example regex 'a+ba' has an alphabet of {a, b}
	* The program fails if you enter characters out of the alphabet for testing. For example, testing 'abc' on the above fails since 'c' is not in the defined alphabet
	* This has the same restriction with the NFA and DFA conversion

## DFA File Format

The demos are located in the _/demos_ sub-directory. The first line is a string of characters that will be the alphabet.

The second line lists the names of the states. The starting state is preceded by a _>_ and the end states are preceeded by a _*_.

The next set of rows consists of transitions for each of those states. The line starts with the state name, then a _|_, and then a comma-separated list. Each item in that list contains a symbol in the alphabet, then a _:_, and finally the state in which it transitions to.

See _input.txt_ as a simple example of this.


## Possible future notes
* Allow the usage of wildcard '.'
* When running the regex program, allow test input with characters outside of the dictionary