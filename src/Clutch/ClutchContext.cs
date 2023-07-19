using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Clutch.Building;
using Clutch.Configuration;
using Clutch.Configuration.Issues;

namespace Clutch
{
    public class ClutchContext
    {
        private readonly Dictionary<Type, EntityTypeData> _entityData;
        private readonly ProxyAssemblyBuilder.SerializerDelegate _serializer;
        private readonly IProxyFactory _factory;
        private readonly JsonEncodedText _discriminator;
        private static readonly UTF8Encoding s_utf8Encoding = new UTF8Encoding(false, true);

        public ClutchContext(ContextData config, ProxyAssemblyContext assemblyContext)
        {
            _entityData = assemblyContext.EntityDatas.ToDictionary(s => s.Type);
            _serializer = (ProxyAssemblyBuilder.SerializerDelegate)assemblyContext.Serializer.UntypedDelegate;
            _factory = assemblyContext.ProxyFactory;
            _discriminator = JsonEncodedText.Encode(ProxyBuilderConstants.DiscriminatorPropertyName);
        }

        public T Create<T>()
        {
            return _factory.Create<T>();
        }

        public Func<T> GetFactoryMethod<T>()
        {
            if (_entityData.TryGetValue(typeof(T), out var config))
                return () => (T)config.Creator();

            ProxyHelperFunctions.ThrowUnknownType(typeof(T));
            return null;
        }

        public static ClutchContext CreateContext(IContextConfigurator configurator, out IEnumerable<IIssue> issues)
        {
            var builder = new ClutchContextBuilder();
            configurator.Configure(builder);
            return builder.Build(out issues);
        }

        public static ClutchContext CreateContext(Action<ClutchContextBuilder> build, out IEnumerable<IIssue> issues)
        {
            var builder = new ClutchContextBuilder();
            build(builder);
            return builder.Build(out issues);
        }

        public string Serialize<T>(T[] array)
        {
            var bufferWriter = new ArrayBufferWriter<byte>(1024);
            var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { SkipValidation = false });
            writer.WriteStartArray();

            foreach(var obj in array)
            {
                writer.WriteStartObject();

                IUtf8JsonSerializable provider = (IUtf8JsonSerializable)obj;
                provider.Serialize(writer);

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.Flush();

            return ReadOnlySpanToString(bufferWriter.WrittenSpan);
        }

        private static unsafe string ReadOnlySpanToString(ReadOnlySpan<byte> utf8Unescaped)
        {
            try
            {
                if (utf8Unescaped.IsEmpty)
                    return string.Empty;
                fixed (byte* bytes = &utf8Unescaped.GetPinnableReference())
                    return s_utf8Encoding.GetString(bytes, utf8Unescaped.Length);
            }
            catch (DecoderFallbackException e)
            {
                throw new ClutchRuntimeException("Unable to decode Utf8 bytes to string. This should never happen... Contact Clutch authors", e);
            }
        }

        public T[] Deserialize<T>(string json)
        {
            // TODO: check weather chunking is possible
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            var readerState = new JsonReaderState(new JsonReaderOptions
                                                  {
                                                      AllowTrailingCommas = true,
                                                      CommentHandling = JsonCommentHandling.Skip,
                                                      MaxDepth = 64
                                                  });
            var reader = new Utf8JsonReader(jsonBytes, true, readerState);
            try
            {
                var result = DeserializeList(ref reader);

                var finalState = reader.CurrentState;
                if (reader.BytesConsumed != jsonBytes.Length)
                    throw new ClutchRuntimeException($"Not all bytes are consumed during deserialization ({reader.BytesConsumed} out of {jsonBytes.Length}). Looks like there is something else at the end");

                return result.OfType<T>().ToArray();
            }
            catch (JsonException e)
            {
                throw new ClutchRuntimeException($"Json(Reader)Exception occured: '{e.Message}'", e);
            }
            catch (FormatException e)
            {
                throw new ClutchRuntimeException($"FormatException occured: '{e.Message}'" + GetReaderDetails(ref reader), e);
            }
            catch (ClutchRuntimeException e)
            {
                throw new ClutchRuntimeException(e.Message + GetReaderDetails(ref reader), e);
            }
        }

        private static string GetReaderDetails(ref Utf8JsonReader r) => $" (at byte position: {r.BytesConsumed})."; // TODO: wait while line number/position is available

        private List<object> DeserializeList(ref Utf8JsonReader reader)
        {
            List<object> result = new List<object>();
            reader.Read();
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new ClutchRuntimeException("Start of array '[' is expected");
            while(reader.Read())
            {
                JsonTokenType tokenType = reader.TokenType;
                if (tokenType == JsonTokenType.EndArray)
                    break;

                if (tokenType != JsonTokenType.StartObject)
                    throw new ClutchRuntimeException("Start of object '{' is expected");

                reader.Read();
                tokenType = reader.TokenType;
                if (tokenType != JsonTokenType.PropertyName) //TODO: absence of Discriminator (or int discriminator)
                    throw new ClutchRuntimeException("Discriminator (property name) is expected");

                if (reader.ValueTextEquals(_discriminator.EncodedUtf8Bytes))
                {
                    // fast reading
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.String) 
                        throw new ClutchRuntimeException("String value for discriminator (property name) is expected");

                    var val = _serializer(ref reader);
                    result.Add(val);
                }
                else
                    throw new ClutchRuntimeException($"Discriminator ('{ProxyBuilderConstants.DiscriminatorPropertyName}') should by the first property in an object");
            }

            return result;
        }
    }
}
