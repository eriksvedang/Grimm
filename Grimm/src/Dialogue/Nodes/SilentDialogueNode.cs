using System;

namespace GrimmLib
{
	// When a silent node is entered nothing happens and the dialogue won't continue from there.
	
	public class SilentDialogueNode : DialogueNode
	{
		public override void OnEnter()
		{
			Stop();
		}
	}
}

