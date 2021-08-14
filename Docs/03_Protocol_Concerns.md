# 03 Protocol Concerns

The assigmant for the first milestone is to build a simple basic solution with infrastructure for both the Device (a console App) and the Monitor (a browser SPA) to connect with SignalR Core to the Server and have the Devices emit heartbeat messages that the Monitor  receives through the Server relay.
This provides the information the Monitor needs for displaying an overview of the Device statusses to the user.

At the functional level, the Client process is fairly simple:

1. Build, configure and start a SignalR connection to the Server's Hub.
2. Register the Device c.q. Monitor with the Server.
3. Send (Device) or Receive (Monitor) heartbeat information.

A Device passes its ```DeviceId``` to the Server during registration.
A Monitor has -for this milestone- no data to pass and, apart from registering, only receives data.

Once a Device is registered, it starts sending the heartbeat messages (note I like to think in terms of messages, although strictly speaking SignalR provides a Remote Procedure Call service). 

One way to look at this as a layered protocol stack. 
SignalR provides the Transport, Session and Presentation layers with connection establishment and marshalling. 
We provide the Application level, where we have registration and, after that heartbeat signaling.

Note that the Server is essentially reactive, it:

1. Waits for connections.
2. Upon Device Registration records it in an internal store and multicasts the information to all Monitors.
3. Upon Device Heartbeat updates the status store and multicasts the update to all Monitors.
4. Upon Monitor registration sends it a copy of the current store.


### SignalR Connection

The Server provides a Hub, which is an endpoint that Clients can connect to.
Hubs are identified with a URL.
Clients create connections to a Hub, after which there is bi-directional communication through remote procedure calls.

A SignalR Client first needs to build an internal ```HubConnection``` using the ```HubConnectionBuilder``` and then configure and start the connection, e.g.

```csharp
 connection = new HubConnectionBuilder()     .WithUrl(hubUrl)     .Build();// set connection life-cycle callback(s)connection.Closed += ConnectionClosedHandler;// set the method(s) that the Server will call remotely over SignalRconnection.On<Dto>("ReceiveDeviceHeartbeatUpdate", ReceiveDeviceHeartbeatUpdate);

// Start the connection
await connection.StartAsync(cancellationToken);
```

### Application Connection

At the application level there really are two connection types:

1. Between the Device and the Server
2. Between the Monitor and the Server

This means that the Server needs to act as a relay at the application level.

The assigment was for both connections to terminate at the same Server SignalR Hub, which can be typed (on the Server side). Because I have a different use case in mind that I want to try out here, but also because I do not like to terminate two differently typed connections at the same Hub, I decided to use seperate Hubs instead: ```/device``` and ```/monitor```.
While this is -imho- cleaner, it is also more difficult to deal with sequencing and error conditions, as described below.

#### Device

The Device

1. Registers its ```DeviceId```
2. Sends a heartbeat once registered
3. Currently need not handle any calls from the Server

As this is (emulates) an IoT Device that has no direct user interaction, my assumption is that it will need to have robust error handling. In particular it will need to:

1. Reconnect and re-register upon any network or Server failure.
2. Reconnect and re-register after the Server closes the connection.
3. Have a reasonable backoff strategy for reconnections after failure.

The second point can be debated, but this is how I implemented the application protocol.

Furthermore, the Server needs to be robust and thus tolerate Device disconnects (note it currently his not sending anything, but that should also be robust once it is needed).

#### Monitor

The Monitor SPA does have a user, but it is still prefereble that it too is robust against network and Server failures without relying on user interaction.

Note that with seperate Device and Monitor Hubs, the Monitor registration, which carries no data, could be omitted with the SignalR connection establishment serving both purposes in one go. 
This would make error detection and recovery significantly easier.
But as a solution is already needed for the Device, I kept the registration as a dedicated step with a similar implementation in the Device and the Monitor.

#### Server

As noted before, the Server is entirely reactive. 
Still there are some choices to make for its behaviour.
I have -fairly arbitrarily- decided upon the following:

* The Server will not check for duplicate ```DeviceId```s or heartbeats from unregistered ```DeviceId```s, as there is no simple mechanism to do anything meaningfull, such as rejecting the registration. 
In any real-world deployment Devices would have to be commisioned and the Server would likely have access to the commissioning database. 
Also there would be some authentication mechanism for IoT Devices.

* Likewise there is no authentication for Monitor users.

* The Server will not retry sending failed status updates. 
This would be difficult (especially with multicast messages) and the 15 second heartbeat update gives a quick enough recovery anyway. 

### SignalR Error Handling

As in almost all distributed applications, dealing with network and node failures is a hard problem.
Also I must say that the official SignalR Core documentation is very unclear in this area.
The happy path is fairly well described, but even there it is unclear what guarantees -if any- SignalR gives as to atomicity and sequencing of operations.
This is particularly the case when multicasting to a group, e.g.


```csharp
// Example: Server RPC to Clients with a typed Hub initiated from outside the Hub
await hubContext.Clients.Group("monitors").ReceiveDeviceRegistration(registrationDto);
await hubContext.Clients.Group("monitors").ReceiveHeartbeat(heartbeatDto);

```

It is not clear (to me) if it is possible that the second multicast starts "on the wire" before the first finishes, and hence that the -in this case different- RPCs are called out-of-order on some of the Monitors in the group.
Note that the application level multicast really is a collection of unicasts at the socket level!
My guess is that the operations will be sequenced, but that is all it is: a guess.

Also what happens with transient network errors is -imho- not very well specified.
My expectation is that any transmission failures on the underlying transport (typically a websocket over TCP/IP) will result in an exception being delivered to the Task return type on the calling side, but I cannot confirm that from documentation and it is nearly impossible to test.
What is clear, is that any long lived problem is detected by the SignalR internal heartbeat.
On the Server side, unreacheable clients can be detected by overriding the Hub's ```OnDisconnectedAsync``` method.
On the Client side, an unreacheable Hub triggers a connection closed event, for which clients can define a handler:

```csharp
connection.Closed += ConnectionClosedHandler;
```
Note that SignalR provides some support for automatic reconnection in case of failures.
But that is not available for starting the connection and, out of the box, it only has a limited number of retries.
It is possible, however, to define and hook-up any tailored backoff and retry strategy.
For these reasons I decided to do my own error recovery and only rely on catchhing exceptions and the ```connection.Closed``` event handler.

### Application Error Handling

The following error handling strategies are used in the Device and Monitor clients:

* Connection start and registration are combined such that they both need to succeed before proceding. This is implemented with a retry strategy on failures that uses an incrementing backof time before each retry.
* Connection closed events re-start the connection and re-register the Client.

For the Server:

* Client disconnects are logged but otherwise ignored.
* Client registrations with a duplicate ```DeviceId``` are accepted at face value.
* Transmission failure exceptions are caught and logged, but otherwise ignored.
