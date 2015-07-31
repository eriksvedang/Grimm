using System;
using GameTypes;
using RelayLib;
namespace GrimmLib
{
	// When the ListeningDialogueNode is activated in a dialogue it registers itself with the dialogue runner
	// and then immediately activates the next node.
	// The block (starting with 'branchNode') connected with the ListeningDialogueNode will execute when the event happens.
	// When the conversation file that contains the ListeningDialogueNode is stopped all listeners in there will be unregistered.
	
	public class ListeningDialogueNode : DialogueNode, IRegisteredDialogueNode
	{
		ValueEntry<string> CELL_eventName;     
        ValueEntry<bool> CELL_isListening;
		ValueEntry<string> CELL_branchNode;
        ValueEntry<bool> CELL_hasBranch;
        ValueEntry<string> CELL_handle;

		public string ScopeNode() {
			return scopeNode;
		}

		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_eventName = EnsureCell("eventName", "undefined");
            CELL_hasBranch = EnsureCell("hasBranch", false);
			CELL_branchNode = EnsureCell("branchNode", "undefined");
            CELL_isListening = EnsureCell("isListening", false);
            CELL_handle = EnsureCell("handle", "");
		}
		
		public override void OnEnter()
		{
			isListening = true;
			if(hasBranch) {
				Stop();
				StartNextNode();
			}
		}
		
		public void EventHappened()
		{
			_dialogueRunner.logger.Log("The event of ListeningDialogueNode '" + name + "' in conversation '" + conversation + "' happened");
			
			isListening = false;
			if(hasBranch) {
				DialogueNode n = _dialogueRunner.GetDialogueNode(conversation, branchNode);
				n.Start();
			}
			else {
				Stop();
				StartNextNode();
			}			
		}

		public override string ToString ()
		{
			return string.Format ("[ListeningDialogueNode: eventName={0}, hasBranch={1}, branchNode={2}, isListening={3}, handle={4}]", eventName, hasBranch, branchNode, isListening, handle);
		}
		
		#region ACCESSORS
		
		public string eventName
		{
			get {
				return CELL_eventName.data;
			}
			set {
				CELL_eventName.data = value;
			}
		}
		
		public bool hasBranch
		{
			get {
				return CELL_hasBranch.data;
			}
			set {
				CELL_hasBranch.data = value;
			}
		}
		
		public string branchNode
		{
			get {
				return CELL_branchNode.data;
			}
			set {
				CELL_branchNode.data = value;
			}
		}
		
		public bool isListening
		{
			get {
				return CELL_isListening.data;
			}
			set {
				CELL_isListening.data = value;
			}
		}
		
		public string handle
		{
			get {
				return CELL_handle.data;
			}
			set {
				CELL_handle.data = value;
			}				
		}
		
		#endregion
	}
}

