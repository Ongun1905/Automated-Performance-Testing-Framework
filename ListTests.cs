using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gauntlet;
using UnrealGame;

namespace BeOptimal.Automation
{
	public class ListTests : DefaultTest
	{
		public ListTests(Gauntlet.UnrealTestContext inContext)
			: base(inContext)
		{
		}

		// Periodically called while test is running. A chance for tests to examine their health, log updates, etc.
		public override void TickTest()
		{
			base.TickTest();
		}

		private string GauntletController = "ListGauntletController"; // Name of GauntletController test will utilize

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

			string mapName = Context.TestParams.ParseValue("testmap", "");
			if (mapName.Length <= 0)
			{
				// TODO: Update this with new single bat file format
				throw new ArgumentException("No map specified. Correct usage: run.bat [testname] [mapname]");
			}

			clientRole.CommandLine += $" -testmap={mapName}";
			clientRole.CommandLine += $"-windowed -ResX=640 -ResY=480 -nosteam";

			clientRole.Controllers.Add(GauntletController);

			config.MaxDuration = 10 * 60; // 10 minutes: this is a time limit, not the time the tests will take
			config.MaxDurationReachedResult = EMaxDurationReachedResult.Failure;
			return config;
		}
	}
}

