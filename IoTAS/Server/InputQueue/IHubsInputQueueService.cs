using System;
using System.Threading;

namespace IoTAS.Server.InputQueue
{
    public interface IHubsInputQueueService : IDisposable
    {
        Request Dequeue(CancellationToken token);
        void Enqueue(Request request);
    }
}