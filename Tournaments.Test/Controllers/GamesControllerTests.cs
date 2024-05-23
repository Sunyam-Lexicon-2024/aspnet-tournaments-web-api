namespace Tournaments.Test.Controllers;

public class GamesControllerTests
{
    private readonly GamesController _gamesController;
    private readonly Mock<ILogger<Game>> _mockLogger = new();
    private readonly IMapper _mapper;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly List<Game> _mockGames = GameFactory.Generate(10);

    public GamesControllerTests()
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
    }

    [Fact]
    public async Task GetGames_Returns_OkObjectResult_When_Entities_Exist()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .GetAllAsync())
            .ReturnsAsync(_mockGames);

        // Act
        var response = await _gamesController.GetGames();

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetGames_Returns_All_Entities_When_Entities_Exist()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .GetAllAsync())
            .ReturnsAsync(_mockGames);

        // Act
        var result = await _gamesController.GetGames();
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<GameAPIModel>)okResult.Value!;

        // Assert
        Assert.Equal(_mockGames.Count, responseItems.Count());
    }

    [Fact]
    public async Task GetGames_Returns_NoContentResult_When_No_Entities_Exist()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .GetAllAsync())
            .ReturnsAsync([]);

        // Act
        var response = await _gamesController.GetGames();

        // Assert
        Assert.IsType<NoContentResult>(response.Result);
    }

    [Fact]
    public async Task GetGameById_Returns_OkObjectResult_When_Entity_Exists()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .GetAsync(1))
            .ReturnsAsync(_mockGames[0]);

        // Act
        var response = await _gamesController.GetGameById(1);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetGameById_Returns_Entity_When_Entity_Exists()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .GetAsync(1))
            .ReturnsAsync(_mockGames[0]);

        // Act
        var result = await _gamesController.GetGameById(1);
        var okResult = (OkObjectResult)result.Result!;
        var responseItem = (GameAPIModel)okResult.Value!;

        // Assert
        Assert.Equal(responseItem.Id, _mockGames[0].Id);
    }

    [Fact]
    public async Task GetGameById_Returns_NotFoundResult_When_Entity_Does_Not_Exist()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .GetAsync(3))
            .ReturnsAsync(() => null);

        // Act
        var response = await _gamesController.GetGameById(3);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task CreateGame_Returns_OkObjectResult_If_Entity_Is_Created()
    {
        // Arrange
        GameCreateAPIModel createModel = GamGameCreateAPIModelFactory
            .GenerateSingle();
        Game createdGame = GameFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(It.IsAny<Expression<Func<Game, bool>>>()))
            .ReturnsAsync((Expression<Func<Game, bool>> predicate) => false);

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AddAsync(createdGame))
            .ReturnsAsync(createdGame);

        // Act
        var response = await _gamesController.CreateGame(createModel);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }


    [Fact]
    public async Task CreateGames_Returns_OkObjectResult_If_Entity_Is_Created()
    {
        // Arrange
        List<GameCreateAPIModel> createModels = GamGameCreateAPIModelFactory.Generate(5);
        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AddAsync(It.IsAny<Game>()))
            .ReturnsAsync((Game g) => g);

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(It.IsAny<Expression<Func<Game, bool>>>()))
            .ReturnsAsync((Expression<Func<Game, bool>> predicate) => false);

        // Act
        var response = await _gamesController.CreateGames(createModels);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateGames_Returns_All_Created_Entities_If_Entity_Is_Created()
    {
        // Arrange
        List<GameCreateAPIModel> createModels = GamGameCreateAPIModelFactory.Generate(5);
        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AddAsync(It.IsAny<Game>()))
            .ReturnsAsync((Game g) => g);

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(It.IsAny<Expression<Func<Game, bool>>>()))
            .ReturnsAsync((Expression<Func<Game, bool>> predicate) => false);

        // Act
        var result = await _gamesController.CreateGames(createModels);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<GameAPIModel>)okResult.Value!;

        // Assert
        Assert.Equal(createModels.Count, responseItems.Count());
    }

    [Fact]
    public async Task PutGame_Returns_OkObjectResult_If_Entity_Is_Updated()
    {
        // Arrange
        GameEditAPIModel editModel = GameEditAPIModelFactory.GenerateSingle();
        Game gameToEdit = GameFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.GameRepository
           .AnyAsync(It.IsAny<Expression<Func<Game, bool>>>()))
           .ReturnsAsync((Expression<Func<Game, bool>> predicate) => true);

        _mockUnitOfWork.Setup(uow => uow.GameRepository.UpdateAsync(gameToEdit))
            .ReturnsAsync(gameToEdit);

        // Act
        var response = await _gamesController.PutGame(editModel);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task PutGame_Returns_NotFoundResult_If_Entity_Does_Not_Exist()
    {
        // Arrange
        GameEditAPIModel editModel = GameEditAPIModelFactory.GenerateSingle();
        Game gameToEdit = GameFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(It.IsAny<Expression<Func<Game, bool>>>()))
            .ReturnsAsync((Expression<Func<Game, bool>> predicate) => false);

        _mockUnitOfWork.Setup(uow => uow.GameRepository.UpdateAsync(gameToEdit))
            .ReturnsAsync(gameToEdit);

        // Act
        var response = await _gamesController.PutGame(editModel);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task PutGames_Returns_OkObjectResult_When_All_Games_Are_Updated()
    {
        // Arrange
        List<GameEditAPIModel> apiModels = GameEditAPIModelFactory.Generate(5);
        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .UpdateAsync(It.IsAny<Game>()))
            .ReturnsAsync((Game g) => g);

        _mockUnitOfWork.Setup(uow => uow.GameRepository
          .AnyAsync(It.IsAny<Expression<Func<Game, bool>>>()))
          .ReturnsAsync((Expression<Func<Game, bool>> predicate) => true);

        // Act
        var response = await _gamesController.PutGames(apiModels);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task PatchGame_Returns_OkObjectResult_If_Entity_Is_Patched()
    {
        // Arrange
        Game gameToPatch = GameFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsync(1))
            .ReturnsAsync(gameToPatch);

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(It.IsAny<Expression<Func<Game, bool>>>()))
            .ReturnsAsync((Expression<Func<Game, bool>> predicate) => true);

        JsonPatchDocument<Game> patchDocument = JsonPatchDocumentFactory
            .GenerateGamePatchDocument();

        // Act
        var response = await _gamesController.PatchGame(1, patchDocument);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task PatchGame_Returns_NotFoundResult_If_Entity_Is_Does_Not_Exist()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsync(1))
            .ReturnsAsync(() => null);

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(It.IsAny<Expression<Func<Game, bool>>>()))
            .ReturnsAsync((Expression<Func<Game, bool>> predicate) => true);

        JsonPatchDocument<Game> patchDocument = JsonPatchDocumentFactory
            .GenerateGamePatchDocument();

        // Act
        var response = await _gamesController.PatchGame(1, patchDocument);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task DeleteGame_Returns_OkObjectResult_If_Entity_Is_Deleted()
    {
        // Arrange
        Game deletedGame = GameFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.GameRepository
          .AnyAsync(It.IsAny<Expression<Func<Game, bool>>>()))
          .ReturnsAsync((Expression<Func<Game, bool>> predicate) => true);

        _mockUnitOfWork.Setup(uow => uow.GameRepository.RemoveAsync(1))
            .ReturnsAsync(deletedGame);

        // Act
        var response = await _gamesController.DeleteGame(1);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task DeleteGame_Returns_NotFoundResult_If_Entity_Does_Not_Exist()
    {
        // Arrange
        Game deletedGame = GameFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.GameRepository
         .AnyAsync(It.IsAny<Expression<Func<Game, bool>>>()))
         .ReturnsAsync((Expression<Func<Game, bool>> predicate) => false);

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .RemoveAsync(deletedGame.Id))
            .ReturnsAsync(deletedGame);

        // Act
        var response = await _gamesController.DeleteGame(1);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }
}