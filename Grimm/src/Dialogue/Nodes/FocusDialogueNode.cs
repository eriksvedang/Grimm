using System;

namespace GrimmLib
{
	public class FocusDialogueNode : DialogueNode
	{
		public override void OnEnter()
		{
			Stop();
			_dialogueRunner.FocusConversation(conversation);
			StartNextNode();
		}
	}
	
	public class DefocusDialogueNode : DialogueNode
	{
		public override void OnEnter()
		{
			Stop();
			_dialogueRunner.DefocusConversation(conversation);
			StartNextNode();
		}
	}
}
