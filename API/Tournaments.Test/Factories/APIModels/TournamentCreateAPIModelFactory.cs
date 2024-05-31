namespace Tournaments.Test.Factories.APIModels;

public class TournamentCreateAPIModelFactory()
{
    public static List<TournamentCreateAPIModel> Generate(int count)
    {
        List<TournamentCreateAPIModel> apiModels = [];

         for (int i = 0; i < count; i++)
        {
            TournamentCreateAPIModel apiModel = new()
            {
                Title = $"Tournament-{i + 1}",
                StartDate = new DateOnly(2024, 1, 1)
            };
            apiModels.Add(apiModel);
        }
        return apiModels;
    }

    public static TournamentCreateAPIModel GenerateSingle()
    {
        return new TournamentCreateAPIModel()
        {
            Title = "Tournament-1",
            StartDate = new DateOnly(2024, 1, 1)
        };
    }
}