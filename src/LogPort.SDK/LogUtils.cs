namespace LogPort.SDK;

internal static class LogUtils
{
    public static IReadOnlyList<string> ParseTemplateKeys(string template)
    {
        var names = new List<string>();

        for (int i = 0; i < template.Length; i++)
        {
            if (template[i] != '{') continue;

            int start = i + 1;
            int end = template.IndexOf('}', start);
            if (end < 0) break;

            var name = template.Substring(start, end - start);
            if (!string.IsNullOrWhiteSpace(name))
                names.Add(name);

            i = end;
        }

        return names;
    }
}
