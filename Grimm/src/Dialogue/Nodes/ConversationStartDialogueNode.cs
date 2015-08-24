using System;

namespace GrimmLib
{
	public class ConversationStartDialogueNode : DialogueNode
	{
		public override void Update(float dt)
		{
			Stop();
			StartNextNode();
		}
	}
}

