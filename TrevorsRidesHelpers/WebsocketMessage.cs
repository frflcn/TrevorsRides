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
        public object Message { get; set; }

        public WebsocketMessage(MessageType messageType, object message)
        {
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
        DriverUpdate
    }
}
