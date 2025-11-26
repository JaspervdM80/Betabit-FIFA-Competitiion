namespace FC26Competition.Models;

public class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string TeamName { get; set; }
    public required string Player1Name { get; set; }
    public required string Player2Name { get; set; }
    public required string ClubCountry { get; set; }
    public GroupName? GroupName { get; set; }
    
    // Group Stage Statistics
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int GoalDifference => GoalsFor - GoalsAgainst;
    public int Points => (Won * 3) + Drawn;
    
    public void ResetStatistics()
    {
        Played = 0;
        Won = 0;
        Drawn = 0;
        Lost = 0;
        GoalsFor = 0;
        GoalsAgainst = 0;
    }
}

public enum GroupName
{
    GroupA,
    GroupB
}
