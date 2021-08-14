# 04 Device Design

The design of the IoT Smart Speaker Device (Device for short) emulation component is a rather straighforward .NET 5 Console App that proceeds in a linear fashion.

The App starts by asking the interactive user to enter a ```DeviceID```. It currently is the user's responsability to ensure no duplicate IDs are entered.
Once a valid ```DeviceID``` has been obtained, the program sets a ```Ctrl-C``` handler, configures the SignalR connection, starts the connection, and registers the Device with the server. 
When all that is succesfull, the 15 second Hearbeat operation is started.
Pending operations can be cancelled by the ```Ctrl-C``` handler at any time. 
This is implemented with a standard ```CancellationTokenSource```, which's ```CancellationToken``` is used in all ```async``` operations. 
Upon cancellation, the connection will be torn down and the App will exit cleanly.

SignalR supports auto-reconnection on an existing connection, but auto-reconnect does not work for the initial connection attempt. 
Also, out of the box, auto-reconnect only has a limited number of retries (although you can define your own retry-strategy and hook that into the auto retry mechanism). 
To keep things simple, the resposability for reconnection and re-registration is handled by the App itself. 
By registering on the ```HubConnection.Closed``` event, the App gets notified when the connection is closed and whether this is due to an error (which may be trancient) or forced by the Server. 
In both of these cases, the App tries to re-establish a connection and re-registers itself.

The following excerpts show the simplified code of the App. It is simplified in that code not essential for comprehension has been omitted and that logging calls have been replaced by comments.
It all starts in the Main function, where a ```HubConnection``` is build and configured with connection life-cycle callbacks.
This is followed by a call to the ```StartConnectionAndRegisterAsync ``` method, which kicks off the interaction with the Server.
Once connection start and registration are both succesful, the ```DoHeartBeatAsync``` ```Task``` is started and awaited.
This ```Task``` only completes when the Ctrl-C handler fires and cancels the ```tokenSource```.

 
```csharp
public class Program

// Must be fields so the event handlers can access them
private static readonly CancellationTokenSource tokenSource = new();private static HubConnection connection;

static async Task Main(string[] args){
    // Setting up Ctrl-C handler and other initialization omitted ...
    
    connection = new HubConnectionBuilder().WithUrl(hubUrl).Build();
    
    // set connection life-cycle callback    connection.Closed += ConnectionClosedHandler;
    
    bool operational = await StartConnectionAndRegisterAsync();    if (operational)    {
        // Only completes on cancellation of tokenSource due to Ctrl-C         await DoHeartbeatAsync();
    }
    
    // We only get here after a cancellation on the tokenSource from Ctrl-C    await TeardownConnectionAsync();    tokenSource.Dispose();
}```

The ```ConnectAndRegisterAsync``` methods keeps trying until it either is succesful in both starting the connection and registering the Device, or it is cancelled through the ```tokenSource```. In case of start or registration failure, it uses retries with an incremental backoff strategy (not shown here) in order to avoid putting too much load on the server (which may be too busy or restarting). 

```csharp
private static async Task<bool> StartConnectionAndRegisterAsync(){    bool started = false;    bool registered = false;    // Keep trying until we can start and register or the token source is canceled.    while (!tokenSource.IsCancellationRequested && !registered)    {        if (!started)        {             started = await TryToStartConnectionAsync();        }        if (started)        {             registered = await TryToRegisterDeviceAsync();        }        if (registered) return true;        if (tokenSource.IsCancellationRequested) return false;        // Either Start or Registration failed, backoff and try again ...        try        {
            // calculation of randomized and increasing delay omitted            await Task.Delay(delay, tokenSource.Token);        }        catch (TaskCanceledException)        {            // ConnectWithRetryAsync cancelled while waiting to retry connection        }
    }
    
    // We only get here due to cancellation, all other failure 
    // conditions retry the operation(s) until succesful ...    return false;}```
Note that the same backof is used regardless whether starting the connection or registering the Device failed.
Although in practise it is highly unlikely that registration would fail just after the connection was started, it is better to be safe than sorry and build robust code.

In the ```ConnectionClosedHandler```, we determine the reason behind the connection closing and either exit (when cancellation was requested) or retry to start and register with the same ```StartConnectionAndRegisterAsync()``` method that was initially used to kick off the Device's activity.

```csharp
// Called by SignalR runtime on cancellation, error(?) or keep-alive time-outprivate static async Task ConnectionClosedHandler(Exception e){    if (tokenSource.IsCancellationRequested)    {        // Connection to server closed due to cancellation - exit        return;    }    if (e is null)    {        // Connection closed by Server - retry
    }    else    {        // Connection to Server has been lost - retry    }    _ = await StartConnectionAndRegisterAsync();}
```

Note that this code does not stop or suspend sending the heartbeat in case of connection loss.
This is deliberate; the heartbeat method simply checks wheter the connection is up before sending the heartbeat and skips sending if it is down.

```csharp
private static async Task DoHeartbeatAsync(){    while (!tokenSource.IsCancellationRequested)    {        // Delay 15 seconds; in case of cancellation: end the Task        try        {            await Task.Delay(15 * 1000, tokenSource.Token);        }        catch (TaskCanceledException)        {             // cancelled while waiting to send heartbeat - exit             return;        }        if (connection.State == HubConnectionState.Connected)        {            // Send the heartbeat; in case of cancellation: end the Task            try            {                await connection.InvokeAsync("ReceiveDeviceHeartbeat"), dto, tokenSource.Token);            }            catch (TaskCanceledException)            {                // cancelled while sending heartbeat - exit
                return;            }            catch (Exception e)            {                // Error while sending heartbeat - ignore
            }
        }        else        {            // heartbeat skipped (no connection)
        }    }
}
```

## Improvement opportunities
For the future milestones, and definately if this was a real product deployment, the following improvement opportunities should be considered.

### Use ```HostedService``` in stead of ```Ctrl-C``` interception

I only learned about the possibility to use the ```HostedService``` outside of ASP.NET after essentially finishing the Device code. Building upon it would allow:

* To easily use standard Logging
* To use Dependency Injection
* To do the heartbeat sending in a class derived from ```BackgroundService```

See [https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0) for details.

### Use standard .NET Core Logging

This is enabled by the ```HostedService``.

