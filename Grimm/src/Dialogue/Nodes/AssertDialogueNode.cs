using System;

namespace GrimmLib
{
	public class AssertDialogueNode : ExpressionDialogueNode
	{	
		public override void OnEnter()
		{
			Stop();
			if(_dialogueRunner.EvaluateExpression(expression, args) == false) {
				var argsConcatenated = string.Join(", ", args);
				throw new GrimmAssertException("Assertion " + expression + "(" + argsConcatenated + ") failed in conversation '" + conversation + "'");
			}
			StartNextNode();
		}
	}
}

