using System.ComponentModel.DataAnnotations;

namespace Tournaments.Core.Entities;

public class GameCreateAPIModel
{
    [Required]
    [MinLength(5, ErrorMessage = "Title must be at least 5 characters")]
    [MaxLength(25, ErrorMessage = "Title cannot exceed 25 characters")]
    public string Title { get; set; } = null!;
    [Required]
    public int TournamentId { get; set; }
    public DateTime StartTime { get; set; }
}