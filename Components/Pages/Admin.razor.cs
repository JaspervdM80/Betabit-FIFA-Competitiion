using FC26Competition.Models;
using FC26Competition.Services;
using Microsoft.AspNetCore.Components;

namespace FC26Competition.Components.Pages;

public partial class Admin : IDisposable
{
    [Inject]
    private CompetitionService CompetitionService { get; set; } = default!;

    private List<Team> teams = new();
    private List<Match> groupMatches = new();
    private List<Match> knockoutMatches = new();
    private Dictionary<Guid, int> homeScores = new();
    private Dictionary<Guid, int> awayScores = new();
    private bool showGroupStage = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        CompetitionService.OnChange += OnCompetitionChanged;
    }

    private void OnCompetitionChanged()
    {
        InvokeAsync(async () =>
        {
            await LoadDataAsync();
            StateHasChanged();
        });
    }

    private async Task LoadDataAsync()
    {
        teams = await CompetitionService.GetAllTeamsAsync();
        groupMatches = await CompetitionService.GetGroupStageMatchesAsync();
        knockoutMatches = await CompetitionService.GetKnockoutMatchesAsync();
        InitializeScoreDictionaries();
    }

    private void InitializeScoreDictionaries()
    {
        homeScores.Clear();
        awayScores.Clear();
        
        foreach (var match in groupMatches.Concat(knockoutMatches))
        {
            homeScores[match.Id] = match.HomeScore ?? 0;
            awayScores[match.Id] = match.AwayScore ?? 0;
        }
    }

    private async Task SaveMatchResultAsync(Match match)
    {
        if (homeScores.TryGetValue(match.Id, out var homeScore) &&
            awayScores.TryGetValue(match.Id, out var awayScore))
        {
            await CompetitionService.UpdateMatchResultAsync(match.Id, homeScore, awayScore);
        }
    }

    private void EditMatchResult(Match match)
    {
        if (match.HomeScore.HasValue && match.AwayScore.HasValue)
        {
            homeScores[match.Id] = match.HomeScore.Value;
            awayScores[match.Id] = match.AwayScore.Value;
            match.HomeScore = null;
            match.AwayScore = null;
        }
    }

    private Team? GetTeam(Guid teamId) => teams.FirstOrDefault(t => t.Id == teamId);

    public void Dispose()
    {
        CompetitionService.OnChange -= OnCompetitionChanged;
    }
}
