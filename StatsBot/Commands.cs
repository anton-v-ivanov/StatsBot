namespace StatsBot
{
	public class Commands
	{
		internal const string Start = "/start";
		internal const string Stats = "/stat";
		internal const string Rules = "/rules";

		public static bool IsCommand(string text)
		{
			return text.StartsWith(Start) || text.StartsWith(Stats) || text.StartsWith(Rules);
		}
	}
}