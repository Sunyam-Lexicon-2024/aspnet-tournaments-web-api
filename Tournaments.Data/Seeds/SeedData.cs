using Bogus;

namespace Tournaments.Data.Seeds;

public class SeedData(IUnitOfWork unitOfWork)
{
    private IUnitOfWork _unitOfWork = unitOfWork;
    private Faker _faker = new();
    private Random _rnd = new();

    private List<Tournament> _tournaments = [];
    private List<Game> _games = [];

    public async Task InitAsync()
    {

        bool tournamentExists = (await _unitOfWork.TournamentRepository.GetAllAsync()).Any();
        bool gameExists = (await _unitOfWork.GameRepository.GetAllAsync()).Any();

        if (tournamentExists || gameExists)
        {
            return;
        }

        GenerateTournaments(2);
        GenerateGames(10);

        foreach (var t in _tournaments)
        {
            await _unitOfWork.TournamentRepository.AddAsync(t);
        }

        foreach (var g in _games)
        {
            await _unitOfWork.GameRepository.AddAsync(g);
        }

    }

    private void GenerateTournaments(int count)
    {
        DateOnly startDate = DateOnly.FromDateTime(new DateTime(2000, 1, 1));
        DateOnly endDate = DateOnly.FromDateTime(new DateTime(2024, 1, 1));

        for (int i = 0; i < count; i++)
        {
            Tournament tournament = new($"Tournament-{i}")
            {
                StartDate = _faker.Date.BetweenDateOnly(startDate, endDate),
            };
            _tournaments.Add(tournament);
        }

    }
    private void GenerateGames(int count)
    {

        for (int i = 0; i < count; i++)
        {
            var tournament = _faker.PickRandom(_tournaments);
            var startTime = tournament.StartDate.ToDateTime(new TimeOnly());
            var endTime = tournament.StartDate.AddDays(_rnd.Next(3, 9)).ToDateTime(new TimeOnly());
            Game game = new($"Game-{i}")
            {
                TournamentId = tournament.Id,
                StartTime = _faker.Date.Between(startTime, endTime)
            };
            tournament.Games.Add(game);
            _games.Add(game);
        }
    }
}