
using System.IO;
using System.Text.Json;
using Clutch.Building.ProxySupport;

namespace Clutch.Building
{
    static class PropertyTypeHandlerContextType
    {
        private static readonly JsonEncodedText s_discriminator = JsonEncodedText.Encode(ProxyBuilderConstants.DiscriminatorPropertyName);

        private static void WriteObject(Utf8JsonWriter writer, JsonEncodedText property, object obj)
        {
            writer.WriteStartObject(property);

            IUtf8JsonSerializable provider = (IUtf8JsonSerializable)obj;
            provider.Serialize(writer);

            writer.WriteEndObject();
        }

        public static void ReadObject(ref Utf8JsonReader reader)
        {
            JsonTokenType tokenType = reader.TokenType;

            if (tokenType != JsonTokenType.StartObject)
                throw new ClutchRuntimeException("Start of object '{' is expected");

            reader.Read();
            tokenType = reader.TokenType;
            if (tokenType != JsonTokenType.PropertyName) //TODO: absence of Discriminator (or int discriminator)
                throw new ClutchRuntimeException("Discriminator (property name) is expected");

            if (reader.ValueTextEquals(s_discriminator.EncodedUtf8Bytes))
            {
                // fast reading
                reader.Read();
                if (reader.TokenType != JsonTokenType.String)
                    throw new ClutchRuntimeException("String value for discriminator (property name) is expected");
            }
            else
                throw new ClutchRuntimeException($"Discriminator ('{ProxyBuilderConstants.DiscriminatorPropertyName}') should by the first property in an object");
        }
    }
}
