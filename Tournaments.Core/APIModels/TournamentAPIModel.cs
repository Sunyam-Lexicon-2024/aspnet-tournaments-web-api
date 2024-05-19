namespace Tournaments.Core.APIModels;

public class TournamentAPIModel(string title)
{
    public int Id { get; set; }
    public string Title { get; set; } = title;
    public DateOnly StartDate { get; set; }
    public ICollection<GameAPIModel> Games { get; } = [];
}