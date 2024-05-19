namespace Tournaments.API.Controllers;

[Route("[controller]")]
public class GamesController(
    ILogger logger,
    IMapper mapper,
    IUnitOfWork unitOfWork) : ControllerBase
{
    private readonly ILogger _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    // Get
    [HttpGet]
    public async Task<ActionResult<GameAPIModel>> GetGames()
    {
        var games = await _unitOfWork.GameRepository.GetAllAsync();
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
    // Get {Id}
    [HttpGet("{gameId}")]
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
    // Post
    [HttpPost]
    public async Task<ActionResult<GameCreateAPIModel>> CreateGame(
        GameCreateAPIModel createModel)
    {
        if (!ModelState.IsValid)
        {
            // TBD append error details here
            return BadRequest();
        }

        if (await GameExists(createModel.Id))
        {
            return Conflict($"Game with ID {createModel.Id} already exists");
        }

        var gameToCreate = await Task.Run(() => _mapper.Map<Game>(createModel));

        try
        {
            await _unitOfWork.GameRepository.AddAsync(gameToCreate);
            await _unitOfWork.CompleteAsync();
            return Ok(createModel);
        }
        catch (DbUpdateException ex)
        {
            // TBD append error details here
            _logger.LogError("{Message}",
                $"Could not create new game {createModel.Id}: " + ex.Message);
            return StatusCode(500);
        }
    }
    // Put
    [HttpPut]
    public async Task<ActionResult<GameAPIModel>> PutGame(GameEditAPIModel editModel)
    {
        if (!ModelState.IsValid)
        {
            // TBD append error details here
            return BadRequest();
        }

        if (!await GameExists(editModel.Id))
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
            _logger.LogError("{Message}",
                $"Could not update game {editModel.Id}: " + ex.Message);
            return StatusCode(500);
        }
    }

    // Patch {Id}
    [HttpPatch("{gameId}")]
    public async Task<ActionResult<GameAPIModel>> PatchGame(
        int gameId,
        [FromBody] JsonPatchDocument<Game> patchDocument)
    {
        if (patchDocument is not null)
        {
            var gameToPatch = await _unitOfWork.GameRepository.GetAsync(gameId);
            if (gameToPatch is not null)
            {
                patchDocument.ApplyTo(gameToPatch, ModelState);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    return Ok(_mapper.Map<GameAPIModel>(gameToPatch));
                }
            }
            else
            {
                return NotFound();
            }
        }
        // TBD append error details here
        return BadRequest();
    }
    // Delete {Id}
    [HttpDelete]
    public async Task<ActionResult<GameAPIModel>> DeleteGame(int gameId)
    {
        if (!await GameExists(gameId))
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
                var apiModel = _mapper.Map<GameAPIModel>(deletedGame);
                return Ok(apiModel);
            }
            catch (DbUpdateException ex)
            {
                // TBD append error details here
                _logger.LogError("{Message}",
                    $"Could not delete game {gameId}: " + ex.Message);
                return StatusCode(500);
            }
        }
    }

    private async Task<bool> GameExists(int gameId)
    {
        return await _unitOfWork.GameRepository.AnyAsync(gameId);
    }
}