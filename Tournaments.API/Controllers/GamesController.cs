using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<GameDTO>> GetGames()
    {
        var games = await _unitOfWork.GameRepository.GetAllAsync();
        if (games.Any())
        {
            var gameDTOs = await Task.Run(() => _mapper.Map<IEnumerable<GameDTO>>(games));
            return Ok(gameDTOs);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet("{gameId}")]
    public async Task<ActionResult<GameDTO>> GetGameById(int gameId)
    {
        var game = await _unitOfWork.GameRepository.GetAsync(gameId);
        if (game is not null)
        {
            return Ok(_mapper.Map<GameDTO>(game));
        }
        else
        {
            return NotFound();
        }
    }

    // Post
    [HttpPost]
    public async Task<ActionResult<GameCreateDTO>> CreateGame(GameCreateDTO gameDTO)
    {
        if (!ModelState.IsValid)
        {
            // TBD append error details here
            return BadRequest();
        }

        if (await GameExists(gameDTO.Id))
        {
            return Conflict($"Game with ID {gameDTO.Id} already exists");
        }

        var gameToCreate = await Task.Run(() => _mapper.Map<Game>(gameDTO));

        try
        {
            await _unitOfWork.GameRepository.AddAsync(gameToCreate);
            return Ok(gameDTO);
        }
        catch (DbUpdateException ex)
        {
            // TBD append error details here
            _logger.LogError("{Message}", "Could not create new game: " + ex.Message);
            return StatusCode(500);
        }
    }
    // Put
    [HttpPut]
    public async Task<ActionResult<GameDTO>> PutGame(GameEditDTO editDTO)
    {
        var gameToUpdate = _mapper.Map<Game>(editDTO);
        var updatedTournament = await _unitOfWork.GameRepository.UpdateAsync(gameToUpdate);
        var dto = _mapper.Map<GameDTO>(updatedTournament);
        return Ok(dto);
    }

    // Patch
    [HttpPatch("{gameId}")]
    public async Task<ActionResult<GameDTO>> PatchGame(
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
                    return Ok(_mapper.Map<GameDTO>(gameToPatch));
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

    // Delete

    [HttpDelete]
    public async Task<ActionResult<GameDTO>> Delete(int gameId)
    {

        if (!await GameExists(gameId))
        {
            return NotFound();
        }
        else
        {
            var deletedTournament = await _unitOfWork.GameRepository.RemoveAsync(gameId);
            var dto = _mapper.Map<GameDTO>(deletedTournament);
            return Ok(dto);
        }
    }

    private async Task<bool> GameExists(int gameId)
    {
        return await _unitOfWork.GameRepository.AnyAsync(gameId);
    }
}