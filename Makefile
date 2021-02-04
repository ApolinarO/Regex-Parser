UTILITY_FILES=src/program.cs src/TransitionState.cs src/MachineUtilities.cs
NFA_FILES=src/NFA.cs src/NFAState.cs src/TFunction.cs src/TState.cs
DFA_FILES=src/DFA.cs src/DFAState.cs

CS_FILES=src/RegularExpression.cs ${DFA_FILES} ${NFA_FILES} ${UTILITY_FILES}
PROGRAM=program

run: ${PROGRAM}
	mono ${PROGRAM}

$(PROGRAM): ${CS_FILES} RegBaseListener.dll
	mcs ${CS_FILES} /reference:RegBaseListener.dll /reference:Antlr4.Runtime.Standard.dll -out:${PROGRAM}

# Builds Parser
RegBaseListener.dll:
	cd ./RegParser; make dll

clean: 
	rm -f ${PROGRAM}
	cd ./RegParser; make clean
