using System;

namespace GrimmLib
{
	public class FocusDialogueNode : DialogueNode
	{
		public override void OnEnter()
		{
			Stop();
			StartNextNode();
			_dialogueRunner.FocusConversation(conversation);
		}
	}
	
	public class DefocusDialogueNode : DialogueNode
	{
		public override void OnEnter()
		{
			Stop();
			StartNextNode();
			_dialogueRunner.DefocusConversation(conversation);
		}
	}
}
