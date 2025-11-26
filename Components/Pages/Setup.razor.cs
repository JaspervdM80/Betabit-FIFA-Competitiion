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
    private TimeOnly? startTimeInput = new(18, 30);
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadTeamsAsync();
        CompetitionService.OnChange += OnCompetitionChanged;
    }

    private void GoToTeams() => Navigation.NavigateTo("/");

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

    private bool HasOverlap() => groupATeamIds.Intersect(groupBTeamIds).Any();

    private bool IsGroupACheckboxDisabled(Guid teamId) => groupATeamIds.Count >= 4 && !groupATeamIds.Contains(teamId);
    private bool IsGroupBCheckboxDisabled(Guid teamId) => groupBTeamIds.Count >= 4 && !groupBTeamIds.Contains(teamId);
    private bool IsSaveDisabled() => groupATeamIds.Count != 4 || groupBTeamIds.Count != 4 || HasOverlap();

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
            var time = startTimeInput ?? new TimeOnly(18, 0);
            var today = DateTime.Today;
            var startTime = new DateTime(today.Year, today.Month, today.Day, time.Hour, time.Minute, 0);
            await CompetitionService.GenerateGroupStageFixturesAsync(startTime);
            var knockoutStartTime = startTime.AddMinutes(105);
            await CompetitionService.GenerateKnockoutFixturesAsync(knockoutStartTime);
            Navigation.NavigateTo("/admin");
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    public void Dispose() => CompetitionService.OnChange -= OnCompetitionChanged;
}
