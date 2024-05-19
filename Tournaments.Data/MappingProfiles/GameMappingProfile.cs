namespace Tournaments.Data.Mappings;

public class GameMappingProfile : Profile
{
    public GameMappingProfile()
    {
        CreateMap<Game, GameAPIModel>()
            .ConstructUsing(src => new GameAPIModel(src.Title));
        CreateMap<Game, GameCreateAPIModel>().ReverseMap();
        CreateMap<Game, GameEditAPIModel>().ReverseMap();
    }
}