using FC26Competition.Models;
using FC26Competition.Services;
using Microsoft.AspNetCore.Components;

namespace FC26Competition.Components.Pages;

public partial class Setup : IDisposable
{
    [Inject]
    private CompetitionService CompetitionService { get; set; } = default!;

    private List<Team> teams = new();
    private HashSet<Guid> groupATeamIds = new();
    private HashSet<Guid> groupBTeamIds = new();
    private bool groupsAssigned = false;
    private string startTimeInput = "18:30";
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadTeamsAsync();
        CompetitionService.OnChange += OnCompetitionChanged;
    }

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
        
        // Load existing group assignments
        groupATeamIds = teams.Where(t => t.GroupName == GroupName.GroupA).Select(t => t.Id).ToHashSet();
        groupBTeamIds = teams.Where(t => t.GroupName == GroupName.GroupB).Select(t => t.Id).ToHashSet();
        
        groupsAssigned = groupATeamIds.Count == 4 && groupBTeamIds.Count == 4;
    }

    private void ToggleGroupA(Guid teamId)
    {
        if (groupATeamIds.Contains(teamId))
        {
            groupATeamIds.Remove(teamId);
        }
        else if (groupATeamIds.Count < 4 && !groupBTeamIds.Contains(teamId))
        {
            groupATeamIds.Add(teamId);
        }
        errorMessage = string.Empty;
    }

    private void ToggleGroupB(Guid teamId)
    {
        if (groupBTeamIds.Contains(teamId))
        {
            groupBTeamIds.Remove(teamId);
        }
        else if (groupBTeamIds.Count < 4 && !groupATeamIds.Contains(teamId))
        {
            groupBTeamIds.Add(teamId);
        }
        errorMessage = string.Empty;
    }

    private bool HasOverlap()
    {
        return groupATeamIds.Intersect(groupBTeamIds).Any();
    }

    private async Task SaveGroupAssignments()
    {
        if (groupATeamIds.Count != 4 || groupBTeamIds.Count != 4)
        {
            errorMessage = "Each group must have exactly 4 teams.";
            return;
        }

        if (HasOverlap())
        {
            errorMessage = "Teams cannot be in both groups.";
            return;
        }

        try
        {
            await CompetitionService.AssignGroupsAsync(groupATeamIds.ToList(), groupBTeamIds.ToList());
            await LoadTeamsAsync();
            errorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task GenerateSchedule()
    {
        try
        {
            var today = DateTime.Today;
            var timeParts = startTimeInput.Split(':');
            var startTime = new DateTime(today.Year, today.Month, today.Day, 
                int.Parse(timeParts[0]), int.Parse(timeParts[1]), 0);

            // Generate group stage fixtures
            await CompetitionService.GenerateGroupStageFixturesAsync(startTime);

            // Calculate knockout start time (after group stage + break)
            // 6 rounds of group stage × 15 min = 90 min
            // + 15 min break = 105 min total
            var knockoutStartTime = startTime.AddMinutes(105);
            await CompetitionService.GenerateKnockoutFixturesAsync(knockoutStartTime);

            Navigation.NavigateTo("/admin");
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    public void Dispose()
    {
        CompetitionService.OnChange -= OnCompetitionChanged;
    }
}
