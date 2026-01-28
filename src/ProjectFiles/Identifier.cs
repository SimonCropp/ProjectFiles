public static class Identifier
{
    public static string Build(string name)
    {
        if (name.Length == 0)
        {
            return "_";
        }

        var builder = new StringBuilder(name.Length + 1);
        var first = name[0];
        if (char.IsLetter(first) || first == '_')
        {
            builder.Append(first);
        }
        else
        {
            builder.Append('_');
            if (char.IsDigit(first))
            {
                builder.Append(first);
            }
        }

        for (var index = 1; index < name.Length; index++)
        {
            var ch = name[index];
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
            }
            else
            {
                builder.Append('_');
            }
        }

        return KeywordDetect.Sanitize(builder);
    }
}