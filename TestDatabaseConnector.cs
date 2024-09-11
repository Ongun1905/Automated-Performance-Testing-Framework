using Xunit;
using BeOptimal.Automation;

namespace BeOptimal.Automation
{
	public class DatabaseConnector_IsConnectedShould
	{
		
		[Fact]
		public void IsSaveTestInfo_Correct()
		{
			var connector = new DatabaseConnector();
			string CSVPath = "Z:\\ProjectName\\Game\\PerfTests\\MockCSVFiles\\serverpassfail-mock.csv";
			string expected = $"INSERT INTO test_info VALUES (0, '2024-06-26-08-32-56', 'This is a test description.', 'TestName', 0, 1, 0, False)";
			string actual = connector.SaveTestInfo(CSVPath, false);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void IsSaveClientPassFail_Correct()
		{
			var connector = new DatabaseConnector();
			string CSVPath = "Z:\\ProjectName\\Game\\PerfTests\\MockCSVFiles\\saveclientpassfail-mock.csv";
			string expected = $"INSERT INTO client_pass_fail () VALUES (0,36,1,'TRUE')";
			string actual = connector.SaveClientPassFail(CSVPath);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void IsSaveServerPassFail_Correct()
		{
			var connector = new DatabaseConnector();
			string CSVPath = "Z:\\ProjectName\\Game\\PerfTests\\MockCSVFiles\\serverpassfail-mock.csv";
			string expected = $"INSERT INTO server_pass_fail () VALUES (0,3.24,1,'TRUE')";
			string actual = connector.SaveServerPassFail(CSVPath);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void IsSaveClientMetric_Correct()
		{
			var connector = new DatabaseConnector();
			string CSVPath = "Z:\\ProjectName\\Game\\PerfTests\\MockCSVFiles\\saveclientpassfail-mock.csv";
			List<String> columnNames = File.ReadAllLines(CSVPath)[0].Split(',').ToList();
			var lines = File.ReadAllLines(CSVPath);
			List<String> line = new List<string> { "", "", "", "36", "1", "TRUE"};

			string expected = $"INSERT INTO client_metrics VALUES (0, 0,36)";
			string actual = connector.SaveClientMetric(line, columnNames, 0);
		
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void IsSaveServerMetric_Correct()
		{
			var connector = new DatabaseConnector();
			string CSVPath = "Z:\\ProjectName\\Game\\PerfTests\\MockCSVFiles\\serverpassfail-mock.csv";
			List<String> columnNames = File.ReadAllLines(CSVPath)[0].Split(',').ToList();
			var lines = File.ReadAllLines(CSVPath);
			List<String> line = new List<string> { "", "", "", "3.24", "1", "TRUE", "0", "1", "0" };

			string expected = $"INSERT INTO server_metrics VALUES (0, 0,3.24)";
			string actual = connector.SaveServerMetric(line, columnNames, 0);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void IsDeleteValueFromTable_Correct()
		{
			var connector = new DatabaseConnector();
			string tableName = "test_info";
			int deleteID = 50;

			string expected = $"DELETE FROM test_info WHERE test_id = 50";
			string actual = connector.DeleteValueFromTable(tableName, deleteID);

			Assert.Equal(expected, actual);
		}


	}
}