//
// Copyright (c) 2021 Hugh Maaskant
// MIT License
//

using System;
using System.Text;

using IoTAS.Shared.Hubs;

namespace IoTAS.Server.InputQueue
{
    /// <summary>
    /// The Request record that gets queued on input from (i.e. call to) a Hub method
    /// </summary>
    /// <remarks>
    /// The Request record contains any and all information needed to process the request
    /// </remarks>
    public record Request(DateTime ReceivedAt, string ConnectionId, BaseHubInDto ReceivedDto)
    {
        private static readonly string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Create a Request from an incoming Hub call
        /// </summary>
        /// <param name="connectionId">The incoming connectionId</param>
        /// <param name="receivedDto">The incoming hub DTO</param>
        /// <returns>A Request record</returns>
        public static Request FromClientCall(string connectionId, BaseHubInDto receivedDto)
        {
            return new Request(DateTime.Now, connectionId, receivedDto);
        }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append(nameof(Request));
            sb.Append(" { ");
            sb.Append(nameof(ReceivedAt));
            sb.Append(" = ");
            sb.Append(ReceivedAt.ToString(dateTimeFormat));
            sb.Append(", ");
            sb.Append(nameof(ConnectionId));
            sb.Append(" = ");
            sb.Append(ConnectionId);
            sb.Append(", ");
            sb.Append(nameof(ReceivedDto));
            sb.Append(" = ");
            sb.Append(ReceivedDto.ToString());
            sb.Append(" } ");

            return sb.ToString();
        }

    }  
}
