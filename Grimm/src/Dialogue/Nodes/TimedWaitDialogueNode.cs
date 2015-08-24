using System;
using RelayLib;
using GameTypes;

namespace GrimmLib
{
	public class TimedWaitDialogueNode : DialogueNode
	{
		ValueEntry<float> CELL_timer;
		ValueEntry<float> CELL_timerStartValue;
		
		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_timer = EnsureCell( "timer", 1.0f);
			CELL_timerStartValue = EnsureCell("timerStartValue", CELL_timer.data);
		}

		public override void OnExit()
		{
			timer = timerStartValue;
		}
		
		public override void Update(float dt)
		{
			//Console.WriteLine("Updating timed wait node, timer = " + timer);

			if(timer > 0) {
				timer -= dt;
				if(timer <= 0.0f) {
					Stop();
					StartNextNode();
				}
			}
		}
		
		#region ACCESSORS
		
		public float timer
		{
			get {
				return CELL_timer.data;
			}
			set {
				CELL_timer.data =  value;
			}
		}
		
		public float timerStartValue
		{
			get {
				return CELL_timerStartValue.data;
			}
			set {
				CELL_timerStartValue.data = value;
			}
		}
		
		#endregion
	}
}