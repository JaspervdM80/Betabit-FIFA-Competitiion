using FC26Competition.Data;
using FC26Competition.Models;
using Microsoft.EntityFrameworkCore;

namespace FC26Competition.Services;

public class CompetitionService
{
    private readonly IDbContextFactory<FC26CompetitionContext> _contextFactory;

    public event Action? OnChange;

    public CompetitionService(IDbContextFactory<FC26CompetitionContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // Team Management
    public async Task<List<Team>> GetAllTeamsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Teams.OrderBy(t => t.TeamName).ToListAsync();
    }

    public async Task AddTeamAsync(Team team)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var count = await context.Teams.CountAsync();
        if (count >= 8)
        {
            throw new InvalidOperationException("Maximum 8 teams allowed");
        }
        
        context.Teams.Add(team);
        await context.SaveChangesAsync();
        NotifyStateChanged();
    }

    public async Task UpdateTeamAsync(Team team)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var existingTeam = await context.Teams.FindAsync(team.Id);
        if (existingTeam == null) return;

        existingTeam.TeamName = team.TeamName;
        existingTeam.Player1Name = team.Player1Name;
        existingTeam.Player2Name = team.Player2Name;
        existingTeam.ClubCountry = team.ClubCountry;
        
        await context.SaveChangesAsync();
        NotifyStateChanged();
    }

    public async Task DeleteTeamAsync(Guid teamId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var team = await context.Teams.FindAsync(teamId);
        if (team != null)
        {
            context.Teams.Remove(team);
        }
        
        var matches = await context.Matches
            .Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
            .ToListAsync();
        context.Matches.RemoveRange(matches);
        
        await context.SaveChangesAsync();
        NotifyStateChanged();
    }

    public async Task<Team?> GetTeamAsync(Guid teamId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Teams.FindAsync(teamId);
    }

    // Group Assignment
    public async Task AssignGroupsAsync(List<Guid> groupATeamIds, List<Guid> groupBTeamIds)
    {
        if (groupATeamIds.Count != 4 || groupBTeamIds.Count != 4)
        {
            throw new InvalidOperationException("Each group must have exactly 4 teams");
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        
        foreach (var teamId in groupATeamIds)
        {
            var team = await context.Teams.FindAsync(teamId);
            if (team != null)
            {
                team.GroupName = GroupName.GroupA;
                team.ResetStatistics();
            }
        }
        
        foreach (var teamId in groupBTeamIds)
        {
            var team = await context.Teams.FindAsync(teamId);
            if (team != null)
            {
                team.GroupName = GroupName.GroupB;
                team.ResetStatistics();
            }
        }
        
        await context.SaveChangesAsync();
        NotifyStateChanged();
    }

    // Match Management
    public async Task<List<Match>> GetAllMatchesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Matches
            .OrderBy(m => m.ScheduledTime)
            .ThenBy(m => m.ScreenNumber)
            .ToListAsync();
    }

    public async Task<List<Match>> GetGroupStageMatchesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Matches
            .Where(m => m.Stage == MatchStage.GroupStage)
            .OrderBy(m => m.ScheduledTime)
            .ThenBy(m => m.ScreenNumber)
            .ToListAsync();
    }

    public async Task<List<Match>> GetKnockoutMatchesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Matches
            .Where(m => m.Stage == MatchStage.Knockout)
            .OrderBy(m => m.LeagueType)
            .ThenBy(m => m.KnockoutRound)
            .ThenBy(m => m.ScheduledTime)
            .ToListAsync();
    }

    public async Task GenerateGroupStageFixturesAsync(DateTime startTime)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var teams = await context.Teams.ToListAsync();
        var groupATeams = teams.Where(t => t.GroupName == GroupName.GroupA).ToList();
        var groupBTeams = teams.Where(t => t.GroupName == GroupName.GroupB).ToList();

        if (groupATeams.Count != 4 || groupBTeams.Count != 4)
        {
            throw new InvalidOperationException("Each group must have exactly 4 teams assigned");
        }

        // Remove existing group stage matches
        var existingMatches = await context.Matches
            .Where(m => m.Stage == MatchStage.GroupStage)
            .ToListAsync();
        context.Matches.RemoveRange(existingMatches);

        var currentTime = startTime;

        // Generate round-robin for Group A and Group B
        var groupAMatches = GenerateRoundRobinMatches(groupATeams, GroupName.GroupA);
        var groupBMatches = GenerateRoundRobinMatches(groupBTeams, GroupName.GroupB);

        // Pair matches from both groups to be played simultaneously
        for (int i = 0; i < groupAMatches.Count; i++)
        {
            var groupAMatch = groupAMatches[i];
            var groupBMatch = groupBMatches[i];

            groupAMatch.ScheduledTime = currentTime;
            groupBMatch.ScheduledTime = currentTime;

            // Alternate screens each round to split screen usage evenly per group
            if (i % 2 == 0)
            {
                groupAMatch.ScreenNumber = 1;
                groupBMatch.ScreenNumber = 2;
            }
            else
            {
                groupAMatch.ScreenNumber = 2;
                groupBMatch.ScreenNumber = 1;
            }

            context.Matches.Add(groupAMatch);
            context.Matches.Add(groupBMatch);

            currentTime = currentTime.AddMinutes(15);
        }

        await context.SaveChangesAsync();
        NotifyStateChanged();
    }

    private List<Match> GenerateRoundRobinMatches(List<Team> teams, GroupName groupName)
    {
        var matches = new List<Match>();
        
        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                matches.Add(new Match
                {
                    HomeTeamId = teams[i].Id,
                    AwayTeamId = teams[j].Id,
                    Stage = MatchStage.GroupStage,
                    GroupName = groupName
                });
            }
        }
        
        return matches;
    }

    public async Task GenerateKnockoutFixturesAsync(DateTime startTime)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Check if knockout fixtures already exist
        var existingKnockout = await context.Matches
            .Where(m => m.Stage == MatchStage.Knockout)
            .AnyAsync();

        if (existingKnockout)
        {
            // Knockout already generated, skip
            return;
        }

        // Check if all group stage matches are played
        var groupMatches = await context.Matches
            .Where(m => m.Stage == MatchStage.GroupStage)
            .ToListAsync();

        if (!groupMatches.Any() || !groupMatches.All(m => m.IsPlayed))
        {
            throw new InvalidOperationException("All group stage matches must be completed first");
        }

        // Get group standings
        var teams = await context.Teams.ToListAsync();
        var groupAStandings = GetGroupStandings(teams, GroupName.GroupA);
        var groupBStandings = GetGroupStandings(teams, GroupName.GroupB);

        var currentTime = startTime;

        // Generate Champions League Semi-Finals
        // Group A 1st vs Group B 2nd
        context.Matches.Add(new Match
        {
            HomeTeamId = groupAStandings[0].Id,
            AwayTeamId = groupBStandings[1].Id,
            Stage = MatchStage.Knockout,
            KnockoutRound = KnockoutRound.SemiFinal,
            LeagueType = LeagueType.ChampionsLeague,
            ScheduledTime = currentTime,
            ScreenNumber = 1
        });

        // Europa League Semi-Final
        // Group A 3rd vs Group B 4th
        context.Matches.Add(new Match
        {
            HomeTeamId = groupAStandings[2].Id,
            AwayTeamId = groupBStandings[3].Id,
            Stage = MatchStage.Knockout,
            KnockoutRound = KnockoutRound.SemiFinal,
            LeagueType = LeagueType.EuropaLeague,
            ScheduledTime = currentTime,
            ScreenNumber = 2
        });

        currentTime = currentTime.AddMinutes(15);

        // Generate second semi-finals
        // Group B 1st vs Group A 2nd
        context.Matches.Add(new Match
        {
            HomeTeamId = groupBStandings[0].Id,
            AwayTeamId = groupAStandings[1].Id,
            Stage = MatchStage.Knockout,
            KnockoutRound = KnockoutRound.SemiFinal,
            LeagueType = LeagueType.ChampionsLeague,
            ScheduledTime = currentTime,
            ScreenNumber = 1
        });

        // Group B 3rd vs Group A 4th
        context.Matches.Add(new Match
        {
            HomeTeamId = groupBStandings[2].Id,
            AwayTeamId = groupAStandings[3].Id,
            Stage = MatchStage.Knockout,
            KnockoutRound = KnockoutRound.SemiFinal,
            LeagueType = LeagueType.EuropaLeague,
            ScheduledTime = currentTime,
            ScreenNumber = 2
        });

        currentTime = currentTime.AddMinutes(15);

        // Generate Finals sequentially (Europa League first, then Champions League with 10-minute pause)
        // Europa League Final
        context.Matches.Add(new Match
        {
            HomeTeamId = Guid.Empty, // TBD
            AwayTeamId = Guid.Empty, // TBD
            Stage = MatchStage.Knockout,
            KnockoutRound = KnockoutRound.Final,
            LeagueType = LeagueType.EuropaLeague,
            ScheduledTime = currentTime,
            ScreenNumber = 1
        });

        // Add 10 minute pause after Europa League final
        currentTime = currentTime.AddMinutes(15 + 10);

        // Champions League Final
        context.Matches.Add(new Match
        {
            HomeTeamId = Guid.Empty, // TBD
            AwayTeamId = Guid.Empty, // TBD
            Stage = MatchStage.Knockout,
            KnockoutRound = KnockoutRound.Final,
            LeagueType = LeagueType.ChampionsLeague,
            ScheduledTime = currentTime,
            ScreenNumber = 1
        });

        await context.SaveChangesAsync();
        NotifyStateChanged();
    }

    public async Task UpdateMatchResultAsync(Guid matchId, int homeScore, int awayScore)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var match = await context.Matches.FindAsync(matchId);
        if (match == null) return;

        var wasGroupStageMatch = match.Stage == MatchStage.GroupStage;

        // For knockout finals, check if this completes a semi-final and update final
        if (match.Stage == MatchStage.Knockout && match.KnockoutRound == KnockoutRound.SemiFinal)
        {
            var winnerId = homeScore > awayScore ? match.HomeTeamId : match.AwayTeamId;
            
            // Find the corresponding final
            var final = await context.Matches
                .FirstOrDefaultAsync(m => 
                    m.Stage == MatchStage.Knockout && 
                    m.KnockoutRound == KnockoutRound.Final &&
                    m.LeagueType == match.LeagueType);

            if (final != null)
            {
                // Get all semi-finals for this league
                var semiFinals = await context.Matches
                    .Where(m => 
                        m.Stage == MatchStage.Knockout && 
                        m.KnockoutRound == KnockoutRound.SemiFinal &&
                        m.LeagueType == match.LeagueType)
                    .ToListAsync();

                // Update this semi-final's score first
                match.HomeScore = homeScore;
                match.AwayScore = awayScore;
                match.PlayedDate = DateTime.Now;

                // Check if both semi-finals are now complete
                if (semiFinals.Count == 2 && semiFinals.All(sf => sf.Id == match.Id ? true : sf.IsPlayed))
                {
                    var winner1 = semiFinals[0].Id == match.Id ? winnerId : semiFinals[0].GetWinnerId();
                    var winner2 = semiFinals[1].Id == match.Id ? winnerId : semiFinals[1].GetWinnerId();

                    if (winner1.HasValue && winner2.HasValue)
                    {
                        final.HomeTeamId = winner1.Value;
                        final.AwayTeamId = winner2.Value;
                    }
                }
            }
        }

        var homeTeam = await context.Teams.FindAsync(match.HomeTeamId);
        var awayTeam = await context.Teams.FindAsync(match.AwayTeamId);

        // Only update team statistics for group stage matches
        if (match.Stage == MatchStage.GroupStage && homeTeam != null && awayTeam != null)
        {
            // Remove old result if exists
            if (match.HomeScore.HasValue && match.AwayScore.HasValue)
            {
                RevertMatchStatistics(match, homeTeam, awayTeam);
            }

            // Update match
            match.HomeScore = homeScore;
            match.AwayScore = awayScore;
            match.PlayedDate = DateTime.Now;

            // Update team statistics
            UpdateTeamStatistics(match, homeTeam, awayTeam);
        }
        else if (match.Stage == MatchStage.Knockout)
        {
            // For knockout matches, just update the score (already done above for semi-finals)
            if (match.KnockoutRound != KnockoutRound.SemiFinal)
            {
                match.HomeScore = homeScore;
                match.AwayScore = awayScore;
                match.PlayedDate = DateTime.Now;
            }
        }

        await context.SaveChangesAsync();

        // Check if this was the last group stage match and auto-generate knockout
        if (wasGroupStageMatch)
        {
            var allGroupMatches = await context.Matches
                .Where(m => m.Stage == MatchStage.GroupStage)
                .ToListAsync();

            // If all group matches are now complete, auto-generate knockout
            if (allGroupMatches.All(m => m.IsPlayed))
            {
                // Get the scheduled time of the last group match + 15 minute break
                var lastGroupMatchTime = allGroupMatches
                    .Where(m => m.ScheduledTime.HasValue)
                    .Max(m => m.ScheduledTime!.Value);

                var knockoutStartTime = lastGroupMatchTime.AddMinutes(15);

                try
                {
                    await GenerateKnockoutFixturesAsync(knockoutStartTime);
                }
                catch
                {
                    // Knockout already exists or some other issue, ignore
                }
            }
        }

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

    public List<Team> GetGroupStandings(List<Team> teams, GroupName groupName)
    {
        return teams
            .Where(t => t.GroupName == groupName)
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.GoalDifference)
            .ThenByDescending(t => t.GoalsFor)
            .ToList();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
