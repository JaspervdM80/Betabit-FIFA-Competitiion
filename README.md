# FC26 Competition Manager

A professional yet playful .NET 9 Blazor web application for managing FC26 football competitions.

## Features

### 🏆 Competition Structure
- **16 Teams**: Each team consists of two players representing a club or country
- **Round-Robin Tournament**: Every team plays every other team once (120 matches total)
- **Split Leagues**: After the main competition, teams are split into:
  - **Betabit Champions League**: Top 4 teams
  - **Betabit Europa League**: Bottom 4 teams

### ⚽ Team Management
- Add, edit, and delete teams (max 16 teams)
- Each team has:
  - Team name
  - Two player names
  - Club/Country representation
- Real-time statistics tracking

### 📊 Competition Views
1. **Teams Page**: Manage all teams
2. **Main Competition**: View standings and enter match results
3. **Split Leagues**: View Champions League and Europa League separately

### 🎨 Design
- Professional football-themed color scheme
- Responsive layout for all screen sizes
- Interactive match result entry
- Real-time standings updates
- Visual indicators for league qualification

## Technical Stack

- **.NET 9**
- **Blazor Server** with Interactive render mode
- **C# 13** with nullable reference types enabled
- Clean architecture with separation of concerns
- Singleton service for state management

## Project Structure

```
FC26Competition/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor          # Main layout with navigation
│   ├── Pages/
│   │   ├── Teams.razor               # Team management
│   │   ├── Competition.razor         # Main competition view
│   │   └── SplitLeagues.razor        # Champions & Europa League
│   ├── App.razor                     # Root component
│   ├── Routes.razor                  # Routing configuration
│   └── _Imports.razor                # Global using directives
├── Models/
│   ├── Team.cs                       # Team model with statistics
│   └── Match.cs                      # Match model with phases
├── Services/
│   └── CompetitionService.cs         # Business logic service
├── wwwroot/
│   └── app.css                       # Main stylesheet
├── Program.cs                        # Application entry point
├── appsettings.json                  # Configuration
└── FC26Competition.csproj            # Project file
```

## Getting Started

### Prerequisites
- .NET 9 SDK installed
- Any modern web browser

### Running the Application

1. Navigate to the project directory:
```bash
cd FC26Competition
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Run the application:
```bash
dotnet run
```

4. Open your browser and navigate to the URL shown in the console (typically `https://localhost:5001` or `http://localhost:5000`)

## How to Use

### Step 1: Add Teams
1. Go to the **Teams** page
2. Click "Add Team" button
3. Fill in:
   - Team name (e.g., "The Legends")
   - Player 1 and Player 2 names
   - Club/Country representation
4. Repeat until all 16 teams are added

### Step 2: Main Competition
1. Navigate to **Main Competition**
2. Click "Generate Fixtures" to create all 120 matches
3. Enter match results as games are played
4. View the live standings table
5. Once all matches are complete, click "Split into Champions & Europa League"

### Step 3: Split Leagues
1. Navigate to **Champions & Europa League**
2. Enter results for Champions League matches (top 4 teams)
3. Enter results for Europa League matches (bottom 4 teams)
4. Crown your champions! 🏆

## Features in Detail

### Standings Calculation
- **Points**: 3 for win, 1 for draw, 0 for loss
- **Tie-breakers**: 
  1. Goal difference
  2. Goals scored

### Match Management
- Enter new results
- Edit existing results
- Automatic statistics updates
- Visual feedback for played/unplayed matches

### League Split Logic
- Top 4 teams → Champions League
- Bottom 4 teams → Europa League
- Middle 8 teams do not participate in split leagues
- Each split league runs a round-robin tournament (6 matches each)

## Design Highlights

- **Color Scheme**:
  - Primary: Football pitch green (#1a472a)
  - Champions League: Blue (#0066cc)
  - Europa League: Orange (#ff6600)
- **Responsive Design**: Works on desktop, tablet, and mobile
- **Playful Elements**: Emoji icons, gradient backgrounds, hover effects
- **Professional Layout**: Clean tables, organized cards, clear navigation

## Future Enhancements (Optional)

- Data persistence (JSON file or database)
- Match scheduling with dates/times
- Player statistics (top scorers, assists)
- Knockout rounds after group stages
- Export results to PDF
- Match history and replay system
- Image uploads for teams/players

## License

This is a custom application for Betabit FC26 competitions.

---

**Enjoy your FC26 Competition! ⚽🏆**
