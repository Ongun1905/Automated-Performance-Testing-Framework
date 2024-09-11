using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealGame;
using Gauntlet;

namespace BeOptimal.Automation
{
	public class Compare : DefaultTest
	{
		private string GauntletController = "CompareGauntletController"; // Name of GauntletController test will utilize
		public Compare(Gauntlet.UnrealTestContext inContext)
			: base(inContext)
		{
		}
		
		/*
		 * Periodically called while test is running. A chance for tests to examine their health, log updates, etc.
		 * 
		 */
		public override void TickTest()
		{
			base.TickTest();
		}

		/*
		* Function to Configure General Test Settings
		*
		* Origin: overrides GetConfiguration() from DefaultTest
		* Returns: UnrealTestConfig - default set of options for testing the game build.
		*/
		public override UnrealTestConfig GetConfiguration()
		{
			UnrealTestConfig config = base.GetConfiguration();
			UnrealTestRole clientRole = config.RequireRole(UnrealTargetRole.Client);

			clientRole.Controllers.Add(GauntletController);

			clientRole.CommandLine += $"-windowed -ResX=640 -ResY=480 -nosteam";

			// Determine whether to compare files or gen config
			bool genConfig = Globals.Params.ParseParam("genconfig");

			if (!genConfig)
			{
				string newClientInputFile = Globals.Params.ParseValue("newClientInputFile", "");
				string oldClientInputFile = Globals.Params.ParseValue("oldClientInputFile", "");
				string newServerInputFile = Globals.Params.ParseValue("newServerInputFile", "");
				string oldServerInputFile = Globals.Params.ParseValue("oldServerInputFile", "");

				clientRole.CommandLine += $"-newClientInputFile={newClientInputFile}";
				clientRole.CommandLine += $"-oldClientInputFile={oldClientInputFile}";
				clientRole.CommandLine += $"-newServerInputFile={newServerInputFile}";
				clientRole.CommandLine += $"-oldServerInputFile={oldServerInputFile}";
			}
			else
			{
				clientRole.CommandLine += $" -genconfig";
			}

			string configFileDir = Globals.Params.ParseValue("configdir=", "");
			clientRole.CommandLine += $" -configdir={configFileDir}";

			config.MaxDuration = 10 * 60; // 10 minutes: this is a time limit, not the time the tests will take
			config.MaxDurationReachedResult = EMaxDurationReachedResult.Failure;
			return config;
		}
	}
}
