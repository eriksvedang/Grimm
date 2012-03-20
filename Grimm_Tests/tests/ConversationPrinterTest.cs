using System;
using RelayLib;
using NUnit.Framework;
using GameTypes;

namespace GrimmLib.tests
{
	[TestFixture()]
	public class ConversationPrinterTest
	{
		RelayTwo _relay;
		DialogueRunner _dialogueRunner;
		
		[SetUp()]
		public void SetUp()
		{
			_relay = new RelayTwo();
			_relay.CreateTable(DialogueNode.TABLE_NAME);
			_dialogueRunner = new DialogueRunner(_relay, Language.SWEDISH);
		}
		
		[Test()]
		public void SimpleConversation()
		{
			DialogueScriptLoader loader = new DialogueScriptLoader(_dialogueRunner);
			loader.LoadDialogueNodesFromFile("../conversations/conversation5.dia");
			DialogueScriptPrinter printer = new DialogueScriptPrinter(_dialogueRunner);
			printer.PrintConversation("conversation5");
		}
		
		[Test()]
		public void ComplicatedConversation()
		{
			DialogueScriptLoader loader = new DialogueScriptLoader(_dialogueRunner);
			loader.LoadDialogueNodesFromFile("../conversations/conversation4.dia");
			DialogueScriptPrinter printer = new DialogueScriptPrinter(_dialogueRunner);
			printer.PrintConversation("conversation4");
		}
	}
}

