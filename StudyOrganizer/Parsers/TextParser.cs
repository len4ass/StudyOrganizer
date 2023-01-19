namespace StudyOrganizer.Parsers;

public static class TextParser
{
    public static IList<string> ParseMessageToCommand(string message)
    {
        message = message.Trim();
        if (!message.StartsWith('/'))
        {
            return Enumerable.Empty<string>().ToList();
        }

        message = message.TrimStart('/');
        var entries = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var atIndex = entries[0].IndexOf('@');
        if (atIndex >= 0)
        {
            entries[0] = entries[0][..atIndex];
        }
        
        return entries;
    } 
}