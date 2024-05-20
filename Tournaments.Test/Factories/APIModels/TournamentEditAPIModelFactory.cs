namespace Tournaments.Test.Factories.APIModels;

public class TournamentEditAPIModelFactory()
{
    public static List<TournamentEditAPIModel> Generate(int count)
    {
        List<TournamentEditAPIModel> apiModels = [];

         for (int i = 0; i < count; i++)
        {
            TournamentEditAPIModel apiModel = new($"Tournament-{i + 1}")
            {
                Id = i + 1,
                StartDate = new DateOnly(2024, 1, 1)
            };
            apiModels.Add(apiModel);
        }
        return apiModels;
    }

    public static TournamentEditAPIModel GenerateSingle()
    {
        return new TournamentEditAPIModel("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 1, 1)
        };
    }
}