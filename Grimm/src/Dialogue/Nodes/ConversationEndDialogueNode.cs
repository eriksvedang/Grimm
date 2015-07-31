using System;
using GameTypes;

namespace GrimmLib
{
	public class ConversationEndDialogueNode : DialogueNode
	{
		public override void OnEnter()
		{
			Stop();
			_dialogueRunner.StopConversation(conversation);
		}
	}
}

