using System;
using RelayLib;
namespace GrimmLib
{
	public class CancelDialogueNode : DialogueNode
	{
		ValueEntry<string> CELL_handle;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_handle = EnsureCell("handle", "");
		}
		
		public override void OnEnter()
		{
			Stop();
			_dialogueRunner.CancelRegisteredNode(conversation, handle);
			StartNextNode();
		}
		
		#region ACCESSORS
		
		public string handle {
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

