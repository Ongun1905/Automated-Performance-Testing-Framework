using UnrealGame;
using AutomationTool;

namespace BeOptimal.Automation
{
	public class Database : BuildCommand
	{

		/*
		* Build command entry point
		*
		* @Origin overrides ExecuteBuild() from BuildCommand
		*/
		public override void ExecuteBuild()
		{
			DatabaseConnector connector = new DatabaseConnector();

			// Determine functionality through CLI params
			bool bSave = ParseParam("save");
			bool bRemove = ParseParam("remove");

			if (bSave)
			{
				// Grab file paths from CLI
				string clientPostProcessedFile = ParseParamValue("clientPostProcessedFile", "");
				string serverPostProcessedFile = ParseParamValue("serverPostProcessedFile", "");
				string clientRawFile = ParseParamValue("clientRawFile", "");
				string serverRawFile = ParseParamValue("serverRawFile", "");

				Console.WriteLine("# - Saving to database...");
				connector.SaveReportToDB(clientPostProcessedFile, serverPostProcessedFile, clientRawFile, serverRawFile, false);
				Console.WriteLine("# - Successfully saved to database...");
			}
			else if (bRemove)
			{
				// Grab test ID
				string testID = ParseParamValue("testID", "");
			
				if (int.Parse(testID) > connector.GetLatestTestID())
				{
					Console.WriteLine("# - Given test ID does not exist...");
					return;

				}

				Console.WriteLine("# - Removing from database...");
				connector.RemoveTestSession(int.Parse(testID));
				Console.WriteLine("# - Successfully removed from database...");
			}
		}
	}
}
