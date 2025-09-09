This project runs on .NET 9 and uses Entity Framework Core for database migrations. Make sure you have the .NET 9 SDK installed along with the dotnet-ef tool.

First go to the backend folder with
cd RadioSignalsAnalysis/RadioSignalsBackEnd

Then create the database name Signals (You can do it in pgAdmin directly)

Then create the initial migration with
dotnet ef migrations add InitialCreate --project ./Repository --startup-project ./RadioSignalsWeb

Apply the migration with
dotnet ef database update InitialCreate --project ./Repository --startup-project ./RadioSignalsWeb

Run the project with
dotnet run --project RadioSignalsWeb/RadioSignalsWeb.csproj

The application will start on http://localhost:5229 for http and https://localhost:7265 for https. 
You can test the API at /scalar/v1.