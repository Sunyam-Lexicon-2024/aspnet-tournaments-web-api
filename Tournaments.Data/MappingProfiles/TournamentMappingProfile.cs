namespace Tournaments.Data.Mappings;

public class TournamentMappingProfile : Profile
{
    public TournamentMappingProfile()
    {
        CreateMap<Tournament, TournamentAPIModel>()
            .ConstructUsing(src => new TournamentAPIModel(src.Title));
        CreateMap<Tournament, TournamentCreateAPIModel>().ReverseMap();
        CreateMap<Tournament, TournamentEditAPIModel>().ReverseMap();
    }
}