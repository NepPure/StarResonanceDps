namespace StarResonanceDpsAnalysis.WPF.Data;

using System.Collections.Generic;
using System.Threading;

public interface IDataSource
{
    /// <summary>
    /// Asynchronously enumerate DPS events. Consumers can use `await foreach`.
    /// </summary>
    IAsyncEnumerable<DpsEvent> GetDpsEventsAsync(CancellationToken cancellation = default);
}