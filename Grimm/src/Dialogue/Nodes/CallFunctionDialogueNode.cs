using System;
using RelayLib;
namespace GrimmLib
{
	public class CallFunctionDialogueNode : DialogueNode
	{
		ValueEntry<string> CELL_function;
		ValueEntry<string[]> CELL_args;
		
		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_function = EnsureCell("function", "undefined");
			CELL_args = EnsureCell("args", new string[] {});
		}
		
		public override void OnEnter()
		{
			Stop();
			
			try {
				_dialogueRunner.CallFunction(function, args);
			}
			catch(Exception e) {
				throw new GrimmException ("Error when calling function from node " + this.name + " in conversation '" + this.conversation + "': " + e.Message + " \nStack trace: " + e.StackTrace);
			}
			
			StartNextNode();
		}
		
		#region ACCESSORS
		
		public string function
		{
			get {
				return CELL_function.data;
			}
			set {
				CELL_function.data = value;
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
	}
}

