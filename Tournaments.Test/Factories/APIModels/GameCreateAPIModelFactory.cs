namespace Tournaments.Test.Factories.APIModels;

public class GamGameCreateAPIModelFactory()
{
    public static List<GameCreateAPIModel> Generate(int count)
    {
        List<GameCreateAPIModel> apiModels = [];

         for (int i = 0; i < count; i++)
        {
            GameCreateAPIModel game = new()
            {
                Title = $"Game-{i + 1}",
                TournamentId = 1,
                StartTime = new DateTime(2024, 1, 1)
            };
            apiModels.Add(game);
        }
        
        return apiModels;
    }

    public static GameCreateAPIModel GenerateSingle()
    {
        return new GameCreateAPIModel()
        {
            Title = "Game-1",
            TournamentId = 1,
            StartTime = new DateTime(2024, 1, 1)
        };
    }
}