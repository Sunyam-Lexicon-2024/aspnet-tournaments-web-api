namespace Tournaments.Test.Factories.APIModels;

public class GameAPIModelFactory
{
    public static IEnumerable<GameAPIModel> Generate(int count)
    {
        List<GameAPIModel> apiModels = [];

         for (int i = 0; i < count; i++)
        {
            GameAPIModel game = new($"Game-{i + 1}")
            {
                TournamentId = 1,
                StartTime = new DateTime(2024, 1, 1)
            };
            apiModels.Add(game);
        }
        return apiModels;
    }

    public static GameAPIModel GenerateSingle()
    {
        return new GameAPIModel("Game-1")
        {
            Id = 1,
            TournamentId = 1,
            StartTime = new DateTime(2024, 1, 1)
        };
    }
}