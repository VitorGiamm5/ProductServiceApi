using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProductServiceApp.Api.Conveters;

public class CustomDateTimeConverter(string dateFormat) : JsonConverter<DateTime?>
{
    private readonly string _dateFormat = dateFormat;

    public override void Write(Utf8JsonWriter writer, DateTime? date, JsonSerializerOptions options)
    {
        if (date != null)
        {
            writer.WriteStringValue(date.Value.ToUniversalTime().ToString(_dateFormat));
        }
    }

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var readerString = reader.GetString();

        if (string.IsNullOrEmpty(readerString))
        {
            return null;
        }

        var date = DateTime.Parse(readerString);

        if (date.Date == DateTime.MinValue)
        {
            return null;
        }

        var dateUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

        return dateUtc;
    }

}
