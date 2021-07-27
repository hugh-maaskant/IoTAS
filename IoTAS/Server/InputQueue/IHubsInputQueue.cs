using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoTAS.Server.InputQueue
{
    public interface IHubsInputQueue : IDisposable
    {
        Task<Request> DequeueAsync(CancellationToken token);
        void Enqueue(Request request);
    }
}