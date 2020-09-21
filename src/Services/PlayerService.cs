using AutoMapper;
using Sketch.DTOs;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using Sketch.Models;
using Sketch.Services.Interfaces;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IMapper _mapper;

        public PlayerService(IPlayerRepository playerRepository, IMapper mapper)
        {
            _playerRepository = playerRepository;
            _mapper = mapper;
        }

        public async ValueTask<(bool result, PlayerViewModel? player)> Login(string username)
        {
            if (await _playerRepository.NicknameIsInUse(username))
            {
                return (false, null);
            }

            Player player = new Player { Username = username };
            await _playerRepository.Add(player);
            return (true, _mapper.Map<Player, PlayerViewModel>(player));
        }
    }
}
