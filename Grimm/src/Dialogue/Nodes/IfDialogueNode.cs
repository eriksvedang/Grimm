using System;
using RelayLib;
using System.Collections.Generic;

namespace GrimmLib
{
	public class IfDialogueNode : DialogueNode
	{
        ValueEntry<string> CELL_ifTrueNode;
		ValueEntry<string[]> CELL_elifNodes;
		ValueEntry<string> CELL_ifFalseNode;
		
		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_ifTrueNode = EnsureCell("ifTrueNode", "");
			CELL_elifNodes = EnsureCell("elifNode", new string[] {});
            CELL_ifFalseNode = EnsureCell("ifFalseNode", "");
		}
		
		public override void OnEnter()
		{
			string originalNextNode = nextNode;
			
			/*
			if(_dialogueRunner.EvaluateExpression(expression, args)) {
				nextNode = ifTrueNode;
			}
			else if(ifFalseNode != "") {
				nextNode = ifFalseNode;
			}
			*/
			
			bool hasFoundTruthyExpression = false;
			
			if(ifTrueNode.Evaluate()) {
				Console.WriteLine("IF " + ifTrueNode.expression + " was true");
				hasFoundTruthyExpression = true;
				nextNode = ifTrueNode.nextNode; // jumping directly to the node after the expression
			}
			
			if(!hasFoundTruthyExpression) {
				foreach(ExpressionDialogueNode e in elifNodes) {
					if(e.Evaluate()) {
						Console.WriteLine("ELIF " + e.expression + " was true");
						hasFoundTruthyExpression = true;
						nextNode = e.nextNode;
					}
				}
			}
			
			if(!hasFoundTruthyExpression && ifFalseNode != null) {
				Console.WriteLine("IF WAS FALSE; GOING INTO ELSE STATEMENT");
				nextNode = ifFalseNode.nextNode;
			}
			
			Stop();
			StartNextNode();
			nextNode = originalNextNode;
		}
		
		#region ACCESSORS
		
		public ExpressionDialogueNode ifTrueNode
		{
			get {
				if(CELL_ifTrueNode.data != "") {
					return _dialogueRunner.GetDialogueNode(conversation, CELL_ifTrueNode.data) as ExpressionDialogueNode;
				}
				else {
					return null;
				}
			}
			set {
				CELL_ifTrueNode.data = (value != null) ? value.name : "";
			}
		}
		
		public ExpressionDialogueNode[] elifNodes
		{
			get {
				List<ExpressionDialogueNode> nodes = new List<ExpressionDialogueNode>();
				foreach(string elifNodeNames in CELL_elifNodes.data)
				{
					nodes.Add(_dialogueRunner.GetDialogueNode(conversation, elifNodeNames) as ExpressionDialogueNode);
				}
				return nodes.ToArray();
			}
			set {
				List<string> nodeNames = new List<string>();
				foreach(ExpressionDialogueNode node in value)
				{
					nodeNames.Add(node.name);
				}
				CELL_elifNodes.data = nodeNames.ToArray();
			}
		}
		
		public DialogueNode ifFalseNode
		{
			get {
				if(CELL_ifFalseNode.data != "") {
					return _dialogueRunner.GetDialogueNode(conversation, CELL_ifFalseNode.data);
				}
				else {
					return null;
				}
			}
			set {
				CELL_ifFalseNode.data = (value != null) ? value.name : "";
			}
		}
		
		#endregion
	}
}

