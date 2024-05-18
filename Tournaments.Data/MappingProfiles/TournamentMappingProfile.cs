namespace Tournaments.Data.Mappings;

public class TournamentMappingProfile : Profile
{
    public TournamentMappingProfile()
    {
        CreateMap<Tournament, TournamentDTO>()
            .ConstructUsing(src => new TournamentDTO(src.Title));
        CreateMap<Tournament, TournamentCreateDTO>().ReverseMap();
        CreateMap<Tournament, TournamentEditDTO>().ReverseMap();
    }
}