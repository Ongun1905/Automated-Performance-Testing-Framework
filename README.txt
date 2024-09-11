This is the source code for BeOptimal, an automated performance testing framework. A more detailed description of the code can be found in the Software Transfer Document.

Files that should be excluded from the code analysis:
- Unit tests: 
	- \Engine-Source-Programs-BeOptimal.Automation\TestDatabaseConnector.cs - file
	- \ProjectName-Plugins-BeOptimal\Source\BeOptimal\Testing - directory
	- \ProjectName-Source-ProjectName-BeOptimal\unit tests - directory

- Unchanged third party files:
	- \ProjectName-Plugins-BeOptimal\Source\BeOptimal\CSVProcessing\Private\csv.hpp - file
	- \ProjectName-Plugins-BeOptimal\Source\BeOptimal\CSVProcessing\Private\ini.h - file

- Generated files:
	- \ProjectName-Plugins-BeOptimal\Source\BeOptimal\BeOptimal.Build.cs - file
	- \Engine-Source-Programs-BeOptimal.Automation\BeOptimal.Automation.csproj - file
	- \ProjectName-Plugins-BeOptimal\BeOptimal.uplugin - file

- Non-data and non-class files:
	- MySQL_Schemas - directory
	- BeOptimal_Grafana_Dashboard.json - file
	- BeOptimal_HOW_TO_USE.txt - file
	- cook.bat - file
	- BeOptimal.bat - file
	- README.txt - file 
	- \ProjectName-Plugins-BeOptimal\Resources - directory

The rest of the files are all data/class files (.h, .cpp, .cs).

IMPORTANT Note:
BeOptimal heavily depends on the external functionality of Unreal Engine, as it is not a standalone application. Moreover, the source code is edited to follow the confidentiality agreement with the SEP client. Thus, the static analysis software used by the course (Understand) does not accurately represent some metrics. The following list argues about such metrics:

- Module Size (SLOC - CountLineCode): The software flags some elements of the project. These are all directories or namespaces that contain multiple classes. There is no actual class that passes the 400 threshold.

- Module Coupling (CBO - CountClassCoupled): The software flags the file "RunTests.cs". This file is the central point of the framework and communicates with all of the external Unreal Engine components. Most classes that are coupled to this file are required Unreal Engine components that must be used for the framework to work as expected. Thus, the file uses far less than 15 classes that the group created (more specifically, it uses only 2 of our classes).

- Code Commenting (%LOCM - RatioCommentToCode): The software flags a number of classes. If the class files are inspected, it can be observed that the actual .cpp files of the class do exceed the 15% threshold.