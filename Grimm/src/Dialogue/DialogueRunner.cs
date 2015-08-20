using System;
using System.Collections.Generic;
using System.Text;
using RelayLib;
using GameTypes;
using System.Reflection;
using System.IO;

namespace GrimmLib
{
	/// <summary>
	/// Main responsibilities are to instantiate and update dialogue nodes.
	/// DialogueNodes also uses the OnSomeoneSaidSomething-event to send out Speech data.
	/// Expression- and Function delegates can be registered so that they are accessible from the scripts.
	/// </summary>
    public class DialogueRunner
    {
		public delegate void OnSomeoneSaidSomething(Speech pSpeech);
		public delegate bool Expression(string[] args);
		public delegate void Function (string[] args);

		public delegate void OnFocusConversation (string pConversation);

		public delegate void OnEvent (string pEventName);

		public Logger logger = new Logger();
		private TableTwo _dialogueTable;
		private Language _language;
		private List<DialogueNode> _dialogueNodes;
		private Dictionary<string, List<DialogueNode>> _nodesForConversation = new Dictionary<string, List<DialogueNode>>();

		private event OnSomeoneSaidSomething _onSomeoneSaidSomething;

		private Dictionary<string, Expression> _expressions = new Dictionary<string, Expression>();
		private Dictionary<string, Function> _functions = new Dictionary<string, Function>();
		private List<IRegisteredDialogueNode> _registeredDialogueNodes = new List<IRegisteredDialogueNode>();

		private event OnFocusConversation _onFocusConversation, _onDefocusConversation;
		private event OnEvent _onEvent;

		public Action<string> onGrimmError;

		private List<DialogueNode> _nodesThatAreOn = new List<DialogueNode>();
		private List<DialogueNode> _nodesToTurnOn = new List<DialogueNode>();
		private List<DialogueNode> _nodesToTurnOff = new List<DialogueNode>();

		public void AddToTurnOnNodeList (DialogueNode pNodeToTurnOn)
		{
			_nodesToTurnOn.Add(pNodeToTurnOn);
		}

		public void AddToTurnOffNodeList (DialogueNode pNodeToTurnOff)
		{
			_nodesToTurnOff.Add(pNodeToTurnOff);
		}
		
        public DialogueRunner(RelayTwo pRelay, Language pLanguage)
        {
			D.isNull(pRelay);
			
			_dialogueTable = pRelay.GetTable(DialogueNode.TABLE_NAME);
            _language = pLanguage;
            _dialogueNodes = InstantiatorTwo.Process<DialogueNode>(_dialogueTable);
			foreach(DialogueNode n in _dialogueNodes) {
				n.SetRunner(this);
				if(n is IRegisteredDialogueNode) {
					IRegisteredDialogueNode ir = n as IRegisteredDialogueNode;
					_registeredDialogueNodes.Add(ir);
				}
				if(n.isOn) {
					_nodesThatAreOn.Add(n);
				}
				AddNodeToNodeForConversationDictionary(n.conversation, n);
			}
			RegisterBuiltInAPIExpressions();
        }

		void AddNodeToNodeForConversationDictionary (string pConversation, DialogueNode n)
		{
			List<DialogueNode> maybeList = null;
			if(!_nodesForConversation.TryGetValue(pConversation, out maybeList)) {
				maybeList = new List<DialogueNode>();
				_nodesForConversation.Add(pConversation, maybeList);
			}
			//D.Log("Adding " + n.name + " to NodeForConversationDictionary, with conversation " + pConversation);
			maybeList.Add(n);
		}
		
		public T Create<T>(string pConversation, Language pLanguage, string pName) where T : DialogueNode
		{
			T newDialogueNode = Activator.CreateInstance<T>();
			
			newDialogueNode.CreateNewRelayEntry(_dialogueTable, newDialogueNode.GetType().Name);
			newDialogueNode.conversation = pConversation;
			newDialogueNode.language = pLanguage;
			newDialogueNode.name = pName;
			newDialogueNode.SetRunner(this);
			_dialogueNodes.Add(newDialogueNode);
			AddNodeToNodeForConversationDictionary(pConversation, newDialogueNode);
			if(newDialogueNode is IRegisteredDialogueNode) {
				IRegisteredDialogueNode ir = newDialogueNode as IRegisteredDialogueNode;
				_registeredDialogueNodes.Add(ir);
			}
			return newDialogueNode;
		}

		public void LogNodesThatAreOn() {
			Console.WriteLine("Nodes that are on");
			foreach (DialogueNode d in _nodesThatAreOn) {
				Console.WriteLine("- " + d.name);
			}
			Console.WriteLine("-----------------");
		}

        public void Update(float dt)
        {
			foreach (DialogueNode d in _nodesThatAreOn) {
				try {
					if(d.isOn) {
						d.Update(dt);
					}
				}
				catch(Exception e) {
					string description = d.name + ": " + e.ToString ();
					D.Log ("GRIMM ERROR: " + description);
					if (onGrimmError != null) {
						onGrimmError (description);
					}
				}
			}

			foreach(var nodeToTurnOn in _nodesToTurnOn) {
				if(_nodesThatAreOn.Find(n => n == nodeToTurnOn) != null) {
					// already there
					continue;
				} else {
					_nodesThatAreOn.Add(nodeToTurnOn);
				}
			}
			_nodesToTurnOn.Clear();
			
			foreach(var nodeToTurnOff in _nodesToTurnOff) {
				if(_nodesThatAreOn.Find(n => n == nodeToTurnOff) != null) {
					_nodesThatAreOn.Remove(nodeToTurnOff);
				}
			}
			_nodesToTurnOff.Clear();
        }
				
		public DialogueNode GetDialogueNode(string pConversation, string pName) 
		{
			List<DialogueNode> nodesInConvo = GetNodesForConversation (pConversation);

			DialogueNode n = nodesInConvo.Find(o => (o.name == pName));

			/*o.language == _language && */
			
			if(n != null) {
				return n;
			} else {
//				DialogueNode ignoreLanguage = _dialogueNodes.Find(o => (o.conversation == pConversation && o.name == pName));
//				if(ignoreLanguage != null) {
//					throw new GrimmException("Can't find DialogueNode with name '" + pName + "' in conversation '" + pConversation + "'" + " when using language " + _language);
//				} else {
//				}
				throw new GrimmException("Can't find DialogueNode with name '" + pName + "' in conversation '" + pConversation + "'");
			}
		}

		public List<DialogueNode> GetAllNodes() {
			return _dialogueNodes;
		}
		
		public List<IRegisteredDialogueNode> GetRegisteredDialogueNodes()
		{
			return _registeredDialogueNodes;
		}

		List<DialogueNode> GetNodesForConversation (string pConversation)
		{
			if(string.IsNullOrEmpty(pConversation)) {
				return new List<DialogueNode>();
			}

			if(pConversation == runStringAsFunctionCMD) {
				return _dialogueNodes.FindAll (n => n.conversation == pConversation);
			}

#if DEBUG
			if(!_nodesForConversation.ContainsKey(pConversation)) {
				D.Log("Can't find conversation " + pConversation + " in " + _nodesForConversation);
			}
#endif

			return _nodesForConversation [pConversation];
		}

		/// <returns>
		/// Returns null if there is no active branching dialogue node in the conversation
		/// </returns>
		public BranchingDialogueNode GetActiveBranchingDialogueNode(string pConversation)
		{
			List<DialogueNode> nodesInConvo = GetNodesForConversation (pConversation);

			BranchingDialogueNode n = nodesInConvo.Find(o => 
			                                     (o.isOn) &&
			                                     (o.language == _language) && 
			                                     (o is BranchingDialogueNode)
			                                     ) as BranchingDialogueNode;
			return n;
		}
		
		private TimedDialogueNode GetActiveTimedDialogueNode(string pConversation)
		{
			List<DialogueNode> nodesInConvo = GetNodesForConversation (pConversation);

			TimedDialogueNode n = nodesInConvo.Find(o => 
			                                     (o.isOn) &&
			                                     (o.language == _language) && 
			                                     (o is TimedDialogueNode)
			                                     ) as TimedDialogueNode;
			return n;
		}
		
		public void FastForwardCurrentTimedDialogueNode(string pConversation)
		{
			var activeTimedNode = GetActiveTimedDialogueNode(pConversation);

			if (activeTimedNode == null) {
				//D.Log("Can't fast forward in " + pConversation + " since it's not on a timed dialogue node");
			} else if((activeTimedNode.timerStartValue - activeTimedNode.timer) < 0.5f) { // < (activeTimedNode.timerStartValue * 0.5f)) {
				//D.Log("Will not fast forward in " + pConversation + " since it just switched to a new node");
			} else {
				activeTimedNode.timer = 0.01f;
				//D.Log("Fast forwarded timed dialogue node " + activeTimedNode.name);
			}
		}

		private void CheckThatThereIsOnlyOneActiveNodeInTheConversation(string pConversation)
		{
			List<DialogueNode> nodesInConvo = GetNodesForConversation (pConversation);

			DialogueNode[] nodes = nodesInConvo.FindAll(o => (o.language == _language && o.isOn)).ToArray();

			if(nodes.Length > 1) {
				StringBuilder sb = new StringBuilder();
				foreach(var node in nodes) {
					sb.Append(node.name + ", ");
				}
				throw new GrimmException("There are " + nodes.Length + " active nodes in the conversation " + pConversation + ": " + sb.ToString());
			}
		}
		
		public bool ConversationIsRunning(string pConversation)
		{
			List<DialogueNode> nodesInConvo = GetNodesForConversation (pConversation);

			return (nodesInConvo.Find(o => (o.language == _language && o.isOn)) != null);
		}
		
		/// <summary>
		/// Looks in a conversation for a ConversationStartDialogueNode
		/// </summary>
		public void StartConversation(string pConversation)
		{
			if(ConversationIsRunning(pConversation)) {
				logger.Log("Trying to start conversation " + pConversation + " again, even though it's already running");
				return;
			}

			List<DialogueNode> nodesInConvo = GetNodesForConversation (pConversation);

			DialogueNode conversationStartNode = 
				nodesInConvo.Find(o => (o.language == _language && o is ConversationStartDialogueNode));
			
			if(conversationStartNode != null) {
				logger.Log("Starting conversation '" + pConversation + "'");
				conversationStartNode.Start();
			}
			else {
				if(HasConversation(pConversation)) {
					throw new GrimmException("Can't find a ConversationStartDialogueNode in conversations '" + pConversation + "'");
				}
				else {
					throw new GrimmException("The dialogue runner doesn't contain the conversation '" + pConversation + "'");
				}
			}
		}

		/// <summary>
		/// Start all non-running conversations with a name containing some string
		/// </summary>
		public string[] StartAllConversationsContaining(string pPartialName)
		{
			return DoSomethingToAllConversationsContaining (pPartialName, o => !ConversationIsRunning(o.conversation), convStartNode => convStartNode.Start(), "Started");
		}

		/// <summary>
		/// Stop all running conversations with a name containing some string
		/// </summary>
		public string[] StopAllConversationsContaining(string pPartialName)
		{
			return DoSomethingToAllConversationsContaining (pPartialName, o => ConversationIsRunning(o.conversation), convStartNode => StopConversation(convStartNode.conversation), "Stopped");
		}

		private string[] DoSomethingToAllConversationsContaining(string pPartialName, Predicate<DialogueNode> pPred, Action<ConversationStartDialogueNode> pAction, string pDescription)
		{
			var conversations = GetAllConversationsWithNameContaining (pPartialName, pPred);
			conversations.ForEach (pAction);
			var names = conversations.ConvertAll (o => o.conversation).ToArray ();
			if (conversations.Count > 0) {
				logger.Log (pDescription + " " + conversations.Count + " conversations with partial name " + pPartialName + ": " + string.Join (", ", names));
			}
			return names;
		}

		private List<ConversationStartDialogueNode> GetAllConversationsWithNameContaining(string pPartialName, Predicate<DialogueNode> pPred) 
		{
			var foundNodes = new List<ConversationStartDialogueNode> ();
			foreach (var node in _dialogueNodes) {
				if (node is ConversationStartDialogueNode &&
					node.conversation.Contains (pPartialName) &&
				    pPred(node)) 
				{
					foundNodes.Add (node as ConversationStartDialogueNode);
				}
			}
			return foundNodes;
		}

		public string[] GetNamesOfAllStoppedConversationsWithNameContaining(string pPartialName)
		{
			return GetAllConversationsWithNameContaining(pPartialName, o => !ConversationIsRunning(o.conversation)).ConvertAll (o => o.conversation).ToArray();
		}

		public void StopConversation(string pConversation)
		{
			logger.Log("Stopping conversation '" + pConversation + "'");

			List<DialogueNode> nodesInConvo = GetNodesForConversation (pConversation);

			foreach(DialogueNode n in nodesInConvo) {
				if(n.isOn) {
					//D.Log("Stopping node " + n);
					n.Stop();
				}
				var r = n as IRegisteredDialogueNode;
				if(r != null) {
					//D.Log("Stopping listening node " + n);
					r.isListening = false;
					n.Stop(); // <-- THIS IS NEW, MAKE SURE IT DOESN'T CAUSE TROUBLE! Trying to get conversations to stop ALL nodes.
				}
			}
		}

		public void RemoveConversation(string pConversation)
		{
			var nodes = GetNodesForConversation(pConversation);

			foreach (DialogueNode d in nodes)
            {
				_nodesToTurnOff.Add(d);
				_dialogueNodes.Remove(d);
				if(d is IRegisteredDialogueNode) {
					IRegisteredDialogueNode ir = d as IRegisteredDialogueNode;
					_registeredDialogueNodes.Remove(ir);
				}
				_dialogueTable.RemoveRowAt(d.objectId);
			}

			if(_nodesForConversation.ContainsKey(pConversation)) {
				_nodesForConversation.Remove(pConversation);
			}
		}
		
		public bool HasConversation(string pConversation)
		{
			int nrOfNodesForConversation = 0;
			foreach(DialogueNode n in _dialogueNodes)
			{
				if(n.conversation == pConversation)
				{
					nrOfNodesForConversation++;
				}
			}
			return (nrOfNodesForConversation > 0);
		}
		
		public void AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething pOnSomeoneSaidSomething)
		{
			_onSomeoneSaidSomething += pOnSomeoneSaidSomething;
		}
		
		public void RemoveOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething pOnSomeoneSaidSomething)
		{
			_onSomeoneSaidSomething -= pOnSomeoneSaidSomething;
		}
		
		internal void SomeoneSaidSomething(Speech pSpeech)
		{
			D.isNull(pSpeech);
			if(_onSomeoneSaidSomething != null) {
				_onSomeoneSaidSomething(pSpeech);
			}
			else {
				logger.Log("No listeners to dialogue runner");
			}
		}
		
		public void AddExpression(string pName, Expression pExpression)
		{
			_expressions[pName] = pExpression;
		}
		
		public bool HasExpression(string pName)
		{
			return _expressions.ContainsKey(pName);
		}
		
		public bool EvaluateExpression(string pExpressionName, string[] args)
		{
#if DEBUG
			if(!_expressions.ContainsKey(pExpressionName)) {
				string msg = "Can't find expression '" + pExpressionName + "' in Dialogue Runner";
				D.Log("ERROR: " + msg);

				if (onGrimmError != null) {
					onGrimmError (msg);
				}

				//throw new GrimmException(msg);
			}
#endif
			//Console.WriteLine(System.Environment.StackTrace.ToString());

			Expression e = _expressions[pExpressionName];
			bool result = e(args);
			if(result) {
				//logger.Log("Result of expression '" + pExpressionName + "' was true!");
			} else {
				//logger.Log("Result of expression '" + pExpressionName + "' was false...");
			}
			return result;
		}
		
		public string GetExpressionsAsString()
		{
			string[] keys = new string[_expressions.Count];
			int i = 0;
			foreach(string s in _expressions.Keys) {
				keys[i++] = s;
			}
			return string.Join(", ", keys);
		}
	
		public void AddFunction(string pName, Function pFunction)
		{
			_functions[pName] = pFunction;
		}
		
		public bool HasFunction(string pName)
		{
			return _functions.ContainsKey(pName);
		}

		public void CallFunction(string pFunctionName, string[] args)
		{
			if (_functions.ContainsKey (pFunctionName)) {
				Function f = _functions [pFunctionName];
				f (args);
			} else {
				string msg = "Can't find function '" + pFunctionName + "' in Dialogue Runner";
				D.Log ("ERROR! " + msg);
				//throw new GrimmException(msg);

				if (onGrimmError != null) {
					onGrimmError (msg);
				}
			}
		}
			
		public string GetFunctionsAsString()
		{
			string[] keys = new string[_functions.Count];
			int i = 0;
			foreach(string s in _functions.Keys) {
				keys[i++] = s;
			}
			return string.Join(", ", keys);
		}

		const string runStringAsFunctionCMD = "CMD";

		public void RunStringAsFunction(string pCommand)
		{
			RemoveConversation(runStringAsFunctionCMD);
			DialogueScriptLoader d = new DialogueScriptLoader(this);
			d.CreateDialogueNodesFromString(pCommand, runStringAsFunctionCMD);
			StartConversation(runStringAsFunctionCMD);
		}

		public void AddOnEventListener(OnEvent pOnEvent)
		{
			_onEvent += pOnEvent;
		}

		public void RemoveOnEventListener(OnEvent pOnEvent)
		{
			_onEvent -= pOnEvent;
		}
		
		public void EventHappened(string pEventName)
		{
			//logger.Log("Event [" + pEventName + "]");
			foreach(IRegisteredDialogueNode listeningNode in _registeredDialogueNodes.ToArray())
			{
				if(listeningNode.isListening && listeningNode.eventName == pEventName) {
					listeningNode.EventHappened();
				}
			}

			if(_onEvent != null) {
				_onEvent(pEventName);
			}
		}
		
		public void ScopeEnded(string pConversation, string pScopeNode)
		{
			logger.Log("Scope '" + pScopeNode + "' for conversation '" + pConversation + "' ended");
			foreach(IRegisteredDialogueNode registeredNode in _registeredDialogueNodes)
			{
				if(registeredNode.conversation == pConversation) {
					if (registeredNode.ScopeNode() == pScopeNode) {
						if (registeredNode.isListening) {
							//logger.Log ("Stop listening: " + registeredNode.name + ": " + registeredNode.eventName);
							registeredNode.isListening = false;
						}
						else {
							//logger.Log ("Not listening: " + registeredNode.name + ": " + registeredNode.eventName);
						}
					} else {
						//logger.Log ("Not same scope: " + registeredNode.name + "; it has scope node '" + registeredNode.ScopeNode() + "'");
					}
				}
			}
		}
		
		public void CancelRegisteredNode(string pConversation, string pListenerHandle)
		{
			foreach(IRegisteredDialogueNode registeredNode in _registeredDialogueNodes)
			{
				if(registeredNode.conversation == pConversation && registeredNode.handle == pListenerHandle) {
					logger.Log("Cancelled node " + registeredNode.name + " in conversation " + pConversation);
					registeredNode.isListening = false;
					if (registeredNode is ListeningDialogueNode) {
						//logger.Log ("Also end scope for listening node " + registeredNode.name);
						var listeningNode = (registeredNode as ListeningDialogueNode);
						ScopeEnded (pConversation, listeningNode.name);
					}
					else if (registeredNode is WaitDialogueNode) {
						//logger.Log ("Also end scope for wait node " + registeredNode.name);
						var waitingNode = (registeredNode as WaitDialogueNode);
						ScopeEnded (pConversation, waitingNode.name);
					}
				}
			}
		}

		public bool IsWaitingOnEvent(string pEventName)
		{
			foreach(IRegisteredDialogueNode l in _registeredDialogueNodes)
			{
				if(l.isListening && l.eventName == pEventName) {
					return true;
				}
			}
			return false;
		}

		public void AddFocusConversationListener(OnFocusConversation pOnFocusConversation)
		{
			_onFocusConversation += pOnFocusConversation;
		}
		
		public void RemoveFocusConversationListener(OnFocusConversation pOnFocusConversation)
		{
			_onFocusConversation -= pOnFocusConversation;
		}
		
		public void AddDefocusConversationListener(OnFocusConversation pOnDefocusConversation)
		{
			_onDefocusConversation += pOnDefocusConversation;
		}
		
		public void RemoveDefocusConversationListener(OnFocusConversation pOnDefocusConversation)
		{
			_onDefocusConversation -= pOnDefocusConversation;
		}
		
		/// <summary>
		/// The active conversation is the conversation that the player currently is engeged in, and doing input to
		/// </summary>
		public void FocusConversation(string pConversation)
		{
			if(_onFocusConversation != null) {
				_onFocusConversation(pConversation);
			}
			else {
				throw new GrimmException("Trying to focus conversation " + pConversation + " but there is no onFocusConversation listener.");
			}
		}
		
		public void DefocusConversation(string pConversation)
		{
			if(_onDefocusConversation != null) {
				_onDefocusConversation(pConversation);
				EventHappened("Defocused_" + pConversation);
			}
			else {
				//throw new GrimmException("Trying to defocus conversation " + pConversation + " but there is no onDefocusConversation listener.");
				D.Log("Trying to defocus conversation " + pConversation + " but there is no onDefocusConversation listener.");
			}
		}
		
		public Language language {
			set {
				_language = value;
			}
		}
		
		private string RegisteredNodesAsString() {
			var registeredNodes = GetRegisteredDialogueNodes();
			var registeredNodeNames = new List<string>();
			foreach(var node in registeredNodes) {
				registeredNodeNames.Add("[" + node.name + " in '" + node.conversation + "' " + (node.isListening ? "LISTENING" : "NOT listening") + "]");
			}
			return "Registered nodes: " + string.Join(", ", registeredNodeNames.ToArray());
		}
		
		public override string ToString()
		{
			return string.Format("DialogueRunner ({0} dialogue nodes, {1} registered dialogue nodes)", _dialogueNodes.Count, _registeredDialogueNodes.Count);
		}

		public string GetAllDialogueAsString()
		{
			var sb = new StringBuilder();
			foreach (var node in _dialogueNodes) {
				if (node is TimedDialogueNode) {
					sb.Append((node as TimedDialogueNode).line + " ");
					if (Randomizer.OneIn(10)) {
						sb.AppendLine("");
					}
				}
			}
			return sb.ToString();
		}

		public HashSet<string> GetActiveConversations()
		{
			var activeConversations = new HashSet<string>();
			foreach (var node in _dialogueNodes) {
				if (node.isOn) {
					activeConversations.Add(node.conversation);
				}
			}
			return activeConversations;
		}

		private void RegisterBuiltInAPIExpressions()
		{
			AddExpression("IsActive", IsActive);
			AddFunction ("StartAll", StartAll);
			AddFunction ("StopAll", StopAll);
		}
		
		// IsActive(string conversation)
		private bool IsActive(string[] args)
		{
			return ConversationIsRunning(args[0]);
		}

		// StartAll(string pPartialName)
		private void StartAll(string[] args) {
			StartAllConversationsContaining (args [0]);
		}

		// StopAll(string pPartialName)
		private void StopAll(string[] args) {
			StopAllConversationsContaining (args [0]);
		}
    }    
}

