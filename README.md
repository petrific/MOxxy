Moxxy: A simple mock server

Requirements:
- .NET Core 2.1
- Angular 6

To build:
- Run build.bat, failing that, you need to:
	- dotnet restore
	- npm install (for all the packages used)
	- ng build (for the site)
	- dotnet build

To run
- run.bat to execute with the sample json, located in ./Sample/test.json
- If you wish to use a custom json file, just run "dotnet run <path of json file>"


The test.json file should provide a glimpse as to how to build your responses. The server will pick the response 
for the path that it considers the best fit, meaning that it will always pick a path without wildcards over one containing
wildcards.
