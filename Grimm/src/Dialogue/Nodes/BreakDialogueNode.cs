using System;
using RelayLib;

namespace GrimmLib
{
	public class BreakDialogueNode : DialogueNode
	{
		ValueEntry<string> CELL_breakTargetLoop;

		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_breakTargetLoop = EnsureCell("breakTarget", "undefined");
		}
		
		public override void Update(float dt)
		{
			Stop();
			LoopDialogueNode targetLoopDialogueNode = _dialogueRunner.GetDialogueNode(conversation, breakTargetLoop) as LoopDialogueNode;
			if(targetLoopDialogueNode == null) {
				throw new GrimmException("targetLoopDialogueNode was not of type LoopDialogueNode");
			}
			targetLoopDialogueNode.Break();
			//StartNextNode();
		}
		
		public string breakTargetLoop
		{
			get {
				return CELL_breakTargetLoop.data;
			}
			set {
				CELL_breakTargetLoop.data = value;
			}
		}
	}
}

