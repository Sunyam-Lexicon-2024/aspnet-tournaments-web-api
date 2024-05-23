namespace Tournaments.API.Controllers;

/**
 <summary>
 Creates a Tournaments Controller Instance
 </summary>
 <param name="logger"></param>
 <param name="mapper"></param>
 <param name="unitOfWork"></param>
 <returns> A TournamentsController Instance </returns>
 */
[Route("[controller]")]
public class TournamentsController(
    ILogger<Tournament> logger,
    IMapper mapper,
    IUnitOfWork unitOfWork) : BaseController(unitOfWork)
{

    private readonly ILogger<Tournament> _logger = logger;
    private readonly IMapper _mapper = mapper;

    /**
    <summary>
    Creates a new tournament
    </summary>
    <returns> A list of newly created tournaments </returns>
    <remarks>
    Sample request: 
        GET /Tournaments/1
    </remarks>
    */
    [HttpGet]
    [ProducesResponseType<IEnumerable<TournamentAPIModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<IEnumerable<TournamentAPIModel>>> GetTournaments(
        [FromQuery] QueryParameters? queryParameters = null)
    {
        IEnumerable<Tournament> tournaments;

        if (queryParameters is not null)
        {
            tournaments = await _unitOfWork.TournamentRepository
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
            tournaments = await _unitOfWork.TournamentRepository.GetAllAsync();
        }

        if (tournaments.Any())
        {
            var tournamentAPIModels = _mapper
                .Map<IEnumerable<TournamentAPIModel>>(tournaments);
            return Ok(tournamentAPIModels);
        }
        else
        {
            return NoContent();
        }
    }

    /**
    <summary>
    Creates a new tournament
    </summary>
    <param name="tournamentId"></param>
    <param name="queryParams"></param>
    <returns> A list of newly created tournaments </returns>
    <remarks>
    Sample request: 
        GET /Tournaments/1
    </remarks>
    */
    [HttpGet("{tournamentId}")]
    [ProducesResponseType<TournamentAPIModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TournamentAPIModel>> GetTournamentById(
        int tournamentId,
        [FromQuery] QueryParameters? queryParams = null!)
    {
        Tournament? tournament;

        if (queryParams?.IncludeChildren is true)
        {
            tournament = await _unitOfWork.TournamentRepository
                .GetAsyncWithChildren(tournamentId);
        }
        else
        {
            tournament = await _unitOfWork.TournamentRepository
                .GetAsync(tournamentId);
        }

        if (tournament is not null)
        {
            return Ok(_mapper.Map<TournamentAPIModel>(tournament));
        }
        else
        {
            return NotFound();
        }
    }

    /**
    <summary>
    Creates a new tournament
    </summary>
    <param name="createModel"></param>
    <returns> A list of newly created tournaments </returns>
    <remarks>
    Sample request: 
        POST /Tournaments/collection {
            "title": "Tournament-1",
            "startTime": "2024-05-23T13:39:43.974Z",
        }
    </remarks>
    */
    [HttpPost]
    [ProducesResponseType<TournamentAPIModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TournamentCreateAPIModel>> CreateTournament(
        TournamentCreateAPIModel createModel)
    {
        if (!await TournamentExists(t => t.Title == createModel.Title))
        {
            return BadRequest($"Tournament with title {createModel.Title} already exists");
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ErrorResponseBody(ModelState));
        }

        var tournamentToCreate = await Task.Run(() => _mapper
            .Map<Tournament>(createModel));

        try
        {
            await _unitOfWork.TournamentRepository.AddAsync(tournamentToCreate);
            await _unitOfWork.CompleteAsync();
            return Ok(createModel);
        }
        catch (DbUpdateException ex)
        {
            // TBD append error details here
            LogError(ex, createModel, _logger);
            return StatusCode(500);
        }
    }

    /**
    <summary>
    Creates multiple tournaments at once
    </summary>
    <param name="createModels"></param>
    <returns> A list of newly created tournaments </returns>
    <remarks>
    Sample request: 
        POST /Tournaments/collection 
        [
        {
            "Title": "Tournament-1",
            "startTime": "2024-05-23T13:39:43.974Z",
        },
        Title": "Tournament-1",
            "startTime": "2024-05-23T13:39:43.974Z",
        }
        ]
    </remarks>
    */
    [HttpPost]
    [Route("collection")]
    [ProducesResponseType<IEnumerable<TournamentAPIModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TournamentAPIModel>>> CreateTournaments(
        IEnumerable<TournamentCreateAPIModel> createModels)
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
                    var tournamentToCreate = _mapper.Map<Tournament>(cm);
                    var createdTournament = await _unitOfWork.TournamentRepository.AddAsync(tournamentToCreate);
                    if (createdTournament is null)
                    {
                        cancellationTokenSource.Cancel();
                        break;
                    }
                    apiModels.Add(_mapper.Map<GameAPIModel>(createdTournament));
                }
            }, cancellationTokenSource.Token);
            await _unitOfWork.CompleteAsync();
            return Ok(apiModels);
        }
        catch (DbUpdateException ex)
        {
            LogError(ex, apiModels, _logger);
            return StatusCode(500);
        }
    }


    /**
 <summary>
 Updates a Tournament
 </summary>
 <param name="editModel"></param>
 <returns> The updated Tournament </returns>
 */
    [HttpPut]
    [ProducesResponseType<TournamentAPIModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TournamentAPIModel>> PutTournament(
            TournamentEditAPIModel editModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ErrorResponseBody(ModelState));
        }

        if (!await TournamentExists(t => t.Id == editModel.Id))
        {
            return NotFound();
        }

        try
        {
            var tournamentToUpdate = _mapper.Map<Tournament>(editModel);
            var updatedTournament = await _unitOfWork.TournamentRepository
                .UpdateAsync(tournamentToUpdate);
            await _unitOfWork.CompleteAsync();
            var apiModel = _mapper.Map<TournamentAPIModel>(updatedTournament);
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
Updates multiple Tournaments
</summary>
<param name="editModels"></param>
<returns> A list of the updated Tournaments </returns>
*/
    [HttpPut]
    [Route("collection")]
    [ProducesResponseType<TournamentAPIModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TournamentAPIModel>>> PutTournaments(
        IEnumerable<TournamentEditAPIModel> editModels)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ErrorResponseBody(ModelState));
        }

        List<TournamentAPIModel> apiModels = [];
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
                    if (!await TournamentExists(t => t.Id == em.Id))
                    {
                        // activte canellationToken here
                        cancellationTokenSource.Cancel();
                        break;
                    }
                    var tournamentToUpdate = _mapper.Map<Tournament>(em);
                    var updatedTournament = await _unitOfWork.TournamentRepository
                        .UpdateAsync(tournamentToUpdate);
                    if (updatedTournament is null)
                    {
                        cancellationTokenSource.Cancel();
                        break;
                    }
                    apiModels.Add(_mapper.Map<TournamentAPIModel>(updatedTournament));
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
    Partially updates the target Tournament
    </summary>
    <param name="tournamentId"></param>
    <param name="patchDocument"></param>
    <returns> The partilly updated Tournament </returns>
    */
    [HttpPatch("{tournamentId}")]
    [ProducesResponseType<TournamentAPIModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TournamentAPIModel>> PatchTournament(
        int tournamentId,
        [FromBody] JsonPatchDocument<Tournament> patchDocument)
    {
        if (!await TournamentExists(t => t.Id == tournamentId))
        {
            return NotFound();
        }
        if (patchDocument is not null)
        {
            var tournamentToPatch = await _unitOfWork.TournamentRepository
                .GetAsync(tournamentId);

            if (tournamentToPatch is not null)
            {
                try
                {
                    await Task.Run(() => patchDocument.ApplyTo(tournamentToPatch, ModelState));
                    await _unitOfWork.CompleteAsync();

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ErrorResponseBody(ModelState));
                    }
                    else
                    {
                        return Ok(_mapper.Map<TournamentAPIModel>(tournamentToPatch));
                    }
                }
                catch (JsonPatchException ex)
                {
                    LogError(ex, tournamentId, _logger);
                    return BadRequest("Invalid Json Patch Document");
                }
                catch (DbUpdateException ex)
                {
                    LogError(ex, tournamentId, _logger);
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
    Deletes the target Tournament
    </summary>
    <param name="tournamentId"></param>
    <returns> The deleted Tournament </returns>
    */
    [HttpDelete]
    [ProducesResponseType<TournamentAPIModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TournamentAPIModel>> DeleteTournament(int tournamentId)
    {
        if (!await TournamentExists(t => t.Id == tournamentId))
        {
            return NotFound();
        }
        else
        {
            try
            {
                var deletedTournament = await _unitOfWork.TournamentRepository
                    .RemoveAsync(tournamentId);
                await _unitOfWork.CompleteAsync();
                var apiModel = await Task.Run(() => _mapper.Map<TournamentAPIModel>(deletedTournament));
                return Ok(apiModel);
            }
            catch (DbUpdateException ex)
            {
                LogError(ex, tournamentId, _logger);
                return StatusCode(500);
            }
        }
    }

    private async Task<bool> TournamentExists(Expression<Func<Tournament, bool>> predicate)
    {
        return await _unitOfWork.TournamentRepository.AnyAsync(predicate);
    }
}