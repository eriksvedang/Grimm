using System;
using RelayLib;

namespace GrimmLib
{
	public class TimedDialogueNode : DialogueNode
	{
		public static float speedScaling = 1.0f;

		ValueEntry<float> CELL_timer;
		ValueEntry<float> CELL_timerStartValue;
		ValueEntry<string> CELL_speaker;
		ValueEntry<string> CELL_line;
		
		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_timer = EnsureCell( "timer", 2.0f);
			CELL_timerStartValue = EnsureCell("timerStartValue", CELL_timer.data);
			CELL_speaker = EnsureCell("speaker", "unknown");
			CELL_line = EnsureCell("line", "");
		}

		public void CalculateAndSetTimeBasedOnLineLength(bool isOptionNode)
		{
			float baseTime = isOptionNode ? 0.8f : 1.3f;
			float timePerChar = isOptionNode ? 0.020f : 0.040f;			
			timerStartValue = timer = baseTime + line.Length * timePerChar;
		}
		
		public override void OnEnter()
		{
			_dialogueRunner.SomeoneSaidSomething(new Speech(conversation, name, speaker, line));
		}
		
		public override void OnExit()
		{
			_dialogueRunner.SomeoneSaidSomething(new Speech(conversation, name, speaker, ""));
			timer = timerStartValue;
		}
		
		public override void Update(float dt)
		{
			if(timer > 0) {
				timer -= dt * speedScaling;
				if(timer <= 0.0f) {
					Stop();
					StartNextNode();
				}
			}
		}

		public override string ToString ()
		{
			return string.Format ("[TimedDialogueNode: timer={0}, timerStartValue={1}, speaker={2}, line={3}, conversionat = {4}]", timer, timerStartValue, speaker, line,conversation);
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
		
		public string speaker {
			get {
				return CELL_speaker.data;
			}
			set {
				CELL_speaker.data = value;
			}
		}
		
		public string line {
			get {
                return CELL_line.data;
			}
			set {
				CELL_line.data = value;
			}
		}
		
		#endregion
	}
}