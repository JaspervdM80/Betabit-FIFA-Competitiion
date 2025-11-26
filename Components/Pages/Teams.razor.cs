using FC26Competition.Models;
using FC26Competition.Services;
using Microsoft.AspNetCore.Components;

namespace FC26Competition.Components.Pages;

public partial class Teams : IDisposable
{
    [Inject]
    private CompetitionService CompetitionService { get; set; } = default!;

    private List<Team> teams = new();
    private bool showModal = false;
    private Team? editingTeam = null;
    private string teamName = string.Empty;
    private string player1Name = string.Empty;
    private string player2Name = string.Empty;
    private string clubCountry = string.Empty;
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
    }

    private void ShowAddModal()
    {
        editingTeam = null;
        teamName = string.Empty;
        player1Name = string.Empty;
        player2Name = string.Empty;
        clubCountry = string.Empty;
        errorMessage = string.Empty;
        showModal = true;
    }

    private void ShowEditModal(Team team)
    {
        editingTeam = team;
        teamName = team.TeamName;
        player1Name = team.Player1Name;
        player2Name = team.Player2Name;
        clubCountry = team.ClubCountry;
        errorMessage = string.Empty;
        showModal = true;
    }

    private void HideModal()
    {
        showModal = false;
        editingTeam = null;
        errorMessage = string.Empty;
    }

    private async Task SaveTeamAsync()
    {
        if (string.IsNullOrWhiteSpace(teamName) ||
            string.IsNullOrWhiteSpace(player1Name) ||
            string.IsNullOrWhiteSpace(player2Name) ||
            string.IsNullOrWhiteSpace(clubCountry))
        {
            errorMessage = "All fields are required.";
            return;
        }

        try
        {
            if (editingTeam == null)
            {
                var newTeam = new Team
                {
                    TeamName = teamName.Trim(),
                    Player1Name = player1Name.Trim(),
                    Player2Name = player2Name.Trim(),
                    ClubCountry = clubCountry.Trim()
                };
                await CompetitionService.AddTeamAsync(newTeam);
            }
            else
            {
                editingTeam.TeamName = teamName.Trim();
                editingTeam.Player1Name = player1Name.Trim();
                editingTeam.Player2Name = player2Name.Trim();
                editingTeam.ClubCountry = clubCountry.Trim();
                await CompetitionService.UpdateTeamAsync(editingTeam);
            }

            await LoadTeamsAsync();
            HideModal();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task DeleteTeamAsync(Guid teamId)
    {
        await CompetitionService.DeleteTeamAsync(teamId);
        await LoadTeamsAsync();
    }

    public void Dispose()
    {
        CompetitionService.OnChange -= OnCompetitionChanged;
    }
}
