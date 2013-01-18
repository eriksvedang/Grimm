using System;

namespace GrimmLib
{
	public interface IRegisteredDialogueNode
	{
		string handle { get; set; }
		string conversation { get; set; }
		string name { get; set; }
		bool isListening { get; set; }
		string eventName { get; set; }
		void EventHappened();
	}
}