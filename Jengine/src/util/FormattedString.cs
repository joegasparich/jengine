using System.Text.RegularExpressions;
using Raylib_cs;

namespace JEngine.util;

public static class FormattedStringExtension {
    public static FormattedString Coloured(this string str, Color col) {
        return new FormattedString(str, "c", col.ToInt().ToString());
    }
}

public partial class FormattedString {
    internal string _taggedString;

    // TODO: Check regex perf and cache stripped tag result
    [GeneratedRegex("<[^>]+>")]
    private static partial Regex StripTagRegex();
    public string StripTags => StripTagRegex().Replace(_taggedString, "");
    
    public int Length => StripTags.Length;
    
    public FormattedString() {
        _taggedString = "";
    }
    public FormattedString(string rawString, string tag = "s", string val = "") {
        _taggedString = Tag(rawString, tag, val);
    }

    private static string Tag(string str, string tag, string val = "") {
        if (val.NullOrEmpty())
            return $"<{tag}>{str}</{tag}>";
        
        return $"<{tag}={val}>{str}</{tag}>";
    }

    public IEnumerable<FormattedString> Split(char separator) {
        var split = _taggedString.Split(separator);
        foreach (var str in split) {
            var formatted = new FormattedString();
            formatted._taggedString = str;
            yield return formatted;
        }
    }

    public FormattedString Trim() {
        var formatted = new FormattedString();
        formatted._taggedString = _taggedString.Trim();
        return formatted;
    }

    [GeneratedRegex("<(.*?)(?:=(.*?))?>([\\s\\S]*?)<\\/.*?>")]
    private static partial Regex ResolveTagsRegex();
    public IEnumerable<(string tagName, string attribute, string contents)> Resolve() {
        var matches = ResolveTagsRegex().Matches(_taggedString);

        foreach (Match match in matches) {
            yield return (match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
        }
    }

    public static FormattedString operator +(FormattedString a, FormattedString b) {
        return new FormattedString {
            _taggedString = a._taggedString + b._taggedString
        };
    }
    public static FormattedString operator +(FormattedString a, string b) {
        return new FormattedString {
            _taggedString = a._taggedString + new FormattedString(b)._taggedString
        };
    }
    public static FormattedString operator +(string a, FormattedString b) {
        return new FormattedString {
            _taggedString = new FormattedString(a)._taggedString + b._taggedString
        };
    }

    public override string ToString() {
        return _taggedString;
    }

    public static implicit operator FormattedString(string str) => new(str);
}