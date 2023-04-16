namespace PickUpAndHaul
{
	static class Log
	{
		[System.Diagnostics.Conditional("DEBUG")]
		public static void Message(string x) => Verse.Log.Message(x);
	}
}
