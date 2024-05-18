using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Tournaments.API.Controllers;

[Route("[controller]")]
public class TournamentsController(
    ILogger logger,
    IMapper mapper,
    IUnitOfWork unitOfWork) : ControllerBase
{
    private readonly ILogger _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    // Get
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TournamentDTO>>> GetTournaments()
    {
        var tournaments = await _unitOfWork.TournamentRepository.GetAllAsync();
        if (tournaments.Any())
        {
            var tournamentDTOs = _mapper.Map<IEnumerable<TournamentDTO>>(tournaments);
            return Ok(tournamentDTOs);
        }
        else
        {
            return NoContent();
        }
    }

    [HttpGet("{tournamentId}")]
    public async Task<ActionResult<TournamentDTO>> GetTournamentById(int tournamentId)
    {
        var tournament = await _unitOfWork.TournamentRepository.GetAsync(tournamentId);
        if (tournament is not null)
        {
            return Ok(_mapper.Map<TournamentDTO>(tournament));
        }
        else
        {
            return NotFound();
        }
    }

    // Post
    [HttpPost]
    public async Task<ActionResult<TournamentCreateDTO>> CreateTournament(TournamentCreateDTO tournamentDTO)
    {
        if (!ModelState.IsValid)
        {
            // TBD append error details here
            return BadRequest();
        }

        if (await TournamentExists(tournamentDTO.Id))
        {
            return Conflict($"Game with ID {tournamentDTO.Id} already exists");
        }

        var tournamentToCreate = await Task.Run(() => _mapper.Map<Tournament>(tournamentDTO));

        try
        {
            await _unitOfWork.TournamentRepository.AddAsync(tournamentToCreate);
            return Ok(tournamentDTO);
        }
        catch (DbUpdateException ex)
        {
            // TBD append error details here
            _logger.LogError("{Message}", "Could not create new tournament: " + ex.Message);
            return StatusCode(500);
        }
    }
    
    // Put
    [HttpPut]
    public async Task<ActionResult<TournamentDTO>> PutTournament(TournamentEditDTO editDTO)
    {
        if (!ModelState.IsValid)
        {
            // TBD append error details here
            return BadRequest();
        }

        if (!await TournamentExists(editDTO.Id))
        {
            return NotFound();
        }

        var tournamentToUpdate = _mapper.Map<Tournament>(editDTO);
        var updatedTournament = await _unitOfWork.TournamentRepository.UpdateAsync(tournamentToUpdate);
        var dto = _mapper.Map<TournamentDTO>(updatedTournament);
        return Ok(dto);
    }

    // Patch
    [HttpPatch("{tournamentId}")]
    public async Task<ActionResult<TournamentDTO>> PatchTournament(
        int tournamentId,
        [FromBody] JsonPatchDocument<Tournament> patchDocument)
    {
        if (patchDocument is not null)
        {
            var tournamentToPatch = await _unitOfWork.TournamentRepository.GetAsync(tournamentId);
            if (tournamentToPatch is not null)
            {
                patchDocument.ApplyTo(tournamentToPatch, ModelState);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    return Ok(_mapper.Map<TournamentDTO>(tournamentToPatch));
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
    public async Task<ActionResult<TournamentDTO>> DeleteTournament(int tournamentId)
    {

        if (!await TournamentExists(tournamentId))
        {
            return NotFound();
        }
        else
        {
            var deletedTournament = await _unitOfWork.TournamentRepository.RemoveAsync(tournamentId);
            var dto = _mapper.Map<TournamentDTO>(deletedTournament);
            return Ok(dto);
        }
    }

    private async Task<bool> TournamentExists(int tournamentId)
    {
        return await _unitOfWork.TournamentRepository.AnyAsync(tournamentId);
    }
}