using System.Text.RegularExpressions;
using Raylib_cs;

namespace JEngine.util;

public static class FormattedStringExtension {
    public static FormattedString Coloured(this string str, Color col) {
        return new FormattedString(str, "c", col.ToInt().ToString());
    }
}

public partial class FormattedString {
    internal string taggedString;

    // TODO: Check regex perf and cache stripped tag result
    [GeneratedRegex("<[^>]+>")]
    private static partial Regex StripTagRegex();
    public string StripTags => StripTagRegex().Replace(taggedString, "");
    
    public int Length => StripTags.Length;
    
    public FormattedString() {
        taggedString = "";
    }
    public FormattedString(string rawString, string tag = "s", string val = "") {
        this.taggedString = Tag(rawString, tag, val);
    }

    private static string Tag(string str, string tag, string val = "") {
        if (val.NullOrEmpty())
            return $"<{tag}>{str}</{tag}>";
        
        return $"<{tag}={val}>{str}</{tag}>";
    }

    public IEnumerable<FormattedString> Split(char separator) {
        var split = taggedString.Split(separator);
        foreach (var str in split) {
            var formatted = new FormattedString();
            formatted.taggedString = str;
            yield return formatted;
        }
    }

    public FormattedString Trim() {
        var formatted = new FormattedString();
        formatted.taggedString = taggedString.Trim();
        return formatted;
    }

    [GeneratedRegex("<(.*?)(?:=(.*?))?>([\\s\\S]*?)<\\/.*?>")]
    private static partial Regex ResolveTagsRegex();
    public IEnumerable<(string tagName, string attribute, string contents)> Resolve() {
        var matches = ResolveTagsRegex().Matches(taggedString);

        foreach (Match match in matches) {
            yield return (match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
        }
    }

    public static FormattedString operator +(FormattedString a, FormattedString b) {
        return new FormattedString {
            taggedString = a.taggedString + b.taggedString
        };
    }
    public static FormattedString operator +(FormattedString a, string b) {
        return new FormattedString {
            taggedString = a.taggedString + new FormattedString(b).taggedString
        };
    }
    public static FormattedString operator +(string a, FormattedString b) {
        return new FormattedString {
            taggedString = new FormattedString(a).taggedString + b.taggedString
        };
    }

    public override string ToString() {
        return taggedString;
    }

    public static implicit operator FormattedString(string str) => new(str);
}