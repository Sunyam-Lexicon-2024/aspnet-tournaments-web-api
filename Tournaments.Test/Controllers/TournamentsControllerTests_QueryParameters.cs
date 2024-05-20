namespace Tournaments.Test.Controllers;

public class TournamentsControllerTests_QueryParameters
{
    private readonly TournamentsController _tournamentsController;
    private readonly Mock<ILogger<Tournament>> _mockLogger = new();
    private readonly IMapper _mapper;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly List<Tournament> _mockTournaments = TournamentFactory.GenerateWithChildren(15);

    public TournamentsControllerTests_QueryParameters()
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
    public async Task GetTournaments_Returns_Sorted_Entities_When_Sort_Parameter_Is_Used()
    {
        // Arrange
        QueryParameters queryParams = QueryParametersFactory.SortParams("Id");
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsyncByParams(queryParams))
            .ReturnsAsync(_mockTournaments.OrderBy(t => t.Id));

        // Act
        var result = await _tournamentsController.GetTournaments(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<TournamentAPIModel>)okResult.Value!;

        // Assert
        Assert.Equal(_mockTournaments.Last().Title, responseItems.Last().Title);
    }


    [Fact]
    public async Task GetTournaments_Returns_Entities_With_Children_When_IncludChildren_Param_Is_Used()
    {
        // Arrange
        QueryParameters queryParams = QueryParametersFactory.IncludeChildrenParams();
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsyncByParams(queryParams))
            .ReturnsAsync(_mockTournaments);

        // Act
        var result = await _tournamentsController.GetTournaments(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<TournamentAPIModel>)okResult.Value!;

        // Assert
        Assert.Equal(_mockTournaments.First().Games.Count, responseItems.First().Games.Count);
    }

    [Fact]
    public async Task GetTournaments_Returns_Entities_Matching_Title_Parameter()
    {
        // Arrange
        QueryParameters queryParams = QueryParametersFactory.TitleParams("Tournament-1");
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsyncByParams(queryParams))
            .ReturnsAsync(_mockTournaments.Where(t => t.Title.Equals(queryParams.Title)));

        // Act
        var result = await _tournamentsController.GetTournaments(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<TournamentAPIModel>)okResult.Value!;

        // Assert
        Assert.Single(responseItems);
    }

    [Fact]
    public async Task GetTournaments_Returns_Entities_Matching_Search_Parameter()
    {
        // Arrange
        QueryParameters queryParams = QueryParametersFactory.SearchParams("Tournament-1");
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsyncByParams(queryParams))
            .ReturnsAsync(_mockTournaments.Where(t => t.Title.Contains(queryParams.Search!)));

        // Act
        var result = await _tournamentsController.GetTournaments(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<TournamentAPIModel>)okResult.Value!;

        // Assert
        Assert.Equal(7, responseItems.Count());
    }

    [Fact]
    public async Task GetTournaments_Returns_Entities_Matching_Filter_Parameter()
    {
        // Arrange
        var filter = new Dictionary<string, string> {
            {"Title", "Tournament-1"}
        };
        QueryParameters queryParams = QueryParametersFactory.FilterParams(filter);
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsyncByParams(queryParams))
                    .ReturnsAsync(_mockTournaments.Where(t => t.Title.Equals(filter.Values.First())));


        // Act
        var result = await _tournamentsController.GetTournaments(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<TournamentAPIModel>)okResult.Value!;

        // Assert
        Assert.Single(responseItems);
    }

    [Fact]
    public async Task GetTournaments_Returns_Paginated_Entities_By_LastId_When_LastId_Param_Is_used()
    {
        // Arrange
        QueryParameters queryParams = QueryParametersFactory.PageParamsKeySet(5, 5);
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsyncByParams(queryParams))
            .ReturnsAsync(_mockTournaments.Where(t => t.Id > queryParams.PageSize!.Value)
                .Take(queryParams.PageSize!.Value));

        // Act
        var result = await _tournamentsController.GetTournaments(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<TournamentAPIModel>)okResult.Value!;

        // Assert
        Assert.Equal(10, responseItems.Last().Id);
    }

    [Fact]
    public async Task GetTournaments_Returns_Paginated_Entities_By_CurrentPage_When_CurrentPage_Param_Is_used()
    {
        // Arrange
        QueryParameters queryParams = QueryParametersFactory.PageParamsOffset(5, 2);
        _mockUnitOfWork.Setup(uow => uow.TournamentRepository.GetAsyncByParams(queryParams))
            .ReturnsAsync(_mockTournaments
                .Skip(queryParams.PageSize!.Value * queryParams.CurrentPage!.Value)
                .Take(queryParams.PageSize.Value));

        // Act
        var result = await _tournamentsController.GetTournaments(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<TournamentAPIModel>)okResult.Value!;

        // Assert
        Assert.Equal(15, responseItems.Last().Id);
    }
}