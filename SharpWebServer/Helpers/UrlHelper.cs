using System.Text.RegularExpressions;

namespace SharpWebServer.Helpers;

public static class UrlHelper
{
    public static Dictionary<string, string> GetRouteDic(string originalUrl, string matchUrl)
    {
        var result = new Dictionary<string, string>();
        var match = Regex.Match(matchUrl, "\\{(\\w+)\\}");
        var flag = false;

        while (match.Success)
        {
            var lastIndexOfChar = originalUrl.LastIndexOf('/');
            var nextCharIndex = originalUrl.IndexOf('/', match.Index);

            if (nextCharIndex == -1)
            {
                result.Add(match.Value[1..^1], originalUrl[match.Index..]);
                break;
            }

            if (nextCharIndex == lastIndexOfChar && flag)
            {
                result.Add(match.Value[1..^1], originalUrl[(nextCharIndex + 1)..]);
                break;
            }

            result.Add(match.Value[1..^1], originalUrl.Substring(match.Index, nextCharIndex - match.Index));

            originalUrl = originalUrl[originalUrl.IndexOf('/', match.Index)..];
            matchUrl = matchUrl[(match.Index + match.Length + 1)..];
            match = Regex.Match(matchUrl, "\\{(\\w+)\\}");

            if (nextCharIndex == lastIndexOfChar)
                flag = true;
        }

        return result;
    }
}