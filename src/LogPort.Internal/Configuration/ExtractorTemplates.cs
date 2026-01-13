using System.Collections.Frozen;
using System.Reflection;

namespace LogPort.Internal.Configuration;

internal sealed class ExtractorTemplates
{
    public readonly FrozenDictionary<string, BaseLogEntryExtractorConfig> Templates;

    public ExtractorTemplates()
    {
        Templates = LoadConfiguration();
    }

    private FrozenDictionary<string, BaseLogEntryExtractorConfig> LoadConfiguration()
    {
        var configs = this.GetType()
            .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(f => f.FieldType.IsAssignableTo(typeof(BaseLogEntryExtractorConfig)))
            .ToDictionary(f => f.Name.ToLowerInvariant(), f => (BaseLogEntryExtractorConfig)f.GetValue(null)!);

        return configs.ToFrozenDictionary();
    }

    private static readonly RegexLogEntryExtractorConfig Postgres = new()
    {
        Pattern =
            ".*LOG:\\s+(?<message>.+)$",
        MessageGroup = "message",
        TimestampGroup = "timestamp",
    };
}