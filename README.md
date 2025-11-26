# FC26 Competition Manager

A professional EA FC26-themed .NET 9 Blazor application for managing football competitions with group stages, knockout rounds, and live display screens.

## 🏆 Competition Format

### Group Stage
- **2 Groups of 4 Teams** (Group A and Group B)
- Round-robin within each group (3 matches per team)
- **12 total matches** (6 per group)
- Played simultaneously on 2 screens
- 6 rounds of 15 minutes each

### Knockout Stage
After a 15-minute break:
- **Top 2 from each group** → Champions League Knockout
- **Bottom 2 from each group** → Europa League Knockout
- Semi-finals + Finals for both leagues
- **6 total knockout matches**
- 3 rounds of 15 minutes each

### Schedule
- **Start Time**: 18:30 (customizable)
- **2 Screens**: Matches played simultaneously
- **Match Duration**: 15 minutes per round
- **Total Duration**: ~2.5 hours

## 🎮 Features

### Team Management
- Add, edit, and delete teams (max 8 teams)
- Each team has:
  - Team name
  - Two player names
  - Club/Country representation
- Assign teams to Group A or Group B

### Competition Setup
- Assign 4 teams to each group
- Generate complete schedule with times
- Automatic fixture generation for both stages
- Customizable start time

### Admin Panel (Separate Screen)
- Enter match results in real-time
- Switch between Group Stage and Knockout views
- Edit results if needed
- See which matches are on which screen

### Live Display (Public Screen)
- **Live scores** for current matches
- **Upcoming matches** with countdown timers
- **Group standings** with real-time updates
- **Full schedule** for the event
- Auto-refreshes every 5 seconds
- Color-coded qualification indicators

### Database Persistence
- SQLite database for data storage
- All results saved automatically
- Competition state persists between sessions

## 🎨 Design

- **EA FC26 Gaming Aesthetic**: Dark theme with neon accents
- **Betabit Branding**: Azure blue colors integrated
- **Dual Screen Support**: Admin panel + Public display
- **Responsive**: Works on all device sizes
- **Real-time Updates**: Changes sync across all screens

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK

### Running the Application

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Access the application:**
   - Main: `https://localhost:5001`
   - Admin: `https://localhost:5001/admin`
   - Display: `https://localhost:5001/display`

## 📋 Competition Workflow

### 1. Team Registration
1. Navigate to **Teams** page
2. Add 8 teams with their details
3. Each team gets a unique name, players, and club/country

### 2. Setup Competition
1. Navigate to **Setup** page
2. Assign 4 teams to Group A
3. Assign 4 teams to Group B
4. Set the start time (default 18:30)
5. Click "Generate Full Schedule"

### 3. During Competition
**Admin Screen** (`/admin`):
- Enter match results as games finish
- Switch between Group Stage and Knockout tabs
- Edit results if needed

**Display Screen** (`/display` - open in separate window/screen):
- Shows live matches with scores
- Displays group standings
- Shows upcoming matches
- Auto-updates every 5 seconds

### 4. Progression
- After all group matches complete:
  - Top 2 from each group → Champions League
  - Bottom 2 from each group → Europa League
- Knockout matches auto-generate with correct teams
- Finals determine the champions of each league

## 🖥️ Screen Setup Recommendation

### Option 1: Two Computer Setup
- **Computer 1**: Admin panel for entering results
- **Computer 2**: Display screen for audience

### Option 2: Single Computer with Multiple Monitors
- **Monitor 1**: Admin panel
- **Monitor 2**: Display screen (press F11 for fullscreen)

## 📊 Technical Details

### Tech Stack
- .NET 9
- Blazor Server (Interactive render mode)
- Entity Framework Core 9.0
- SQLite database
- C# with nullable reference types

### Database Schema
- **Teams**: Team info, group assignment, statistics
- **Matches**: Schedule, scores, stage, screen assignment

### Real-time Updates
- Event-driven architecture
- Components subscribe to service changes
- Automatic UI refresh on data updates

## 🎯 Key Features

✅ Dual-screen support (admin + display)
✅ Real-time score updates
✅ Automatic knockout bracket generation
✅ Live match indicators
✅ Countdown timers for upcoming matches
✅ Group standings with qualification zones
✅ Complete match schedule
✅ Data persistence with SQLite
✅ Responsive design
✅ EA FC26 gaming aesthetic

## 📁 Project Structure

```
FC26Competition/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor
│   ├── Pages/
│   │   ├── Teams.razor         # Team management
│   │   ├── Setup.razor         # Group assignment & schedule generation
│   │   ├── Admin.razor         # Match result entry
│   │   └── Display.razor       # Public live display
├── Data/
│   └── FC26CompetitionContext.cs
├── Models/
│   ├── Team.cs                 # Team with group assignment
│   └── Match.cs                # Match with scheduling
├── Services/
│   └── CompetitionService.cs   # Business logic
└── wwwroot/
    └── app.css                 # EA FC26 styling
```

## 🔧 Configuration

### Start Time
Modify in Setup page before generating schedule

### Match Duration
Currently fixed at 15 minutes per round
(Can be modified in `CompetitionService.cs`)

### Auto-refresh Rate
Display page refreshes every 5 seconds
(Can be modified in `Display.razor`)

## 🎪 Event Day Tips

1. **Setup before event**:
   - Register all 8 teams
   - Assign groups
   - Generate schedule
   - Test both screens

2. **During event**:
   - Keep admin screen private
   - Display screen visible to all
   - Enter results immediately after matches
   - Check group standings after each round

3. **Between stages**:
   - 15-minute break after group stage
   - Verify all group matches completed
   - Knockout fixtures auto-generate

## 🎮 Color Coding

- **🔵 Blue**: Group A, Champions League
- **🟠 Orange**: Group B, Europa League  
- **🟢 Green**: Completed matches, scores
- **🔴 Red**: Live matches
- **⏱️ Cyan**: Upcoming matches

## 📱 Browser Compatibility

- Chrome (recommended for display screen)
- Edge
- Firefox
- Safari

## 🏅 Credits

Built for Betabit FC26 competitions with EA FC26-inspired design.

---

**Ready to kick off your tournament! ⚽🎮🏆**
