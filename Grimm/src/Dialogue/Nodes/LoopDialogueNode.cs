using System;
using RelayLib;

namespace GrimmLib
{
	public class LoopDialogueNode : DialogueNode
	{
		ValueEntry<string> CELL_branchNode;
		DialogueNode _branchNodeCache;

		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_branchNode = EnsureCell("branchNode", "undefined");
		}
		
		public override void Update(float dt)
		{
			if(_branchNodeCache == null) {
				_branchNodeCache = _dialogueRunner.GetDialogueNode(conversation, branchNode);
			}
			
			_branchNodeCache.Start();
		}
		
		public void Break()
		{
			Stop();
			_dialogueRunner.ScopeEnded(conversation, this.name);
			StartNextNode();
		}
		
		public string branchNode
		{
			get {
				return CELL_branchNode.data;
			}
			set {
				CELL_branchNode.data = value;
			}
		}
	}
}

