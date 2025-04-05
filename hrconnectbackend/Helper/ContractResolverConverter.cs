using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Serialization;

namespace hrconnectbackend.Helper;

public class ContractResolverConverter : JsonConverter<IContractResolver>
{
    public override IContractResolver Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Implement logic to convert JSON into a concrete class
        return new DefaultContractResolver(); // Example, adjust based on your actual use case
    }

    public override void Write(Utf8JsonWriter writer, IContractResolver value, JsonSerializerOptions options)
    {
        // Implement logic to serialize the object back into JSON
        JsonSerializer.Serialize(writer, value, options);
    }
}
