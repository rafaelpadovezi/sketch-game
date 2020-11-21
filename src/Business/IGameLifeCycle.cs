using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sketch.Services;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Sketch.Business
{
    public interface IGameLifeCycle
    {
        void StartTurn(Guid gameroomId, Guid turnId);
        void Stop(Guid id);
    }

    public class GameLifeCycle : IGameLifeCycle
    {
        private readonly IServiceProvider _services;
        private readonly int _turnDuration;
        private readonly ILogger<GameLifeCycle> _logger;
        private readonly ConcurrentDictionary<Guid, GameTimer> _turnTimers = new ConcurrentDictionary<Guid, GameTimer>();

        public GameLifeCycle(IServiceProvider services, IConfiguration configuration, ILogger<GameLifeCycle> logger)
        {
            _services = services;
            _turnDuration = configuration.GetValue<int>("TurnDuration");
            _logger = logger;
        }

        public void StartTurn(Guid gameroomId, Guid turnId)
        {
            // TODO: is it threadsafe?
            _turnTimers.GetOrAdd(turnId, new GameTimer(
                async () =>
                {
                    try
                    {
                        var scope = _services.CreateScope();
                        var game = scope.ServiceProvider
                            .GetRequiredService<IRoundService>();
                        await game.EndTurn(gameroomId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error ending turn");
                    }
                }, _turnDuration));
        }

        public void Stop(Guid turnId)
        {
            _turnTimers.TryGetValue(turnId, out var timer);
            timer?.Stop();
        }
    }

    public class GameTimer
    {
        // TODO Threading.Timer vs Timers.Timer?
        private readonly Timer _timer;
        private readonly Func<Task> _task;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTimer"/> class.
        /// Execute a task once afiter the due time.
        /// </summary>
        /// <param name="task">Task executed after duetime.</param>
        /// <param name="dueTime">The amount of time to delay before callback is invoked, in milisseconds.</param>
        public GameTimer(Func<Task> task, int dueTime)
        {
            _timer = new Timer(Heartbeat, null, dueTime, Timeout.Infinite);
            _task = task;
        }

        // https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#timer-callbacks
        private void Heartbeat(object? state)
        {
            _ = _task();
        }

        /// <summary>
        /// Stop the timer.
        /// </summary>
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}