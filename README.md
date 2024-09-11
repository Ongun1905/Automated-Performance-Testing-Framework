# Automated-Gameplay-Performance-Testing-Framework
This framework was created for our client, Behaviour Interactive, with the purpose of optimizing the testing process of an Unreal Engine 5 game project. This codebase consists of my contribution to the project.

The framework consists of multiple components including a new Unreal Engine plugin, console-based reporting tool, database for storing test results, and a Grafana web dashboard to visualize and compare test results. The plugin enables to create static and dynamic performance tests within Unreal Engine and run these tests.

I developed the database of the framework which runs on MySQL and its C# library MySQLConnector. After a test is run through our console-based reporting tool, each result is automatically uploaded to the database and is immediately shown on Grafana's web dashboard. Moreover, the database is used to automatically compare a test with the previous iteration automatically during a test run.


