using EpicGames.Core;
using MySqlConnector;

namespace BeOptimal.Automation
{

	public class DatabaseConnector
	{
		private int testID;

		private MySqlConnectionStringBuilder connectionBuilder = new MySqlConnectionStringBuilder
		{
			Server = "10.10.28.108",
			UserID = "root",
			Password = "PRJPerformance!",
			Database = "PRJPerformanceTool",
		};

		/// <summary>
		/// Calls all subfunctions to save all the data from 
		/// the csvs into the respective tables in the database
		/// </summary>
		/// <param name="clientPath">the file path of the client csv.</param>
		/// <param name="serverPath">the file path of the server csv.</param>
		public void SaveReportToDB(string processedClientPath, string processedServerPath,
			string rawClientPath, string rawServerPath, bool comparisonPass)
		{
			testID = GetLatestTestID() + 1;
			ExecuteSQL(SaveTestInfo(processedServerPath, comparisonPass), "Writing Test Info");
			SaveClientMetrics(processedClientPath);
			ExecuteSQL(SaveClientPassFail(processedClientPath), "Writing Client Pass/Fail");
			SaveServerMetrics(processedServerPath);
			ExecuteSQL(SaveServerPassFail(processedServerPath), "Writing Server Pass/Fail");
			WriteBlobToDatabase(rawClientPath, rawServerPath);
		}

		/// <summary>
		/// Executes an sql query.
		/// </summary>
		/// <param name="sql">the sql command to be executed.</param>
		/// <param name="log">an optional log message when running the function.</param>
		private void ExecuteSQL(string sql, string log = "")
		{
			// Create the connection to the database
			using var connection = new MySqlConnection(connectionBuilder.ConnectionString);
			connection.Open();

			//Print optional log message
			if (!log.Equals(""))
			{
				Console.WriteLine(log);
			}

			// Execute the Query
			MySqlCommand cmd = new MySqlCommand(sql, connection);
			cmd.ExecuteNonQuery();
			connection.Close();
		}

		/// <summary>
		/// Given the raw binary version of a file, 
		/// saves it as a blob on database
		/// </summary>
		/// <param name="clientPath">the file path of the client csv.</param>
		/// <param name="serverPath">the file path of the server csv.</param>
		private void WriteBlobToDatabase(string rawClientPath, string rawServerPath)
		{
			// Initilatize the file variables
			FileStream serverFile = null;
			UInt32 serverLength = 0;

			// Read the input files
			FileStream clientFile = new FileStream(@rawClientPath, FileMode.Open, FileAccess.Read);
			UInt32 clientLength = (uint)clientFile.Length;

			try
			{
				if (!string.IsNullOrEmpty(rawServerPath))
				{
					serverFile = new FileStream(@rawServerPath, FileMode.Open, FileAccess.Read);
					serverLength = (uint)serverFile.Length;
				}

			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not find the raw server file", e);
			}

			// Create the connection to the database
			using var connection = new MySqlConnection(connectionBuilder.ConnectionString);
			connection.Open();

			// Create the SQL Query to be run on the database
			string sql = "INSERT INTO raw_csvs VALUES(@id, @clientcsv, @servercsv, @clientsize, @serversize)";
			MySqlCommand cmd = new MySqlCommand(sql, connection);
			cmd.Parameters.AddWithValue("@id", testID);
			cmd.Parameters.AddWithValue("@clientcsv", ReadRawBinary(clientFile));
			cmd.Parameters.AddWithValue("@clientsize", clientLength);
			cmd.Parameters.AddWithValue("@servercsv", ReadRawBinary(serverFile));
			cmd.Parameters.AddWithValue("@serversize", serverLength);

			// Execute the Query
			Console.WriteLine("Writing Blob Data");
			cmd.ExecuteNonQuery();
			connection.Close();
		}

		/// <summary>
		/// Given a file, returns the raw binary version
		/// </summary>
		/// <param name="testFile">the file that's read to raw binary</param>
		/// <returns>the given file in raw binary form</returns>
		private byte[] ReadRawBinary(FileStream testFile)
		{
			try
			{
				// Read the file into raw binary
				byte[] rawData;
				rawData = new byte[testFile.Length];
				testFile.Read(rawData, 0, (int)testFile.Length);
				testFile.Close();
				return rawData;
			}
			catch (Exception e)
			{
				throw new ApplicationException("# - Provided raw binary file is null");
			}
		}

		/// <summary>
		/// Given the CSV output for the client, 
		/// generates sql code to save client metrics
		/// </summary>
		/// <param name="CSVPath">the file path for the client csv.</param>
		public void SaveClientMetrics(string CSVPath)
		{
			// Print log
			Console.WriteLine("Writing Client Metrics");

			// Read the whole csv file
			List<String> columnNames = File.ReadAllLines(CSVPath)[0].Split(',').ToList();
			var lines = File.ReadAllLines(CSVPath);

			// We skip the first 3 columns, because they do not belong in this table
			for (int j = 3; j < lines.Length; ++j)
			{
				List<String> line = lines[j].Split(',').ToList();

				// Passing on j - 3 as frame number since j starts at 3
				ExecuteSQL(SaveClientMetric(line, columnNames, j - 3));				
			}
		}

		/// <summary>
		/// Generates one line of sql code to save a single frame of client metric
		/// </summary>
		/// <param name="metricLine">The passed on line from the csv.</param>
		/// <param name="columnNames">The names of the columns in the csv.</param>
		/// <param name="frameNum">The frame number</param>
		///<returns>the sql query for uploading client metrics in the database</returns>
		public string SaveClientMetric(List<string> metricLine, List<string> columnNames, int frameNum)
		{
			string sql = $"INSERT INTO client_metrics VALUES ({testID}, {frameNum}";

			// We skip the first 3 columns because they do not belong in this table
			for (int i = 3; i < metricLine.Count; ++i)
			{
				// Create SQL Query and only add the columns with multiple metrics
				if (!columnNames[i].ToUpper().Contains("CONFIG") && !columnNames[i].ToUpper().Contains("PASS") &&
				!columnNames[i].ToUpper().Contains("HIGH") && !columnNames[i].ToUpper().Contains("LOW") &&
				!columnNames[i].ToUpper().Contains("MEAN") && !columnNames[i].ToUpper().Contains("MEDIAN") &&
				!columnNames[i].ToUpper().Contains("TOP"))
				{
					if (metricLine[i] == "")
					{
						sql += ",NULL";
					}
					else
					{
						sql += "," + metricLine[i];
					}
				}
			}
			// End the line
			sql += ")";

			return sql;
		}

		/// <summary>
		/// Given the CSV output for the client, 
		/// generates sql code for the results of the pass/fail criteria
		/// </summary>
		/// <param name="CSVPath">the file path for the client csv.</param>
		/// <returns>the sql query for uploading client pass fail in the database</returns>
		public string SaveClientPassFail(string CSVPath)
		{
			// Create the SQL Query
			List<String> columnNames = File.ReadAllLines(CSVPath)[0].Split(',').ToList();
			List<String> csvLine = File.ReadAllLines(CSVPath)[1].Split(',').ToList();
			string sql = "INSERT INTO client_pass_fail () VALUES (" + testID;

			for (int i = 3; i < csvLine.Count; ++i)
			{
				if (columnNames[i].ToUpper().Contains("CONFIG"))
				{
					sql += ",'" + csvLine[i] + "'";
				}
				else
				{
					sql += "," + csvLine[i];
				}
			}
			// End the line
			sql += ")";

			return sql;
		}

		/// <summary>
		/// Given the CSV output for the server, 
		/// generates sql code for the results of the server metrics
		/// </summary>
		/// <param name="CSVPath">the file path for the server csv.</param>
		private void SaveServerMetrics(string CSVPath)
		{
			// Read the whole csv file
			List<String> columnNames = File.ReadAllLines(CSVPath)[0].Split(',').ToList();
			var lines = File.ReadAllLines(CSVPath);

			// Printing log message
			Console.WriteLine("Writing Server Metrics");

			for (int j = 3; j < lines.Length; ++j)
			{
				List<String> line = lines[j].Split(',').ToList();

				// Passing on j - 3 as frame number since j starts at 3
				ExecuteSQL(SaveServerMetric(line, columnNames, j - 3));
			}
		}

		/// <summary>
		/// Generates one line of sql code to save a single frame of server metric
		/// </summary>
		/// <param name="metricLine">The passed on line from the csv.</param>
		/// <param name="columnNames">The names of the columns in the csv.</param>
		/// <param name="frameNum">The frame number</param>
		/// <returns>the sql query for uploading server metrics in the database</returns>
		public string SaveServerMetric(List<string> metricLine, List<string> columnNames, int frameNum)
		{
			string sql = $"INSERT INTO server_metrics VALUES ({testID}, {frameNum}";

			// For loop until count - 3, as the last 3 lines contain data for a different table
			// We also skip the first 3 columns as they do not belong in this table
			for (int i = 3; i < metricLine.Count - 3; ++i)
			{
				if (!columnNames[i].ToUpper().Contains("CONFIG") && !columnNames[i].ToUpper().Contains("PASS") &&
				!columnNames[i].ToUpper().Contains("HIGH") && !columnNames[i].ToUpper().Contains("MEAN"))
				{
					sql += "," + metricLine[i];
				}
			}
			// End the line
			sql += ")";

			return sql;
		}


		/// <summary>
		/// Given the CSV output of the server, 
		/// generates the sql code for the results of the pass/fail criteria
		/// </summary>
		/// <param name="CSVPath">the file path for the server csv.</param>
		/// <returns>the sql query for uploading server pass fail in the database</returns>
		public string SaveServerPassFail(string CSVPath)
		{
			// Create the SQL Query to be run on the database
			List<String> columnNames = File.ReadAllLines(CSVPath)[0].Split(',').ToList();
			List<String> line = File.ReadAllLines(CSVPath)[1].Split(',').ToList();
			string sql = "INSERT INTO server_pass_fail () VALUES (" + testID;

			// For loop until count - 3, as the last 3 lines contain data for a different table
			// We also skip the first three columns as they do not belong in this table
			for (int i = 3; i < line.Count - 3; ++i)
			{
				if (columnNames[i].ToUpper().Contains("CONFIG"))
				{
					sql += ",'" + line[i] + "'";
				}
				else
				{
					sql += "," + line[i];
				}
			}
			// End the line
			sql += ")";

			return sql;
		}

		/// <summary>
		/// Generates sql code to save the information related to the test 
		/// to the database
		/// </summary>
		/// <param name="CSVPath">the file path for the server csv.</param>
		/// <returns>the sql query for saving the test info in the database</returns>
		public string SaveTestInfo(string CSVPath, bool comparisonPass)
		{
			// Create SQL Query
			List<String> line = File.ReadAllLines(CSVPath)[1].Split(',').ToList();
			int numColumns = line.Count;

			var testDate = line[0];
			var testDescription = line[1];
			var testName = line[2];
			var clientResult = line[numColumns - 3];
			var serverResult = line[numColumns - 2];
			var testResult = line[numColumns - 1];

			string sql = $"INSERT INTO test_info VALUES ({testID}, '{testDate}', '{testDescription}', '{testName}', {clientResult}, {serverResult}, {testResult}, {comparisonPass})";

			return sql;
		}

		/// <summary>
		/// Removes all entries related to a test instance 
		/// from the database
		/// </summary>
		/// <param name="deleteID">the unique id of the client test instance</param>
		public void RemoveTestSession(int deleteID)
		{
			// Define the tables and ids we're using to remove the test
			string[] tableNames = {"client_metrics", "client_pass_fail", "server_metrics",
									"server_pass_fail", "raw_csvs", "test_info"};

			for (int i = 0; i < tableNames.Length; ++i)
			{
				ExecuteSQL(DeleteValueFromTable(tableNames[i], deleteID), $"Delete data from {tableNames[i]}");
			}
		}

		/// <summary>
		/// Generates sql to delete an entry from the database
		/// </summary>
		/// <param name="tableName">The name of the table to delete from.</param>
		/// <param name="deleteID">The test ID to delete the entry from.</param>
		/// <returns>the sql query for deleting a test from the table</returns>
		public string DeleteValueFromTable(string tableName, int deleteID)
		{
			string sql = $"DELETE FROM {tableName} WHERE test_id = {deleteID}";

			return sql;
		}

		/// <summary>
		/// Converts a blob retrieved from the database back 
		/// into the uploaded file extension
		/// </summary>
		/// <param name="forcedComparisonPass"> Boolean variable defining whether comparison pass has to be true </param>
		/// <param name="testName"> Name of the test </param>
		/// <returns> the filepaths of the newly created csvs</returns>
		public string[] GetLatestRawCSVFromDB(bool forcedComparisonPass, string testName)
		{
			// Get latest test id 
			int latestTestID = GetLatestTestID(forcedComparisonPass, testName);

			// Create csvs from the latest test and return their filepaths
			return ReadBlobFromDatabase(latestTestID);
		}

		/// <summary>
		/// Fetches the latest test id from the database 
		/// </summary>
		/// <returns> the latest test id from the db </returns> 
		public int GetLatestTestID()
		{
			// Create the connection to the database
			using var connection = new MySqlConnection(connectionBuilder.ConnectionString);
			connection.Open();

			// Fetch all tests from DB in order of latest first
			string SQL = "SELECT test_id from test_info ORDER BY test_id DESC";
			MySqlCommand cmd = new MySqlCommand(SQL, connection);

			// Run the SQL query
			MySqlDataReader myData = cmd.ExecuteReader();

			// Get the latest test ID
			int latestTestID = 0;
			if (myData.Read())
			{
				latestTestID = myData.GetInt32(myData.GetOrdinal("test_id"));
			}

			return latestTestID;
		}

		/// <summary>
		/// Fetches the latest test id from the database where the testName and comparison pass match
		/// </summary>
		/// <param name="forcedComparisonPass"> Boolean variable defining whether comparison pass has to be true </param>
		/// <param name="testName"> Name of the test </param>
		/// <returns>the latest test id from the db</returns>
		private int GetLatestTestID(bool forcedComparisonPass, string testName)
		{
			// Create the connection to the database
			using var connection = new MySqlConnection(connectionBuilder.ConnectionString);
			connection.Open();
			MySqlCommand cmd;
			string SQL;
			if (forcedComparisonPass)
			{
				SQL = $"SELECT test_id FROM test_info WHERE test_name = '{testName}' AND comparison_pass = {forcedComparisonPass} ORDER BY test_id DESC";
			}
			else
			{ 
				SQL = $"SELECT test_id FROM test_info WHERE test_name = '{testName}' ORDER BY test_id DESC";
			}

			// Run the SQL query
			cmd = new MySqlCommand(SQL, connection);
			MySqlDataReader myData = cmd.ExecuteReader();

			// Get the latest test ID
			int latestTestID = 0;
			if (myData.Read())
			{
				latestTestID = myData.GetInt32(myData.GetOrdinal("test_id"));
			}

			return latestTestID;
		}

		/// <summary>
		/// Converts a blob retrieved from the database back 
		/// into the uploaded file extension
		/// </summary>
		/// <param name="latestTestID">id of the latest test in the database</param>
		/// <returns>the filepaths of the newly created csvs in the form { raw client csv, raw server csv }</returns>
		private string[] ReadBlobFromDatabase(int latestTestID)
		{
			// Create the connection to the database
			using var connection = new MySqlConnection(connectionBuilder.ConnectionString);
			connection.Open();

			// Create the SQL Query to be run on the database
			string SQL = "SELECT * from raw_csvs where test_id = " + latestTestID;
			MySqlCommand cmd = new MySqlCommand(SQL, connection);

			// Run the SQL query
			MySqlDataReader myData = cmd.ExecuteReader();

			if (myData.Read())
			{
				string latestClientPath = CreateCSVFromBlobData(myData, "csv_profiler", "csv_profiler_size", "latest_client");
				string latestServerPath = CreateCSVFromBlobData(myData, "server_profiler", "server_profiler_size", "latest_server");
				return new string[] { latestClientPath, latestServerPath };
			}
			else
			{
				Console.WriteLine("# - There are no previous tests");
				return new string[] { "", "" };
			}
		}

		/// <summary>
		/// Given the database data and blob information, creates 
		/// a csv in the PerfTests folder
		/// </summary>
		/// <param name="myData">MySQL table data</param>
		/// <param name="blobData">column name of blobdata location</param>
		/// <param name="blobSize">column name of blobdat size description</param>
		/// <param name="csvName">name of produced csv</param>
		/// <returns>the filepath of the newly created csv</returns>
		private string CreateCSVFromBlobData(MySqlDataReader myData, string blobData, string blobSize, string csvName)
		{
			// Get the file size from the sql table column
			UInt32 fileSize = myData.GetUInt32(myData.GetOrdinal(blobSize));
			byte[] rawData = new byte[fileSize];

			// Get the correct test from the sql table column
			myData.GetBytes(myData.GetOrdinal(blobData), 0, rawData, 0, (int)fileSize);

			// Write it into a new file
			string filePath = "Z:/ProjectName/Game/PerfTests/CSV/" + csvName + ".csv";
			FileStream fileStream = new FileStream(@filePath, FileMode.OpenOrCreate, FileAccess.Write);
			fileStream.Write(rawData, 0, (int)fileSize);
			fileStream.Close();

			return filePath;

		}
	}
}