using System;
using RelayLib;

namespace GrimmLib
{
	public class ExpressionDialogueNode : DialogueNode
	{
		ValueEntry<string> CELL_expression;
		ValueEntry<string[]> CELL_args;
		
		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_expression = EnsureCell("expression", "undefined");
			CELL_args = EnsureCell("args", new string[] {});
		}
		
		#region ACCESSORS
		
		public string expression
		{
			get {
				return CELL_expression.data;
			}
			set {
                CELL_expression.data = value;
			}
		}
		
		public string[] args
		{
			get {
				return CELL_args.data;
			}
			set {
				CELL_args.data = value;
			}
		}
		
		#endregion

		public bool Evaluate()
		{
			try {
				return _dialogueRunner.EvaluateExpression(expression, args);
			}
			catch(Exception e) {
				throw new GrimmException("Error when evaluating expression " + expression + " in " + conversation + " with args: " + string.Join(", ", args) + " e: " + e.Message);
			}
		}
	}
}

