using System;
using System.Collections.Generic;
using RelayLib;
using GameTypes;
using NUnit.Framework;

namespace GrimmLib.tests
{
	[TestFixture()]
	public class LoadConversationsFromScriptFilesTest
	{
		RelayTwo _relay;
		DialogueRunner _dialogueRunner;
		List<Speech> _speech;
		
		void LogSpeech(Speech pSpeech) {
			Console.WriteLine("Got Speech: " + pSpeech.speaker + " said " + pSpeech.line);
			_speech.Add(pSpeech);
		}
		
		[SetUp()]
		public void SetUp()
		{
			_relay = new RelayTwo();
			_relay.CreateTable(DialogueNode.TABLE_NAME);
			
			_dialogueRunner = new DialogueRunner(_relay, Language.SWEDISH);
		}
		
		[Test()]
		public void Basics()
		{
			_speech = new List<Speech>();			
			_dialogueRunner.AddOnSomeoneSaidSomethingListener(LogSpeech);
			DialogueScriptLoader loader = new DialogueScriptLoader(_dialogueRunner);
			loader.LoadDialogueNodesFromFile("../conversations/conversation1.dia");			
			
			List<DialogueNode> nodes = new List<DialogueNode>();
			nodes.Add(_dialogueRunner.GetDialogueNode("conversation1", "__Start__"));
			nodes.Add(_dialogueRunner.GetDialogueNode("conversation1", "0"));
			nodes.Add(_dialogueRunner.GetDialogueNode("conversation1", "1"));
			nodes.Add(_dialogueRunner.GetDialogueNode("conversation1", "2"));
			nodes.Add(_dialogueRunner.GetDialogueNode("conversation1", "__End__"));
			foreach(DialogueNode n in nodes)
			{
				Console.WriteLine("Has a node called '{0}' with next node '{1}' of type {2}", n.name, n.nextNode, n.GetType());
			}
			
			_dialogueRunner.StartConversation("conversation1");
			for(int i = 0; i < 1000; i++) {
				_dialogueRunner.Update(1.0f);
			}
			
			Assert.AreEqual(6, _speech.Count);
			
			Assert.AreEqual("Hoho",  _speech[0].line);
			Assert.AreEqual("", 	 _speech[1].line);
			Assert.AreEqual("Hjälp!",_speech[2].line);
			Assert.AreEqual("", 	 _speech[3].line);
			Assert.AreEqual("Oh no", _speech[4].line);
			Assert.AreEqual("", 	 _speech[5].line);
			
			Assert.AreEqual("Tomten", _speech[0].speaker);
			Assert.AreEqual("Tomten", _speech[1].speaker);
			Assert.AreEqual("Barn",   _speech[2].speaker);
			Assert.AreEqual("Barn",   _speech[3].speaker);
			Assert.AreEqual("Tomten", _speech[4].speaker);
			Assert.AreEqual("Tomten", _speech[5].speaker);
		}
		
		[Test()]
		public void GetTheRightConversationNameFromAFilePath()
		{
			{
				string path = "folder/folder/folder/something.whatever";
				string conversationName = DialogueScriptLoader.GetConversationNameFromFilepath(path);
				Assert.AreEqual("something", conversationName);
			}
			{
				string path = "folder/folder/folder/something";
				string conversationName = DialogueScriptLoader.GetConversationNameFromFilepath(path);
				Assert.AreEqual("something", conversationName);
			}
			{
				string path = "something";
				string conversationName = DialogueScriptLoader.GetConversationNameFromFilepath(path);
				Assert.AreEqual("something", conversationName);
			}
			{
				string path = "something.whatever";
				string conversationName = DialogueScriptLoader.GetConversationNameFromFilepath(path);
				Assert.AreEqual("something", conversationName);
			}
		}
		
		[Test()]
		public void CustomNodeNames()
		{
			DialogueScriptLoader loader = new DialogueScriptLoader(_dialogueRunner);
			loader.LoadDialogueNodesFromFile("../conversations/conversation2.dia");			
			Assert.IsNotNull(_dialogueRunner.GetDialogueNode("conversation2", "importantNode"));
		}
		
		[Test()]
		public void LoadingADialogueWithOptions()
		{
			DialogueScriptLoader loader = new DialogueScriptLoader(_dialogueRunner);
			loader.LoadDialogueNodesFromFile("../conversations/conversation3.dia");			
			
			_dialogueRunner.StartConversation("conversation3");
			BranchingDialogueNode n = _dialogueRunner.GetActiveBranchingDialogueNode("conversation3");
			
			Assert.IsNotNull(n);
			TimedDialogueNode option1 = _dialogueRunner.GetDialogueNode("conversation3", n.nextNodes[0]) as TimedDialogueNode;
			TimedDialogueNode option2 = _dialogueRunner.GetDialogueNode("conversation3", n.nextNodes[1]) as TimedDialogueNode;
			
			Assert.AreEqual("Option 1", option1.line);
			Assert.AreEqual("Option 2", option2.line);
			Assert.AreEqual("first", option1.nextNode);
			Assert.AreEqual("second", option2.nextNode);
			
			DialogueNode option1response = _dialogueRunner.GetDialogueNode("conversation3", option1.nextNode);
			DialogueNode option2response = _dialogueRunner.GetDialogueNode("conversation3", option2.nextNode);
			
			DialogueNode start = _dialogueRunner.GetDialogueNode("conversation3", "__Start__");
			DialogueNode end = _dialogueRunner.GetDialogueNode("conversation3", "__End__");
			
			Assert.IsNotNull(option1response);
			Assert.IsNotNull(option2response);
			Assert.IsNotNull(start);
			Assert.IsNotNull(end);
		}
		
		List<string> _lines;
		
		private void LogDialogueRunner(string s)
		{
			Console.WriteLine(s);
		}
		
		private void OnSomeoneSaidSomething(Speech pInfo)
		{
			_lines.Add(pInfo.line);
		}
		
		[Test()]
		public void GotoNodes()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.SWEDISH);
			dialogueRunner.logger.AddListener(LogDialogueRunner);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			_lines = new List<string>();
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation6.dia");
			DialogueScriptPrinter printer = new DialogueScriptPrinter(dialogueRunner);
			printer.PrintConversation("conversation6");
			dialogueRunner.StartConversation("conversation6");
			for(int i = 0; i < 1000; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			Console.WriteLine("Output:");
			foreach(string s in _lines)
			{
				Console.WriteLine(s);
			}
			Assert.AreEqual(6, _lines.Count);
			Assert.AreEqual("a", _lines[0]);
			Assert.AreEqual("", _lines[1]);
			Assert.AreEqual("b", _lines[2]);
			Assert.AreEqual("", _lines[3]);
			Assert.AreEqual("c", _lines[4]);
			Assert.AreEqual("", _lines[5]);
		}
		
		bool TimeForSleep(string[] args)
		{
			return false;
		}
		
		bool TimeForDinner(string[] args)
		{
			return true;
		}
		
		[Test()]
		public void IfNode()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.SWEDISH);
			dialogueRunner.logger.AddListener(LogDialogueRunner);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			
			dialogueRunner.AddExpression("TimeForSleep", TimeForSleep);
			dialogueRunner.AddExpression("TimeForDinner", TimeForDinner);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation7.dia");
			
			DialogueScriptPrinter printer = new DialogueScriptPrinter(dialogueRunner);
			printer.PrintConversation("conversation7");
			
			dialogueRunner.StartConversation("conversation7");
			for(int i = 0; i < 1000; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Console.WriteLine("Output:");
			foreach(string s in _lines)
			{
				Console.WriteLine(s);
			}
			
			Assert.AreEqual(4, _lines.Count);
			Assert.AreEqual("Let's eat", _lines[0]);
			Assert.AreEqual("", _lines[1]);
			Assert.AreEqual("I'm hungry", _lines[2]);
			Assert.AreEqual("", _lines[3]);
		}
		
		[Test()]
		public void ElseNode()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.SWEDISH);
			dialogueRunner.logger.AddListener(LogDialogueRunner);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			
			dialogueRunner.AddExpression("TimeForSleep", TimeForSleep);
			dialogueRunner.AddExpression("TimeForDinner", TimeForDinner);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation8.dia");
			
			DialogueScriptPrinter printer = new DialogueScriptPrinter(dialogueRunner);
			printer.PrintConversation("conversation8");
			
			dialogueRunner.StartConversation("conversation8");
			for(int i = 0; i < 1000; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Console.WriteLine("Output:");
			foreach(string s in _lines)
			{
				Console.WriteLine(s);
			}
			
			Assert.AreEqual(4, _lines.Count);
			Assert.AreEqual("come on!", _lines[0]);
			Assert.AreEqual("", _lines[1]);
			Assert.AreEqual("let's party", _lines[2]);
			Assert.AreEqual("", _lines[3]);
		}
		
		[Test()]
		public void ImmediateNodeWithCustomName()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.SWEDISH);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation9.dia");
			
			DialogueNode n = dialogueRunner.GetDialogueNode("conversation9", "Örjan");
			Assert.IsNotNull(n);
		}		
		
		[Test()]
		public void Languages()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.ENGLISH);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation10.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation10");
			
			dialogueRunner.StartConversation("conversation10");
			for(int i = 0; i < 1000; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Console.WriteLine("Output:");
			foreach(string s in _lines)
			{
				Console.WriteLine(s);
			}
			
			Assert.AreEqual(4, _lines.Count);
			Assert.AreEqual("Hi", _lines[0]);
			Assert.AreEqual("", _lines[1]);
			Assert.AreEqual("Hi!", _lines[2]);
			Assert.AreEqual("", _lines[3]);
		}
		
		[Test()]
		public void SwitchLanguageInTheMiddleOfConversation()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.ENGLISH);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation10.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation10");
			
			dialogueRunner.StartConversation("conversation10");
			while(_lines.Count < 1)
			{
				dialogueRunner.Update(1.0f);
			}
			
			dialogueRunner.language = Language.SWEDISH;
			
			for(int i = 0; i < 1000; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Console.WriteLine("Output:");
			foreach(string s in _lines)
			{
				Console.WriteLine(s);
			}
			
			Assert.AreEqual(4, _lines.Count);
			Assert.AreEqual("Hi", _lines[0]);
			Assert.AreEqual("", _lines[1]);
			Assert.AreEqual("Hej!", _lines[2]);
			Assert.AreEqual("", _lines[3]);
		}
		
		[Test()]
		public void StartingOtherDialoguesAndStoryNodes()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation1.dia");
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation11.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation11");
			
			dialogueRunner.StartConversation("conversation11");
		
			for(int i = 0; i < 1000; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Console.WriteLine("Output:");
			foreach(string s in _lines)
			{
				Console.WriteLine(s);
			}
			
			Assert.AreEqual(6, _lines.Count);
			
			Assert.AreEqual("Hoho",  _lines[0]);
			Assert.AreEqual("", 	 _lines[1]);
			Assert.AreEqual("Hjälp!",_lines[2]);
			Assert.AreEqual("", 	 _lines[3]);
			Assert.AreEqual("Oh no", _lines[4]);
			Assert.AreEqual("", 	 _lines[5]);
		}
		
			
		static bool s_sunIsShining = false;
		
		bool SunIsShining(string[] args)
		{
			return s_sunIsShining;
		}
		
		[Test()]
		public void WaitForCondition()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.AddExpression("SunIsShining", SunIsShining);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation12.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation12");
			
			dialogueRunner.StartConversation("conversation12");
		
			for(int i = 0; i < 1000; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Assert.AreEqual(0, _lines.Count);
			
			for(int i = 0; i < 1000; i++)
			{
				if(i == 500) { s_sunIsShining = true; }
				dialogueRunner.Update(1.0f);
			}
			
			Assert.AreEqual(2, _lines.Count);
			Assert.AreEqual("Yay!",  _lines[0]);
			Assert.AreEqual("", 	 _lines[1]);
		}
		
		static bool s_fooWasCalled = false;
		void foo(string[] args) {
			s_fooWasCalled = true;
		}
		
		[Test()]
		public void CallAFunctionFromDialogueScript()
		{		
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddFunction("foo", foo);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation13.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation13");
			
			dialogueRunner.StartConversation("conversation13");
			dialogueRunner.Update(1.0f);
			
			Assert.IsTrue(s_fooWasCalled);
		}
		
		static bool s_pooWasCalledCorrectly = false;
		void poo(string[] args) {
			if(args.Length == 2 && args[0] == "cat" && args[1] == "dog") {
				s_pooWasCalledCorrectly = true;
			}
		}
		
		[Test()]
		public void CallAFunctionWithArguments()
		{		
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddFunction("poo", poo);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation14.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation14");
			
			dialogueRunner.StartConversation("conversation14");
			dialogueRunner.Update(1.0f);
			
			Assert.IsTrue(s_pooWasCalledCorrectly);
		}
		
		[Test()]
		public void CallAFunctionWithDotOperator()
		{
			s_pooWasCalledCorrectly = false;
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddFunction("poo", poo);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation15.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation15");
			
			dialogueRunner.StartConversation("conversation15");
			dialogueRunner.Update(1.0f);
			
			Assert.IsTrue(s_pooWasCalledCorrectly);
		}
		
		bool Pass(string[] args) {
			return true;
		}
		
		bool Fail(string[] args) {
			return false;
		}
		
		[Test()]
		public void AssertCommando()
		{
			_lines = new List<string>();
			
			s_pooWasCalledCorrectly = false;
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.AddExpression("Pass", Pass);
			dialogueRunner.AddExpression("Fail", Fail);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation16.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation16");
			
			Assert.Throws<GrimmAssertException>(() => {
				dialogueRunner.StartConversation("conversation16");
				for(int i = 0; i < 100; i++)
				{
					dialogueRunner.Update(1.0f);
				}
			});
			
			Assert.AreEqual(4, _lines.Count);
			Assert.AreEqual("hej1", _lines[0]);
			Assert.AreEqual("", _lines[1]);
			Assert.AreEqual("hej2", _lines[2]);
			Assert.AreEqual("", _lines[3]);
		}
		
		[Test()]
		public void StopCommando()
		{
			_lines = new List<string>();
			
			s_pooWasCalledCorrectly = false;
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation17.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation17");
			
			dialogueRunner.StartConversation("conversation17");
			for(int i = 0; i < 100; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Console.WriteLine("OUTPUT:");
			foreach(string s in _lines) {
				Console.WriteLine(s);
			}
			
			Assert.AreEqual(2, _lines.Count);
			Assert.AreEqual("hej1", _lines[0]);
			Assert.AreEqual("", _lines[1]);
		}
		
		static string s_monsterState = "Happy";
		bool StateIs(string[] args)
		{
			if(args[0] == "Monster" && args[1] == s_monsterState) {
				return true;
			}
			else {
				return false;
			}
		}
		
		[Test()]
		public void WaitForConditionWithArguments()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.AddExpression("StateIs", StateIs);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation18.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation18");
			
			dialogueRunner.StartConversation("conversation18");
		
			for(int i = 0; i < 100; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Assert.AreEqual(0, _lines.Count);
			
			for(int i = 0; i < 100; i++)
			{
				if(i == 50) { s_monsterState = "Angry"; }
				dialogueRunner.Update(1.0f);
			}
			
			Assert.AreEqual(2, _lines.Count);
			Assert.AreEqual("Ahhhhh!",  _lines[0]);
			Assert.AreEqual("", 	 _lines[1]);
		}
		
		
		static bool s_tensionWentUp = false;
		bool TensionWentUp()
		{
			return s_tensionWentUp;
		}
		
		static bool s_somethingElse = false;
		bool SomethingElse()
		{
			return s_somethingElse;
		}
		
		[Test()]
		public void ListeningForEvent()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(LogDialogueRunner);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation19.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation19");
			
			dialogueRunner.StartConversation("conversation19");
			
			Assert.AreEqual(1, _lines.Count);
			Assert.AreEqual("Blah blah blah",  _lines[0]);
					
			dialogueRunner.EventHappened("TensionWentUp");
			
			for(int i = 0; i < 500; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Assert.AreEqual(4, _lines.Count);
			Assert.AreEqual("Oh!",  _lines[1]);
			Assert.AreEqual("",  _lines[2]);
			Assert.AreEqual("",  _lines[3]);
			
			Assert.IsFalse(dialogueRunner.ConversationIsRunning("conversation19"));
			               
			dialogueRunner.EventHappened("SomethingElse"); // this is not supposed to do anything since the story is over
			
			for(int i = 0; i < 500; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Assert.AreEqual(4, _lines.Count);
		}
		
		[Test()]
		public void ListeningForEventAndSaveInBetween()
		{
			_lines = new List<string>();
			
			{
				RelayTwo relay = new RelayTwo();
				relay.CreateTable(DialogueNode.TABLE_NAME);
		
				DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
				dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
				dialogueRunner.logger.AddListener(LogDialogueRunner);
				
				DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
				scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation19.dia");
				
				DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
				scriptPrinter.PrintConversation("conversation19");
				
				dialogueRunner.StartConversation("conversation19");
				
				Assert.AreEqual(1, _lines.Count);
				Assert.AreEqual("Blah blah blah",  _lines[0]);
				relay.SaveAll("save.xml");
			}
			
			{
				RelayTwo relay = new RelayTwo("save.xml");
				DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
				dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
				dialogueRunner.logger.AddListener(LogDialogueRunner);
				
				dialogueRunner.EventHappened("TensionWentUp");
				
				for(int i = 0; i < 500; i++)
				{
					dialogueRunner.Update(1.0f);
				}
				
				Assert.AreEqual(4, _lines.Count);
				Assert.AreEqual("Oh!",  _lines[1]);
				Assert.AreEqual("",  _lines[2]);
				Assert.AreEqual("",  _lines[3]);
				
				Assert.IsFalse(dialogueRunner.ConversationIsRunning("conversation19"));
				               
				dialogueRunner.EventHappened("SomethingElse"); // this is not supposed to do anything since the story is over
				
				for(int i = 0; i < 500; i++)
				{
					dialogueRunner.Update(1.0f);
				}
				
				Assert.AreEqual(4, _lines.Count);
			}
		}
		
		[Test()]
		public void Broadcast()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(LogDialogueRunner);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation20.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation20");
			
			dialogueRunner.StartConversation("conversation20");
			for(int i = 0; i < 100; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Assert.AreEqual(2, _lines.Count);
			Assert.AreEqual("Yay!",  _lines[0]);
			Assert.AreEqual("",  _lines[1]);
		}
		
		[Test()]
		public void ListeningDialogueNodeWithNoBranch()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(LogDialogueRunner);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation21.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation21");
			
			dialogueRunner.StartConversation("conversation21");
			
			Assert.AreEqual(0, _lines.Count);
					
			for(int i = 0; i < 500; i++)
			{
				if(i == 250) {
					dialogueRunner.EventHappened("RainStarted");
				}
				dialogueRunner.Update(1.0f);
			}
			
			Assert.AreEqual(2, _lines.Count);
			Assert.AreEqual("Where is my umbrella?",  _lines[0]);
			Assert.AreEqual("",  _lines[1]);
		}
		
		[Test()]
		public void Cancel()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(LogDialogueRunner);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation22.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation22");
			
			dialogueRunner.StartConversation("conversation22");
					
			for(int i = 0; i < 500; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Assert.AreEqual(2, _lines.Count);
			Assert.AreEqual("Oh no!", _lines[0]);
			Assert.AreEqual("", _lines[1]);
		}
		
		[Test()]
		public void Focus()
		{
			_lines = new List<string>();
			bool madeActive = false;
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(LogDialogueRunner);
			dialogueRunner.AddFocusConversationListener((string pConversation) => {
				if(pConversation == "conversation23") { madeActive = true; }
			});			                                                     
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation23.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation23");
			
			dialogueRunner.StartConversation("conversation23");
					
			for(int i = 0; i < 500; i++)
			{
				dialogueRunner.Update(1.0f);
			}
			
			Assert.IsTrue(madeActive);
		}
		
		[Test()]
		public void EmptyLoop()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation24.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation24");
			
			dialogueRunner.StartConversation("conversation24");
			dialogueRunner.Update(1.0f);
			
			Assert.IsTrue(dialogueRunner.ConversationIsRunning("conversation24")); // never ends
		}
		
		[Test()]
		public void SaySomethingEachFrameUsingLoop()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation25.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation25");
			
			dialogueRunner.StartConversation("conversation25");
			for(int i = 0; i < 10; i++) {
				dialogueRunner.Update(5.0f);
			}
			
			Assert.AreEqual(20, _lines.Count);
			
			for(int i = 0; i < 10; i++) {
				Assert.AreEqual("hej", _lines[i * 2]);
				Assert.AreEqual("", _lines[i * 2 + 1]);
			}
		}
		
		[Test()]
		public void BreakOutOfLoop()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(Console.WriteLine);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation26.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation26");
			
			Console.WriteLine("Starting...");
			dialogueRunner.StartConversation("conversation26");
		
			for(int i = 0; i < 10; i++) {
				Console.WriteLine("Update " + i);
				dialogueRunner.Update(0.3f);
			}
			
			foreach(string s in _lines) {
				Console.WriteLine(s);
			}
			
			Assert.IsFalse(dialogueRunner.ConversationIsRunning("conversation26"));
			Assert.AreEqual(2, _lines.Count);
			Assert.AreEqual("yo", _lines[0]);
			Assert.AreEqual("", _lines[1]);
		}
		
		[Test()]
		public void WaitBlocks()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(Console.WriteLine);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation27.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation27");		
		}
		
		[Test()]
		public void WaitForMultipleConditions()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(Console.WriteLine);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation28.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation28");
		}
		
		[Test()]
		public void Elif()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(Console.WriteLine);
			dialogueRunner.AddExpression("A", ((string[] args) => (false)));
			dialogueRunner.AddExpression("B", ((string[] args) => (false)));
			dialogueRunner.AddExpression("C", ((string[] args) => (true)));
			dialogueRunner.AddExpression("D", ((string[] args) => (false)));
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation29.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation29");
			
			dialogueRunner.StartConversation("conversation29");
			for(int i = 0; i < 100; i++)
			{
				dialogueRunner.Update(0.5f);
			}
			
			Assert.AreEqual(2, _lines.Count);
			Assert.AreEqual("C", _lines[0]);
		}
		
		[Test()]
		public void ShortIf()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(Console.WriteLine);
			dialogueRunner.AddExpression("Foo", ((string[] args) => (true)));
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation30.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation30");
		}
		
		[Test()]
		public void StopAnotherConversation()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(Console.WriteLine);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation32.dia");
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation31.dia");
			
			dialogueRunner.StartConversation("conversation32");
			
			for(int i = 0; i < 100; i++)
			{
				dialogueRunner.Update(0.1f);
			}
			
			dialogueRunner.StartConversation("conversation31");
			
			for(int i = 0; i < 100; i++)
			{
				dialogueRunner.Update(0.1f);
			}
			
			Assert.IsFalse(dialogueRunner.ConversationIsRunning("conversation32"));
			Assert.IsFalse(dialogueRunner.ConversationIsRunning("conversation31"));
		}
		
		[Test()]
		public void InterruptCommando()
		{
			_lines = new List<string>();
			
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(Console.WriteLine);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation1.dia");
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation33.dia");
			
			dialogueRunner.StartConversation("conversation33");
			
			for(int i = 0; i < 300; i++)
			{
				dialogueRunner.Update(0.1f);
			}
			
			Assert.AreEqual(10, _lines.Count);
			
			Assert.AreEqual("before", 	_lines[0]);
			Assert.AreEqual("", 		_lines[1]);
			Assert.AreEqual("Hoho", 	_lines[2]);
			Assert.AreEqual("", 		_lines[3]);
			Assert.AreEqual("Hjälp!", 	_lines[4]);
			Assert.AreEqual("",	 		_lines[5]);
			Assert.AreEqual("Oh no", 	_lines[6]);
			Assert.AreEqual("", 		_lines[7]);
			Assert.AreEqual("after", 	_lines[8]);
			Assert.AreEqual("",			_lines[9]);
		}
		
		[Test()]
		public void ChoiceKeyword()
		{
			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation34.dia");
			
			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation34");
		}

		[Test()]
		public void WaitForEvents()
		{
			_lines = new List<string>();

			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(Console.WriteLine);
			dialogueRunner.AddExpression("Whatever", ((string[] args) => (true)));

			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation35.dia");

			//DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			//scriptPrinter.PrintConversation("conversation35");

			dialogueRunner.StartConversation("conversation35");
			for(int i = 0; i < 100; i++)
			{
				dialogueRunner.Update(0.5f);
			}
			
			Assert.AreEqual(2, _lines.Count);
			Assert.AreEqual("Yeah1", _lines[0]);

			dialogueRunner.EventHappened("Bam");

			for(int i = 0; i < 100; i++)
			{
				dialogueRunner.Update(0.5f);
			}

			Assert.AreEqual(4, _lines.Count);
			Assert.AreEqual("Yeah2", _lines[2]);
		}

		[Test()]
		public void WaitSpecifiedTime()
		{
			_lines = new List<string>();

			RelayTwo relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
	
			DialogueRunner dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			dialogueRunner.logger.AddListener(Console.WriteLine);

			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile("../conversations/conversation36.dia");

			DialogueScriptPrinter scriptPrinter = new DialogueScriptPrinter(dialogueRunner);
			scriptPrinter.PrintConversation("conversation36");

			dialogueRunner.StartConversation("conversation36");

			dialogueRunner.Update(1.0f);
			Assert.AreEqual(0, _lines.Count);

			dialogueRunner.Update(3.0f);
			Assert.AreEqual(2, _lines.Count);
			Assert.AreEqual("Woo!", _lines[0]);
		}

		string GetTextFromOption(BranchingDialogueNode branchingNode, int nr)
		{
			string conversationName = branchingNode.conversation;
			TimedDialogueNode option = _dialogueRunner.GetDialogueNode(conversationName, branchingNode.nextNodes[nr]) as TimedDialogueNode;
			return option.line;
		}

		[Test()]
		public void RemoveOptionsAfterTheyHaveBeenSelected()
		{
			DialogueScriptLoader loader = new DialogueScriptLoader(_dialogueRunner);
			loader.LoadDialogueNodesFromFile("../conversations/conversation37.dia");			

			_dialogueRunner.StartConversation("conversation37");
			BranchingDialogueNode n = _dialogueRunner.GetActiveBranchingDialogueNode("conversation37");

			Assert.IsNotNull(n);
			Assert.AreEqual (3, n.nextNodes.Length);
			Assert.AreEqual ("a", GetTextFromOption (n, 0));
			Assert.AreEqual ("b", GetTextFromOption (n, 1));
			Assert.AreEqual ("c", GetTextFromOption (n, 2));

			n.Choose (1);

			for (int i = 0; i < 100; i++) {
				_dialogueRunner.Update (0.1f);
			}

			BranchingDialogueNode sameNodeButLaterAfterLooping = _dialogueRunner.GetActiveBranchingDialogueNode("conversation37");
			Assert.AreEqual (2, sameNodeButLaterAfterLooping.nextNodes.Length);
			Assert.AreEqual ("a", GetTextFromOption (sameNodeButLaterAfterLooping, 0));
			Assert.AreEqual ("c", GetTextFromOption (sameNodeButLaterAfterLooping, 1));

			n.Choose (1);

			BranchingDialogueNode nodeAgain = _dialogueRunner.GetActiveBranchingDialogueNode("conversation37");
			Assert.AreEqual (1, nodeAgain.nextNodes.Length);
			Assert.AreEqual ("a", GetTextFromOption (nodeAgain, 0));

			n.Choose (0);

			BranchingDialogueNode finalTime = _dialogueRunner.GetActiveBranchingDialogueNode("conversation37");
			Assert.AreEqual (0, finalTime.nextNodes.Length);
		}
	}
}

