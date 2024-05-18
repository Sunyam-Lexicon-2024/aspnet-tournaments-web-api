using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Tournaments.Test;

public class TournamentsControllerTests
{
    private readonly TournamentsController _tournamentsController;
    private readonly Mock<ILogger> _mockLogger = new();
    private readonly IMapper _mapper;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly List<Tournament> _mockTournaments =
        [
            new("tournament-1") {
                Id = 1,
                StartDate = new DateOnly(2023,1,1)
            },
            new("tournament-2") {
                Id = 2,
                StartDate = new DateOnly(2023,1,2)
            }
        ];

    public TournamentsControllerTests()
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
    }

    [Fact]
    public async Task GetTournaments_Returns_OkObjectResult_When_Entities_Exist()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAllAsync())
            .ReturnsAsync(_mockTournaments);

        // Act
        var response = await _tournamentsController.GetTournaments();

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetTournaments_Returns_All_Entities_When_Entities_Exist()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAllAsync())
            .ReturnsAsync(_mockTournaments);

        // Act
        var result = await _tournamentsController.GetTournaments();
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<TournamentDTO>)okResult.Value!;

        // Assert
        Assert.Equal(_mockTournaments.Count, responseItems.Count());
    }

    [Fact]
    public async Task GetTournaments_Returns_NoContentResult_When_No_Entities_Exist()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAllAsync())
            .ReturnsAsync([]);

        // Act
        var response = await _tournamentsController.GetTournaments();

        // Assert
        Assert.IsType<NoContentResult>(response.Result);
    }

    [Fact]
    public async Task GetTournamentById_Returns_OkObjectResult_When_Entity_Exists()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsync(1))
            .ReturnsAsync(_mockTournaments[0]);

        // Act
        var response = await _tournamentsController.GetTournamentById(1);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetTournamentById_Returns_Entity_When_Entity_Exists()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsync(1))
            .ReturnsAsync(_mockTournaments[0]);

        // Act
        var result = await _tournamentsController.GetTournamentById(1);
        var okResult = (OkObjectResult)result.Result!;
        var responseItem = (TournamentDTO)okResult.Value!;

        // Assert
        Assert.Equal(responseItem.Id, _mockTournaments[0].Id);
    }

    [Fact]
    public async Task GetTournamentById_Returns_NotFoundResult_When_Entity_Does_Not_Exist()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsync(3))
            .ReturnsAsync(() => null);

        // Act
        var response = await _tournamentsController.GetTournamentById(3);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task CreateTournament_Returns_OkObjectResult_If_Entity_Is_Created()
    {
        // Arrange
        TournamentCreateDTO createDTO = new()
        {
            Id = 3,
            Title = "Tournament-3",
            StartDate = new DateOnly(2024, 1, 1)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.AnyAsync(3))
            .ReturnsAsync(false);

        // Act
        var response = await _tournamentsController.CreateTournament(createDTO);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateTournament_Returns_ConflictObjectResult_If_Entity_Already_Exists()
    {
        // Arrange
        TournamentCreateDTO createDTO = new()
        {
            Id = 3,
            Title = "Tournament-3",
            StartDate = new DateOnly(2024, 1, 1)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.AnyAsync(3))
            .ReturnsAsync(true);

        // Act
        var response = await _tournamentsController.CreateTournament(createDTO);

        // Assert
        Assert.IsType<ConflictObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateTournament_Returns_BadRequestResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        TournamentCreateDTO createDTO = new()
        {
            Id = 3,
            StartDate = new DateOnly(2024, 1, 1)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.AnyAsync(3))
            .ReturnsAsync(false);

        _tournamentsController.ModelState.AddModelError("test", "test");

        // Act
        var response = await _tournamentsController.CreateTournament(createDTO);

        // Assert
        Assert.IsType<BadRequestResult>(response.Result);
    }

    [Fact]
    public async Task PutTournament_Returns_OkObjectResult_If_Entity_Is_Updated()
    {
        // Arrange
        TournamentEditDTO editDTO = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };
        Tournament tournamentToEdit = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.AnyAsync(1))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.UpdateAsync(tournamentToEdit))
            .ReturnsAsync(tournamentToEdit);

        // Act
        var response = await _tournamentsController.PutTournament(editDTO);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task PutTournament_Returns_NotFoundResult_If_Entity_Does_Not_Exist()
    {
        // Arrange
        TournamentEditDTO editDTO = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };
        Tournament tournamentToEdit = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.AnyAsync(1))
            .ReturnsAsync(false);

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.UpdateAsync(tournamentToEdit))
            .ReturnsAsync(tournamentToEdit);

        // Act
        var response = await _tournamentsController.PutTournament(editDTO);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task PutTournament_Returns_BadRequestResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        TournamentEditDTO editDTO = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.AnyAsync(3))
            .ReturnsAsync(true);

        _tournamentsController.ModelState.AddModelError("test", "test");

        // Act
        var response = await _tournamentsController.PutTournament(editDTO);

        // Assert
        Assert.IsType<BadRequestResult>(response.Result);
    }

    [Fact]
    public async Task PatchTournament_Returns_OkObjectResult_If_Entity_Is_Patched()
    {
        // Arrange
        TournamentEditDTO editDTO = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };
        Tournament tournamentToPatch = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsync(1))
            .ReturnsAsync(tournamentToPatch);

        JsonPatchDocument<Tournament> patchDocument = new();

        patchDocument.Add(t => t.StartDate, new DateOnly(2024, 3, 3));

        // Act
        var response = await _tournamentsController.PatchTournament(1, patchDocument);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task PatchTournament_Returns_NotFoundResult_If_Entity_Is_Does_Not_Exist()
    {
        // Arrange
        TournamentEditDTO editDTO = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsync(1))
            .ReturnsAsync(() => null);

        JsonPatchDocument<Tournament> patchDocument = new();

        patchDocument.Add(t => t.StartDate, new DateOnly(2024, 3, 3));

        // Act
        var response = await _tournamentsController.PatchTournament(1, patchDocument);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task PatchTournament_Returns_BadRequestResult_If_Patch_Document_Is_Null()
    {
        // Arrange
        TournamentEditDTO editDTO = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };

        Tournament tournamentToPatch = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsync(1))
            .ReturnsAsync(tournamentToPatch);

        // Act
        var response = await _tournamentsController.PatchTournament(1, null!);

        // Assert
        Assert.IsType<BadRequestResult>(response.Result);
    }

    [Fact]
    public async Task PatchTournament_Returns_BadRequestResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        TournamentEditDTO editDTO = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };

        Tournament tournamentToPatch = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 5, 5)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsync(1))
            .ReturnsAsync(tournamentToPatch);

        JsonPatchDocument<Tournament> patchDocument = new();

        patchDocument.Add(t => t.StartDate, new DateOnly(2024, 3, 3));

        _tournamentsController.ModelState.AddModelError("test", "test");

        // Act
        var response = await _tournamentsController.PatchTournament(1, null!);

        // Assert
        Assert.IsType<BadRequestResult>(response.Result);
    }

    [Fact]
    public async Task DeleteTournament_Returns_OkObjectResult_If_Entity_Is_Deleted()
    {
        // Arrange
        Tournament deletedTournament = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 1, 1)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.AnyAsync(1))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.RemoveAsync(1))
            .ReturnsAsync(deletedTournament);

        // Act
        var response = await _tournamentsController.DeleteTournament(1);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task DeleteTournament_Returns_NotFoundResult_If_Entity_Does_Not_Exist()
    {
        // Arrange
        Tournament deletedTournament = new("Tournament-1")
        {
            Id = 1,
            StartDate = new DateOnly(2024, 1, 1)
        };

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.AnyAsync(1))
            .ReturnsAsync(false);

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.RemoveAsync(1))
            .ReturnsAsync(deletedTournament);

        // Act
        var response = await _tournamentsController.DeleteTournament(1);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }
}