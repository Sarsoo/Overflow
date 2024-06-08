using System.Text.Json.Serialization;
using Overflow.SouthernWater;

namespace Overflow;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PagedItems<Spill>))]
public partial class JsonSerialiser : JsonSerializerContext
{

}