using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.WPF.Data;

/// <summary>
/// Simple simulated producer implementing IDataSource for benchmark/testing.
/// </summary>
internal class SimulatedDataSource : BaseDataSource
{
    private readonly Channel<DpsEvent> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _producerTask;
    private long _produced;
    private long _dropped;

    public SimulatedDataSource(int totalEvents, int burstPerMs = 1, int capacity = 10_000)
    {
        _channel = Channel.CreateBounded<DpsEvent>(new BoundedChannelOptions(capacity)
        {
            SingleWriter = true,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });

        _producerTask = Task.Run(async () =>
        {
            try
            {
                for (int i = 0; i < totalEvents && !_cts.Token.IsCancellationRequested; i++)
                {
                    // produce bursts to simulate high-rate
                    for (int b = 0; b < burstPerMs; b++)
                    {
                        var ev = new DpsEvent { Timestamp = DateTimeOffset.Now, Value = i };
                        if (!_channel.Writer.TryWrite(ev))
                            Interlocked.Increment(ref _dropped);
                        else
                            Interlocked.Increment(ref _produced);
                    }
                    // tiny delay to control rate
                    await Task.Delay(1, _cts.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _channel.Writer.TryComplete();
            }
        }, _cts.Token);
    }

    public long Produced => Interlocked.Read(ref _produced);
    public long Dropped => Interlocked.Read(ref _dropped);

    public override async IAsyncEnumerable<DpsEvent> GetDpsEventsAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation, _cts.Token);
        var reader = _channel.Reader;
        while (await reader.WaitToReadAsync(linked.Token).ConfigureAwait(false))
        {
            while (reader.TryRead(out var item))
                yield return item;
        }
    }

    public override void Dispose()
    {
        _cts.Cancel();
        try { _producerTask.Wait(500); } catch { }
        _channel.Writer.TryComplete();
        _cts.Dispose();
    }
}

/// <summary>
/// Small benchmark you can run from a debug entry point to tune capacity/policy.
/// Example use: await HighRateSimulator.RunAsync(); (run from a console/test harness)
/// </summary>
internal static class HighRateSimulator
{
    public static async Task RunAsync(int producers = 4, int eventsPerProducer = 200_000, int burstPerMs = 10, int capacity = 50_000)
    {
        var sources = new List<IDataSource>();
        var simulated = new List<SimulatedDataSource>();
        for (int i = 0; i < producers; i++)
        {
            var s = new SimulatedDataSource(eventsPerProducer, burstPerMs, capacity: Math.Max(1000, capacity / producers));
            simulated.Add(s);
            sources.Add(s);
        }

        using var multi = new MultipleDpsDataSource(sources, capacity: capacity);

        long consumed = 0;
        var sw = Stopwatch.StartNew();

        var cts = new CancellationTokenSource();
        var consumer = Task.Run(async () =>
        {
            await foreach (var ev in multi.GetDpsEventsAsync(cts.Token))
            {
                // cheap consume; in real UI you'd marshal minimal info to UI thread
                consumed++;
            }
        }, cts.Token);

        // wait producers to finish
        foreach (var s in simulated)
        {
            // spin until writer completes (best-effort)
            while (s.Produced + s.Dropped < eventsPerProducer && !cts.IsCancellationRequested)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        // give some time for drain
        await Task.Delay(500).ConfigureAwait(false);

        // stop consumer & dispose
        cts.Cancel();
        try { await consumer.ConfigureAwait(false); } catch { }

        sw.Stop();

        long totalProduced = 0, totalDropped = 0;
        foreach (var s in simulated)
        {
            totalProduced += s.Produced;
            totalDropped += s.Dropped;
        }

        Console.WriteLine($"Producers: {producers}, eventsPerProducer: {eventsPerProducer}, burstPerMs: {burstPerMs}, capacity: {capacity}");
        Console.WriteLine($"Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Produced (enqueued): {totalProduced}");
        Console.WriteLine($"Dropped at producers (local): {totalDropped}");
        Console.WriteLine($"Dropped at fan-in (MultipleDpsDataSource): {multi.DroppedEvents}");
        Console.WriteLine($"Consumed: {consumed}");
        Console.WriteLine($"Lost total (producedRequested - consumed): {producers * (long)eventsPerProducer - consumed}");
        Console.WriteLine("Tune channel capacity and FullMode based on observed drops and memory.");
    }
}