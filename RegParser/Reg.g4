grammar Reg;

/* Source: http://www.cs.sfu.ca/~cameron/Teaching/384/99-3/regexp-plg.html
/*
 * Parser Rules
 */
pattern			: regex ;
regex			: (union | simpleregex);
union			: simpleregex '|' regex ;
simpleregex		: (concat | basicregex) ;
concat			: basicregex simpleregex ;
basicregex		: (star | plus | elementaryregex) ;
star			: elementaryregex '*' ;
plus			: elementaryregex '+' ;
elementaryregex	: (group | set | STRING) ;
group			: '(' regex ')';
set				: '[' STRING ']';

/*
 * Lexer Rules
 */
fragment CHARACTER	: ([0-9] | [a-z] | [A-Z]) ;
STRING     			: CHARACTER+ ;
