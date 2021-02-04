using System;

public abstract class TransitionState
{
	public string Name { get; set; }
	public bool IsEndState { get; set; }
	public bool IsStartState { get; set; }

	// Generates ∂(q, w) string representation
	public string ToTransitionString(string input)
	{
		if(string.IsNullOrEmpty(input))
			return $"δ({Name}, λ)";
		return $"δ({Name}, \"{input}\")";
	}
}
