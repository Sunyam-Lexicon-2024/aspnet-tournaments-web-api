namespace Tournaments.Test.Factories.APIModels;

public class TournamentAPIModelFactory()
{
    public static List<TournamentAPIModel> Generate(int count)
    {
        List<TournamentAPIModel> apiModels = [];

         for (int i = 0; i < count; i++)
        {
            TournamentAPIModel apiModel = new($"Tournament-{i + 1}")
            {
                Id = i + 1,
                StartDate = new DateOnly(2024, 1, 1)
            };
            apiModels.Add(apiModel);
        }
        return apiModels;
    }

    public static TournamentAPIModel GenerateSingle()
    {
        return new TournamentAPIModel("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 1, 1)
        };
    }
}