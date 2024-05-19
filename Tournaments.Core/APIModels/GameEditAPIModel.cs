using System.ComponentModel.DataAnnotations;

namespace Tournaments.Core.Entities;

public class GameEditAPIModel(string title)
{
    public int Id { get; set; }
    [Required]
    [MinLength(10, ErrorMessage = "Title must be at least 10 characters")]
    [MaxLength(25, ErrorMessage = "Title cannot exceed 25 characters")]
    public string Title { get; set; } = title;
    public int TournamentId { get; set; }
    public DateTime StartTime { get; set; }
}