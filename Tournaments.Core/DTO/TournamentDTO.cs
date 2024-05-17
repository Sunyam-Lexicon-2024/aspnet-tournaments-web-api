namespace Tournaments.Core.DTO;

public class TournamentDTO(string title)
{
    public int Id { get; set; }
    public string Title { get; set; } = title;
    public DateOnly StartDate { get; set; }
    public ICollection<GameDTO> Games { get; } = [];
}