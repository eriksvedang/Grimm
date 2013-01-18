//#define WRITE_DEBUG

using System;
using GameTypes;
using RelayLib;
using System.Text;

namespace GrimmLib
{
	public class DialogueScriptPrinter
	{
		DialogueRunner _dialogueRunner;
		StringBuilder _output;
		string _conversation;
		int _indentationLevel;
		
		public DialogueScriptPrinter(DialogueRunner pDialogueRunner)
		{
			D.isNull(pDialogueRunner);
			_dialogueRunner = pDialogueRunner;
		}
		
		public void PrintConversation(string pConversation)
		{
			_conversation = pConversation;
			_indentationLevel = 0;
			DialogueNode startNode = _dialogueRunner.GetDialogueNode(pConversation, "__Start__");
			_output = new StringBuilder();
			SwitchOnNode(startNode);
			Console.WriteLine("Printing conversation '" + pConversation + "':");
			Console.WriteLine(_output.ToString());
		}
		
		private void SwitchOnNode(DialogueNode pDialogueNode)
		{
			D.isNull(pDialogueNode);
			
			if(pDialogueNode.isOn) {
				_output.Append("ON ---> ");
			}
			
#if WRITE_DEBUG
			Console.WriteLine("Switching on node " + pDialogueNode.name + " with indentation level " + _indentationLevel);
#endif
			
			if(pDialogueNode is BranchingDialogueNode)
			{
				PrintBranchingDialogueNode(pDialogueNode as BranchingDialogueNode);
			}
			else if(pDialogueNode is ConversationEndDialogueNode)
			{
				PrintConversationEndDialogueNode(pDialogueNode as ConversationEndDialogueNode);
			}
			else if(pDialogueNode is ConversationStartDialogueNode)
			{
				PrintConversationStartDialogueNode(pDialogueNode as ConversationStartDialogueNode);
			}
			else if(pDialogueNode is TimedDialogueNode)
			{
				PrintTimedDialogueNode(pDialogueNode as TimedDialogueNode);
			}
			else if(pDialogueNode is UnifiedEndNodeForScope)
			{
				PrintUnifiedEndNodeForScope(pDialogueNode as UnifiedEndNodeForScope);
			}
			else if(pDialogueNode is GotoDialogueNode)
			{
				PrintGotoNode(pDialogueNode as GotoDialogueNode);
			}
			else if(pDialogueNode is IfDialogueNode)
			{
				PrintIfNode(pDialogueNode as IfDialogueNode);
			}
			else if(pDialogueNode is ImmediateNode)
			{
				PrintImmediateNode(pDialogueNode as ImmediateNode);
			}
			else if(pDialogueNode is StartCommandoDialogueNode)
			{
				PrintStartCommandoDialogueNode(pDialogueNode as StartCommandoDialogueNode);
			}
			else if(pDialogueNode is StopDialogueNode)
			{
				PrintStopCommandoDialogueNode(pDialogueNode as StopDialogueNode);
			}
			else if(pDialogueNode is InterruptDialogueNode)
			{
				PrintInterruptDialogueNode(pDialogueNode as InterruptDialogueNode);
			}
			else if(pDialogueNode is WaitDialogueNode)
			{
				PrintWaitDialogueNode(pDialogueNode as WaitDialogueNode);
			}
			else if(pDialogueNode is CallFunctionDialogueNode)
			{
				PrintCallFunctionDialogueNode(pDialogueNode as CallFunctionDialogueNode);
			}
			else if(pDialogueNode is AssertDialogueNode)
			{
				PrintAssertDialogueNode(pDialogueNode as AssertDialogueNode);
			}
			else if(pDialogueNode is ListeningDialogueNode)
			{
				PrintListeningDialogueNode(pDialogueNode as ListeningDialogueNode);
			}
			else if(pDialogueNode is SilentDialogueNode)
			{
				PrintSilentDialogueNode(pDialogueNode as SilentDialogueNode);
			}
			else if(pDialogueNode is BroadcastDialogueNode)
			{
				PrintBroadcastDialogueNode(pDialogueNode as BroadcastDialogueNode);
			}
			else if(pDialogueNode is CancelDialogueNode)
			{
				PrintCancelDialogueNode(pDialogueNode as CancelDialogueNode);
			}
			else if(pDialogueNode is FocusDialogueNode)
			{
				PrintFocusNode(pDialogueNode as FocusDialogueNode);
			}
			else if(pDialogueNode is DefocusDialogueNode)
			{
				PrintDefocusNode(pDialogueNode as FocusDialogueNode);
			}
			else if(pDialogueNode is LoopDialogueNode)
			{
				PrintLoopDialogueNode(pDialogueNode as LoopDialogueNode);
			}
			else if(pDialogueNode is BreakDialogueNode)
			{
				PrintBreakDialogueNode(pDialogueNode as BreakDialogueNode);
			}
			else if(pDialogueNode is ExpressionDialogueNode)
			{
				PrintExpressionDialogueNode(pDialogueNode as ExpressionDialogueNode);
			}
			else
			{
				throw new GrimmException("Don't understand node type " + pDialogueNode.GetType());
			}
		}
		
		private void PrintTimedDialogueNode(TimedDialogueNode pTimedDialogueNode)
		{
			Indentation();
			_output.Append(pTimedDialogueNode.speaker + ": \"" + pTimedDialogueNode.line + "\"");
			_output.Append("\n");
			
			if(pTimedDialogueNode.nextNode != "")
			{
				DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pTimedDialogueNode.nextNode);
				SwitchOnNode(nextNode);
			}
			else
			{
				throw new GrimmException("TimedDialogueNode with name '" + pTimedDialogueNode.name + "' doesn't have a next node");
			}
		}
		
		private void PrintImmediateNode(ImmediateNode pImmediateNode)
		{
			Indentation();
			_output.Append(pImmediateNode.name + "\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pImmediateNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintGotoNode(GotoDialogueNode pGotoDialogueNode)
		{
			Indentation();
			_output.Append("GOTO " + pGotoDialogueNode.linkedNode + "\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pGotoDialogueNode.nextNode);
			SwitchOnNode(nextNode);
		}

		private void PrintSilentDialogueNode(SilentDialogueNode par1)
		{
			Indentation();
			_output.Append("SILENT NODE (won't continue from here) \n");
		}
		
		private void PrintStartCommandoDialogueNode(StartCommandoDialogueNode pStartCommandoNode)
		{
			Indentation();
			_output.Append("START " + pStartCommandoNode.commando + "\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pStartCommandoNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintInterruptDialogueNode(InterruptDialogueNode pInterruptNode)
		{
			Indentation();
			_output.Append("INTERRUPT " + pInterruptNode.interruptingConversation + "\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pInterruptNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintStopCommandoDialogueNode(StopDialogueNode pStopNode)
		{
			Indentation();
			_output.Append("STOP " + pStopNode.conversationToStop + "\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pStopNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintCancelDialogueNode(CancelDialogueNode pCancelNode)
		{
			Indentation();
			_output.Append("CANCEL " + pCancelNode.handle + "\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pCancelNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintWaitDialogueNode(WaitDialogueNode pWaitNode)
		{
			Indentation();
			_output.Append("WAIT_UNTIL expressions: ");

			foreach(var e in pWaitNode.expressions) {
				_output.Append(e.expression + ", ");
			}
			if(pWaitNode.eventName != "") {
				_output.Append("LISTEN for event: " + pWaitNode.eventName);
			}
			_output.Append("\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pWaitNode.nextNode);
			SwitchOnNode(nextNode);
		}

		private void PrintListeningDialogueNode(ListeningDialogueNode pListeningNode)
		{
			Indentation();
			_output.Append("LISTEN_FOR " + pListeningNode.eventName + " " + pListeningNode.handle + " {\n");
			
			if(pListeningNode.hasBranch) {
				_indentationLevel++;
				DialogueNode branchStartNode = _dialogueRunner.GetDialogueNode(_conversation, pListeningNode.branchNode);
				SwitchOnNode(branchStartNode);
				_indentationLevel--;
			}
			_output.Append("}\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pListeningNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintBroadcastDialogueNode(BroadcastDialogueNode pBroadcastNode)
		{
			Indentation();
			_output.Append("BROADCAST " + pBroadcastNode.eventName + "\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pBroadcastNode.nextNode);
			SwitchOnNode(nextNode);
		}

		void PrintFocusNode(FocusDialogueNode pNode)
		{
			Indentation();
			_output.Append("FOCUS\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		void PrintDefocusNode(FocusDialogueNode pNode)
		{
			Indentation();
			_output.Append("DEFOCUS\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintCallFunctionDialogueNode(CallFunctionDialogueNode pFunctionNode)
		{
			Indentation();
			
			StringBuilder sb = new StringBuilder();
			int i = 0;
			foreach(string arg in pFunctionNode.args) {
				sb.Append(arg);
				i++;
				if(i < pFunctionNode.args.Length) {
					sb.Append(", ");
				}
			}
			
			_output.Append("CALL FUNCTION " + pFunctionNode.function + "(" + sb.ToString() + ")\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pFunctionNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintIfNode(IfDialogueNode pIfDialogueNode)
		{
			Indentation();
			
			_output.Append("IF");
			_indentationLevel++;
			SwitchOnNode(pIfDialogueNode.ifTrueNode);
			_indentationLevel--;
			foreach(ExpressionDialogueNode elifNode in pIfDialogueNode.elifNodes) {
				_output.Append("ELIF");
				_indentationLevel++;
				SwitchOnNode(elifNode);
				_indentationLevel--;	
			}
			if(pIfDialogueNode.ifFalseNode != null) {
				_output.Append("ELSE\n");
				_indentationLevel++;
				SwitchOnNode(pIfDialogueNode.ifFalseNode);
				_indentationLevel--;
			}
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pIfDialogueNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintAssertDialogueNode(AssertDialogueNode pAssertNode) 
		{
			Indentation();
			
			_output.Append("ASSERT " + pAssertNode.expression + "\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pAssertNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintLoopDialogueNode(LoopDialogueNode pLoopNode) 
		{
			Indentation();
			
			_output.Append("LOOP{\n");
			
			_indentationLevel++;
			DialogueNode branchStartNode = _dialogueRunner.GetDialogueNode(_conversation, pLoopNode.branchNode);
			SwitchOnNode(branchStartNode);
			_indentationLevel--;
			
			_output.Append("}\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pLoopNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintBreakDialogueNode(BreakDialogueNode pBreakNode) 
		{
			Indentation();
			
			_output.Append("BREAK\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pBreakNode.nextNode);
			SwitchOnNode(nextNode);
		}
		
		private void PrintExpressionDialogueNode(ExpressionDialogueNode pExpressionNode)
		{
			Indentation();
			
			_output.Append("Expression " + pExpressionNode.expression + "\n");
			
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pExpressionNode.nextNode);
			SwitchOnNode(nextNode);
		}
	
		private void PrintBranchingDialogueNode(BranchingDialogueNode pBranchingDialogueNode)
		{
			D.isNull(pBranchingDialogueNode);
			
			Indentation();
			_output.Append("{\n");
			_indentationLevel++;
			
			int optionNr = 1;
			foreach(string s in pBranchingDialogueNode.nextNodes)
			{
				TimedDialogueNode optionNode = _dialogueRunner.GetDialogueNode(_conversation, s) as TimedDialogueNode;
				D.isNull(optionNode);
				
				Indentation();
				_output.Append(optionNr++ + ". \"" + optionNode.line + "\":\n");
				
				D.assert(optionNode.nextNode != "");
				DialogueNode nodePointedToByOption = _dialogueRunner.GetDialogueNode(_conversation, optionNode.nextNode);
				D.isNull(nodePointedToByOption);
				
				_indentationLevel++;
				SwitchOnNode(nodePointedToByOption);
				_indentationLevel--;
			}
			
			_indentationLevel--;
			Indentation();
			_output.Append("}\n");
			
			D.assert(pBranchingDialogueNode.name != "");
			UnifiedEndNodeForScope unifiedEndNodeForScope = _dialogueRunner.GetDialogueNode(_conversation, pBranchingDialogueNode.unifiedEndNodeForScope) as UnifiedEndNodeForScope;
			D.isNull(unifiedEndNodeForScope);
			D.assert(unifiedEndNodeForScope.name != "");
			//Console.WriteLine("Unified end node " + unifiedEndNodeForScope.name + " has next node " + unifiedEndNodeForScope.nextNode);
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, unifiedEndNodeForScope.nextNode);
			//Console.WriteLine("Next node has type " + nextNode.GetType());
			D.isNull(nextNode);
			
			SwitchOnNode(nextNode);
		}
		
		private void PrintUnifiedEndNodeForScope(UnifiedEndNodeForScope pUnifiedEndNodeForScope)
		{
			D.isNull(pUnifiedEndNodeForScope);
			
			//Indentation();
			//_output.Append("end\n");
		}
	
		private void PrintConversationStartDialogueNode(ConversationStartDialogueNode pConversationStartDialogueNode)
		{
			_output.Append("Start -> \n");
			DialogueNode nextNode = _dialogueRunner.GetDialogueNode(_conversation, pConversationStartDialogueNode.nextNode);
			SwitchOnNode(nextNode);
		}
			
		private void PrintConversationEndDialogueNode(ConversationEndDialogueNode pConversationEndDialogueNode)
		{
			Indentation();
			_output.Append("-> End\n");
		}
		
		private void Indentation()
		{
			for(int i = 0; i < _indentationLevel; i++)
			{
				_output.Append("\t");
			}
		}
	}
}

