//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoTAS.Server.InputQueue;

public interface IHubsInputQueueService : IDisposable
{
    Task<Request> DequeueAsync(CancellationToken token);
    void Enqueue(Request request);
}