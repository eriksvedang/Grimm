using System;
using RelayLib;

namespace GrimmLib
{
	public class StopDialogueNode : DialogueNode
	{
		ValueEntry<string> CELL_conversationToStop;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_conversationToStop = EnsureCell("conversationToStop", "");
		}
		
		public override void OnEnter()
		{
			Stop();
			_dialogueRunner.DefocusConversation (conversationToStop);
			_dialogueRunner.StopConversation(conversationToStop);
			if(conversationToStop != conversation) {
				// if the stopped conversation is another conversation we just go on to the next node
				StartNextNode();
			}
		}
		
		#region ACCESSORS

		public string conversationToStop
		{
			get {
				return CELL_conversationToStop.data;
			}
			set {
				CELL_conversationToStop.data = value;
			}
		}
		
		#endregion
	}
}

