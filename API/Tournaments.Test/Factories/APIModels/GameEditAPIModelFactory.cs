namespace Tournaments.Test.Factories.APIModels;

public class GameEditAPIModelFactory()
{
    public static List<GameEditAPIModel> Generate(int count)
    {
        List<GameEditAPIModel> apiModels = [];

         for (int i = 0; i < count; i++)
        {
            GameEditAPIModel game = new($"Game-{i + 1}")
            {
                TournamentId = 1,
                StartTime = new DateTime(2024, 1, 1)
            };
            apiModels.Add(game);
        }

        return apiModels;
    }

    public static GameEditAPIModel GenerateSingle()
    {
        return new GameEditAPIModel("Game-1")
        {
            TournamentId = 1,
            StartTime = new DateTime(2024, 1, 1)
        };
    }
}