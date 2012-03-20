using System;
using GameTypes;
using RelayLib;
namespace GrimmLib
{	
	public class StartCommandoDialogueNode : DialogueNode
	{		
		ValueEntry<string> CELL_commando;
		
		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_commando = EnsureCell("commando", "undefined");
		}
		
		public override void OnEnter()
		{
			Stop();
			_dialogueRunner.StartConversation(commando);
			StartNextNode();
		}
		
		#region ACCESSORS
		
		public string commando
		{
			get {
				return CELL_commando.data;
			}
			set {
				CELL_commando.data = value;
			}
		}
		
		#endregion
	}
}

