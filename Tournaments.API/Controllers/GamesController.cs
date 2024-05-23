namespace Tournaments.API.Controllers;

/** 
<summary>
Creates a new GamesController Instance
</summary>
<param name="logger"></param>
<param name="mapper"></param>
<param name="unitOfWork"></param>
<returns> A new GamesController Instance </returns>
*/
[Route("[controller]")]
public class GamesController(
    ILogger<Game> logger,
    IMapper mapper,
    IUnitOfWork unitOfWork) : BaseController(unitOfWork)
{
    private readonly ILogger<Game> _logger = logger;
    private readonly IMapper _mapper = mapper;

    /** 
    <summary>
    Returns all Games
    </summary>
    <returns> A list of all Games </returns>
    */
    [HttpGet]
    [ProducesResponseType<IEnumerable<GameAPIModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<IEnumerable<GameAPIModel>>> GetGames(
        [FromQuery] QueryParameters? queryParameters = null)
    {
        IEnumerable<Game> games;

        if (queryParameters is not null)
        {
            games = await _unitOfWork.GameRepository
                .GetAsyncByParams(queryParameters);
            if (queryParameters.PageSize is not null)
            {
                foreach (var (type, value) in PaginationHeaders(queryParameters))
                {
                    HttpContext.Response.Headers.Append(type, value);
                }
            }
        }
        else
        {
            games = await _unitOfWork.GameRepository.GetAllAsync();
        }

        if (games.Any())
        {
            var apiModels = await Task.Run(() => _mapper
                .Map<IEnumerable<GameAPIModel>>(games));
            return Ok(apiModels);
        }
        else
        {
            return NoContent();
        }
    }

    /** 
    <summary>
    Gets a specific game by ID
    </summary>
    <param name="gameId"></param>
    <returns> A Game matching the given ID </returns>
    <remarks>
    Sample request: 
            GET /Games/1
    </remarks>
    */
    [HttpGet("{gameId}")]
    [ProducesResponseType<GameAPIModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameAPIModel>> GetGameById(int gameId)
    {
        var game = await _unitOfWork.GameRepository.GetAsync(gameId);
        if (game is not null)
        {
            return Ok(_mapper.Map<GameAPIModel>(game));
        }
        else
        {
            return NotFound();
        }
    }

    /** 
    <summary>
    Creates a new Game
    </summary>
    <param name="createModel"></param>
    <returns> The newly created Game </returns>
    <remarks>
    Sample request: 
            POST /Games {
            {
                "id": 1,
                "title": "New Game"
                "tournamentId": 2
                "startTime": "2024-05-23T13:39:43.974Z",
            }
    </remarks>
    */
    [HttpPost]
    [ProducesResponseType<GameAPIModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GameAPIModel>> CreateGame(
        GameCreateAPIModel createModel)
    {
        if (!await GameExists(g => g.Title == createModel.Title &&
            g.TournamentId == createModel.TournamentId))
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponseBody(ModelState));
            }

        var gameToCreate = await Task.Run(() => _mapper.Map<Game>(createModel));

        try
        {
            var createdGame = await _unitOfWork.GameRepository.AddAsync(gameToCreate);
            await _unitOfWork.CompleteAsync();
            var apiModel = await Task.Run(() => _mapper.Map<GameAPIModel>(createdGame));
            return Ok(apiModel);
        }
        catch (DbUpdateException ex)
        {
            LogError(ex, createModel, _logger);
            return StatusCode(500);
        }
    }

    /** 
    <summary>
    Creates multiple new Games
    </summary>
    <param name="createModels"></param>
    <returns> A list of the newly created Games </returns>
    <remarks>
    Sample request: 
            POST /Games 
            [
            {
                "id": 1,
                "title": "New Game 1"
                "tournamentId": 2
                "startTime": "2024-05-23T13:39:43.974Z",
            },
            {
                "id": 1,
                "title": "New Game 2"
                "tournamentId": 2
                "startTime": "2024-05-23T13:39:43.974Z",
            },
            ]
    </remarks>
    */
    [HttpPost]
    [Route("collection")]
    [ProducesResponseType<IEnumerable<GameAPIModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GameAPIModel>>> CreateGames(
        IEnumerable<GameCreateAPIModel> createModels)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ErrorResponseBody(ModelState));
        }

        List<GameAPIModel> apiModels = [];
        CancellationTokenSource cancellationTokenSource = new();

        try
        {
            await Task.Run(async () =>
            {
                foreach (var cm in createModels)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    var gameToCreate = _mapper.Map<Game>(cm);
                    var createdGame = await _unitOfWork.GameRepository.AddAsync(gameToCreate);
                    if (createdGame is null)
                    {
                        cancellationTokenSource.Cancel();
                        break;
                    }
                    apiModels.Add(_mapper.Map<GameAPIModel>(createdGame));
                }
            }, cancellationTokenSource.Token);
            await _unitOfWork.CompleteAsync();
            return Ok(apiModels);
        }
        catch (OperationCanceledException ex)
        {
            LogError(ex, apiModels, _logger);
            return BadRequest("Invalid attribute settings on one or more input objects found.");
        }
        catch (DbUpdateException ex)
        {
            LogError(ex, apiModels, _logger);
            return StatusCode(500);
        }
    }

    /** 
    <summary>
    Updates a specific Game
    </summary>
    <param name="editModel"></param>
    <returns> The newly updated Game </returns>
    <remarks>
    Sample request: 
            PUT /Games
            {
                "id": 1,
                "title": "Updated Game"
                "tournamentId": 2
                "startTime": "2024-05-23T13:39:43.974Z",
            }
    </remarks>
    */
    [HttpPut]
    [ProducesResponseType<GameAPIModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GameAPIModel>> PutGame(GameEditAPIModel editModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ErrorResponseBody(ModelState));
        }

        if (!await GameExists(g => g.Id == editModel.Id))
        {
            return NotFound();
        }

        try
        {
            var gameToUpdate = _mapper.Map<Game>(editModel);
            var updatedGame = await _unitOfWork.GameRepository
                .UpdateAsync(gameToUpdate);
            await _unitOfWork.CompleteAsync();
            var apiModel = _mapper.Map<GameAPIModel>(updatedGame);
            return Ok(apiModel);
        }
        catch (DbUpdateException ex)
        {
            // TBD append error details here
            LogError(ex, editModel, _logger);
            return StatusCode(500);
        }
    }

    /** 
    <summary>
    Updates multiple Games
    </summary>
    <param name="editModels"></param>
    <returns> The newly updated Games </returns>
    <remarks>
    Sample request: 
            PUT /Games
            [
            {
                "id": 1,
                "title": "Updated Game"
                "tournamentId": 2
                "startTime": "2024-05-23T13:39:43.974Z",
            },
            {
                "id": 2,
                "title": "Updated Game 2"
                "tournamentId": 2
                "startTime": "2024-05-24T13:39:43.974Z",
            }
            ]
    </remarks>
    */
    [HttpPut]
    [Route("collection")]
    [ProducesResponseType<IEnumerable<GameAPIModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GameAPIModel>>> PutGames(
        IEnumerable<GameEditAPIModel> editModels)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ErrorResponseBody(ModelState));
        }

        List<GameAPIModel> apiModels = [];
        CancellationTokenSource cancellationTokenSource = new();

        try
        {
            await Task.Run(async () =>
            {
                foreach (var em in editModels)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    if (!await GameExists(g => g.Id == em.Id))
                    {
                        // activte canellationToken here
                        cancellationTokenSource.Cancel();
                        break;
                    }
                    var gameToUpdate = _mapper.Map<Game>(em);
                    var updatedGame = await _unitOfWork.GameRepository
                        .UpdateAsync(gameToUpdate);
                    if (updatedGame is null)
                    {
                        cancellationTokenSource.Cancel();
                        break;
                    }
                    apiModels.Add(_mapper.Map<GameAPIModel>(updatedGame));
                }
            }, cancellationTokenSource.Token);

            await _unitOfWork.CompleteAsync();
            return Ok(apiModels);
        }
        catch (OperationCanceledException ex)
        {
            LogError(ex, apiModels, _logger);
            return BadRequest("Invalid attribute settings on one or more input objects found.");
        }
        catch (DbUpdateException ex)
        {
            // TBD append error details here
            LogError(ex, editModels, _logger);
            return StatusCode(500);
        }
    }

    /** 
    <summary>
    Partially updates multiple Game
    </summary>
    <param name="gameId"></param>
    <param name="patchDocument"></param>
    <returns> The newly partially updated Game </returns>
    <remarks>
    Sample request: 
            PATCH /Games/1
    </remarks>
    */
    [HttpPatch("{gameId}")]
    [ProducesResponseType<GameAPIModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GameAPIModel>> PatchGame(
        int gameId,
        [FromBody] JsonPatchDocument<Game> patchDocument)
    {
        if (!await GameExists(g => g.Id == gameId))
        {
            return NotFound();
        }
        if (patchDocument is not null)
        {
            var gameToPatch = await _unitOfWork.GameRepository.GetAsync(gameId);
            if (gameToPatch is not null)
            {
                try
                {
                    await Task.Run(() => patchDocument.ApplyTo(gameToPatch, ModelState));
                    await _unitOfWork.CompleteAsync();

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ErrorResponseBody(ModelState));
                    }
                    else
                    {
                        return Ok(_mapper.Map<GameAPIModel>(gameToPatch));
                    }
                }

                catch (JsonPatchException ex)
                {
                    LogError(ex, gameId, _logger);
                    return BadRequest("Invalid Json Patch Document");
                }
                catch (DbUpdateException ex)
                {
                    LogError(ex, gameId, _logger);
                    return StatusCode(500);
                }
            }
            else
            {
                return NotFound();
            }
        }
        return BadRequest("No Patch Document In Body");
    }

    /** 
    <summary>
    Deletes a Game
    </summary>
    <param name="gameId"></param>
    <returns> The newly deleted Games </returns>
    <remarks>
    Sample request: 
            DELETE /Games/1
    </remarks>
    */
    [HttpDelete("{gameId}")]
    [ProducesResponseType<GameAPIModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GameAPIModel>> DeleteGame(int gameId)
    {
        if (!await GameExists(g => g.Id == gameId))
        {
            return NotFound();
        }
        else
        {
            try
            {
                var deletedGame = await _unitOfWork.GameRepository
                    .RemoveAsync(gameId);
                await _unitOfWork.CompleteAsync();
                var apiModel = await Task.Run(() => _mapper.Map<GameAPIModel>(deletedGame));
                return Ok(apiModel);
            }
            catch (DbUpdateException ex)
            {
                // TBD append error details here
                LogError(ex, gameId, _logger);
                return StatusCode(500);
            }
        }
    }

    private async Task<bool> GameExists(Expression<Func<Game, bool>> predicate)
    {
        return await _unitOfWork.GameRepository.AnyAsync(predicate);
    }
}