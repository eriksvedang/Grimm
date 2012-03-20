using System;

namespace GrimmLib
{
	public class ConversationEndDialogueNode : DialogueNode
	{
		public override void OnEnter()
		{
			Stop();
			_dialogueRunner.ConversationEnded(conversation);
		}
	}
}

