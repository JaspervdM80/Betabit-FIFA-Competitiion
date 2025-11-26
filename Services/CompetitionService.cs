namespace FC26Competition.Services;

using FC26Competition.Models;

public class CompetitionService
{
    private readonly List<Team> _teams = new();
    private readonly List<Match> _matches = new();

    public event Action? OnChange;

    // Team Management
    public IReadOnlyList<Team> GetAllTeams() => _teams.AsReadOnly();

    public void AddTeam(Team team)
    {
        if (_teams.Count >= 16)
        {
            throw new InvalidOperationException("Maximum 16 teams allowed");
        }
        _teams.Add(team);
        NotifyStateChanged();
    }

    public void UpdateTeam(Team team)
    {
        var existingTeam = _teams.FirstOrDefault(t => t.Id == team.Id);
        if (existingTeam == null) return;

        existingTeam.TeamName = team.TeamName;
        existingTeam.Player1Name = team.Player1Name;
        existingTeam.Player2Name = team.Player2Name;
        existingTeam.ClubCountry = team.ClubCountry;
        NotifyStateChanged();
    }

    public void DeleteTeam(Guid teamId)
    {
        _teams.RemoveAll(t => t.Id == teamId);
        _matches.RemoveAll(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId);
        NotifyStateChanged();
    }

    public Team? GetTeam(Guid teamId) => _teams.FirstOrDefault(t => t.Id == teamId);

    // Match Management
    public IReadOnlyList<Match> GetAllMatches() => _matches.AsReadOnly();

    public IReadOnlyList<Match> GetMainCompetitionMatches() =>
        _matches.Where(m => m.Phase == CompetitionPhase.MainCompetition).ToList().AsReadOnly();

    public IReadOnlyList<Match> GetLeagueMatches(LeagueType leagueType) =>
        _matches.Where(m => m.LeagueType == leagueType).ToList().AsReadOnly();

    public void GenerateMainCompetitionFixtures()
    {
        if (_teams.Count != 16)
        {
            throw new InvalidOperationException("Exactly 16 teams required to generate fixtures");
        }

        // Remove existing main competition matches
        _matches.RemoveAll(m => m.Phase == CompetitionPhase.MainCompetition);

        // Reset team statistics
        foreach (var team in _teams)
        {
            team.ResetStatistics();
        }

        // Generate round-robin fixtures (each team plays every other team once)
        for (int i = 0; i < _teams.Count; i++)
        {
            for (int j = i + 1; j < _teams.Count; j++)
            {
                _matches.Add(new Match
                {
                    HomeTeamId = _teams[i].Id,
                    AwayTeamId = _teams[j].Id,
                    Phase = CompetitionPhase.MainCompetition
                });
            }
        }

        NotifyStateChanged();
    }

    public void UpdateMatchResult(Guid matchId, int homeScore, int awayScore)
    {
        var match = _matches.FirstOrDefault(m => m.Id == matchId);
        if (match == null) return;

        var homeTeam = GetTeam(match.HomeTeamId);
        var awayTeam = GetTeam(match.AwayTeamId);
        if (homeTeam == null || awayTeam == null) return;

        // Remove old result if exists
        if (match.IsPlayed)
        {
            RevertMatchStatistics(match, homeTeam, awayTeam);
        }

        // Update match
        match.HomeScore = homeScore;
        match.AwayScore = awayScore;
        match.PlayedDate = DateTime.Now;

        // Update team statistics
        UpdateTeamStatistics(match, homeTeam, awayTeam);

        NotifyStateChanged();
    }

    private void RevertMatchStatistics(Match match, Team homeTeam, Team awayTeam)
    {
        if (!match.HomeScore.HasValue || !match.AwayScore.HasValue) return;

        var homeScore = match.HomeScore.Value;
        var awayScore = match.AwayScore.Value;

        homeTeam.Played--;
        awayTeam.Played--;
        homeTeam.GoalsFor -= homeScore;
        homeTeam.GoalsAgainst -= awayScore;
        awayTeam.GoalsFor -= awayScore;
        awayTeam.GoalsAgainst -= homeScore;

        if (homeScore > awayScore)
        {
            homeTeam.Won--;
            awayTeam.Lost--;
        }
        else if (homeScore < awayScore)
        {
            homeTeam.Lost--;
            awayTeam.Won--;
        }
        else
        {
            homeTeam.Drawn--;
            awayTeam.Drawn--;
        }
    }

    private void UpdateTeamStatistics(Match match, Team homeTeam, Team awayTeam)
    {
        if (!match.HomeScore.HasValue || !match.AwayScore.HasValue) return;

        var homeScore = match.HomeScore.Value;
        var awayScore = match.AwayScore.Value;

        homeTeam.Played++;
        awayTeam.Played++;
        homeTeam.GoalsFor += homeScore;
        homeTeam.GoalsAgainst += awayScore;
        awayTeam.GoalsFor += awayScore;
        awayTeam.GoalsAgainst += homeScore;

        if (homeScore > awayScore)
        {
            homeTeam.Won++;
            awayTeam.Lost++;
        }
        else if (homeScore < awayScore)
        {
            homeTeam.Lost++;
            awayTeam.Won++;
        }
        else
        {
            homeTeam.Drawn++;
            awayTeam.Drawn++;
        }
    }

    // Split Competition
    public bool CanSplitCompetition()
    {
        var mainMatches = GetMainCompetitionMatches();
        return mainMatches.Count > 0 && mainMatches.All(m => m.IsPlayed);
    }

    public void SplitCompetition()
    {
        if (!CanSplitCompetition())
        {
            throw new InvalidOperationException("All main competition matches must be completed first");
        }

        // Get sorted teams by standings
        var sortedTeams = _teams
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.GoalDifference)
            .ThenByDescending(t => t.GoalsFor)
            .ToList();

        var championsLeagueTeams = sortedTeams.Take(4).ToList();
        var europaLeagueTeams = sortedTeams.Skip(12).Take(4).ToList();

        // Remove existing split competition matches
        _matches.RemoveAll(m => m.Phase != CompetitionPhase.MainCompetition);

        // Generate Champions League fixtures
        GenerateSplitLeagueFixtures(championsLeagueTeams, LeagueType.ChampionsLeague);

        // Generate Europa League fixtures
        GenerateSplitLeagueFixtures(europaLeagueTeams, LeagueType.EuropaLeague);

        NotifyStateChanged();
    }

    private void GenerateSplitLeagueFixtures(List<Team> teams, LeagueType leagueType)
    {
        var phase = leagueType == LeagueType.ChampionsLeague 
            ? CompetitionPhase.ChampionsLeague 
            : CompetitionPhase.EuropaLeague;

        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                _matches.Add(new Match
                {
                    HomeTeamId = teams[i].Id,
                    AwayTeamId = teams[j].Id,
                    Phase = phase,
                    LeagueType = leagueType
                });
            }
        }
    }

    public List<Team> GetLeagueStandings(LeagueType? leagueType = null)
    {
        IEnumerable<Team> teams = _teams;

        if (leagueType == LeagueType.ChampionsLeague)
        {
            // Get top 4 teams based on main competition
            teams = _teams
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GoalDifference)
                .ThenByDescending(t => t.GoalsFor)
                .Take(4);
        }
        else if (leagueType == LeagueType.EuropaLeague)
        {
            // Get bottom 4 teams based on main competition
            teams = _teams
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GoalDifference)
                .ThenByDescending(t => t.GoalsFor)
                .Skip(12)
                .Take(4);
        }

        return teams
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.GoalDifference)
            .ThenByDescending(t => t.GoalsFor)
            .ToList();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
