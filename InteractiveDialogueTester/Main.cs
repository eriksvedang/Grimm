using System;
using GrimmLib;
using RelayLib;
using System.Collections.Generic;
using GameTypes;

namespace InteractiveDialogueTester
{	
	class MainClass
	{	
		public static void Main(string[] args)
		{			
			try 
			{
				RunDialogue();
			}
			catch(Exception e)
			{
				Console.WriteLine("Error of type " + e.GetType() + " with message: " + e.Message + " callstack: " + e.StackTrace);
			}			
		}

		static void RunDialogue()
		{
			string conversationName = "meeting"; // "PixieMeeting1";
			
			RelayTwo relay;
			DialogueRunner dialogueRunner;
			
			relay = new RelayTwo();
			relay.CreateTable(DialogueNode.TABLE_NAME);
			
			dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			dialogueRunner.AddExpression("CoinFlip", CoinFlip);
			dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSpeech);
			dialogueRunner.logger.AddListener(Log);
			
			DialogueScriptLoader scriptLoader = new DialogueScriptLoader(dialogueRunner);
			scriptLoader.LoadDialogueNodesFromFile(conversationName + ".dia");
			
			DialogueScriptPrinter printer = new DialogueScriptPrinter(dialogueRunner);
			printer.PrintConversation(conversationName);
			
			Console.WriteLine(" - " + conversationName + " - ");
			dialogueRunner.StartConversation(conversationName);
			
			while(dialogueRunner.ConversationIsRunning(conversationName))
			{				
				//printer.PrintConversation(conversationName);
				
				dialogueRunner.Update(1.0f);
				DialogueNode activeDialogueNode = dialogueRunner.GetActiveBranchingDialogueNode(conversationName);
				if(activeDialogueNode is BranchingDialogueNode)
				{
					BranchingDialogueNode branchingNode = activeDialogueNode as BranchingDialogueNode;
					
					//printer.PrintConversation(conversationName);
					
					int i = 1;
					Console.WriteLine("Choose an alternative:");
					foreach(string optionNodeName in branchingNode.nextNodes)
					{
						TimedDialogueNode optionNode = dialogueRunner.GetDialogueNode(conversationName, optionNodeName) as TimedDialogueNode;
						Console.WriteLine(i++ + ". " + optionNode.line);
					}
					
					int choice = -1;
					while(choice < 0 || choice > branchingNode.nextNodes.Length - 1) {
						try {
							choice = 0; //Convert.ToInt32(Console.ReadLine()) - 1;
						}
						catch {
							choice = -1;
						}
					}
					
					branchingNode.Choose(choice);	
				}
			}
		}
		
		private static void OnSpeech(Speech pSpeech)
		{
			if(pSpeech.line != "") {
				Console.WriteLine(pSpeech.speaker + ": \"" + pSpeech.line + "\"");
			}
		}
		
		private static void Log(string pMessage)
		{
			Console.WriteLine("Log: " + pMessage);
		}
		
		static Random r = new Random((int)DateTime.Now.Millisecond);
		
		private static bool CoinFlip(string[] args)
		{
			return r.Next(2) == 0;
		}
	}
}
