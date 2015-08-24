#define LOG

using System;
using System.Collections.Generic;
using GameTypes;
using RelayLib;

namespace GrimmLib
{
    public abstract class DialogueNode : RelayObjectTwo
    {
		public const string TABLE_NAME = "Dialogues";
		
		protected DialogueRunner _dialogueRunner;

        ValueEntry<string> CELL_name;
        ValueEntry<bool> CELL_isOn;
        ValueEntry<string> CELL_conversation;
        ValueEntry<Language> CELL_language;
        ValueEntry<string> CELL_nextNode;
		ValueEntry<string> CELL_scopeNode;

		bool _isOnCache;
		
		public void SetRunner(DialogueRunner pRunner)
		{
			_dialogueRunner = pRunner;
		}
		
		protected void Invariant() {
			D.assert(_dialogueRunner != null);
		}
        
		protected override void SetupCells()
		{
            CELL_name = EnsureCell("name", "unnamed");
            CELL_isOn = EnsureCell("isOn", false);
            CELL_nextNode = EnsureCell("nextNode", "");
            CELL_conversation = EnsureCell("conversation", "");
            CELL_language = EnsureCell("language", Language.SWEDISH);
			CELL_scopeNode = EnsureCell("scopeNode", "");

			_isOnCache = CELL_isOn.data;
		}
		
		public void Start() {
#if LOG
			Console.WriteLine("Starting node " + name);
#endif
			Invariant();
            isOn = true;
			OnEnter();
		}
		
        public void Stop() { 
#if LOG
			Console.WriteLine("Stopping node " + name);
#endif
			Invariant();
            isOn = false;
			OnExit();
		}
		
		protected void StartNextNode() {
			if(nextNode == "") {
				throw new GrimmException("No nextNode in dialogue node '" + name + "' in conversation '" + conversation + "'");
			}
			DialogueNode n = _dialogueRunner.GetDialogueNode(this.conversation, nextNode);
			//_dialogueRunner.logger.Log("DialogueNode '" + name + "' is starting '" + n.name + "'" + " of type " + n.GetType().ToString() + " in conversation " + n.conversation);
			//Console.WriteLine("DialogueNode '" + name + "' is starting '" + n.name + "'");
			n.Start();
		}
		
		public virtual void OnEnter() {}
		public virtual void OnExit() {}
		public virtual void Update(float dt) {}
		
		#region ACCESSORS
		
		public string name {
			get {
				return CELL_name.data;
			}
			set {
				CELL_name.data = value;
			}
		}
		
		public bool isOn {
			get 
            {
				return _isOnCache;
				//return CELL_isOn.data;
			}
            private set 
            { 
				_isOnCache = value;
               	CELL_isOn.data = value;
				if(value) {
					_dialogueRunner.AddToTurnOnNodeList(this);
				}
				else {
					_dialogueRunner.AddToTurnOffNodeList(this);
				}
            }
		}
		
		public string nextNode {
			get {
				return CELL_nextNode.data;
			}
			set {
                CELL_nextNode.data = value;
			}
		}
		
		public string conversation {
			get 
            {
                return CELL_conversation.data;
			}
			set 
            {
                CELL_conversation.data = value;
			}
		}
		
		public string scopeNode {
			get 
            {
                return CELL_scopeNode.data;
			}
			set 
            {
                CELL_scopeNode.data = value;
			}
		}
		
		public Language language
		{
			get {
				return CELL_language.data;
			}
			set {
                CELL_language.data = value;
			}
		}
		
		#endregion
    }
}
