namespace FC26Competition.Models;

public class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid HomeTeamId { get; set; }
    public required Guid AwayTeamId { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public bool IsPlayed => HomeScore.HasValue && AwayScore.HasValue;
    public DateTime? PlayedDate { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public MatchStage Stage { get; set; }
    public GroupName? GroupName { get; set; }
    public KnockoutRound? KnockoutRound { get; set; }
    public LeagueType? LeagueType { get; set; }
    public int? ScreenNumber { get; set; } // 1 or 2
    
    public string GetResult(Guid teamId)
    {
        if (!IsPlayed) return "-";
        
        if (teamId == HomeTeamId)
        {
            if (HomeScore > AwayScore) return "W";
            if (HomeScore < AwayScore) return "L";
            return "D";
        }
        else
        {
            if (AwayScore > HomeScore) return "W";
            if (AwayScore < HomeScore) return "L";
            return "D";
        }
    }

    public Guid? GetWinnerId()
    {
        if (!IsPlayed) return null;
        if (HomeScore > AwayScore) return HomeTeamId;
        if (AwayScore > HomeScore) return AwayTeamId;
        return null; // Draw
    }
}

public enum MatchStage
{
    GroupStage,
    Knockout
}

public enum KnockoutRound
{
    SemiFinal,
    Final
}

public enum LeagueType
{
    ChampionsLeague,
    EuropaLeague
}
