using System.Linq.Expressions;

namespace Tournaments.Test.Controllers;

public class GamesControllerTests_InvalidModelState
{
    private readonly GamesController _gamesController;
    private readonly Mock<ILogger<Game>> _mockLogger = new();
    private readonly IMapper _mapper;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();

    public GamesControllerTests_InvalidModelState()
    {
        var config = new MapperConfiguration(config =>
        {
            config.AddProfile<TournamentMappingProfile>();
            config.AddProfile<GameMappingProfile>();
        });

        _mapper = config.CreateMapper();

        _gamesController = new(
            _mockLogger.Object,
            _mapper,
            _mockUnitOfWork.Object);

        _gamesController.ModelState.AddModelError("testErrorType", "testErrorMessage");
    }

    [Fact]
    public async Task CreateGame_Returns_BadRequestObjectResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        GameCreateAPIModel createModel = GamGameCreateAPIModelFactory
            .GenerateSingle();
        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(g => g.Title == createModel.Title &&
                g.TournamentId == createModel.TournamentId))
            .ReturnsAsync(false);

        // Act
        var response = await _gamesController.CreateGame(createModel);

        // Assert
        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task PutGame_Returns_BadRequestObjectResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        GameEditAPIModel editModel = GameEditAPIModelFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(g => g.Id == It.IsAny<Game>().Id))
            .ReturnsAsync(true);

        // Act
        var response = await _gamesController.PutGame(editModel);

        // Assert
        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task PatchGame_Returns_BadRequestObjectResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        Game gameToPatch = GameFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsync(gameToPatch.Id))
            .ReturnsAsync(gameToPatch);

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(It.IsAny<Expression<Func<Game, bool>>>()))
            .ReturnsAsync((Expression<Func<Game, bool>> predicate) =>
                predicate.Compile().Invoke(gameToPatch));

        JsonPatchDocument<Game> patchDocument = JsonPatchDocumentFactory
            .GenerateGamePatchDocument();

        // Act
        var response = await _gamesController.PatchGame(gameToPatch.Id, null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(response.Result);
    }
}