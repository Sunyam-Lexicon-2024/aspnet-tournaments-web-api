using System.ComponentModel.DataAnnotations;

namespace Tournaments.Core.APIModels;

public class TournamentCreateAPIModel : IBaseAPIModel
{
    [MinLength(5, ErrorMessage = "Title must be at least 5 characters")]
    [MaxLength(25, ErrorMessage = "Title cannot exceed 25 characters")]
    [Required]
    public string Title { get; set; } = null!;
    public DateOnly StartDate { get; set; }
}