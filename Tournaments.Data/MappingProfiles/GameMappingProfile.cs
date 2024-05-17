namespace Tournaments.Data.Mappings;

public class GameMappingProfile : Profile
{
    public GameMappingProfile()
    {
        CreateMap<Tournament, TournamentCreateDTO>();
        CreateMap<Tournament, TournamentCreateDTO>().ReverseMap();
    }
}