using AutoMapper;
using Sketch.Models;

namespace Sketch.DTOs.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Player, PlayerViewModel>();
            CreateMap<GameRoom, GameRoomViewModel>();
        }
    }
}
