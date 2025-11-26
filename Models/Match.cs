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
    public CompetitionPhase Phase { get; set; }
    public LeagueType? LeagueType { get; set; }
    
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
}

public enum CompetitionPhase
{
    MainCompetition,
    ChampionsLeague,
    EuropaLeague
}

public enum LeagueType
{
    ChampionsLeague,
    EuropaLeague
}
