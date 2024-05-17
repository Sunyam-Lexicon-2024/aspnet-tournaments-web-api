namespace Tournaments.Core.Entities;

public class Tournament(string title): IBaseEntity {
    public int Id { get; set;}
    public string Title { get; set;} = title;
    public DateOnly StartDate { get; set;}
    public ICollection<Game> Games { get;} = [];
}