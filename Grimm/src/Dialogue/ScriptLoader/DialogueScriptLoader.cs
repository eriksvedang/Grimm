
//#define DEBUG_WRITE
//#define WRITE_NODE_LINKS

using System;
using System.Collections.Generic;
using RelayLib;
using GameTypes;
using System.Text;
using System.IO;

namespace GrimmLib
{
	public class DialogueScriptLoader
	{
		const string NAME_OF_START_NODE = "__Start__";
		const string NAME_OF_END_NODE = "__End__";
		
		DialogueRunner _dialogueRunner;
		string _playerCharacterName = "Sebastian";
		string _conversationName;
		Language _language;
		List<Token> _tokens;
		Token[] _lookahead;
		int _lookaheadIndex = 0; // circular index
		int _nextTokenIndex;
		int k = 2; // how many lookahead symbols
		int _nodeCounter;
		Stack<DialogueNode> _loopStack;
		
		public DialogueScriptLoader(DialogueRunner pDialogueRunner)
		{
			D.isNull(pDialogueRunner);
			_dialogueRunner = pDialogueRunner;
		}
		
		public void CreateDialogueNodesFromString(string pString, string pConversation)
		{
			using(StringReader sr = new StringReader(pString)) 
			{
				CreateDialogueNodes(sr, pConversation);
				sr.Close();
			}
		}
		
		public void LoadDialogueNodesFromFile(string pFilepath)
		{
			
			string conversation = DialogueScriptLoader.GetConversationNameFromFilepath(pFilepath);
			using(StreamReader sr = File.OpenText(pFilepath)) 
			{
				CreateDialogueNodes(sr, conversation);
				sr.Close();
			}
		}
		
		private void CreateDialogueNodes(TextReader pTextReader, string pConversation)
		{
			_conversationName = pConversation;
			
			Tokenizer tokenizer = new Tokenizer();
			_tokens = tokenizer.process(pTextReader);
			_loopStack = new Stack<DialogueNode>();
			
			#if PRINT_TOKENS
			Console.WriteLine("Tokens:");
			foreach(Token t in _tokens)
			{
				Console.WriteLine(t.getTokenType().ToString() + ": " + t.getTokenString());
			}
			#endif
			
			_nextTokenIndex = 0;
			_lookaheadIndex = 0;
			_lookahead = new Token[k];
			for (int i = 0; i < k; i++) {
				ConsumeCurrentToken();
			}
			
			Languages();
			//CreateTreeOfDialogueNodes();
		}

		public static string GetConversationNameFromFilepath(string pFilepath)
		{
			if(pFilepath == null || pFilepath == "") {
				throw new GrimmException("Filepath is empty!");
			}
            int index = pFilepath.LastIndexOf("/");
            int index2 = pFilepath.LastIndexOf(@"\");
            if (index2 > index)
                index = index2;
            string filenameWithEnding = pFilepath.Substring(index + 1);
			string conversationName = filenameWithEnding;
			int i = filenameWithEnding.LastIndexOf(".");
			if(i > -1) {
				conversationName = filenameWithEnding.Substring(0, i);
			}
			return conversationName;
		}
		
		private void Languages()
		{
			#if DEBUG_WRITE
			Console.WriteLine("Languages()");
			#endif	
			
			_language = Language.DEFAULT;
			
			while(lookAheadType(1) != Token.TokenType.EOF) 
			{
				if(lookAheadType(1) == Token.TokenType.LANGUAGE) {
					match(Token.TokenType.LANGUAGE);
					Token languageNameToken = match(Token.TokenType.NAME);
					string languageName = languageNameToken.getTokenString();
					switch(languageName.ToLower()) {
					case "swedish":
						_language = Language.SWEDISH;
						break;
					case "english":
						_language = Language.ENGLISH;
						break;
					default:
						throw new GrimmException("Can't handle language '" + languageName + "'");
					}
				}
				
				#if DEBUG_WRITE
				Console.WriteLine("Creating dialogue nodes for language " + _language);
				#endif	
				
				CreateTreeOfDialogueNodes();
			}
		}

		private void CreateTreeOfDialogueNodes() 
		{
			_nodeCounter = 0;			

			ConversationStartDialogueNode startNode = _dialogueRunner.Create<ConversationStartDialogueNode>(_conversationName, _language, NAME_OF_START_NODE);
			ConversationEndDialogueNode endNode = _dialogueRunner.Create<ConversationEndDialogueNode>(_conversationName, _language, NAME_OF_END_NODE);
			
			#if DEBUG_WRITE
			Console.WriteLine("Created a ConversationStartDialogueNode with name '" + startNode.name + "'");
			Console.WriteLine("Created a ConversationEndDialogueNode with name '" + endNode.name + "'");
			#endif
			
			// Start parsing
			Nodes(startNode, endNode);
		}
		
		private void Nodes(DialogueNode pPrevious, DialogueNode pScopeEndNode) 
		{
			#if DEBUG_WRITE
			Console.WriteLine("Nodes()");
			#endif	
			
			while(	lookAheadType(1) != Token.TokenType.EOF &&
				   	lookAheadType(1) != Token.TokenType.BLOCK_END &&
			       	lookAheadType(1) != Token.TokenType.QUOTED_STRING &&
			      	lookAheadType(1) != Token.TokenType.LANGUAGE)
			{
				DialogueNode n = Statement(pPrevious);
				
				if(n != null) {
					pPrevious = n;
				}
			}
						
			AddLinkFromPreviousNode(pPrevious, pScopeEndNode);
		}
		
		private DialogueNode Statement(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			//Console.WriteLine("Statement() Lookahead: " + lookAhead(1).getTokenString());
			#endif
			
			if (lookAheadType(1) == Token.TokenType.NEW_LINE) {
				match(Token.TokenType.NEW_LINE);
			}
			else if (lookAheadType(1) == Token.TokenType.EOF) {
				match(Token.TokenType.EOF);
			}
			else if (lookAheadType(1) == Token.TokenType.NAME && 
			         lookAheadType(2) == Token.TokenType.QUOTED_STRING)
            {
				return VisitTimedDialogueNode(pPrevious);
            }
			else if (lookAheadType(1) == Token.TokenType.NAME && 
			         lookAheadType(2) == Token.TokenType.PARANTHESIS_LEFT)
            {
				return VisitFunctionDialogueNode(pPrevious);
            }
			else if (lookAheadType(1) == Token.TokenType.NAME && 
			         lookAheadType(2) == Token.TokenType.DOT)
            {
				return VisitFunctionDialogueNode(pPrevious);
            }
			else if( lookAheadType(1) == Token.TokenType.GOTO) {
				return VisitGotoDialogueNode(pPrevious);
			}
			else if( lookAheadType(1) == Token.TokenType.START) {
				return VisitStartCommandoDialogueNode(pPrevious);
			}
			else if( lookAheadType(1) == Token.TokenType.IF) {
				return VisitIfDialogueNode(pPrevious);
			}
			else if( lookAheadType(1) == Token.TokenType.ASSERT) {
				return VisitAssertDialogueNode(pPrevious);
			}
			else if( lookAheadType(1) == Token.TokenType.STOP) {
				return VisitStopDialogueNode(pPrevious);
			}
			else if( lookAheadType(1) == Token.TokenType.BROADCAST) {
				return VisitBroadcastDialogueNode(pPrevious);
			}
			else if( lookAheadType(1) == Token.TokenType.LISTEN) {
				return VisitListeningDialogueNode(pPrevious);
			}
			else if( lookAheadType(1) == Token.TokenType.CANCEL) {
				return VisitCancelDialogueNode(pPrevious);
			}
			else if( lookAheadType(1) == Token.TokenType.WAIT) {
				return VisitWaitDialogueNode(pPrevious);
			}
			else if( lookAheadType(1) == Token.TokenType.FOCUS) {
				return VisitFocusDialogueNode(pPrevious);
			}
			else if( lookAheadType(1) == Token.TokenType.DEFOCUS) {
				return VisitDefocusDialogueNode(pPrevious);
			}
			else if ( lookAheadType(1) == Token.TokenType.LOOP )
			{
				return VisitLoopDialogueNode(pPrevious);
			}
			else if(lookAheadType(1) == Token.TokenType.BREAK)
			{
				return VisitBreakDialogueNode(pPrevious);
			}
			else if ( lookAheadType(1) == Token.TokenType.BLOCK_BEGIN )
			{
				return VisitBranchingDialogueNode(pPrevious);
			}
			else if(lookAheadType(1) == Token.TokenType.BRACKET_LEFT) {
				return VisitEmptyNodeWithName(pPrevious);
			}
			else 
            {
                throw new GrimmException("Can't figure out statement type of token " + 
					                    lookAheadType(1) + " with string " + 
					                    lookAhead(1).getTokenString() + " on line " +
					                    lookAhead(1).LineNr + " and position" + lookAhead(1).LinePosition + 
				                         " in conversation " + _conversationName
				                         );
			}
			
			return null;
		}
		
		private TimedDialogueNode VisitTimedDialogueNode(DialogueNode pPrevious) 
		{
			#if DEBUG_WRITE
			Console.WriteLine("VisitTimedDialogueNode()");
			#endif
			
			Token speakerToken = match(Token.TokenType.NAME);
			string speaker = speakerToken.getTokenString();
			
			Token lineToken = match(Token.TokenType.QUOTED_STRING);
			string line = lineToken.getTokenString();
			
			TimedDialogueNode n = _dialogueRunner.Create<TimedDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString()); // + " (" + line + ")");
			n.speaker = speaker;
			n.line = line;
			
			if(lookAheadType(1) == Token.TokenType.BRACKET_LEFT) {
				match(Token.TokenType.BRACKET_LEFT);
				string nodeCustomName = match(Token.TokenType.NAME).getTokenString();
				n.name = nodeCustomName;
				match(Token.TokenType.BRACKET_RIGHT);
			}
			
			#if DEBUG_WRITE
			Console.WriteLine("Added TimedDialogueNode with name '" + n.name + "' and line '" + n.line + "'");
			#endif
			
			AddLinkFromPreviousNode(pPrevious, n);
			
			return n;		
		}

		DialogueNode VisitCancelDialogueNode(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			Console.WriteLine("VisitCancelDialogueNode()");
			#endif
			
			match(Token.TokenType.CANCEL);
			Token handleNameToken = match(Token.TokenType.NAME);
			
			CancelDialogueNode n = _dialogueRunner.Create<CancelDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString() + "(cancel)");
			n.handle = handleNameToken.getTokenString();
			
			AddLinkFromPreviousNode(pPrevious, n);
			return n;
		}

		DialogueNode VisitBroadcastDialogueNode(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			Console.WriteLine("VisitBroadcastDialogueNode()");
			#endif
			
			match(Token.TokenType.BROADCAST);
			string eventName = GetAStringFromNextToken(false, false);
			
			BroadcastDialogueNode n = _dialogueRunner.Create<BroadcastDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString() + "(broadcaster)");
			n.eventName = eventName;
			
			AddLinkFromPreviousNode(pPrevious, n);
			return n;
		}
		
		string GetAStringFromNextToken(bool pQuotedStringsAreOK, bool pNumbersAreOK)
		{
			Token token;
			
			if(lookAheadType(1) == Token.TokenType.NUMBER && pNumbersAreOK) {
				token = match(Token.TokenType.NUMBER);
			}
			else if(lookAheadType(1) == Token.TokenType.QUOTED_STRING && pQuotedStringsAreOK) {
				token = match(Token.TokenType.QUOTED_STRING);
			}
			else {
				token = match(Token.TokenType.NAME);
			}
			
			return token.getTokenString();
		}

		DialogueNode VisitListeningDialogueNode(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			Console.WriteLine("VisitListeningDialogueNode()");
			#endif
			
			match(Token.TokenType.LISTEN);
			
			string eventName = GetAStringFromNextToken(false, false);
			
			string handleName = "";
			if(lookAheadType(1) == Token.TokenType.BRACKET_LEFT) {
				match(Token.TokenType.BRACKET_LEFT);
				Token handleToken = match(Token.TokenType.NAME);
				handleName = handleToken.getTokenString();
				match(Token.TokenType.BRACKET_RIGHT);
			}
						
			ListeningDialogueNode n = _dialogueRunner.Create<ListeningDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString() + "(event listener)");
			n.eventName = eventName;
			n.handle = handleName;
			
			if(_loopStack.Count > 0) {
				// Add this listening dialogue node to the scope of the loop so that it is automatically removed as a listener when the loop ends
				n.scopeNode = _loopStack.Peek().name;
			}
			
			SilentDialogueNode silentNode = _dialogueRunner.Create<SilentDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString() + "(silent stop node)");
			
			AllowLineBreak();
			
			if(lookAheadType(1) == Token.TokenType.BLOCK_BEGIN) {
				ImmediateNode eventBranchStartNode = _dialogueRunner.Create<ImmediateNode>(_conversationName, _language, (_nodeCounter++).ToString() + "(eventBranchStartNode)");
				n.branchNode = eventBranchStartNode.name;
				n.hasBranch = true;
				match(Token.TokenType.BLOCK_BEGIN);
				Nodes(eventBranchStartNode, silentNode);
				match(Token.TokenType.BLOCK_END);
			}
			else {
				#if DEBUG_WRITE
				Console.WriteLine("this listening dialogue node had no body");
				#endif
			}
			
			AddLinkFromPreviousNode(pPrevious, n);
			return n;
		}
		
		private CallFunctionDialogueNode VisitFunctionDialogueNode(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			Console.WriteLine("VisitFunctionDialogueNode()");
			#endif
			
			string functionName = "";
			
			string[] args = VisitFunctionCall(out functionName);
			
			CallFunctionDialogueNode n = _dialogueRunner.Create<CallFunctionDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString());
			n.function = functionName;
			n.args = args;
			
			if(!_dialogueRunner.HasFunction(functionName)) {
				//throw new GrimmException("There is no '" + functionName + "' function registered in the dialogue runner");
			}
			
			#if DEBUG_WRITE
			Console.WriteLine("Added CallFunctionDialogueNode() with name '" + n.name + "'");
			#endif
			
			AddLinkFromPreviousNode(pPrevious, n);
			
			return n;		
		}
		
		private string[] VisitFunctionCall(out string pFunctionName)
		{
			List<string> arguments = new List<string>();
			
			if(lookAheadType(2) == Token.TokenType.DOT) {
				// Function looks like this: cat.foo("dog")
				Token tokenBeforeDot = match(Token.TokenType.NAME);
				string arg0 = tokenBeforeDot.getTokenString();
				arguments.Add(arg0);
				#if DEBUG_WRITE
				Console.WriteLine("Added argument 0 based on token before dot: " + arg0);
				#endif
				match(Token.TokenType.DOT);
			}
			
			Token functionNameToken = match(Token.TokenType.NAME);
			pFunctionName = functionNameToken.getTokenString();
			
			match(Token.TokenType.PARANTHESIS_LEFT);
			
			while(true)
			{
				if(lookAheadType(1) == Token.TokenType.PARANTHESIS_RIGHT) {
					break;
				}
				else {
					string argumentString = GetAStringFromNextToken(true, true);
					#if DEBUG_WRITE
					Console.WriteLine("Matched argument " + argumentString);
					#endif
					arguments.Add(argumentString);
					if(lookAheadType(1) == Token.TokenType.COMMA) {
						match(Token.TokenType.COMMA);
					}
					else {
						break;
					}					
				}
			}
			
			match(Token.TokenType.PARANTHESIS_RIGHT);
			
			return arguments.ToArray();
		}
		
		private DialogueNode VisitEmptyNodeWithName(DialogueNode pPreviousNode)
		{
			match(Token.TokenType.BRACKET_LEFT);
			string nodeCustomName = match(Token.TokenType.NAME).getTokenString();
			ImmediateNode n = _dialogueRunner.Create<ImmediateNode>(_conversationName, _language, (_nodeCounter++).ToString());
			n.name = nodeCustomName;
			match(Token.TokenType.BRACKET_RIGHT);
			AddLinkFromPreviousNode(pPreviousNode, n);
			return n;
		}

		void AddLinkFromPreviousNode(DialogueNode pPreviousNode, DialogueNode pNewNode)
		{
			D.isNull(pPreviousNode);
			D.isNull(pNewNode);
			pPreviousNode.nextNode = pNewNode.name;
			#if WRITE_NODE_LINKS
			Console.WriteLine(pPreviousNode.name + ".nextNode = '" + pNewNode.name + "'");
			#endif
		}
		
		private int CalculateTimeout(string pLine) {
			int nrOfCharacters = pLine.Length;
			return (int)(30 + nrOfCharacters * 1.0f);
		}
	
		private GotoDialogueNode VisitGotoDialogueNode(DialogueNode pPrevious) {
			#if DEBUG_WRITE
			Console.WriteLine("GotoDialogueNode()");
			#endif			
			
			match(Token.TokenType.GOTO);
			Token targetNameToken = match(Token.TokenType.NAME);
				
			GotoDialogueNode n = _dialogueRunner.Create<GotoDialogueNode>(_conversationName, _language, (_nodeCounter++) + " (goto)");
			n.linkedNode = targetNameToken.getTokenString();
			
			#if DEBUG_WRITE
			Console.WriteLine("Added GotoDialogueNode() with name '" + n.name + "'");
			#endif

			AddLinkFromPreviousNode(pPrevious, n);
			
			return n;
		}
		
		private GotoDialogueNode VisitStopDialogueNode(DialogueNode pPrevious) {
			#if DEBUG_WRITE
			Console.WriteLine("VisitStopDialogueNode()");
			#endif			
			
			match(Token.TokenType.STOP);
				
			GotoDialogueNode n = _dialogueRunner.Create<GotoDialogueNode>(_conversationName, _language, (_nodeCounter++) + " (stop)");
			n.linkedNode = NAME_OF_END_NODE;
			
			#if DEBUG_WRITE
			Console.WriteLine("Added Stopping GotoDialogueNode with name '" + n.name + "'");
			#endif

			AddLinkFromPreviousNode(pPrevious, n);
			
			return n;
		}
		
		private StartCommandoDialogueNode VisitStartCommandoDialogueNode(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			Console.WriteLine("StartCommandoDialogueNode()");
			#endif			
			
			match(Token.TokenType.START);
			string commandoName = GetAStringFromNextToken(false, false);
			
			StartCommandoDialogueNode n = _dialogueRunner.Create<StartCommandoDialogueNode>(_conversationName, _language, (_nodeCounter++) + " (start commando)");
			n.commando = commandoName;
			
			#if DEBUG_WRITE
			Console.WriteLine("Added StartCommandoDialogueNode() with name '" + n.name + "'");
			#endif

			AddLinkFromPreviousNode(pPrevious, n);
			
			return n;
		}
		
		private WaitDialogueNode VisitWaitDialogueNode(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			Console.WriteLine("WaitDialogueNode()");
			#endif			
			
			match(Token.TokenType.WAIT);
			
			WaitDialogueNode n = _dialogueRunner.Create<WaitDialogueNode>(_conversationName, _language, (_nodeCounter++) + " (start commando)");
			
			List<ExpressionDialogueNode> expressionNodes = new List<ExpressionDialogueNode>();
			
			while(true) {
				
				if(lookAheadType(1) == Token.TokenType.NAME) 
				{						
					string expressionName = "";
					string[] args = VisitFunctionCall(out expressionName);
					
					ExpressionDialogueNode expressionNode = _dialogueRunner.Create<ExpressionDialogueNode>(_conversationName, _language, (_nodeCounter++) + " (expression)");
					expressionNode.expression = expressionName;
					expressionNode.args = args;
					expressionNodes.Add(expressionNode);					
				}
				else if(lookAheadType(1) == Token.TokenType.AND)
				{
					ConsumeCurrentToken();
				}
				else {
					break;
				}
			}
			
			n.expressions = expressionNodes.ToArray();
			
			string handleName = "";
			if(lookAheadType(1) == Token.TokenType.BRACKET_LEFT) {
				match(Token.TokenType.BRACKET_LEFT);
				Token handleToken = match(Token.TokenType.NAME);
				handleName = handleToken.getTokenString();
				match(Token.TokenType.BRACKET_RIGHT);
			}
			
			n.handle = handleName;
			
			if(_loopStack.Count > 0) {
				// Add this listening dialogue node to the scope of the loop so that it is automatically removed when the loop ends
				n.scopeNode = _loopStack.Peek().name;
			}
			
			#if DEBUG_WRITE
			Console.WriteLine("Added WaitDialogueNode() with name '" + n.name + "'");
			#endif
			
			//if(!_dialogueRunner.HasExpression(expressionName)) {
				//throw new GrimmException("There is no '" + expressionName + "' expression registered in the dialogue runner");
			//}
			
			SilentDialogueNode silentEndNode = _dialogueRunner.Create<SilentDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString() + "(silent stop node)");
			
			AllowLineBreak();
			
			if(lookAheadType(1) == Token.TokenType.BLOCK_BEGIN) {
				ImmediateNode eventBranchStartNode = _dialogueRunner.Create<ImmediateNode>(_conversationName, _language, (_nodeCounter++).ToString() + "(waitBranchStartNode)");
				n.branchNode = eventBranchStartNode.name;
				n.hasBranch = true;
				match(Token.TokenType.BLOCK_BEGIN);
				Nodes(eventBranchStartNode, silentEndNode);
				match(Token.TokenType.BLOCK_END);
			}
			else {
				#if DEBUG_WRITE
				Console.WriteLine("this wait dialogue node had no body");
				#endif
			}
			
			AddLinkFromPreviousNode(pPrevious, n);
			
			return n;
		}
		
		private FocusDialogueNode VisitFocusDialogueNode(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			Console.WriteLine("FocusConversationNode()");
			#endif			
			
			match(Token.TokenType.FOCUS);
		
			FocusDialogueNode n = _dialogueRunner.Create<FocusDialogueNode>(_conversationName, _language, (_nodeCounter++) + " (focus)");
		
			#if DEBUG_WRITE
			Console.WriteLine("Added FocusConversationNode() with name '" + n.name + "'");
			#endif			
			
			AddLinkFromPreviousNode(pPrevious, n);
			
			return n;
		}
		
		private DefocusDialogueNode VisitDefocusDialogueNode(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			Console.WriteLine("DefocusConversationNode()");
			#endif			
			
			match(Token.TokenType.DEFOCUS);
		
			DefocusDialogueNode n = _dialogueRunner.Create<DefocusDialogueNode>(_conversationName, _language, (_nodeCounter++) + " (defocus)");
		
			#if DEBUG_WRITE
			Console.WriteLine("Added DefocusConversationNode() with name '" + n.name + "'");
			#endif			
			
			AddLinkFromPreviousNode(pPrevious, n);
			
			return n;
		}
		
		private void AllowLineBreak() {
			if(lookAheadType(1) == Token.TokenType.NEW_LINE) {
				match(Token.TokenType.NEW_LINE);
			}
		}
		
		private DialogueNode VisitLoopDialogueNode(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			Console.WriteLine("VisitLoopDialogueNode()");
			#endif
			
			match(Token.TokenType.LOOP);
			
			LoopDialogueNode n = _dialogueRunner.Create<LoopDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString() + " (loop)");
			AddLinkFromPreviousNode(pPrevious, n);
			
			//ImmediateNode finalNode = _dialogueRunner.Create<ImmediateNode>(_conversationName, _language, (_nodeCounter++).ToString() + "(final node)");
			//n.nextNode = finalNode.name;
			
			AllowLineBreak();
			
			_loopStack.Push(n);
			match(Token.TokenType.BLOCK_BEGIN);
			
			ImmediateNode branchStartNode = _dialogueRunner.Create<ImmediateNode>(_conversationName, _language, (_nodeCounter++).ToString() + " (loop branch node)");
			n.branchNode = branchStartNode.name;
			
			SilentDialogueNode unifiedEndNode = _dialogueRunner.Create<SilentDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString() + " (unified end node for loop)");
			Nodes(branchStartNode, unifiedEndNode);
			
			match(Token.TokenType.BLOCK_END);
			_loopStack.Pop();
			
			return n;
		}
		
		private DialogueNode VisitBreakDialogueNode(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			Console.WriteLine("VisitBreakDialogueNode()");
			#endif
			
			Token breakToken = match(Token.TokenType.BREAK);
			
			BreakDialogueNode n = _dialogueRunner.Create<BreakDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString() + "(break)");
			
			if(_loopStack.Count > 0) {
				n.breakTargetLoop = _loopStack.Peek().name;			
			}
			else {
				throw new GrimmException("Trying to break at weird position? Line: " + breakToken.LineNr + " in conversation '" + _conversationName + "'");
			}
			
			AddLinkFromPreviousNode(pPrevious, n);		
			
			return n;
		}
		
		private DialogueNode VisitAssertDialogueNode(DialogueNode pPrevious) {
			#if DEBUG_WRITE
			Console.WriteLine("VisitAssertDialogueNode()");
			#endif
			
			match(Token.TokenType.ASSERT);
			
			string expressionName = "";
			string[] args = VisitFunctionCall(out expressionName);
			
			AssertDialogueNode n = _dialogueRunner.Create<AssertDialogueNode>(_conversationName, _language, (_nodeCounter++) + " (assert)");
			n.expression = expressionName;
			n.args = args;
			
			if(!_dialogueRunner.HasExpression(expressionName)) {
				throw new GrimmException("There is no '" + expressionName + "' expression registered in the dialogue runner");
			}
			
			AddLinkFromPreviousNode(pPrevious, n);
			
			return n;
		}
	
		private DialogueNode VisitIfDialogueNode(DialogueNode pPrevious) {
			#if DEBUG_WRITE
			Console.WriteLine("IfDialogueNode()");
			#endif			
			
			match(Token.TokenType.IF);
			
			string expressionName = "";
			string[] args = VisitFunctionCall(out expressionName);
			
			AllowLineBreak();
			match(Token.TokenType.BLOCK_BEGIN);
			
			UnifiedEndNodeForScope unifiedEndNode = 
				_dialogueRunner.Create<UnifiedEndNodeForScope>(_conversationName, _language, (_nodeCounter++) + " (unified end of if)");
			
			ExpressionDialogueNode ifTrue = _dialogueRunner.Create<ExpressionDialogueNode>(_conversationName, _language, (_nodeCounter++) + " (if true)");
			Nodes(ifTrue, unifiedEndNode);
			ifTrue.expression = expressionName;
			ifTrue.args = args;
			
			#if DEBUG_WRITE
			Console.WriteLine("Added IfTrue node with expression '" + ifTrue.expression + "'");
			#endif
			
			match(Token.TokenType.BLOCK_END);
			AllowLineBreak();
			
			ImmediateNode ifFalse = null;
			
			List<ExpressionDialogueNode> elifNodes = new List<ExpressionDialogueNode>();
			
			while(lookAheadType(1) == Token.TokenType.ELIF)
			{
				match(Token.TokenType.ELIF);
				
				string elifExpressionName = "";
				string[] elifArgs = VisitFunctionCall(out elifExpressionName);
				
				AllowLineBreak();
				match(Token.TokenType.BLOCK_BEGIN);
				
				ExpressionDialogueNode elifNode = _dialogueRunner.Create<ExpressionDialogueNode>(_conversationName, _language, (_nodeCounter++) + " (elif)");
				Nodes(elifNode, unifiedEndNode);
				elifNode.expression = elifExpressionName;
				elifNode.args = elifArgs;
				
				elifNodes.Add(elifNode);
				
				#if DEBUG_WRITE
				Console.WriteLine("Added Elif node with expression '" + elifNode.expression + "'");
				#endif
				
				match(Token.TokenType.BLOCK_END);
			}
			
			if(lookAheadType(1) == Token.TokenType.ELSE) {
				match(Token.TokenType.ELSE);
				AllowLineBreak();
				match(Token.TokenType.BLOCK_BEGIN);
				
				ifFalse = _dialogueRunner.Create<ImmediateNode>(_conversationName, _language, (_nodeCounter++) + " (if false)");
				Nodes(ifFalse, unifiedEndNode);
				
				match(Token.TokenType.BLOCK_END);
			}
			
			IfDialogueNode ifNode = _dialogueRunner.Create<IfDialogueNode>(_conversationName, _language, (_nodeCounter++) + " (if)");
			
			#if DEBUG_WRITE
			Console.WriteLine("Added IfDialogueNode() with name '" + ifNode.name + "'");
			//foreach(DialogueNode elif in elifNodes) {
			//	Console.WriteLine("Added ElifDialogueNode() with name '" + elif.name + "'");
			//}
			#endif

			AddLinkFromPreviousNode(pPrevious, ifNode);
			
			ifNode.nextNode = unifiedEndNode.name;
			ifNode.ifTrueNode = ifTrue;
			ifNode.elifNodes = elifNodes.ToArray();
			if(ifFalse != null) {
				ifNode.ifFalseNode = ifFalse;
			}
			else {
				ifNode.ifFalseNode = null;
			}
			
			if(!_dialogueRunner.HasExpression(expressionName)) {
				//throw new GrimmException("There is no '" + expressionName + "' expression registered in the dialogue runner");
			}
			
			return unifiedEndNode;
		}
		
		private DialogueNode VisitBranchingDialogueNode(DialogueNode pPrevious)
		{
			#if DEBUG_WRITE
			Console.WriteLine("NodesWithPlayerChoiceLinks()");
			#endif
			
			match(Token.TokenType.BLOCK_BEGIN);
			
			BranchingDialogueNode bn = _dialogueRunner.Create<BranchingDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString() + " (branching node)");
			pPrevious.nextNode = bn.name;
			
			UnifiedEndNodeForScope unifiedEndNodeForScope = _dialogueRunner.Create<UnifiedEndNodeForScope>(_conversationName, _language, (_nodeCounter++) + " (unified end of options)");
			
			bn.unifiedEndNodeForScope = unifiedEndNodeForScope.name;
			
			#if DEBUG_WRITE
			Console.WriteLine("Created a branching node with name '" + bn.name + "'");
			#endif
			
			List<string> nameOfPossibleOptions = new List<string>();
			
			while (lookAheadType(1) != Token.TokenType.EOF &&
				   lookAheadType(1) != Token.TokenType.BLOCK_END) 
			{
				DialogueNode n = FigureOutOptionStatement(unifiedEndNodeForScope);
				if(n != null)
				{
					nameOfPossibleOptions.Add(n.name);
				}
			}
			
			bn.nextNodes = nameOfPossibleOptions.ToArray();			
			
			match(Token.TokenType.BLOCK_END);
			
			return unifiedEndNodeForScope;
		}
		
		private DialogueNode FigureOutOptionStatement(DialogueNode pScopeEndNode) 
		{
			#if DEBUG_WRITE
			Console.WriteLine("FigureOutOptionStatement()");
			#endif
			
			if (lookAheadType(1) == Token.TokenType.NEW_LINE) {
				match(Token.TokenType.NEW_LINE);
				#if DEBUG_WRITE
				Console.Write(" (newline)");
				#endif
			}
			else if (lookAheadType(1) == Token.TokenType.EOF) {
				match(Token.TokenType.EOF);
			}
			else if ( lookAheadType(1) == Token.TokenType.QUOTED_STRING &&
			          lookAheadType(2) == Token.TokenType.COLON )
			{
				return VisitOption(pScopeEndNode);
			}
			else {
				throw new GrimmException("Can't figure out player option statement type of token " + 
				                    lookAheadType(1) + " with string " + 
				                    lookAhead(1).getTokenString() + " on line " +
				                    lookAhead(1).LineNr + " and position" + lookAhead(1).LinePosition);
			}
			
			return null;
		}
		
		private DialogueNode VisitOption(DialogueNode pScopeEndNode) 
		{
			#if DEBUG_WRITE
			Console.WriteLine("VisitOption()");
			#endif
			
			Token t = match(Token.TokenType.QUOTED_STRING);
			match(Token.TokenType.COLON);
			
			TimedDialogueNode optionNode = _dialogueRunner.Create<TimedDialogueNode>(_conversationName, _language, (_nodeCounter++).ToString());
			optionNode.line = t.getTokenString();
			optionNode.speaker = _playerCharacterName;
			
			#if DEBUG_WRITE
			Console.WriteLine("Created an option node with the name '" + optionNode.name + "'" + " and line " + "'" + optionNode.line + "'");
			#endif
			
			Nodes(optionNode, pScopeEndNode);
			
			return optionNode;
		}
		
		// ************************* //
		
		private Token match(Token.TokenType expectedTokenType)
        {
			Token matchedToken = lookAhead(1);
			
			if(lookAheadType(1) == expectedTokenType) {
				ConsumeCurrentToken();
			} 
			else {
				throw new GrimmException(
					"The code word \"" + lookAhead(1).getTokenString() + "\"" +
					" doesn't match the expected (" + expectedTokenType + ")." +
					" at line " + 
					lookAhead(1).LineNr +
				    " and position " +
					lookAhead(1).LinePosition +
					" in conversation '" +
					_conversationName + "'"
					);
			}
			
			return matchedToken;
		}
		
		private void ConsumeCurrentToken() {
			
			Token nextToken;
			
			if (_nextTokenIndex < _tokens.Count) {
				nextToken = _tokens[_nextTokenIndex];
				_nextTokenIndex++;
			}
			else {
				nextToken = new Token(Token.TokenType.EOF, "<EOF>");
			}
			
			_lookahead[_lookaheadIndex] = nextToken;
			_lookaheadIndex = (_lookaheadIndex + 1) % k;
		}
		
		private Token lookAhead(int i) {
			return _lookahead[(_lookaheadIndex + i - 1) % k];
		}
		
		private Token.TokenType lookAheadType(int i) {
			return lookAhead(i).getTokenType();
		}
		
		private void SkipStuffUntilNextLine() {
			#if DEBUG_WRITE
			Console.WriteLine("SkipStuffUntilNextLine()");
			#endif
			while( lookAheadType(1) != Token.TokenType.NEW_LINE ) 
			{
				ConsumeCurrentToken();
			}
		}
		
		public string playerCharacterName {
			get {
				return _playerCharacterName;
			}
			set {
				_playerCharacterName = value;
			}
		}
	}
}
