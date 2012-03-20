using System;

namespace GrimmLib
{
	public class UnifiedEndNodeForScope : DialogueNode
	{
		public override void OnEnter()
		{
			Stop();
			StartNextNode();
		}
	}
}

