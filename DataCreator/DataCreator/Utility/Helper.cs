using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DataCreator.Utility
{
  /***********************************************************************************************
   * Helper / 2014-07-22 / Wethospu                                                              * 
   *                                                                                             * 
   * Class with generic helper functions. These should be usable in other projects too.          *
   * For more GW2 specific helper functions, check GW2Helper.cs.                                 * 
   *                                                                                             * 
   ***********************************************************************************************/
  public static class Helper
  {
    /***********************************************************************************************
     * IsValidURL / 2014-07-22 / Wethospu                                                          * 
     *                                                                                             * 
     * Checks that given url exists and can be connected to.                                       *
     * Slow operation so shouldn't be called by default.                                           *
     *                                                                                             * 
     * Returns success.                                                                            *
     * url: Url to check.                                                                          *
     *                                                                                             * 
     ***********************************************************************************************/

    static public bool IsValidUrl(string url)
    {
      // Check whether validating is disabled.
      if (!Constants.ValidateUrls)
        return true;
      var row = Console.CursorTop;
      Console.Write("Validating " + url);
      HttpWebResponse webResponse = null;
      try
      {
        // Generate web request.
        var webRequest = WebRequest.Create(url) as HttpWebRequest;
        if (webRequest != null)
        {
          webRequest.Timeout = 2000;
          webRequest.Method = "HEAD";
          // Connect.
          webResponse = webRequest.GetResponse() as HttpWebResponse;
        }
        Helper.ClearConsoleLine(row);
        // Get response.
        if (webResponse != null)
        {
          var statusCode = (int)webResponse.StatusCode;
          // Close connection (important!).
          webResponse.Close();
          // Check response validity.
          if (statusCode >= 100 && statusCode < 400)
          {
            // Good request.
            return true;
          }
          if (statusCode >= 500 && statusCode <= 510)
          {
            // Server error.
            return false;
          }
        }
      }
      catch
      {
        Helper.ClearConsoleLine(row);
        // Remember to close connection.
        if (webResponse != null)
          webResponse.Close();
        return false;
      }
      return true;
    }

    /***********************************************************************************************
     * AmountOf / 2014-07-22 / Wethospu                                                            * 
     *                                                                                             * 
     * Counts given characters in a string.                                                        *
     *                                                                                             * 
     * Returns count.                                                                              *
     * str: String to check.                                                                       *
     * toFind: Character to find.                                                                  *
     *                                                                                             * 
     ***********************************************************************************************/

    static public int AmountOf(string str, char toFind)
    {
      return str.Count(ch => ch == toFind);
    }

    /***********************************************************************************************
     * ToUpper / 2014-07-22 / Wethospu                                                             * 
     *                                                                                             * 
     * Changes first character to uppercase.                                                       *
     * Doesn't check for filler words or ignorable characters.                                     *
     *                                                                                             * 
     * Returns modified string.                                                                    *
     * str: String to edit.                                                                        *
     *                                                                                             * 
     ***********************************************************************************************/
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

    static public string ToUpperAll(string str)
    {
      if (str.Length == 0)
        return str;
      // Split string to words.
      var split = str.Split(' ');
      var builder = new StringBuilder();
      foreach (var word in split)
      {
        // Check if word should be ignored.
        if (word.Length == 0 || word.Equals("of") || word.Equals("the") || word.Equals("to") || word.Equals("and")
             || word.Equals("or"))
        {
          builder.Append(word);
          builder.Append(" ");
          continue;
        }
        // Check special cases. / 2015-09-22 / Wethospu
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
        // Check ignorable characters.
        var startIndex = 0;
        for (; startIndex < word.Length; startIndex++)
        {
          if (word[startIndex] == '(' || word[startIndex] == '"' || word[startIndex] == '[')
          {
            // Insert ignorable characters back without any changes.
            builder.Append(word[startIndex]);
          }
          else
            break;
        }
        // startIndex points to start of word so added it uppercase and then rest of the word normally.
        builder.Append(word[startIndex].ToString().ToUpper()).Append(word.Substring(startIndex + 1));
        builder.Append(" ");
      }
      // Remove extra space at end.
      builder.Remove(builder.Length - 1, 1);
      return builder.ToString();
    }

    /***********************************************************************************************
     * ReplaceFirst / 2014-07-22 / Wethospu                                                        * 
     *                                                                                             * 
     * Replaces first found string with another.                                                   *
     * No changes if string is not found.                                                          *
     *                                                                                             * 
     * Returns modified string.                                                                    *
     * str: String to edit.                                                                        *
     * toReplace: String to find and get replaced.                                                 *
     * replacement: String which replaces above.                                                   *
     *                                                                                             * 
     ***********************************************************************************************/

    static public string ReplaceFirst(string str, string toReplace, string replacement)
    {
      // Find the string.
      var pos = str.IndexOf(toReplace, StringComparison.Ordinal);
      if (pos < 0)
        return str;
      // Replace.
      return str.Substring(0, pos) + replacement + str.Substring(pos + toReplace.Length);
    }

    // Currently processed line number, line and file. These are needed for ShowWarning to show a proper generic message.
    private static int _lineNumber;
    private static string _line = "";
    public static string CurrentFile = "";
    // Amount of warnings during one run.
    public static int WarningCount;

    /***********************************************************************************************
     * InitializeWarningSystem / 2014-07-22 / Wethospu                                             * 
     *                                                                                             * 
     * Sets line number and line number for ShowWarning.                                           *
     * If you use ShowWarning this should be called frequently.                                    *
     *                                                                                             * 
     * lineNumber: Number of the currently processed line in currently opened file.                *
     * line: Currently processed line.                                                             *
     *                                                                                             * 
     ***********************************************************************************************/

    static public void InitializeWarningSystem(int lineNumber, string line)
    {
      _lineNumber = lineNumber;
      _line = line;
    }

    /***********************************************************************************************
     * ShowWarning / 2014-07-22 / Wethospu                                                         * 
     *                                                                                             * 
     * Prints a warning message based on generic base message.                                     *
     * This required correct line number, current file and line to work properly. Use              *
     * WarningCountMessage below otherwise.                                                        *
     *                                                                                             * 
     * message: Optional message to add after generic warning message.                             *
     *                                                                                             * 
     ***********************************************************************************************/

    static public void ShowWarning(string message = "")
    {
      // Increase warning counters.
      WarningCount++;
      if (_lineNumber < 0)
        Console.Error.Write("Critical program error.");
      // Show message.
      else if (message.Equals(""))
        Console.Error.WriteLine("(" + _lineNumber + ") " + CurrentFile + ": Warning in line \"" + _line + "\"");
      else
        Console.Error.WriteLine("(" + _lineNumber + ") " + CurrentFile + ": Warning in line \"" + _line + "\":\n" + message);
      Console.Error.WriteLine("");
    }

    /***********************************************************************************************
     * ShowWarning / 2014-07-22 / Wethospu                                                         * 
     *                                                                                             * 
     * Prints a warning message.                                                                   *
     * This doesn't need correct line number, current file and line. However, if these exist use   *
     * ShowWarning above instead.                                                                  *
     *                                                                                             *
     * message: Message to show.                                                                   *
     *                                                                                             * 
     ***********************************************************************************************/

    static public void ShowWarningMessage(string message)
    {
      // Increase warning counters.
      WarningCount++;
      // Show message.
      Console.Error.WriteLine(message);
      Console.Error.WriteLine("");
    }

    /***********************************************************************************************
     * WarningCountMessage / 2014-07-22 / Wethospu                                                 * 
     *                                                                                             * 
     * Generates message about amount of warnings. Also shows amounts for each language.           *
     * Returns generated string.                                                                   *
     *                                                                                             * 
     ***********************************************************************************************/

    static public string WarningCountMessage()
    {
      // Don't show anything without any warnings.
      if (WarningCount == 0)
        return "";
      var message = new StringBuilder();
      message.Append(" with ").Append(WarningCount).Append(" warnings");
      return message.ToString();
    }

    /***********************************************************************************************
     * LastIndexOf / 2014-07-22 / Wethospu                                                         * 
     *                                                                                             * 
     * Finds first found character (from characters) when iterating string backwards from          *
     * startIndex.                                                                                 *
     * Counters pairs of brackets automatically If ')' is found then next '(' is ignored.          *
     *                                                                                             * 
     * Returns index of first found character. Returns -1 if nothing was found.                    *
	 * Return value guaranteed to be from -1 to startIndex - 1.                                    *
     * str: String to search from.                                                                 *
     * characters: Characters to look for.                                                         *
     * startIndex: Index of starting point. Right side of searchable first character.              *
     *                                                                                             * 
     * Example: "goldfish" with startIndex of 4 puts iterator to "gold|fish" so characther 'h'     *
     * will be searched first.                                                                     *
     *                                                                                             * 
     ***********************************************************************************************/

    static public int LastIndexOf(string str, char[] characters, int startIndex)
    {
	    if (startIndex < 0 || startIndex >= str.Length)
      {
        ShowWarningMessage("Critical program error.");
        return -1;
      }
      // Check whether to enable pairbracket search.
      var lookForBracketPairs = characters.Contains('(');
      var lookForSquareBracketPairs = characters.Contains('[');
      var openBrackets = 0;

      startIndex--;
      for (; startIndex >= 0; startIndex--)
      {
        if (characters.Contains(str[startIndex]))
        {
          // Returns if no brackets are open.
          if (openBrackets == 0)
            return startIndex;

          // Check whether a bracket was closed.
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
      // Returns start of string to indicate nothing was found.
      return startIndex;
    }

    /***********************************************************************************************
     * LastIndexOf / 2014-07-22 / Wethospu                                                         * 
     *                                                                                             * 
     * Overload which automatically starts at string end.                                          *
     *                                                                                             * 
     ***********************************************************************************************/

    static public int LastIndexOf(string str, char[] characters)
    {
      return LastIndexOf(str, characters, str.Length);
    }

    /***********************************************************************************************
    * FirstIndexOf / 2014-07-22 / Wethospu                                                        * 
    *                                                                                             * 
    * Finds first found character (from characters) when iterating string forward from            *
    * startIndex.                                                                                 *
    * Counters pairs of brackets automatically If '(' is found then next ')' is ignored.          *
    *                                                                                             * 
    * Returns index of first found character.                                                     *                                                                                            
    * str: String to search from.                                                                 *
    * characters: Characters to look for.                                                         *
    * startIndex: Index of starting point. Points to first searchable character.                  *
    *                                                                                             * 
    ***********************************************************************************************/

    static public int FirstIndexOf(string str, char[] characters, int startIndex)
    {
      // Check whether to enable pairbracket search.
      var lookForBracketPairs = characters.Contains(')');
      var lookForSquareBracketPairs = characters.Contains(']');
      var openBrackets = 0;

      for (; startIndex < str.Length; startIndex++)
      {
        if (characters.Contains(str[startIndex]))
        {
          // Returns if no brackets are open.
          if (openBrackets == 0)
            return startIndex;

          // Check whether a bracket was closed.
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
      // Returns end of string to indicate nothing was found.
      return startIndex;
    }

    /***********************************************************************************************
     * FirstIndexOf / 2014-07-22 / Wethospu                                                        * 
     *                                                                                             * 
     * Overload which automatically starts at string start.                                        *
     *                                                                                             * 
     ***********************************************************************************************/

    static public int FirstIndexOf(string str, char[] characters)
    {
      return FirstIndexOf(str, characters, 0);
    }

    /***********************************************************************************************
     * ConvertSpecial / 2014-07-22 / Wethospu                                                      * 
     *                                                                                             * 
     * Converts special characters to html variants so that they show up on page properly.         *
     * This should be called at end to make program more debuggable.                               *
     *                                                                                             *  
     * Returns converted string.                                                                   *
     * str: String to edit.                                                                        *
     *                                                                                             * 
     * Based on SpecialCharacters.txt                                                              *
     *                                                                                             * 
     ***********************************************************************************************/

    static public string ConvertSpecial(string str)
    {
      // Check that no already converted characters exists.
      // Can be caused by either original data using already converted characters (reduces readability) or programming working inccorrectly (this function already called).
      foreach (var value in Constants.CharacterConversions.Values)
      {
        if (str.Contains(value))
          ShowWarningMessage("Line " + str + "has an already converted character " + value + ". Change to the original one and update 'SpecialCharacters.txt' if needed.");
      }
      foreach (var key in Constants.CharacterConversions.Keys)
        str = str.Replace(key, Constants.CharacterConversions[key]);
      return str;
    }


    /***********************************************************************************************
     * Simplify / 2014-07-22 / Wethospu                                                            * 
     *                                                                                             * 
     * Converts special characters to their simplified counterparts. For example é -> e.           *
     * Also removes some characters such as '(' and '[' and puts string lowercase.                 *
     * These are needed for links, etc. to prevent javascript havinmg issue.                       *
     *                                                                                             *
     * Returns converted string.                                                                   *
     * str: String to edit.                                                                        *
     *                                                                                             * 
     * Based on SpecialCharacters.txt                                                              *
     *                                                                                             * 
     ***********************************************************************************************/

    static public string Simplify(string str)
    {
      return ReplaceSpecial(str).Replace(' ', '_').Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").ToLower();
    }

    private static string ReplaceSpecial(string str)
    {
      return Constants.CharacterSimplifications.Keys.Aggregate(str, (current, key) => current.Replace(key, Constants.CharacterSimplifications[key]));
    }


    /***********************************************************************************************
     * IsInteger / 2015-05-07 / Wethospu                                                           * 
     *                                                                                             * 
     * Returns is string an interger.                                                              *
     *                                                                                             * 
     ***********************************************************************************************/

    static public bool IsInteger(string str)
    {
      int result;
      return int.TryParse(str, out result);
    }

    /***********************************************************************************************
     * ClearLine / 2015-05-25 / Wethospu                                                           * 
     *                                                                                             * 
     * Clears console up to given row.                                                             *
     *                                                                                             * 
     ***********************************************************************************************/
    static public void ClearConsoleLine(int targetRow)
    {
      int currentPosition = Console.CursorTop;
      // Checck that target is above.
      if (currentPosition < targetRow)
        return;
      // Clear rows until the target is met.
      for (; targetRow <= currentPosition; currentPosition--)
      {
        Console.SetCursorPosition(0, currentPosition);
        Console.Write(new string(' ', Console.WindowWidth));
      }
      Console.SetCursorPosition(0, targetRow);
    }

    /***********************************************************************************************
     * RemoveBetween / 2015-05-25 / Wethospu                                                       * 
     *                                                                                             * 
     * Removes characters from a given string between delimiters.                                  *
     *                                                                                             * 
     ***********************************************************************************************/
    static public string RemoveBetween(string s, string begin, string end)
    {
      Regex regex = new Regex(begin + ".*" + end);
      return regex.Replace(s, begin + end);
    }

    /***********************************************************************************************
    * ParseD / 2015-09-13 / Wethospu                                                               * 
    *                                                                                              * 
    * Shortens string -> double conversion.                                                        *
    *                                                                                              * 
    ***********************************************************************************************/
    static public double ParseD(string str)
    {
      try
      {
        return double.Parse(str);
      }
      catch (FormatException)
      {
        if (_lineNumber > 0)
          Helper.ShowWarning("Value " + str + " is not a number.");
        else
          Helper.ShowWarningMessage("Value " + str + " is not a number.");
      }
      return 0.0;
    }

    /***********************************************************************************************
    * ParseI / 2015-09-13 / Wethospu                                                               * 
    *                                                                                              * 
    * Shortens string -> integer conversion.                                                       *
    *                                                                                              * 
    ***********************************************************************************************/
    static public int ParseI(string str)
    {
      try
      {
        return int.Parse(str);
      }
      catch (FormatException)
      {
        if (_lineNumber > 0)
          Helper.ShowWarning("Value " + str + " is not an integer.");
        else
          Helper.ShowWarningMessage("Value " + str + " is not an integer.");
      }
      return 0;
    }

    /***********************************************************************************************
    * CloneJson / 2015-10-05 / Wethospu                                                            * 
    *                                                                                              * 
    * Deep copies an object.                                                                       *
    *                                                                                              * 
    ***********************************************************************************************/
    public static T CloneJson<T>(this T source)
    {
      // Don't serialize a null object, simply return the default for that object
      if (ReferenceEquals(source, null))
      {
        return default(T);
      }

      return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));
    }
  }
}
