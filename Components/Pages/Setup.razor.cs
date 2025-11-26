using FC26Competition.Models;
using FC26Competition.Services;
using Microsoft.AspNetCore.Components;

namespace FC26Competition.Components.Pages;

public partial class Setup : IDisposable
{
    [Inject]
    private CompetitionService CompetitionService { get; set; } = default!;

    private List<Team> teams = new();
    private bool groupsGenerated = false;
    private TimeOnly? startTimeInput = new(18, 30);
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadTeamsAsync();
        CompetitionService.OnChange += OnCompetitionChanged;
    }

    private void GoToTeams() => Navigation.NavigateTo("/");
    private void GoToAdmin() => Navigation.NavigateTo("/admin");

    private void OnCompetitionChanged()
    {
        InvokeAsync(async () =>
        {
            await LoadTeamsAsync();
            StateHasChanged();
        });
    }

    private async Task LoadTeamsAsync()
    {
        teams = await CompetitionService.GetAllTeamsAsync();
        
        // Check if groups are already assigned
        groupsGenerated = teams.Any(t => t.GroupName.HasValue);
    }

    private async Task GenerateCompetition()
    {
        try
        {
            // Shuffle teams randomly
            var random = new Random();
            var shuffledTeams = teams.OrderBy(_ => random.Next()).ToList();

            // Assign first 4 to Group A, last 4 to Group B
            var groupATeamIds = shuffledTeams.Take(4).Select(t => t.Id).ToList();
            var groupBTeamIds = shuffledTeams.Skip(4).Take(4).Select(t => t.Id).ToList();

            // Assign groups
            await CompetitionService.AssignGroupsAsync(groupATeamIds, groupBTeamIds);

            // Generate schedule
            var time = startTimeInput ?? new TimeOnly(18, 0);
            var today = DateTime.Today;
            var startTime = new DateTime(today.Year, today.Month, today.Day, time.Hour, time.Minute, 0);

            // Generate group stage fixtures
            await CompetitionService.GenerateGroupStageFixturesAsync(startTime);

            // Calculate knockout start time (after group stage + break)
            // 6 rounds of group stage × 15 min = 90 min
            // + 15 min break = 105 min total
            var knockoutStartTime = startTime.AddMinutes(105);
            await CompetitionService.GenerateKnockoutFixturesAsync(knockoutStartTime);

            await LoadTeamsAsync();
            errorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task RegenerateCompetition()
    {
        groupsGenerated = false;
        
        // Reset all team groups
        foreach (var team in teams)
        {
            team.GroupName = null;
            team.ResetStatistics();
            await CompetitionService.UpdateTeamAsync(team);
        }
        
        await LoadTeamsAsync();
        await GenerateCompetition();
    }

    public void Dispose()
    {
        CompetitionService.OnChange -= OnCompetitionChanged;
    }
}
