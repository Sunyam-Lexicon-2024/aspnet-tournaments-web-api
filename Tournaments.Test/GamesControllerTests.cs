using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Tournaments.Test;

public class GamesControllerTests
{
    private readonly GamesController _gamesController;
    private readonly Mock<ILogger<Game>> _mockLogger = new();
    private readonly IMapper _mapper;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly List<Game> _mockGames =
        [
            new("game-1") {
                Id = 1,
                TournamentId = 1,
                StartTime = new DateTime(2023,1,1)
            },
            new("game-2") {
                Id = 2,
                TournamentId = 1,
                StartTime = new DateTime(2023,1,2)
            },
            new("game-3") {
                Id = 3,
                TournamentId = 1,
                StartTime = new DateTime(2023,1,3)
            },
            new("game-4") {
                Id = 4,
                TournamentId = 1,
                StartTime = new DateTime(2023,1,4)
            },
            new("game-5") {
                Id = 5,
                TournamentId = 1,
                StartTime = new DateTime(2023,1,5)
            },
        ];

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
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAllAsync())
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
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAllAsync())
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
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAllAsync())
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
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsync(1))
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
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsync(1))
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
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsync(3))
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
        var createModel = TestCreateModel();
        
        // Act
        var response = await _gamesController.CreateGame(createModel);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateGame_Returns_BadRequestResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        var createModel = TestCreateModel();

        _gamesController.ModelState.AddModelError("test", "test");

        // Act
        var response = await _gamesController.CreateGame(createModel);

        // Assert
        Assert.IsType<BadRequestResult>(response.Result);
    }

    [Fact]
    public async Task PutGame_Returns_OkObjectResult_If_Entity_Is_Updated()
    {
        // Arrange
        var editModel = TestEditModel();
        var gameToEdit = TestGame();

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(editModel.Id))
            .ReturnsAsync(true);

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
        var editModel = TestEditModel();
        var gameToEdit = TestGame();

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(editModel.Id))
            .ReturnsAsync(false);

        _mockUnitOfWork.Setup(uow => uow.GameRepository.UpdateAsync(gameToEdit))
            .ReturnsAsync(gameToEdit);

        // Act
        var response = await _gamesController.PutGame(editModel);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task PutGame_Returns_BadRequestResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        var editModel = TestEditModel();

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(editModel.Id))
            .ReturnsAsync(false);

        _gamesController.ModelState.AddModelError("test", "test");

        // Act
        var response = await _gamesController.PutGame(editModel);

        // Assert
        Assert.IsType<BadRequestResult>(response.Result);
    }

    [Fact]
    public async Task PatchGame_Returns_OkObjectResult_If_Entity_Is_Patched()
    {
        // Arrange
        var gameToPatch = TestGame();

        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsync(1))
            .ReturnsAsync(gameToPatch);

        var patchDocument = TestJsonPatchDocument();

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

        var patchDocument = TestJsonPatchDocument();

        // Act
        var response = await _gamesController.PatchGame(1, patchDocument);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task PatchGame_Returns_BadRequestResult_If_Patch_Document_Is_Null()
    {
        // Arrange
        var gameToPatch = TestGame();

        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsync(1))
            .ReturnsAsync(gameToPatch);

        // Act
        var response = await _gamesController.PatchGame(1, null!);

        // Assert
        Assert.IsType<BadRequestResult>(response.Result);
    }

    [Fact]
    public async Task PatchGame_Returns_BadRequestResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        var gameToPatch = TestGame();

        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsync(1))
            .ReturnsAsync(gameToPatch);

        var patchDocument = TestJsonPatchDocument();

        _gamesController.ModelState.AddModelError("test", "test");

        // Act
        var response = await _gamesController.PatchGame(1, null!);

        // Assert
        Assert.IsType<BadRequestResult>(response.Result);
    }

    [Fact]
    public async Task DeleteGame_Returns_OkObjectResult_If_Entity_Is_Deleted()
    {
        // Arrange
        var deletedTournament = TestGame();

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(deletedTournament.Id))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(uow => uow.GameRepository.RemoveAsync(1))
            .ReturnsAsync(deletedTournament);

        // Act
        var response = await _gamesController.DeleteGame(1);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task DeleteGame_Returns_NotFoundResult_If_Entity_Does_Not_Exist()
    {
        // Arrange
        var deletedTournament = TestGame();

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .AnyAsync(deletedTournament.Id))
            .ReturnsAsync(false);

        _mockUnitOfWork.Setup(uow => uow.GameRepository
            .RemoveAsync(deletedTournament.Id))
            .ReturnsAsync(deletedTournament);

        // Act
        var response = await _gamesController.DeleteGame(1);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }

    private static Game TestGame()
    {
        return new Game("Game-1")
        {
            Id = 1,
            TournamentId = 1,
            StartTime = new DateTime(2024, 1, 1)
        };
    }

    private static GameCreateAPIModel TestCreateModel()
    {
        return new GameCreateAPIModel()
        {
            Title = "Game-3",
            StartTime = new DateTime(2024, 5, 5)
        };
    }
    private static GameEditAPIModel TestEditModel()
    {
        return new GameEditAPIModel("Game-1")
        {
            Id = 1,
            TournamentId = 2,
            StartTime = new DateTime(2024, 5, 5)
        };
    }

    private static JsonPatchDocument<Game> TestJsonPatchDocument()
    {
        JsonPatchDocument<Game> patchDocument = new();

        patchDocument.Add(t => t.StartTime, new DateTime(2024, 3, 3));

        return patchDocument;
    }
}