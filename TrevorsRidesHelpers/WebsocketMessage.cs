using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TrevorsRidesHelpers
{
    public class WebsocketMessage
    {
        public MessageType MessageType { get; set; }
        public Guid MessageID { get; set; }
        public object Message { get; set; }
        [JsonConstructor]
        public WebsocketMessage(MessageType messageType, object message, Guid messageID)
        {
            MessageType = messageType;
            MessageID = messageID;
            Message = message;
        }
        public WebsocketMessage(MessageType messageType, object message)
        {
            MessageID = Guid.NewGuid();
            Message = message;
            MessageType = messageType;
        }
        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }
    [JsonConverter(typeof(JsonStringEnumConverter<MessageType>))]
    public enum MessageType
    {
        /// <summary>
        /// Holds a DriverStatus object
        /// </summary>
        DriverUpdate,
        RideRequest,
        RideRequestAccepted,
        RideRequestDeclined
    }
}
