// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal sealed class OperationTestTimer(OperationTestTimeProvider clock, TimerCallback callback, object? state, bool notifyDelay) : ITimer
{
    private bool _disposed;
    internal long? DueTicks { get; set; }
    internal long? PeriodTicks { get; private set; }

    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        lock (clock.Gate)
        {
            if (_disposed)
            {
                return false;
            }
            PeriodTicks = period > TimeSpan.Zero ? period.Ticks : null;
            DueTicks = dueTime == Timeout.InfiniteTimeSpan ? null : clock.GetTimestamp() + dueTime.Ticks;
            clock.Timers.Add(this);
            if (notifyDelay)
            {
                clock.NotifyTimer(dueTime);
            }
            return true;
        }
    }

    internal void Fire() => callback(state);

    public void Dispose()
    {
        lock (clock.Gate)
        {
            _disposed = true;
            clock.Timers.Remove(this);
            DueTicks = null;
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
