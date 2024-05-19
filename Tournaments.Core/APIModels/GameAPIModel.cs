namespace Tournaments.Core.APIModels;

public class GameAPIModel(string title): IBaseEntity {
    public int Id { get; set;}
    public string Title { get; set;} = title;
    public DateTime StartTime { get; set;}
    public int TournamentId { get; set;}
}