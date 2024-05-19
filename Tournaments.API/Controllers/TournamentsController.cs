namespace Tournaments.API.Controllers;

[Route("[controller]")]
public class TournamentsController(
    ILogger<Tournament> logger,
    IMapper mapper,
    IUnitOfWork unitOfWork) : ControllerBase
{
    private readonly ILogger<Tournament> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    // Get
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TournamentAPIModel>>> GetTournaments(
        [FromQuery] QueryParameters? queryParameters)
    {
        IEnumerable<Tournament> tournaments;

        if (queryParameters is not null)
        {
            tournaments = await _unitOfWork.TournamentRepository
                .GetAsyncByParams(queryParameters);
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
    // Get {Id}
    [HttpGet("{tournamentId}")]
    public async Task<ActionResult<TournamentAPIModel>> GetTournamentById(
        int tournamentId,
        [FromQuery] QueryParameters? queryParams)
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
    // Post
    [HttpPost]
    public async Task<ActionResult<TournamentCreateAPIModel>> CreateTournament(
        TournamentCreateAPIModel createModel)
    {
        if (!ModelState.IsValid)
        {
            // TBD append error details here
            return BadRequest();
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
            _logger.LogError("{Message}",
                "Could not create new tournament" +
                $"{JsonSerializer.Serialize(createModel)}:" +
                ex.Message);
            return StatusCode(500);
        }
    }
    // Put
    [HttpPut]
    public async Task<ActionResult<TournamentAPIModel>> PutTournament(
            TournamentEditAPIModel editModel)
    {
        if (!ModelState.IsValid)
        {
            // TBD append error details here
            return BadRequest();
        }

        if (!await TournamentExists(editModel.Id))
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
            _logger.LogError("{Message}",
                $"Could not create update tournament {editModel.Id}: " + ex.Message);
            return StatusCode(500);
        }
    }
    // Patch {Id}
    [HttpPatch("{tournamentId}")]
    public async Task<ActionResult<TournamentAPIModel>> PatchTournament(
        int tournamentId,
        [FromBody] JsonPatchDocument<Tournament> patchDocument)
    {
        if (patchDocument is not null)
        {
            var tournamentToPatch = await _unitOfWork.TournamentRepository
                .GetAsync(tournamentId);

            if (tournamentToPatch is not null)
            {
                patchDocument.ApplyTo(tournamentToPatch, ModelState);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    return Ok(_mapper.Map<TournamentAPIModel>(tournamentToPatch));
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
    public async Task<ActionResult<TournamentAPIModel>> DeleteTournament(int tournamentId)
    {
        if (!await TournamentExists(tournamentId))
        {
            return NotFound();
        }
        else
        {
            var deletedTournament = await _unitOfWork.TournamentRepository
                .RemoveAsync(tournamentId);
            var dto = _mapper.Map<TournamentAPIModel>(deletedTournament);
            return Ok(dto);
        }
    }

    private async Task<bool> TournamentExists(int tournamentId)
    {
        return await _unitOfWork.TournamentRepository.AnyAsync(tournamentId);
    }
}