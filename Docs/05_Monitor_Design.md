# 5 Monitor Design

Monitors execute as a Blazer Web Assembly (WASM) Single Page Application (SPA) in a browser tab.

### Connection Management

As the SignalR startup handling is very similar to that of a Device, it is essentially implemented in the same way but in a totally different context. 
After initialization the Monitor becomes passive with respect to the SignalR protocol and only receives ```DeviceStatus```, ```DeviceStatusList``` and ```Heartbeat``` updates, which it renders into its web page. 

To ensure that the Monitor keeps connected, and thus keeps receiving updates from the Server, the protocol handling is implemented in the App component, which wraps all of the App's pages and their Blazor components.
This allows the user to navigate away from the main status page to other pages in the App and then return to the Monitor page with all data available and fresh. 
It also paves the way for a potential summary component and pages with command actions to send to the Server.

It also means that the initialization code for the connection should be delayed until after the App renders, as it would otherwise further delay the App's initialization, leaving the user staring at a "Loading ..." page even longer (or forever if connection startup fails).
Thus the starting and registering is done in the ```App.OnAfterRender``` callback, and only when ```firstRender``` is ```true```.

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender){    if (firstRender)    {        bool operational = await StartConnectionAndRegisterAsync();        if (!operational)        {            // Failed to connect to and register on Hub
        }    }
}```

The actual behaviour of the connection handling is slightly different from that of the Device in the following ways:

1. There is no Ctrl-C handling; the way to exit is to close the browser tab, which will close and cleanup the connection.
2. When the Server closes the connection no reconnection attempt is made, but it will be shown to the user.

### UI

In order to make the UI a bit more usefull and interesting, the Monitor page recalculates every second whether any Devices are *late* (```status.LastSeenAt``` between 16 and 30 seconds ago) or *overdue* (more than 30 seconds ago).
On-time times are rendered in green, late in orange, and overdue in red.
Furthermore the current time is displayed.

**ToDo:** Screenshot with multiple devices


### Status Store

The Monitor keeps a local copy of the ```DeviceStatusStore``` which is initialised by the Server after registration and updated upon Device registrations and Device Hearbeat update messages. The store will be cleared when the HubConnection is lost, as the information would otherwise be stale and unreliable.

To pass the ```DeviceStatusStore``` to other components, a Blazor ```Cascading Value``` is used. An alternative solution would be to use a service to share the state between Blazor components. 

Another Cascading Value is used by the App component to share a connection status string. This string value is displayed in the ```Footer``` component of the Monitor App.
