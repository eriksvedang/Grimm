using System;
using System.Collections.Generic;
using RelayLib;

namespace GrimmLib
{
	public class WaitDialogueNode : DialogueNode, IRegisteredDialogueNode
	{
		ValueEntry<string> CELL_branchNode;
        ValueEntry<bool> CELL_hasBranch;
		ValueEntry<string> CELL_handle;
		ValueEntry<bool> CELL_isListening;
		ValueEntry<string[]> CELL_expressions;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_hasBranch = EnsureCell("hasBranch", false);
			CELL_branchNode = EnsureCell("branchNode", "undefined");
			CELL_handle = EnsureCell("handle", "");
			CELL_isListening = EnsureCell("isListening", false);
			CELL_expressions = EnsureCell("expressions", new string[] {});
		}
		
		public override void OnEnter()
		{
			if(hasBranch) {
				StartNextNode();
			}
			isListening = true;
			Evaluate();
		}
		
		public override void Update(float dt)
		{
			if(isListening) {
				Evaluate();
			}
			else {
				Stop();
			}
		}
		
		private void Evaluate()
		{
			foreach(ExpressionDialogueNode expressionNode in expressions) {
				if(expressionNode.Evaluate() == false) return;
			}
			
			isListening = false;
			
			Stop();
			if(hasBranch) {
				_dialogueRunner.GetDialogueNode(conversation, branchNode).Start();
			}
			else {
				StartNextNode();
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
		
		public string handle
		{
			get {
				return CELL_handle.data;
			}
			set {
				CELL_handle.data = value;
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
		
		public ExpressionDialogueNode[] expressions
		{
			get {
				List<ExpressionDialogueNode> expressions = new List<ExpressionDialogueNode>();
				foreach(string expressionName in CELL_expressions.data) {
					expressions.Add(_dialogueRunner.GetDialogueNode(conversation, expressionName) as ExpressionDialogueNode);
				}
				return expressions.ToArray();
			}
			set {
				List<string> expressionNames = new List<string>();
				foreach(ExpressionDialogueNode expressionNode in value) {
					expressionNames.Add(expressionNode.name);
				}
				CELL_expressions.data = expressionNames.ToArray();
			}
		}
	}
}

