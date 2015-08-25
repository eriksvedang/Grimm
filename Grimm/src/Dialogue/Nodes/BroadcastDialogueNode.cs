using System;
using RelayLib;
namespace GrimmLib
{
	public class BroadcastDialogueNode : DialogueNode
	{
		ValueEntry<string> CELL_eventName;
		
		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_eventName = EnsureCell("eventName", "undefined");
		}
		
		public string eventName
		{
			get {
				return CELL_eventName.data;
			}
			set {
				CELL_eventName.data = value;
			}
		}
		
		public override void OnEnter()
		{
			Stop();
			_dialogueRunner.EventHappened(eventName);
			StartNextNode();
		}
	}
}

