# Electricity Billing & Customer Management System

Metering and billing operations. Built with C#, ASP.NET Core MVC, Razor and SQL Server, with ADO.NET, stored procedures, a reporting view and indexes.

## Run locally

Use the .NET 7 SDK and SQL Server. The default is SQL Server LocalDB with database `ElectricityPortal`. Set `ConnectionStrings__Default` to use another instance. Initialization requires database creation privileges.

```powershell
dotnet run -- --init-db
dotnet run -- --urls http://localhost:5202
```

Open [http://localhost:5202](http://localhost:5202).

## Demo workflow

Register a customer, record the meter baseline, then add a larger cumulative reading on a later date. SQL Server creates a bill at INR 6.50 per kWh above the previous reading.


This portfolio demo targets the installed .NET 7 SDK. Upgrade to a supported runtime before public hosting.
