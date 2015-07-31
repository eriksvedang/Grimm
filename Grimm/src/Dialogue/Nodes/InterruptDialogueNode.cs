using System;
using RelayLib;
using GameTypes;

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
				D.Log("Detected that interrupting conversation " + interruptingConversation + " has stopped, will continue in " + base.conversation);
				Stop();
				StartNextNode();
			}
//			else {
//				D.Log("Interrupting conversation " + interruptingConversation + " is still going on...");
//			}
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

