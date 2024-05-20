namespace Tournaments.Test.Controllers;

public class GamesControllerTests_QueryParameters
{
    private readonly GamesController _gamesController;
    private readonly Mock<ILogger<Game>> _mockLogger = new();
    private readonly IMapper _mapper;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly List<Game> _mockGames = GameFactory.Generate(15);

    public GamesControllerTests_QueryParameters()
    {

        var config = new MapperConfiguration(config =>
        {
            config.AddProfile<TournamentMappingProfile>();
            config.AddProfile<GameMappingProfile>();
        });

        _mapper = config.CreateMapper();

        _gamesController = new GamesController(
            _mockLogger.Object,
            _mapper,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetGames_Returns_Sorted_Entities_When_Sort_Parameter_Is_Used()
    {
        // Arrange
        QueryParameters queryParams = QueryParametersFactory.SortParams("Id");
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsyncByParams(queryParams))
            .ReturnsAsync(_mockGames.OrderBy(t => t.Id));

        // Act
        var result = await _gamesController.GetGames(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<GameAPIModel>)okResult.Value!;

        // Assert
        Assert.Equal(_mockGames.Last().Title, responseItems.Last().Title);
    }

    [Fact]
    public async Task GetGames_Returns_Entities_Matching_Title_Parameter()
    {
        // Arrange
        QueryParameters queryParams = QueryParametersFactory.TitleParams("Game-1");
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsyncByParams(queryParams))
            .ReturnsAsync(_mockGames.Where(t => t.Title.Equals(queryParams.Title)));

        // Act
        var result = await _gamesController.GetGames(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<GameAPIModel>)okResult.Value!;

        // Assert
        Assert.Single(responseItems);
    }

    [Fact]
    public async Task GetGames_Returns_Entities_Matching_Search_Parameter()
    {
        // Arrange
        QueryParameters queryParams = QueryParametersFactory.SearchParams("Game-1");
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsyncByParams(queryParams))
            .ReturnsAsync(_mockGames.Where(t => t.Title.Contains(queryParams.Search!)));

        // Act
        var result = await _gamesController.GetGames(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<GameAPIModel>)okResult.Value!;

        // Assert
        Assert.Equal(7, responseItems.Count());
    }

    [Fact]
    public async Task GetGames_Returns_Entities_Matching_Filter_Parameter()
    {
        // Arrange
        var filter = new Dictionary<string, string> {
            {"Title", "Game-1"}
        };
        QueryParameters queryParams = QueryParametersFactory.FilterParams(filter);
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsyncByParams(queryParams))
                    .ReturnsAsync(_mockGames.Where(t => t.Title.Equals(filter.Values.First())));


        // Act
        var result = await _gamesController.GetGames(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<GameAPIModel>)okResult.Value!;

        // Assert
        Assert.Single(responseItems);
    }

    [Fact]
    public async Task GetGames_Returns_Paginated_Entities_By_LastId_When_LastId_Param_Is_used()
    {
        // Arrange
        QueryParameters queryParams = QueryParametersFactory.PageParamsKeySet(5, 5);
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsyncByParams(queryParams))
            .ReturnsAsync(_mockGames.Where(t => t.Id > queryParams.PageSize!.Value)
                .Take(queryParams.PageSize!.Value));

        // Act
        var result = await _gamesController.GetGames(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<GameAPIModel>)okResult.Value!;

        // Assert
        Assert.Equal(10, responseItems.Last().Id);
    }

    [Fact]
    public async Task GetGames_Returns_Paginated_Entities_By_CurrentPage_When_CurrentPage_Param_Is_used()
    {
        // Arrange
        QueryParameters queryParams = QueryParametersFactory.PageParamsOffset(5, 2);
        _mockUnitOfWork.Setup(uow => uow.GameRepository.GetAsyncByParams(queryParams))
            .ReturnsAsync(_mockGames
                .Skip(queryParams.PageSize!.Value * queryParams.CurrentPage!.Value)
                .Take(queryParams.PageSize.Value));

        // Act
        var result = await _gamesController.GetGames(queryParams);
        var okResult = (OkObjectResult)result.Result!;
        var responseItems = (IEnumerable<GameAPIModel>)okResult.Value!;

        // Assert
        Assert.Equal(15, responseItems.Last().Id);
    }
}