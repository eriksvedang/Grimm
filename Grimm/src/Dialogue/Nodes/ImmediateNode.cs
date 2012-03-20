using System;

namespace GrimmLib
{
	public class ImmediateNode : DialogueNode
	{
		public override void OnEnter()
		{
			Stop();
			StartNextNode();
		}
	}
}

