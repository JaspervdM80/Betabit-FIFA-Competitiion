using FC26Competition.Models;
using FC26Competition.Services;
using Microsoft.AspNetCore.Components;

namespace FC26Competition.Components.Pages;

public partial class Display : IDisposable
{
    [Inject]
    private CompetitionService CompetitionService { get; set; } = default!;

    private List<Team> teams = new();
    private List<Match> groupMatches = new();
    private List<Match> knockoutMatches = new();
    private List<Match> allMatches = new();
    private DateTime currentTime = DateTime.Now;
    private System.Threading.Timer? refreshTimer;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        CompetitionService.OnChange += OnCompetitionChanged;
        
        // Auto-refresh every 5 seconds: reload data and re-render
        refreshTimer = new System.Threading.Timer(async _ =>
        {
            currentTime = DateTime.Now;
            await LoadDataAsync();
            await InvokeAsync(StateHasChanged);
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
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
        allMatches = groupMatches.Concat(knockoutMatches).ToList();
    }

    private List<Match> GetCurrentAndNextMatches()
    {
        var now = DateTime.Now;
        var currentMatches = allMatches
            .Where(m => !m.IsPlayed && m.ScheduledTime.HasValue && 
                   m.ScheduledTime.Value <= now && 
                   m.ScheduledTime.Value >= now.AddMinutes(-15))
            .ToList();

        var nextMatches = allMatches
            .Where(m => !m.IsPlayed && m.ScheduledTime.HasValue && m.ScheduledTime.Value > now)
            .OrderBy(m => m.ScheduledTime)
            .Take(4 - currentMatches.Count)
            .ToList();

        return currentMatches.Concat(nextMatches).ToList();
    }

    private string GetMatchStatus(Match match)
    {
        if (IsMatchLive(match)) return "live";
        return "upcoming";
    }

    private bool IsMatchLive(Match match)
    {
        if (!match.ScheduledTime.HasValue || match.IsPlayed) return false;
        var now = DateTime.Now;
        return match.ScheduledTime.Value <= now && match.ScheduledTime.Value >= now.AddMinutes(-15);
    }

    private string GetTimeUntil(Match match)
    {
        if (!match.ScheduledTime.HasValue) return "TBD";
        var timeUntil = match.ScheduledTime.Value - DateTime.Now;
        if (timeUntil.TotalMinutes < 0) return "NOW";
        if (timeUntil.TotalMinutes < 60) return $"in {(int)timeUntil.TotalMinutes}min";
        return $"in {(int)timeUntil.TotalHours}h {timeUntil.Minutes}min";
    }

    private Team? GetTeam(Guid teamId) => teams.FirstOrDefault(t => t.Id == teamId);

    public void Dispose()
    {
        CompetitionService.OnChange -= OnCompetitionChanged;
        refreshTimer?.Dispose();
    }
}
