using System.Linq.Expressions;

namespace Tournaments.Test.Controllers;

public class TournamentsControllerTests_InvalidModelState
{
    private readonly TournamentsController _tournamentsController;
    private readonly Mock<ILogger<TournamentsController>> _mockLogger = new();
    private readonly IMapper _mapper;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();

    public TournamentsControllerTests_InvalidModelState()
    {
        var config = new MapperConfiguration(config =>
        {
            config.AddProfile<TournamentMappingProfile>();
            config.AddProfile<GameMappingProfile>();
        });

        _mapper = config.CreateMapper();

        _tournamentsController = new TournamentsController(
            _mockLogger.Object,
            _mapper,
            _mockUnitOfWork.Object);

        _tournamentsController.ModelState.AddModelError("testErrorType", "testErrorMessage");
    }

    [Fact]
    public async Task CreateTournament_Returns_BadRequestObjectResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        TournamentCreateAPIModel createModel = TournamentCreateAPIModelFactory.GenerateSingle();
        Tournament createdTournament = TournamentFactory.GenerateSingle();
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository
            .AnyAsync(It.IsAny<Expression<Func<Tournament, bool>>>()))
            .ReturnsAsync((Expression<Func<Tournament, bool>> predicate) => true);

        // Act
        var response = await _tournamentsController.CreateTournament(createModel);

        // Assert
        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task PutTournament_Returns_BadRequestObjectResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        TournamentEditAPIModel editModel = TournamentEditAPIModelFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository
            .AnyAsync(It.IsAny<Expression<Func<Tournament, bool>>>()))
            .ReturnsAsync((Expression<Func<Tournament, bool>> predicate) => true);

        // Act
        var response = await _tournamentsController.PutTournament(editModel);

        // Assert
        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task PatchTournament_Returns_BadRequestObjectResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        Tournament tournamentToPatch = TournamentFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsync(1))
            .ReturnsAsync(tournamentToPatch);

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository
            .AnyAsync(It.IsAny<Expression<Func<Tournament, bool>>>()))
            .ReturnsAsync((Expression<Func<Tournament, bool>> predicate) => true);

        JsonPatchDocument<Tournament> patchDocument = JsonPatchDocumentFactory
              .GenerateTournamentPatchDocument();

        // Act
        var response = await _tournamentsController.PatchTournament(1, null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(response.Result);
    }
}