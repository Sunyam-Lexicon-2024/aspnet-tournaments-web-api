namespace Tournaments.Test.Controllers;

public class TournamentsControllerTests
{
    private readonly TournamentsController _tournamentsController;
    private readonly Mock<ILogger<Tournament>> _mockLogger = new();
    private readonly IMapper _mapper;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly List<Tournament> _mockTournaments = TournamentFactory.Generate(2);

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
        var responseItems = (IEnumerable<TournamentAPIModel>)okResult.Value!;

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
        var responseItem = (TournamentAPIModel)okResult.Value!;

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
        TournamentCreateAPIModel createModel = TournamentCreateAPIModelFactory.GenerateSingle();
        Tournament createdTournament = TournamentFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.AddAsync(createdTournament))
            .ReturnsAsync(createdTournament);

        // Act
        var response = await _tournamentsController.CreateTournament(createModel);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateTournament_Returns_BadRequestResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        TournamentCreateAPIModel createModel = TournamentCreateAPIModelFactory.GenerateSingle();

        _tournamentsController.ModelState.AddModelError("test", "test");

        // Act
        var response = await _tournamentsController.CreateTournament(createModel);

        // Assert
        Assert.IsType<BadRequestResult>(response.Result);
    }

    [Fact]
    public async Task PutTournament_Returns_OkObjectResult_If_Entity_Is_Updated()
    {
        // Arrange
        TournamentEditAPIModel editModel = TournamentEditAPIModelFactory.GenerateSingle();
        Tournament tournamentToEdit = TournamentFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository
            .AnyAsync(editModel.Id))
            .ReturnsAsync(true);

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.UpdateAsync(tournamentToEdit))
            .ReturnsAsync(tournamentToEdit);

        // Act
        var response = await _tournamentsController.PutTournament(editModel);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task PutTournament_Returns_NotFoundResult_If_Entity_Does_Not_Exist()
    {
        // Arrange
        TournamentEditAPIModel editModel = TournamentEditAPIModelFactory.GenerateSingle();
        Tournament tournamentToEdit = TournamentFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository
            .AnyAsync(editModel.Id))
            .ReturnsAsync(false);

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.UpdateAsync(tournamentToEdit))
            .ReturnsAsync(tournamentToEdit);

        // Act
        var response = await _tournamentsController.PutTournament(editModel);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task PutTournament_Returns_BadRequestResult_If_ModelState_Is_Invalid()
    {
        // Arrange
        TournamentEditAPIModel editModel = TournamentEditAPIModelFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository
            .AnyAsync(editModel.Id))
            .ReturnsAsync(false);

        _tournamentsController.ModelState.AddModelError("test", "test");

        // Act
        var response = await _tournamentsController.PutTournament(editModel);

        // Assert
        Assert.IsType<BadRequestResult>(response.Result);
    }

    [Fact]
    public async Task PatchTournament_Returns_OkObjectResult_If_Entity_Is_Patched()
    {
        // Arrange
        Tournament tournamentToPatch = TournamentFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsync(1))
            .ReturnsAsync(tournamentToPatch);

        JsonPatchDocument<Tournament> patchDocument = JsonPatchDocumentFactory
            .GenerateTournamentPatchDocument();

        // Act
        var response = await _tournamentsController.PatchTournament(1, patchDocument);

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task PatchTournament_Returns_NotFoundResult_If_Entity_Is_Does_Not_Exist()
    {
        // Arrange
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsync(1))
            .ReturnsAsync(() => null);

        JsonPatchDocument<Tournament> patchDocument = JsonPatchDocumentFactory
           .GenerateTournamentPatchDocument();

        // Act
        var response = await _tournamentsController.PatchTournament(1, patchDocument);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task PatchTournament_Returns_BadRequestResult_If_Patch_Document_Is_Null()
    {
        // Arrange
        Tournament tournamentToPatch = TournamentFactory.GenerateSingle();

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
        Tournament tournamentToPatch = TournamentFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsync(1))
            .ReturnsAsync(tournamentToPatch);

        JsonPatchDocument<Tournament> patchDocument = JsonPatchDocumentFactory
              .GenerateTournamentPatchDocument();

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
        Tournament deletedTournament = TournamentFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository
            .AnyAsync(deletedTournament.Id))
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
        Tournament deletedTournament = TournamentFactory.GenerateSingle();

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository
            .AnyAsync(deletedTournament.Id))
            .ReturnsAsync(false);

        _mockUnitOfWork.Setup(uow => uow.TournamentRepository
            .RemoveAsync(deletedTournament.Id))
            .ReturnsAsync(deletedTournament);

        // Act
        var response = await _tournamentsController.DeleteTournament(1);

        // Assert
        Assert.IsType<NotFoundResult>(response.Result);
    }
}