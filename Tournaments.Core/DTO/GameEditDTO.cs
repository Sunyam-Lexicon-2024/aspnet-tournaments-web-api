using System.ComponentModel.DataAnnotations;

namespace Tournaments.Core.Entities;

public class GameEditDTO(string title)
{
    [MinLength(10, ErrorMessage = "Title must be at least 10 characters")]
    [MaxLength(25, ErrorMessage = "Title cannot exceed 25 characters")]
    public string Title { get; set; } = title;
    public DateTime StartTime { get; set; }
}