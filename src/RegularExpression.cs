using System;
using System.Linq;
using Antlr4.Runtime;

public class RegularExpression
{
	public string Pattern { get; set; }
	public char[] Alphabet { get; set; }
	public PatternLine Line { get; set; }
	public NFA NFA { get; set; }
	public readonly char[] DedicatedSymbols = "[]*+|()λ".ToCharArray();
    public bool IsValid
    {
        get
        {
            return Line.Op != RegOperation.Unknown;
        }
    }

	public RegularExpression(string pattern)
	{
		Pattern = pattern;
		Alphabet = pattern.ToCharArray()
							.Where(c => !DedicatedSymbols.Contains(c))
                            .Distinct()
							.ToArray();
		RunParser();
		NFA = Line.Evaluate();
	}

	private void RunParser()
	{
		try
        {
        	var inputStream = new AntlrInputStream(Pattern);
            var lexer = new RegLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new RegParser(tokenStream);

            var headContext = parser.pattern();
            var visitor = new RegVisitor();    
            visitor.Alphabet = Alphabet;
            Line = (PatternLine)visitor.Visit(headContext);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex);
        }
	}

	public override string ToString()
	{
		return Line.ToString();
	}
}

public class RegVisitor : RegBaseVisitor<object>
{
	public char[] Alphabet { get; set; }

    public override object VisitPattern(RegParser.PatternContext context)
    {
        var line = new PatternLine(context.regex(), Alphabet);
        return line;
    }
}


public enum RegOperation { Unknown, String, Set, Union, Concat, Star, Plus, Group, NonGroup };

public class PatternLine
{
    public string Content { get; set; }
    public PatternLine Left { get; set; }
    public PatternLine Right { get; set; }
    public RegOperation Op { get; set; }
    public char[] Alphabet { get; set; }

    public bool IsBasic 
    {
        get
        {
            return Op == RegOperation.String || Op == RegOperation.Set;
        } 
    }

    public PatternLine(RegParser.RegexContext expr, char[] alphabet)
    {
        Alphabet = alphabet;
        if(expr.union() != null)
        {
            Op = RegOperation.Union;
            Left = new PatternLine(expr.union().simpleregex(), alphabet);
            Right = new PatternLine(expr.union().regex(), alphabet);
        }
        else if(expr.simpleregex() != null)
        {
            Op = RegOperation.NonGroup;
            Left = new PatternLine(expr.simpleregex(), alphabet);
        }
        else
        {
            Console.WriteLine("\tR" + expr.GetText());
            Console.WriteLine($"\t{expr.union()}—{expr.simpleregex()}");
            Op = RegOperation.Unknown;
        }
    }

    public PatternLine(RegParser.SimpleregexContext expr, char[] alphabet)
    {
    	Alphabet = alphabet;
        if(expr.concat() != null)
        {
            Op = RegOperation.Concat;
            Left = new PatternLine(expr.concat().basicregex(), alphabet);
            Right = new PatternLine(expr.concat().simpleregex(), alphabet);
        }
        else if(expr.basicregex() != null)
        {
            Op = RegOperation.NonGroup;
            Left = new PatternLine(expr.basicregex(), alphabet);
        }
        else
        {
            Console.WriteLine("\tSR" + expr.GetText());
            Op = RegOperation.Unknown;
        }
    }

    public PatternLine(RegParser.BasicregexContext expr, char[] alphabet)
    {
        Alphabet = alphabet;
        if(expr.star() != null)
        {
            Op = RegOperation.Star;
            Left = new PatternLine(expr.star().elementaryregex(), alphabet);
        }
        else if(expr.plus() != null)
        {
            Op = RegOperation.Plus;
            Left = new PatternLine(expr.plus().elementaryregex(), alphabet);
        }
        else if(expr.elementaryregex() != null)
        {
            Op = RegOperation.NonGroup;
            Left = new PatternLine(expr.elementaryregex(), alphabet);
        }
        else
        {
            Console.WriteLine("\tBR" + expr.GetText());
            Op = RegOperation.Unknown;
        }
    }

    public PatternLine(RegParser.ElementaryregexContext expr, char[] alphabet)
    {
        Alphabet = alphabet;
        if(expr.group() != null)
        {
            Op = RegOperation.Group;
            Left = new PatternLine(expr.group().regex(), alphabet);
        }
        else if(expr.set() != null)
        {
            Op = RegOperation.Set;
            Content = expr.set().STRING().GetText();
        }
        else if(expr.STRING() != null)
        {
            Op = RegOperation.String;
            Content = expr.STRING().GetText();
        }
        else
        {
            Console.WriteLine("\tER" + expr.GetText());
            Op = RegOperation.Unknown;
        }
    }

    public NFA Evaluate()
    {
    	if(Op == RegOperation.String)
            return NFA.GenerateFromString(Content, Alphabet);
    	if(Op == RegOperation.Set)
            return NFA.GenerateFromSet(Content, Alphabet);
        else if(Op == RegOperation.Union)
        	return NFA.Union(Left.Evaluate(), Right.Evaluate());
        else if(Op == RegOperation.Concat)
        	return NFA.Concat(Left.Evaluate(), Right.Evaluate());
        else if(Op == RegOperation.Star)
        	return NFA.KleeneStar(Left.Evaluate());
        else if(Op == RegOperation.Plus)
        	return NFA.Plus(Left.Evaluate());
        else if(Op == RegOperation.Group || Op == RegOperation.NonGroup)
            return Left.Evaluate();
        else
            return null;
    }

    public override string ToString()
    {
        if(IsBasic)
            return Content;
        else if(Op == RegOperation.Union)
            return $"{Left} | {Right}";
        else if(Op == RegOperation.Concat)
            return $"{Left}{Right}";
        else if(Op == RegOperation.Star)
            return $"{Left}*";
        else if(Op == RegOperation.Plus)
            return $"{Left}+";
        else if(Op == RegOperation.NonGroup)
            return Left.ToString();
        else if(Op == RegOperation.Group)
            return $"({Left})";
        else
            return "<UNKNOWN>";

    }
}
