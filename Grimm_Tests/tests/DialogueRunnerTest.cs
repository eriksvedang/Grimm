using System;
using System.Collections.Generic;
using NUnit.Framework;
using RelayLib;
using GameTypes;

namespace GrimmLib.tests
{
	[TestFixture()]
	public class DialogueRunnerTest
	{		
		[Test()]
		public void CreateTimedDialogueNode()
		{
			RelayTwo relay = new RelayTwo();
			TableTwo table = relay.CreateTable(DialogueNode.TABLE_NAME);
			
			TimedDialogueNode t = new TimedDialogueNode();
			t.CreateNewRelayEntry(table, "TimedDialogueNode");
			t.timer = 100;
			t.Update(1.0f);
			
			Assert.AreEqual(99, t.timer, 0.001f);
		}
		
		List<string> _dialogueLog;
		
		[Test()]
		public void UsingTheDialogueRunner()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			DialogueRunner runner = new DialogueRunner(relay, Language.SWEDISH);
			runner.AddOnSomeoneSaidSomethingListener(LogDialogue);
			_dialogueLog = new List<string>();
			
			TimedDialogueNode d1 = runner.Create<TimedDialogueNode>("FirstConverstation", Language.SWEDISH, "DialogueNode1");
			d1.nextNode = "DialogueNode2";
			d1.timer = 0.5f;
			d1.speaker = "Helan";
			d1.line = "Hi, what's up?";
			
			TimedDialogueNode d2 = runner.Create<TimedDialogueNode>("FirstConverstation", Language.SWEDISH, "DialogueNode2");
			d2.speaker = "Halvan";
			d2.line = "I'm fine, thanks";
			
			// Frame 0
			d1.Start();
			
			Assert.IsTrue(d1.isOn);
			Assert.IsFalse(d2.isOn);

			runner.LogNodesThatAreOn();
			
			// Frame 1
			runner.Update(0.2f);
			runner.Update(0.2f);
			runner.Update(0.2f);
			runner.Update(0.2f);

			runner.LogNodesThatAreOn();
			
			Assert.IsFalse(d1.isOn);
			Assert.IsTrue(d2.isOn);
		}
		
		void LogDialogue(Speech pSpeech) {
			Console.WriteLine(pSpeech.speaker + ": " + pSpeech.line);
			_dialogueLog.Add(pSpeech.line);
		}
		
		[Test()]
		public void InstantiateDialoguesFromDatabase()
		{
			{
				RelayTwo relay = new RelayTwo();
				relay.CreateTable(DialogueNode.TABLE_NAME);
				DialogueRunner runner = new DialogueRunner(relay, Language.SWEDISH);
				
				TimedDialogueNode d1 = runner.Create<TimedDialogueNode>("c", Language.SWEDISH, "d1") as TimedDialogueNode;
				d1.speaker = "A";
				
				TimedDialogueNode d2 = runner.Create<TimedDialogueNode>("c", Language.SWEDISH, "d2");
				d2.speaker = "B";
				
				relay.SaveAll("conversation.xml");
			}
			
			{
				RelayTwo relay = new RelayTwo();
				relay.LoadAll("conversation.xml");
				DialogueRunner runner = new DialogueRunner(relay, Language.SWEDISH);
				
				TimedDialogueNode d1 = runner.GetDialogueNode("c", "d1") as TimedDialogueNode;
				TimedDialogueNode d2 = runner.GetDialogueNode("c", "d2") as TimedDialogueNode;
				
				Assert.AreEqual("A", d1.speaker);
				Assert.AreEqual("B", d2.speaker);
			}
		}
		
		[Test()]
		public void CanNotStartDialogueNodeInAnotherConversation()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			DialogueRunner runner = new DialogueRunner(relay, Language.SWEDISH);
			
			TimedDialogueNode n1 = runner.Create<TimedDialogueNode>("Conversation1", Language.SWEDISH, "DialogueNode1");
			n1.nextNode = "DialogueNode2";
			n1.timer = 1;
			
			runner.Create<TimedDialogueNode>("Conversation2", Language.SWEDISH, "DialogueNode2");
			n1.Start();

			string msg = null;

			D.onDLog += (pMessage) => { 
				Console.WriteLine("DLog: " + pMessage);
				msg = pMessage;
			};
		
			runner.Update(1.0f);
			runner.Update(1.0f);
			Assert.NotNull(msg);
		}
		
		[Test()]
		public void RemoveConversation()
		{
			RelayTwo relay = new RelayTwo();
			TableTwo table = relay.CreateTable(DialogueNode.TABLE_NAME);
			DialogueRunner runner = new DialogueRunner(relay, Language.SWEDISH);
			
			runner.Create<TimedDialogueNode>("Convo1", Language.SWEDISH, "Node1");
			runner.Create<TimedDialogueNode>("Convo2", Language.SWEDISH, "Node2");
			runner.Create<TimedDialogueNode>("Convo1", Language.SWEDISH, "Node3");
			runner.Create<TimedDialogueNode>("Convo2", Language.SWEDISH, "Node4");
			runner.Create<TimedDialogueNode>("Convo1", Language.SWEDISH, "Node5");
			runner.Create<TimedDialogueNode>("Convo2", Language.SWEDISH, "Node6");
			
			Assert.IsTrue(runner.HasConversation("Convo1"));
			Assert.IsTrue(runner.HasConversation("Convo2"));
			Assert.AreEqual(6, table.GetRows().Length);
			
			runner.RemoveConversation("Convo1");
			
			Assert.IsFalse(runner.HasConversation("Convo1"));
			Assert.IsTrue(runner.HasConversation("Convo2"));
			Assert.AreEqual(3, table.GetRows().Length);
		}
		
		private void LogDialogueRunner(string s)
		{
			Console.WriteLine(s);
		}
		
		private void OnSomeoneSaidSomething(Speech pInfo)
		{
			_lines.Add(pInfo.line);
		}
		
		List<string> _lines;
		
		[Test()]
		public void PlayerIsPresentedWithDialogueOptions()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.SWEDISH);
			dialogueRunner.logger.AddListener(LogDialogueRunner);
			
			DialogueNode start = dialogueRunner.Create<ConversationStartDialogueNode>("Snack", Language.SWEDISH, "Start");
			start.nextNode = "choice";
			
			BranchingDialogueNode choice = dialogueRunner.Create<BranchingDialogueNode>("Snack", Language.SWEDISH, "choice");
			choice.nextNodes = new string[] { "a", "b", "c" };
			
			TimedDialogueNode a = dialogueRunner.Create<TimedDialogueNode>("Snack", Language.SWEDISH, "a");
			TimedDialogueNode b = dialogueRunner.Create<TimedDialogueNode>("Snack", Language.SWEDISH, "b");
			TimedDialogueNode c = dialogueRunner.Create<TimedDialogueNode>("Snack", Language.SWEDISH, "c");
			
			DialogueNode end = dialogueRunner.Create<ConversationEndDialogueNode>("Snack", Language.SWEDISH, "End");
			
			a.line = "Yo";
			b.line = "Howdy";
			c.line = "Hola";
			
			a.nextNode = "End";
			b.nextNode = "End";
			c.nextNode = "End";
			a.timer = b.timer = c.timer = 1;
			
			start.Start();
			
			BranchingDialogueNode branchingNode = dialogueRunner.GetActiveBranchingDialogueNode("Snack");
			
			List<string> options = new List<string>();
			foreach(string nextNodeName in branchingNode.nextNodes)
			{
				options.Add(nextNodeName);
			}			
			
			Assert.AreEqual(3, options.Count);
			Assert.AreEqual("a", options[0]);
			Assert.AreEqual("b", options[1]);
			Assert.AreEqual("c", options[2]);
			
			DialogueNode activeDialogueNode = dialogueRunner.GetActiveBranchingDialogueNode("Snack");
			
			Assert.AreEqual("choice", activeDialogueNode.name);
			
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			_lines = new List<string>();
			
			branchingNode.nextNode = "b";

			for(int i = 0; i < 4; i++) {
				dialogueRunner.Update(0.5f);
			}
			
			Assert.IsFalse(start.isOn);
			Assert.IsFalse(choice.isOn);
			Assert.IsFalse(a.isOn);
			Assert.IsFalse(b.isOn);
			Assert.IsFalse(c.isOn);
			Assert.IsFalse(end.isOn);
			Assert.AreEqual(2, _lines.Count);
			Assert.AreEqual("Howdy", _lines[0]);
			Assert.AreEqual("", _lines[1]); // = the "shut up message"
		}	

		[Test()]
		public void DontTriggerEventOnWaitingNodeUnlessEventHappensWhenExpressionIsTrue()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			DialogueRunner runner = new DialogueRunner(relay, Language.SWEDISH);

			runner.logger.AddListener (msg => Console.WriteLine ("Dialog runner log: " + msg));

			bool sunny = false;

			runner.AddExpression ("IsSunny", new DialogueRunner.Expression (args => {
				return sunny;
			}));

			var expression = runner.Create<ExpressionDialogueNode>("Conversation1", Language.SWEDISH, "ExpressionNode");
			expression.expression = "IsSunny";
			expression.args = new string[] { };

			var waitNode = runner.Create<WaitDialogueNode>("Conversation1", Language.SWEDISH, "WaitNode");
			waitNode.eventName = "bam!";
			waitNode.expressions = new ExpressionDialogueNode[] {
				expression
			};

			var start = runner.Create<ConversationStartDialogueNode>("Conversation1", Language.SWEDISH, "Start");
			var end = runner.Create<ConversationEndDialogueNode>("Conversation1", Language.SWEDISH, "End");

			start.nextNode = waitNode.name;
			waitNode.nextNode = end.name;

			runner.StartConversation ("Conversation1");
			runner.Update (0.1f);
			Assert.IsTrue (waitNode.isOn);
			Assert.IsTrue (runner.ConversationIsRunning("Conversation1"));

			// Event should not occur since it's not sunny yet
			runner.EventHappened("bam!");
			runner.Update (0.1f);
			Assert.IsTrue (waitNode.isOn);
			Assert.IsTrue (runner.ConversationIsRunning("Conversation1"));

			sunny = true;

			// Now, even though it's sunny the event should still not happen since we're not bam!:ing
			runner.Update (0.1f);
			Assert.IsTrue (waitNode.isOn);
			Assert.IsTrue (runner.ConversationIsRunning("Conversation1"));

			// But if the bam! event happens while sunny, the conversation should end
			runner.EventHappened("bam!");
			runner.Update (0.1f);
			Assert.IsFalse (waitNode.isOn);
			Assert.IsFalse (runner.ConversationIsRunning("Conversation1"));
		}
	}
}

