using System;

namespace GrimmLib
{
	public class ConversationStartDialogueNode : DialogueNode
	{
		public override void OnEnter()
		{
			Stop();
			StartNextNode();
		}
	}
}

