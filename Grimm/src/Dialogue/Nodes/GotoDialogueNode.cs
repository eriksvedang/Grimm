using System;
using RelayLib;
namespace GrimmLib
{
	public class GotoDialogueNode : DialogueNode
	{
		ValueEntry<string> CELL_linkedNode;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_linkedNode = EnsureCell("linkedNode", "");
		}
		
		public override void Update (float dt)
		{
			string originalNextNode = nextNode;
			nextNode = linkedNode;
			Stop();
			//_dialogueRunner.logger.Log("GOTO node '" + name + "' in conversation '" + conversation + "' was triggered and is jumping to '" + nextNode + "'");
			StartNextNode();
			nextNode = originalNextNode;
		}
		
		#region ACCESSORS

		public string linkedNode
		{
			get {
				return CELL_linkedNode.data;
			}
			set {
				CELL_linkedNode.data = value;
			}
		}
		
		#endregion
	}
}

