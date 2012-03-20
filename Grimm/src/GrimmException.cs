using System;

namespace GrimmLib
{
	public class GrimmException : Exception
	{
		public GrimmException (string pMessage) : base(pMessage)
		{
		}
	}
	
	public class GrimmAssertException : Exception
	{
		public GrimmAssertException (string pMessage) : base(pMessage)
		{
		}
	}
}

