using System;

namespace GrimmLib
{
	public class AssertDialogueNode : ExpressionDialogueNode
	{	
		public override void OnEnter()
		{
			Stop();
			if(_dialogueRunner.EvaluateExpression(expression, args) == false) {
				throw new GrimmAssertException("Expression " + expression + " failed in conversation '" + conversation + "'");
			}
			StartNextNode();
		}
	}
}

