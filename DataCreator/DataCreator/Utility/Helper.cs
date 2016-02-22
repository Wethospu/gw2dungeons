using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DataCreator.Utility
{
  /// <summary>
  /// General helper functions. These should be usable in other projects too.
  /// </summary>
  // GW2Helper contains GW2 specific functions.
  public static class Helper
  {
    /// <summary>
    /// Returns whether the program manages to connect to a given URL.
    /// </summary>
    // Warning: Slow operation!
    static public bool IsValidUrl(string url)
    {
      var row = Console.CursorTop;
      Console.Write("Validating " + url);
      HttpWebResponse webResponse = null;
      try
      {
        var webRequest = WebRequest.Create(url) as HttpWebRequest;
        if (webRequest != null)
        {
          webRequest.Timeout = 2000;
          webRequest.Method = "HEAD";
          webResponse = webRequest.GetResponse() as HttpWebResponse;
        }
        Helper.ClearConsoleLine(row);
        if (webResponse == null)
          return false;
        var statusCode = (int)webResponse.StatusCode;
        webResponse.Close();
        return (statusCode >= 100 && statusCode < 400);
      }
      catch
      {
        Helper.ClearConsoleLine(row);
        if (webResponse != null)
          webResponse.Close();
      }
      return false;
    }

    /// <summary>
    /// Changes the first character to upper case.
    /// </summary>
    static public string ToUpper(string str)
    {
      if (str.Length > 0)
        return str[0].ToString().ToUpper() + str.Substring(1);
      return str;
    }


    /***********************************************************************************************
     * ToUpperAll / 2014-07-22 / Wethospu                                                          * 
     *                                                                                             * 
     * Changes first character of every word to uppercase.                                         *
     * Fillers word like of, the, or, and or the are ignored.                                      *
     * Characters '(', '*' and '[' are considered as spaces. For example (ball) -> (Ball).         *
     *                                                                                             * 
     * Returns modified string.                                                                    *
     * str: String to edit.                                                                        *
     *                                                                                             * 
     ***********************************************************************************************/

    /// <summary>
    /// Changes the first character of every word to upper case. Ignores filler words.
    /// </summary>
    static public string ToUpperAll(string str)
    {
      if (str.Length == 0)
        return str;
      var words = str.Split(' ');
      var builder = new StringBuilder();
      foreach (var word in words)
      {
        // Ignore filler words because they are commonly kept lower case. For example "Wizard the Migthy".
        if (word.Length == 0 || word.Equals("of") || word.Equals("the") || word.Equals("to") || word.Equals("and")
             || word.Equals("or"))
        {
          builder.Append(word).Append(" ");
          continue;
        }
        // Some special cases.
        if (word.Equals("aoe"))
        {
          builder.Append("AoE ");
          continue;
        }
        if (word.Equals("pbaoe"))
        {
          builder.Append("PBAoE ");
          continue;
        }
        // Ignore some non-alphabet characters.
        var startIndex = 0;
        for (; startIndex < word.Length; startIndex++)
        {
          if (word[startIndex] == '(' || word[startIndex] == '"' || word[startIndex] == '[')
          {
            builder.Append(word[startIndex]);
          }
          else
            break;
        }
        builder.Append(word[startIndex].ToString().ToUpper()).Append(word.Substring(startIndex + 1));
        builder.Append(" ");
      }
      builder.Remove(builder.Length - 1, 1);
      return builder.ToString();
    }

    /// <summary>
    /// Replaces single substring with another, unlike the standard function which replaces every substring.
    /// </summary>
    static public string ReplaceFirst(string str, string toReplace, string replacement)
    {
      var pos = str.IndexOf(toReplace, StringComparison.Ordinal);
      if (pos < 0)
        return str;
      return str.Substring(0, pos) + replacement + str.Substring(pos + toReplace.Length);
    }

    /// <summary>
    /// Returns the index of the first given characters starting from a given index. Returns -1 if nothing is found.
    /// Pairs of brackets are ignored.
    /// </summary>
    /// <param name="startIndex">Right side of the first character to check.</param>
    // Example: "goldfish" with startIndex of 4 puts iterator to "gold|fish".
    static public int LastIndexOf(string str, char[] charactersToFind, int startIndex)
    {
	    if (startIndex < 0 || startIndex >= str.Length)
      {
        ErrorHandler.ShowWarningMessage("Critical program error.");
        return -1;
      }
      // Pairs of brackets are ignored for some reason.
      // TODO: Get rid of this because it doesn't fit the function.
      var lookForBracketPairs = charactersToFind.Contains('(');
      var lookForSquareBracketPairs = charactersToFind.Contains('[');
      var openBrackets = 0;

      startIndex--;
      for (; startIndex >= 0; startIndex--)
      {
        if (charactersToFind.Contains(str[startIndex]))
        {
          if (openBrackets == 0)
            return startIndex;

          if ((lookForBracketPairs && str[startIndex] == '(')
            || (lookForSquareBracketPairs && str[startIndex] == '['))
            openBrackets--;
        }
        else if ((lookForBracketPairs && str[startIndex] == ')')
          || (lookForSquareBracketPairs && str[startIndex] == ']'))
        {
          openBrackets++;
        }
      }
      return startIndex;
    }
    /// <summary>
    /// Overload which automatically starts at end of the string.
    /// </summary>
    static public int LastIndexOf(string str, char[] characters)
    {
      return LastIndexOf(str, characters, str.Length);
    }

    /// <summary>
    /// Returns the index of the first given character starting from a given index. Returns string size if nothing is found.
    /// Pairs of brackets are ignored.
    /// </summary>
    static public int FirstIndexOf(string str, char[] characters, int startIndex)
    {
      // Pairs of brackets are ignored for some reason.
      // TODO: Move it somewhere else.
      var lookForBracketPairs = characters.Contains(')');
      var lookForSquareBracketPairs = characters.Contains(']');
      var openBrackets = 0;

      for (; startIndex < str.Length; startIndex++)
      {
        if (characters.Contains(str[startIndex]))
        {
          if (openBrackets == 0)
            return startIndex;

          if ((lookForBracketPairs && str[startIndex] == ')')
            || (lookForSquareBracketPairs && str[startIndex] == ']'))
            openBrackets--;
        }
        else if ((lookForBracketPairs && str[startIndex] == '(')
          || (lookForSquareBracketPairs && str[startIndex] == '['))
        {
          openBrackets++;
        }
      }
      return startIndex;
    }

    /// <summary>
    /// Overload which automatically starts at start of the string.
    /// </summary>
    static public int FirstIndexOf(string str, char[] characters)
    {
      return FirstIndexOf(str, characters, 0);
    }

    /// <summary>
    /// Converts special characters to html variants so that they show up on page properly.
    /// </summary>
    // This should be called at the end of conversion to make them program more debuggable.
    // Based on SpecialCharacters.txt
    static public string ConvertSpecial(string str)
    {
      // Check that no already converted characters exists.
      // Can be caused by either original data using already converted characters (reduces readability) or programming working inccorrectly (this function already called).
      foreach (var value in Constants.CharacterConversions.Values)
      {
        if (str.Contains(value))
          ErrorHandler.ShowWarningMessage("Line " + str + "has an already converted character " + value + ". Change to the original one and update 'SpecialCharacters.txt' if needed.");
      }
      foreach (var key in Constants.CharacterConversions.Keys)
        str = str.Replace(key, Constants.CharacterConversions[key]);
      return str;
    }


    /// <summary>
    ///  Converts special letters to standard letters, lowers the case and removes some characters.
    /// </summary>
    // This is needed to create valid links and html ids.
    // Based on SpecialCharacters.txt
    static public string Simplify(string str)
    {
      return ReplaceSpecial(str).Replace(' ', '_').Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").ToLower();
    }

    /// <summary>
    /// Converts special letters to standard letters.
    /// </summary>
    private static string ReplaceSpecial(string str)
    {
      return Constants.CharacterSimplifications.Keys.Aggregate(str, (current, key) => current.Replace(key, Constants.CharacterSimplifications[key]));
    }

    /// <summary>
    /// Simpler interface for a common function.
    /// </summary>
    static public bool IsInteger(string str)
    {
      int result;
      return int.TryParse(str, out result);
    }

    /// <summary>
    /// Clears console up to a given row.
    /// </summary>
    static public void ClearConsoleLine(int targetRow)
    {
      int currentPosition = Console.CursorTop;
      if (currentPosition < targetRow)
        return;
      // Clear console by rewriting rows one by one.
      for (; targetRow <= currentPosition; currentPosition--)
      {
        Console.SetCursorPosition(0, currentPosition);
        Console.Write(new string(' ', Console.WindowWidth));
      }
      Console.SetCursorPosition(0, targetRow);
    }

    /// <summary>
    /// Removes characters from a given string between delimiters.
    /// </summary>
    static public string RemoveBetween(string s, string begin, string end)
    {
      Regex regex = new Regex(begin + ".*" + end);
      return regex.Replace(s, begin + end);
    }

    /// <summary>
    /// Double parsing with an error check.
    /// </summary>
    static public double ParseD(string str)
    {
      try
      {
        return double.Parse(str);
      }
      catch (FormatException)
      {
        ErrorHandler.ShowWarning("Value " + str + " is not a number.");
      }
      return 0.0;
    }

    /// <summary>
    /// Int parsing with an error check.
    /// </summary>
    static public int ParseI(string str)
    {
      try
      {
        return int.Parse(str);
      }
      catch (FormatException)
      {
        ErrorHandler.ShowWarning("Value " + str + " is not an integer.");
      }
      return 0;
    }

    /// <summary>
    /// Json deep copy.
    /// </summary>
    public static T CloneJson<T>(this T source)
    {
      // Null can't be serialized so just return default constructor.
      if (ReferenceEquals(source, null))
        return default(T);

      return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));
    }
  }
}
