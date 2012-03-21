using System;
using RelayLib;

namespace GrimmLib
{
	public class InterruptDialogueNode : DialogueNode
	{
		ValueEntry<string> CELL_interruptingConversation;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_interruptingConversation = EnsureCell("interconvo", "undefined");
		}
		
		public override void OnEnter()
		{
			_dialogueRunner.StartConversation(interruptingConversation);
		}
		
		public override void Update(float dt)
		{
			if(!_dialogueRunner.ConversationIsRunning(interruptingConversation)) {
				Stop();
				StartNextNode();
			}
		}
		
		#region ACCESSORS
		
		public string interruptingConversation
		{
			get {
				return CELL_interruptingConversation.data;
			}
			set {
				CELL_interruptingConversation.data = value;
			}
		}
		
		#endregion
	}
}

