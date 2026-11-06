
AgriStream IoT: Real-Time Processing System
A cloud-based management platform for AgriStream IoT, a smart-farming company. It ingests high-frequency soil-sensor telemetry and drone imagery, and processes them through a serverless Azure pipeline.

Live web app: https://st10099281-cjdph8fagubncxap.southafricanorth-01.azurewebsites.net
Function App: https://<your-function-app>.azurewebsites.net
Repository: https://github.com/\<your-username>/<your-repo>
Architecture
                         +---------------------------+
   Farmer / Admin  --->  |  ASP.NET Core MVC (.NET 9)|  Azure App Service
                         +------+------+------+------+
                                |      |      |
              EF Core           |      |      | HTTP (function key)
                  v             |      |      v
          Azure SQL Database    |      |   Azure Function App (.NET 9 isolated)
          (Users, Roles,        |      |   - GetLatestTelemetry   (HTTP GET)
           Devices)             |      |   - TriggerIrrigation    (HTTP POST -> Queue)
                  ^             |      |   - ProcessIrrigationCommand (Queue trigger)
                  |             |      |   - ProcessDroneImage    (Blob trigger)
                  |             |      |
                  +-------------+------+---- Azure Storage Account
                                             - Blob:  drone-images (private)
                                             - Table: SensorReadings, audit entries
                                             - Queue: irrigation-commands
Service	Purpose
Azure App Service	Hosts the MVC web application
Azure SQL Database	User accounts, roles (Farmer, Agronomist, Admin) and the Devices registry
Azure Blob Storage	Private container drone-images for uploaded drone imagery
Azure Table Storage	SensorReadings telemetry (PartitionKey = field name, RowKey = reverse ticks) and image-processing audit entries
Azure Storage Queue	irrigation-commands queue carrying irrigation commands
Azure Functions	Four serverless functions (see below)
Application Insights	Monitoring of the web app and functions
The four Azure Functions
Function	Trigger	What it does
ProcessDroneImage	Blob	Fires when an image lands in drone-images; logs the upload details and writes a processing-status audit entry to Table Storage
GetLatestTelemetry	HTTP GET	Returns the latest sensor readings from Table Storage for the field dashboard
TriggerIrrigation	HTTP POST	Validates the request and places an irrigation command on the irrigation-commands queue
ProcessIrrigationCommand	Queue	Processes the command and updates the device's Status in the Azure SQL Devices table
Features
Registration and login with BCrypt-hashed passwords
Role-based access: Farmer, Agronomist, Admin
Drone image upload and gallery backed by private Blob Storage
Sensor telemetry simulator writing to Table Storage
Field dashboard that reads telemetry through the GET function
"Trigger Irrigation" button that sends a command through the POST function and queue to SQL
/health endpoint that checks SQL and Function App reachability
Solution structure
AgriStream/
  AgriStream.Web/          ASP.NET Core MVC app (controllers, views, services, EF Core)
  AgriStream.Functions/    Azure Functions project (.NET 9 isolated worker)
  README.md
Configuration
No secrets are committed to this repository. Values below are supplied through user secrets and local.settings.json locally, and through App Service / Function App environment variables in Azure.

MVC app (AgriStream.Web)
Setting	Notes
ConnectionStrings (SQL)	Azure SQL connection string. In App Service, add it under Connection strings with type SQLAzure
AzureStorage:*	Storage account settings for Blob and Table access. In App Service use AzureStorage__<key>
Functions:BaseUrl	Function App base URL (Functions__BaseUrl in App Service)
Functions:Key	Default host key of the Function App (Functions__Key in App Service)
Functions app (AgriStream.Functions)
Setting	Notes
AzureWebJobsStorage	Storage account used by the Functions runtime
AzureStorage	Storage connection used by the blob and queue triggers
TelemetryTable	Table name: SensorReadings
SqlConnection	Azure SQL connection string used by the queue function
Running locally
Requirements: Visual Studio 2022, .NET 9 SDK, Azure Functions workload, access to an Azure Storage account and Azure SQL database (or Azurite / local SQL).

Clone the repository and open AgriStream.sln in Visual Studio.
Right-click AgriStream.Web and choose Manage User Secrets. Add the MVC settings listed above.
In AgriStream.Functions, create local.settings.json (not committed) with the Functions settings above.
Set both projects as startup projects (Configure Startup Projects → Multiple startup projects) and press F5.
Register a user, log in, and use the telemetry simulator to generate data.
Deployment
Create an Azure SQL Database, a Storage Account, a Function App (Windows, .NET 9 isolated, Consumption) and a Web App (Windows, .NET 9).
Add the settings above to each app's environment variables.
Allow Azure services through the SQL server firewall.
Publish AgriStream.Functions first, then copy the Function App's default host key into Functions__Key on the Web App.
Publish AgriStream.Web using Visual Studio's Publish wizard.
Verify with /health, then test upload, dashboard and irrigation end to end.
Load testing, monitoring and auto-scaling
Results, screenshots and analysis for load testing, Azure Monitor / Application Insights monitoring, and App Service scale-out rules are documented in the accompanying submission document.

Technologies
ASP.NET Core MVC, Entity Framework Core, BCrypt.Net, Azure App Service, Azure SQL Database, Azure Blob / Table / Queue Storage, Azure Functions (.NET 9 isolated), Application Insights.


Claude finished the response
