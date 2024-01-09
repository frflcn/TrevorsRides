using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using PhoneNumbers;
using System.Globalization;

namespace TrevorsRidesHelpers
{
    public class Json
    {
        public static JsonSerializerOptions Options { get; set; }
        public static PhoneNumberUtil util = PhoneNumberUtil.GetInstance();
        static Json()
        {
            Options = new JsonSerializerOptions()
            {
                Converters =
                {
                    new Json.PhoneNumberJsonConverter()
                }

            };
        }
        public class PhoneNumberJsonConverter : JsonConverter<PhoneNumber>
        {
            public override PhoneNumber Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                StringToPhoneNumber(reader.GetString()!);

            public override void Write(
                Utf8JsonWriter writer,
                PhoneNumber phoneNumber,
                JsonSerializerOptions options) =>
                    writer.WriteStringValue($"{phoneNumber.NationalNumber} - {util.GetRegionCodeForCountryCode(phoneNumber.CountryCode)}");
        }
        public static PhoneNumber StringToPhoneNumber(string phoneNumber)
        {
            string[] values = phoneNumber.Split(" - ");
            
            return util.Parse(values[0], values[1]);
            
        }
    }
}
