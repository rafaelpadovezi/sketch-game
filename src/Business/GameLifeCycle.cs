using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sketch.Services;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sketch.Business
{
    public sealed class GameLifeCycle : IGameLifeCycle, IAsyncDisposable
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<GameLifeCycle> _logger;
        private readonly ConcurrentDictionary<Guid, GameTimer> _turnTimers = new ConcurrentDictionary<Guid, GameTimer>();

        public GameLifeCycle(IServiceProvider services, IConfiguration configuration, ILogger<GameLifeCycle> logger)
        {
            _services = services;
            TurnDuration = configuration.GetValue<int>("TurnDuration");
            _logger = logger;
        }

        public bool AutoCreateNewTurn { get; set; } = true;
        public int TurnDuration { get; set; }

        public async Task NextTurn(Guid turnId)
        {
            if (_turnTimers.TryRemove(turnId, out var turnTimer))
                await turnTimer.DisposeAsync();

            var scope = _services.CreateScope();
            var game = scope.ServiceProvider
                .GetRequiredService<IRoundService>();
            await game.NextTurn(turnId);
        }

        public async ValueTask DisposeAsync() => await Task.CompletedTask;

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
                        ScheduleNextTurn(turnId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error ending turn");
                    }
                }, TurnDuration));
        }

        public void ScheduleNextTurn(Guid turnId)
        {
            new GameTimer(
                async () =>
                {
                    if (AutoCreateNewTurn)
                        await NextTurn(turnId);
                }, 5000);
        }

        public void Stop(Guid turnId)
        {
            _turnTimers.TryGetValue(turnId, out var timer);
            timer?.Stop();
        }
    }

    public class GameTimer : IAsyncDisposable
    {
        // TODO Threading.Timer vs Timers.Timer?
        private readonly Timer _timer;
        private readonly Func<Task> _task;
        public bool Done { get; private set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTimer"/> class.
        /// Execute a task once afiter the due time.
        /// </summary>
        /// <param name="task">Task executed after duetime.</param>
        /// <param name="dueTime">The amount of time to delay before callback is invoked, in milisseconds.</param>
        public GameTimer(Func<Task> task, int dueTime)
            : this(task, dueTime, Timeout.Infinite)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTimer"/> class.
        /// Execute a task once afiter the due time.
        /// </summary>
        /// <param name="task">Task executed after duetime.</param>
        /// <param name="dueTime">The amount of time to delay before callback is invoked, in milisseconds.</param>
        /// <param name="period">The time interval between invocations of callback, in milliseconds.</param>
        public GameTimer(Func<Task> task, int dueTime, int period)
        {
            _timer = new Timer(Heartbeat, null, dueTime, period);
            _task = task;
        }

        // https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#timer-callbacks
        private void Heartbeat(object? state)
        {
            if (_task == null)
                return;
            _ = _task().ContinueWith((_) => Done = true);
        }

        /// <summary>
        /// Stop the timer.
        /// </summary>
        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            Done = true;
        }

        public ValueTask DisposeAsync()
        {
            return _timer.DisposeAsync();
        }
    }
}