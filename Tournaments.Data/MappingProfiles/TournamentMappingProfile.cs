namespace Tournaments.Data.Mappings;

public class TournamentMappingProfile : Profile
{
    public TournamentMappingProfile()
    {
        CreateMap<Tournament, TournamentCreateDTO>();
        CreateMap<Tournament, TournamentCreateDTO>().ReverseMap();
    }
}