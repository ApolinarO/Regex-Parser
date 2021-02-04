# Design Document
Apolinar Ortega
CS 6800

## Project Workflow
For the user, they will simply enter in a regular expression, which will then be parsed into an NFA-λ, which will then become a DFA, which finally becomes minimized. As these inputs are being processed, the user will be able to see the resulting transition machines. It is worth noting that the regular expressions will be limited to just alphanumeric characters and the following regular expression operations: grouping, concatenation, union, and Kleene star.

## Regular Expression to NFA-λ
This will be the last part of the assignment to be implemented. This will work by first parsing the regular expression string into an expression tree. The tree will then be processed starting from the leaves. When processed, they will build their own finite state machine. When a parent node must be processed, it begins to combine the machines produced by its children.

## NFA-λ to DFA
This part of the program will take in the generated machine generated from the expression machine. This will utilize the algorithm described in the textbook where the machine will generate a t-transition table and from the λ-closure(q0), it will build a DFA machine.

## DFA Minimization
Thanks in part to an assignment of this class, this portion of the project is mostly complete. This is built using the algorithm described in the textbook for minimizing a DFA, where it first builds a table that describes if each state is distinguishable from one other. From that table, it will combine the non-distinguishable states together and produce a new, functionally equivalent state machine.


## References
Thomas A. Sudkamp. 2005. Languages and Machines: An Introduction to the Theory of Computer Science (3rd Edition). Addison-Wesley Longman Publishing Co., Inc., Boston, MA, USA.

