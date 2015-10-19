using System;
using GameTypes;

namespace GrimmLib
{
	public class ConversationEndDialogueNode : DialogueNode
	{
		public override void Update(float dt)
		{
			Stop();
			_dialogueRunner.StopConversation(conversation);
		}
	}
}

