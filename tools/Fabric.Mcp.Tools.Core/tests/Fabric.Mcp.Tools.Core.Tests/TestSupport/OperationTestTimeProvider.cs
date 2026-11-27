// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Channels;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal sealed class OperationTestTimeProvider() : TimeProvider
{
    internal object Gate { get; } = new();
    internal HashSet<OperationTestTimer> Timers { get; } = [];
    private readonly Channel<TimeSpan> _scheduled = Channel.CreateUnbounded<TimeSpan>();
    private long _ticks;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public override long GetTimestamp() => Interlocked.Read(ref _ticks);
    public override DateTimeOffset GetUtcNow() => FabricOperationTestData.Epoch + TimeSpan.FromTicks(GetTimestamp());

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        // Tests await polling delays separately from in-flight request budget timers.
        var timer = new OperationTestTimer(this, callback, state, state is not CancellationTokenSource);
        timer.Change(dueTime, period);
        return timer;
    }

    internal void NotifyTimer(TimeSpan dueTime) => _scheduled.Writer.TryWrite(dueTime);

    internal async Task WaitForTimerAsync(double seconds, CancellationToken cancellationToken)
    {
        while (await _scheduled.Reader.ReadAsync(cancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(10), cancellationToken) != TimeSpan.FromSeconds(seconds))
        {
        }
    }

    internal void Advance(double seconds)
    {
        var target = GetTimestamp() + TimeSpan.FromSeconds(seconds).Ticks;
        while (true)
        {
            OperationTestTimer[] due;
            lock (Gate)
            {
                var next = Timers.Where(timer => timer.DueTicks is not null).Select(timer => timer.DueTicks).Min();
                if (next is null || next > target)
                {
                    Interlocked.Exchange(ref _ticks, target);
                    return;
                }
                Interlocked.Exchange(ref _ticks, next.Value);
                due = Timers.Where(timer => timer.DueTicks == next).ToArray();
                foreach (var timer in due)
                {
                    timer.DueTicks = timer.PeriodTicks is { } period ? next + period : null;
                }
            }
            foreach (var timer in due)
            {
                timer.Fire();
            }
        }
    }
}
