GRAMMAR=Reg
PARSER=${GRAMMAR}Parser.cs
DLL_OUT=RegBaseListener.dll
ANTLR_DLL=Antlr4.Runtime.Standard.dll
PROGRAM=prog
antlr4=java -jar antlr-4.7.1-complete.jar

#${PROGRAM}: program.cs ${PARSER}
#	mcs program.cs ${GRAMMAR}*.cs /reference:${ANTLR_DLL} -out:${PROGRAM}

dll: ${PARSER}
	mcs ${GRAMMAR}*.cs /target:library /reference:${ANTLR_DLL} -out:${DLL_OUT}
	cp ${DLL_OUT} ..
	cp ${ANTLR_DLL} ..

${PARSER}: ${GRAMMAR}.g4
	${antlr4} -visitor -Dlanguage=CSharp ${GRAMMAR}.g4

clean:
	rm -rf ${GRAMMAR}*.cs ${GRAMMAR}.i* ${GRAMMAR}.t* ${GRAMMAR}L* ${PROGRAM} ${DLL_OUT}
	cd ..; rm -rf ${DLL_OUT} ${ANTLR_DLL}