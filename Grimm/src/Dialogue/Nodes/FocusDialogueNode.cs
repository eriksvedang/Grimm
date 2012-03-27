using System;

namespace GrimmLib
{
	public class FocusDialogueNode : DialogueNode
	{
		public override void OnEnter()
		{
			_dialogueRunner.FocusConversation(conversation);
			Stop();
			StartNextNode();
		}
	}
	
	public class DefocusDialogueNode : DialogueNode
	{
		public override void OnEnter()
		{
			_dialogueRunner.DefocusConversation(conversation);
			Stop();
			StartNextNode();
		}
	}
}
