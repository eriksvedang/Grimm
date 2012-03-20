using System;

namespace GrimmLib
{
	/// <summary>
	/// Contains data from the Dialogue Runner's OnSomeoneSaidSomething-event
	/// </summary>
	public struct Speech
	{
		public string conversation;
		public string dialogueNodeName;
		public string speaker;
		public string line;
		
		public Speech (string pConversation, string pDialogueNodeName, string pSpeaker, string pLine)
		{
			conversation = pConversation;
			dialogueNodeName = pDialogueNodeName;
			speaker = pSpeaker;
			line = pLine;
		}
		
		public override string ToString()
		{
			return string.Format("TalkEventInfo conversation = '{0}', dialogueNodeName = '{1}', talker = '{2}', line = '{3}'", conversation, dialogueNodeName, speaker, line);
		}
	}
}

