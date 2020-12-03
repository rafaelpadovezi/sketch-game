using System;

namespace Sketch.Business
{
    public interface IGameLifeCycle
    {
        void StartTurn(Guid gameroomId, Guid turnId);
        void Stop(Guid id);
        int TurnDuration { get; }
        void ScheduleNextTurn(Guid turnId);
    }
}