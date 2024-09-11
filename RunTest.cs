using Gauntlet;
using UnrealGame;

namespace BeOptimal.Automation
{
	public class RunTest : DefaultTest
	{
		public RunTest(Gauntlet.UnrealTestContext inContext)
			: base(inContext)
		{
		}

		private DatabaseConnector connector = new DatabaseConnector();

		private string GauntletController = "PerfTestGauntletController"; // Name of GauntletController test will utilize

		/*
		* Function to Configure General Test Settings
		*
		* Origin: overrides GetConfiguration() from DefaultTest
		* Returns: UnrealTestConfig - default set of options for testing the game build.
		*/
		public override UnrealTestConfig GetConfiguration()
		{
			// Get configuration parameters from command line
			string testName = Globals.Params.ParseValue("runtest", "");
			bool launchServer = Context.TestParams.ParseParam("server");
			bool ignoreComparisonPass = Context.TestParams.ParseParam("ignoreComparisonPass");
			string configFileDir = Globals.Params.ParseValue("configdir", "");
			string uploadDir = Globals.Params.ParseValue("uploaddir", "");

			// Pull latest files and store their paths into strings
			string[] latestRawCSVs = connector.GetLatestRawCSVFromDB(!ignoreComparisonPass, testName);
			Log.Info($"# - Location of Latest Raw Client CSV: {latestRawCSVs[0]}");
			Log.Info($"# - Location of Latest Raw Server CSV: {latestRawCSVs[1]}");
			
			// Get map name from command line
			string mapName = Context.TestParams.ParseValue("testmap", "");
			if (mapName.Length <= 0)
			{
				throw new ArgumentException("No map specified. See: BeOptimal_HOW_TO_USE.txt");
			}


			UnrealTestConfig config = base.GetConfiguration();
			UnrealTestRole clientRole = config.RequireRole(UnrealTargetRole.Client);

			// If "-server" flag is provided in run_gaunt.bat, gauntlet will launch a server instance alongside client
			if (launchServer)
			{
				// Create server role, so UAT knows to launch server instance
				UnrealTestRole serverRole;
				serverRole = config.RequireRole(UnrealTargetRole.Server);

				// Grab computer IP that server will be hosted on from command line
				string networkIP = Context.TestParams.ParseValue("IP", "");
				if (networkIP == null)
				{
					throw new ArgumentException("# - missing -IP flag: see BeOptimal.bat");
				}

				// Set command line params for server-client interaction
				clientRole.CommandLine += $" -log -nosteam -X=X";
				serverRole.CommandLine += $" -dsaip=X -dsaport=X -startmode=3 -port=7777  -starttoken=X";
				serverRole.CommandLine += $"-multihome={networkIP}";

				// Set server map, client connects at "networkIP:7777"
				Log.Info($"# - Location of Latest Raw Client CSV: {mapName}");
				serverRole.MapOverride = $"{mapName}?listen";

				// If server is running client map is server IP, so pass this to CustomGauntletController.cpp
				clientRole.CommandLine += $" -testmap={networkIP}";

			}
			else
			{
				// If server is NOT running, client map is map name passed in CMD, so pass this to CustomGauntletController.cpp
				clientRole.CommandLine += $" -testmap={mapName}";
			}

			// Pass along the test name to GauntletController
			clientRole.CommandLine += $" -runtest={testName}";

			// Pass along the config directory to GauntletController
			clientRole.CommandLine += $" -configdir={configFileDir} -uploaddir={uploadDir}";

			// Pass along the strings containg the paths of the latest CSVs to GauntletControllers
			clientRole.CommandLine += $" -clientOld={latestRawCSVs[0]} -serverOld={latestRawCSVs[1]}";

			clientRole.Controllers.Add(GauntletController);

			config.MaxDuration = 10 * 60; // 10 minutes: this is a time limit, not the time the tests will take
			config.MaxDurationReachedResult = EMaxDurationReachedResult.Failure;
			return config;
		}

		// Periodically called while test is running. A chance for tests to examine their health, log updates, etc.
		public override void TickTest()
		{
			base.TickTest();
		}

		/*
		* Optional function that is called on test completion and gives opportunity to create a report.
		*
		* Use: Informs us in console whether test was passed and if any warnings or other log messages/gauntlet events occured during execution. 
		* Origin: overrides CreateReport() from UnrealTestConfig
		* Params: 
			* Result: Enum that describes the end result of a test (pass/fail)
			* Context: The context - build + global options - that the tests are going to be executed under
			* Build: The Unreal Source build being used
			* RoleResults: Results from each role in the test
			* ArtifactPath: Path to where artifacts from each role are saved
		* Returns: none
		*/
		public override ITestReport CreateReport(TestResult result, UnrealTestContext contex, UnrealBuildSource build, IEnumerable<UnrealRoleResult> roleResults, string artifactPath)
		{
			UnrealRoleArtifacts clientArtifacts = roleResults.Where(R => R.Artifacts.SessionRole.RoleType == UnrealTargetRole.Client).Select(R => R.Artifacts).FirstOrDefault();

			var snapshotSummary = new UnrealSnapshotSummary<UnrealHealthSnapshot>(clientArtifacts.AppInstance.StdOut);

			Log.Info("Performance Report");
			Log.Info(snapshotSummary.ToString());
			Log.Info($"# - Location of Latest Raw Client CSV: {result}");

			return base.CreateReport(result, contex, build, roleResults, artifactPath);
		}

		/*
		* Function to save all artifacts from the compelted test (by default saves logs and crash dumps)
		*
		* Origin: overrides SaveArtifacts_DEPRECATE() from DefaultTest
		* Params: string OutputPath - path to save artifacts too
		* Returns: none
		*/
		public override void SaveArtifacts_DEPRECATED(string outputPath)
		{
			// Grab upload directory path from command line (.bat) args
			string uploadDir = Globals.Params.ParseValue("uploaddir", "");

			Log.Info($"# - Location of Latest Raw Client CSV: {uploadDir}");

			// Ensure string is non-empty and upload directory exists
			if (uploadDir.Count() > 0 && Directory.CreateDirectory(uploadDir).Exists)
			{
				// Manage and grab path strings
				string artifactDir = TestInstance.ClientApps[0].ArtifactPath; // Path to artifacts from the process
																			  // artifacts provide results of the testing process and insights into the overall testing efforts (errors, logs, fired events)
				string profilingDir = Path.Combine(artifactDir, "Profiling");
				string csvDir = Path.Combine(profilingDir, "CSV");
				string targetDir = Path.Combine(uploadDir, "CSV");

				// Create directories for CSV files
				if (!Directory.Exists(targetDir))
					Directory.CreateDirectory(targetDir);

				if (!Directory.Exists(csvDir))
					Directory.CreateDirectory(csvDir);

				// Running Database Commands
				bool comparePass = false;

				// Obtaining comparison result from Gauntlet via the compareResult.txt file
				string comparePassPath = $"{uploadDir}\\compareResult.txt";
				try
				{
					comparePass = Convert.ToBoolean(Convert.ToInt32(File.ReadAllText(comparePassPath)));
				}
				catch (Exception e)
				{
					Log.Error($"Error reading from {comparePassPath}: {e.Message}", e);
				}	

				
				string processedClientPath = "", processedServerPath = "", rawClientPath = "",
					rawServerPath = "";

				// Move CSV file's from Artifacts folder (GauntletTemp) to uploaddir specified in CLI
				string[] csvFiles = Directory.GetFiles(csvDir);
				foreach (string csvFile in csvFiles)
				{
					string csvName = Path.GetFileName(csvFile);
					string targetCSVFile = Path.Combine(targetDir, csvName);
					File.Copy(csvFile, targetCSVFile, true);

					if (csvName.Contains("Processed"))
					{
						if (csvName.Contains("Server"))
						{
							processedServerPath = targetCSVFile;
						}
						else
						{
							processedClientPath = targetCSVFile;
						}
					}
					else
					{
						if (csvName.Contains("Server"))
						{
							rawServerPath = targetCSVFile;
						}
						else
						{
							rawClientPath = targetCSVFile;
						}
					}
				}
				Log.Info("# - Saving Data to Database");
				try
				{
					connector.SaveReportToDB(processedClientPath, processedServerPath, rawClientPath, rawServerPath, comparePass);
					Log.Info("# - Data Successfully saved to Database");
				}
				catch (Exception e) 
				{
					throw new ApplicationException("# - Data failed to be saved to Database", e);
				}

				Log.Info("# - Deleting redundant raw csvs and temp files");
				File.Delete(rawClientPath);
				File.Delete(rawServerPath);
				File.Delete(comparePassPath);
			}
			else
			{
				Log.Error("# - No UploadDir specified, CSV files were not moved from temp directory! Set one with -uploaddir=c:/path/to/dir");
			}
		}
	}
}
