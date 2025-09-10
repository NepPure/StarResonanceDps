using System.Runtime.CompilerServices;
using System.Threading.Channels;
using SharpPcap;

namespace StarResonanceDpsAnalysis.WPF.Data;

internal abstract class BaseDataSource : IDataSource, IDisposable
{
    public abstract IAsyncEnumerable<DpsEvent> GetDpsEventsAsync(CancellationToken cancellation = default);
    public abstract void Dispose();
}

internal class DpsDummyDataSource : BaseDataSource
{
    private readonly Channel<DpsEvent> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _producerTask;

    public DpsDummyDataSource(int capacity = 10_000)
    {
        _channel = Channel.CreateBounded<DpsEvent>(new BoundedChannelOptions(capacity)
        {
            SingleWriter = true,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        _producerTask = Task.Run(async () =>
        {
            long i = 0;
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), _cts.Token).ConfigureAwait(false);
                    var ev = new DpsEvent { Timestamp = DateTimeOffset.Now, Value = (int)i++ };
                    await _channel.Writer.WriteAsync(ev, _cts.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            finally
            {
                _channel.Writer.TryComplete();
            }
        }, _cts.Token);
    }

    public override async IAsyncEnumerable<DpsEvent> GetDpsEventsAsync(
        [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var reader = _channel.Reader;
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        while (await reader.WaitToReadAsync(linkedCts.Token).ConfigureAwait(false))
        {
            while (reader.TryRead(out var item))
            {
                yield return item;
            }
        }
    }

    public override void Dispose()
    {
        _cts.Cancel();
        try
        {
            _producerTask.Wait(500);
        }
        catch
        {
            /* best-effort */
        }

        _channel.Writer.TryComplete();
        _cts.Dispose();
    }
}

internal class CapturedDpsDataSource : BaseDataSource
{
    private readonly Channel<DpsEvent> _channel;
    private readonly ICaptureDevice? _device;
    private long _droppedEvents;

    public CapturedDpsDataSource(ICaptureDevice? device, int capacity = 50_000)
    {
        _channel = Channel.CreateBounded<DpsEvent>(new BoundedChannelOptions(capacity)
        {
            SingleWriter = false, // capture callbacks may come on multiple threads
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });

        _device = device;
        if (_device == null)
        {
            _channel.Writer.TryComplete();
            return;
        }

        // Use non-blocking write on packet arrival to avoid blocking capture threads.
        _device.OnPacketArrival += (s, e) =>
        {
            var dpsEvent = ParsePacket(e);
            if (dpsEvent == null) return;
            if (_channel.Writer.TryWrite(dpsEvent)) return;
            Interlocked.Increment(ref _droppedEvents);
        };

        _device.Open();
        _device.StartCapture();
    }

    public long DroppedEvents => Interlocked.Read(ref _droppedEvents);

    public override async IAsyncEnumerable<DpsEvent> GetDpsEventsAsync(
        [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var reader = _channel.Reader;
        while (await reader.WaitToReadAsync(cancellation).ConfigureAwait(false))
        {
            while (reader.TryRead(out var item))
            {
                yield return item;
            }
        }
    }

    private DpsEvent? ParsePacket(PacketCapture e)
    {
        // TODO: Implement actual parsing logic
        return new DpsEvent { Timestamp = DateTimeOffset.Now, Value = e.Data.Length };
    }

    public override void Dispose()
    {
        try
        {
            if (_device == null) return;
            try
            {
                _device.StopCapture();
            }
            catch (Exception)
            {
                // Ignore
            }

            try
            {
                _device.Close();
            }
            catch (Exception)
            {
                // Ignore
            }
        }
        finally
        {
            _channel.Writer.TryComplete();
        }
    }
}

internal class MultipleDpsDataSource : BaseDataSource
{
    private readonly Channel<DpsEvent> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly List<Task> _fanInTasks = new();
    private long _droppedEvents;

    public MultipleDpsDataSource(IEnumerable<IDataSource> dataSources, int capacity = 50_000)
    {
        _channel = Channel.CreateBounded<DpsEvent>(new BoundedChannelOptions(capacity)
        {
            SingleWriter = false,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropOldest
        });

        foreach (var ds in dataSources)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await foreach (var ev in ds.GetDpsEventsAsync(_cts.Token).ConfigureAwait(false))
                    {
                        // Non-blocking fan-in: prefer dropping to blocking capture threads.
                        if (!_channel.Writer.TryWrite(ev))
                            Interlocked.Increment(ref _droppedEvents);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch
                {
                    /* swallow individual source failures */
                }
            }, _cts.Token);

            _fanInTasks.Add(task);
        }
    }

    public long DroppedEvents => Interlocked.Read(ref _droppedEvents);

    public override async IAsyncEnumerable<DpsEvent> GetDpsEventsAsync(
        [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var reader = _channel.Reader;
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellation, _cts.Token);
        while (await reader.WaitToReadAsync(linkedCts.Token).ConfigureAwait(false))
        {
            while (reader.TryRead(out var item))
            {
                yield return item;
            }
        }
    }

    public override void Dispose()
    {
        _cts.Cancel();
        try
        {
            Task.WhenAll(_fanInTasks).Wait(500);
        }
        catch
        {
            /* best-effort */
        }

        _channel.Writer.TryComplete();
        _cts.Dispose();
    }
}

// Example event class
public class DpsEvent
{
    public DateTimeOffset Timestamp { get; set; }
    public int Value { get; set; }
}