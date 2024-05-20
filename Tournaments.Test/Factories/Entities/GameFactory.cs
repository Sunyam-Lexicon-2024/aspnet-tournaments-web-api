namespace Tournaments.Test.Factories.Entities;

public static class GameFactory
{
    public static List<Game> Generate(int count)
    {
        List<Game> games = [];

        for (int i = 0; i < count; i++)
        {
            Game game = new($"Game-{i + 1}")
            {
                Id = i + 1,
                TournamentId = 1,
                StartTime = new DateTime(2024, 1, 1)
            };
            games.Add(game);
        }
        return games;
    }

    public static Game GenerateSingle()
    {
        return new Game("Game-1")
        {
            Id = 1,
            TournamentId = 1,
            StartTime = new DateTime(2024, 1, 1)
        };
    }
}