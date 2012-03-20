using System;
using GameTypes;
using RelayLib;
namespace GrimmLib
{
	public class BranchingDialogueNode : DialogueNode
	{
		ValueEntry<string[]> CELL_nextNodes;
		ValueEntry<string> CELL_unifiedEndNodeForScope;
		
		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_nextNodes = EnsureCell("nextNodes", new string[] {});
			CELL_unifiedEndNodeForScope = EnsureCell("unifiedEndNodeForScope", "");
		}
		
		public override void Update(float dt)
		{
			if(nextNode != "") {
				Stop();
				StartNextNode();
				nextNode = "";
			}
		}
		
		public void Choose(int pOptionNr)
		{
			D.assert(pOptionNr >= 0);
			D.assert(pOptionNr < nextNodes.Length);
			string nameOfChosenNode = nextNodes[pOptionNr];
			nextNode = nameOfChosenNode;	
		}
		
		#region ACCESSORS
		
		/// <summary>
		/// Names of the possible nodes that this branching node can lead to
		/// </summary>
		public string[] nextNodes {
			get {
				return CELL_nextNodes.data;
			}
			set {
				CELL_nextNodes.data = value;
			}
		}
		
		/// <summary>
		/// Name of the node that will follow after that any branches inside this scope has finished
		/// </summary>
		public string unifiedEndNodeForScope {
			get {
                return CELL_unifiedEndNodeForScope.data;
			}
			set {
				CELL_unifiedEndNodeForScope.data = value;
			}
		}			
		
		#endregion
	}
}

