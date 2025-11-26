# Database Setup Instructions

## SQLite Database with Entity Framework Core

This application now uses SQLite for data persistence. The database is automatically created on application startup.

## What Changed

### Added Packages
- `Microsoft.EntityFrameworkCore.Sqlite` (9.0.0)
- `Microsoft.EntityFrameworkCore.Design` (9.0.0)

### Database Location
The SQLite database file is created as `fc26competition.db` in the project root directory.

### Connection String
Located in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=fc26competition.db"
}
```

## Running the Application

### First Time Setup
1. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

The database will be **automatically created** on the first run using `EnsureCreatedAsync()`.

### Database Persistence
- All teams, matches, and results are now saved to the SQLite database
- Data persists between application restarts
- The database file (`fc26competition.db`) is excluded from git via `.gitignore`

## Database Schema

### Teams Table
- `Id` (Guid) - Primary Key
- `TeamName` (nvarchar(100))
- `Player1Name` (nvarchar(100))
- `Player2Name` (nvarchar(100))
- `ClubCountry` (nvarchar(100))
- `Played` (int)
- `Won` (int)
- `Drawn` (int)
- `Lost` (int)
- `GoalsFor` (int)
- `GoalsAgainst` (int)

### Matches Table
- `Id` (Guid) - Primary Key
- `HomeTeamId` (Guid) - Foreign Key to Teams
- `AwayTeamId` (Guid) - Foreign Key to Teams
- `HomeScore` (int, nullable)
- `AwayScore` (int, nullable)
- `PlayedDate` (datetime, nullable)
- `Phase` (int) - Enum: MainCompetition, ChampionsLeague, EuropaLeague
- `LeagueType` (int, nullable) - Enum: ChampionsLeague, EuropaLeague

## Optional: Using Migrations (Advanced)

If you want to use EF Core Migrations instead of `EnsureCreated()`:

1. Install the EF Core tools:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. Create initial migration:
   ```bash
   dotnet ef migrations add InitialCreate
   ```

3. Apply migration:
   ```bash
   dotnet ef database update
   ```

4. Update `Program.cs` to use migrations instead of `EnsureCreatedAsync()`:
   ```csharp
   // Replace this line:
   await context.Database.EnsureCreatedAsync();
   
   // With:
   await context.Database.MigrateAsync();
   ```

## Resetting the Database

To start fresh (delete all data):

1. Stop the application
2. Delete the `fc26competition.db` file
3. Restart the application - a new empty database will be created

## Troubleshooting

### Database locked error
If you get a "database is locked" error:
- Make sure only one instance of the application is running
- Close any SQLite browser tools that might have the database open

### Package restore issues
```bash
dotnet clean
dotnet restore
dotnet build
```

## Architecture Changes

### Service Layer
- `CompetitionService` now uses `IDbContextFactory<FC26CompetitionContext>`
- All methods are now async (suffix with `Async`)
- Uses scoped DbContext instances for each operation

### Components
- All Blazor pages updated to use async methods (`OnInitializedAsync`, etc.)
- Button click handlers call async methods
- Proper disposal of event handlers

### Benefits
- ✅ Data persists between sessions
- ✅ Supports concurrent access with DbContext factory
- ✅ Clean architecture with separation of concerns
- ✅ Easy to backup (just copy the .db file)
- ✅ No external database server required
