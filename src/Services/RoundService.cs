using Sketch.Business;
using Sketch.DTOs;
using Sketch.Infrastructure.Connection;
using Sketch.Infrastructure.Database.Repositories.Interfaces;
using Sketch.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sketch.Services
{
    public class RoundService : IRoundService
    {
        private readonly IGameRoomRepository _gameRoomRepository;
        private readonly IWordService _wordService;
        private readonly IServerConnection _serverConnection;
        private readonly IGameLifeCycle _gameLifeCycle;

        public RoundService(
            IGameRoomRepository gameRoomRepository,
            IWordService wordService,
            IServerConnection serverConnection,
            IGameLifeCycle gameLifeCycle)
        {
            _gameRoomRepository = gameRoomRepository;
            _wordService = wordService;
            _serverConnection = serverConnection;
            _gameLifeCycle = gameLifeCycle;
        }

        public async Task StartRound(GameRoom gameRoom)
        {
            Word word = await _wordService.PickWord(gameRoom.Type);
            var (round, turn) = SketchGame.NewRound(word, gameRoom.Players);
            gameRoom.Rounds.Add(round);
            var drawingPlayer = turn.DrawingPlayer;

            await _serverConnection.Send(GameResponse.StartTurn(word), drawingPlayer);
            await _serverConnection.Send(
                GameResponse.StartTurn(drawingPlayer),
                gameRoom.Players.Where(x => x.Id != drawingPlayer.Id));

            await _gameRoomRepository.SaveChanges();

            _gameLifeCycle.StartTurn(gameRoom.Id, turn.Id);
        }

        public async Task GuessWord(GameRoom gameRoom, Player player, string guess)
        {
            var turn = gameRoom.CurrentTurn();

            bool hit = SketchGame.GuessWord(player, guess, turn);

            if (hit)
                await _serverConnection.Send(GameResponse.Hit(guess), player);
            else
                await _serverConnection.Send(ChatMessage.Public(player.Username, guess), gameRoom.Players);

            if (turn.PlayersTurns.All(x => x.Hit || x.IsDrawing))
            {
                _gameLifeCycle.Stop(turn.Id);
                await EndTurn(gameRoom);
            }
        }


        public async Task EndTurn(Guid gameRoomId)
        {
            var gameRoom = (await _gameRoomRepository.GetById(gameRoomId))
                ?? throw new Exception($"Game room {gameRoomId} not found");
            await EndTurn(gameRoom);
        }

        public async Task EndTurn(GameRoom gameRoom)
        {
            var turn = gameRoom.CurrentTurn();
            turn.EndTimestamp = DateTime.Now;
            var drawingPlayer = gameRoom.Players.SingleOrDefault(x => x.Id == turn.DrawingPlayerId);
            if (drawingPlayer != null)
            {
                var drawingPlayerTurn = turn.PlayersTurns.Single(x => x.PlayerId == drawingPlayer.Id);
                drawingPlayerTurn.Points = SketchGame.CalculateDrawingPoints(turn.PlayersTurns);
            }

            await _serverConnection.Send(GameResponse.EndOfTurn(turn), gameRoom.Players);
        }

        public async Task NextTurn(Guid lastTurnId)
        {
            var (gameRoom, round, lastTurn) =
                await _gameRoomRepository.GetRoomRoundAndTurn(lastTurnId);

            var isRoundComplete = round.IsComplete(gameRoom.Players);
            // TODO if isRoundComplete

            Word word = await _wordService.PickWord(gameRoom.Type);
            var turn = SketchGame.NextTurn(round, word, gameRoom.Players);
            round.Turns.Add(turn);

            var drawingPlayer = turn.DrawingPlayer;

            await _serverConnection.Send(GameResponse.StartTurn(word), drawingPlayer);
            await _serverConnection.Send(
                GameResponse.StartTurn(drawingPlayer),
                gameRoom.Players.Where(x => x.Id != drawingPlayer.Id));

            await _gameRoomRepository.SaveChanges();

            _gameLifeCycle.StartTurn(gameRoom.Id, turn.Id);
        }
    }
}
