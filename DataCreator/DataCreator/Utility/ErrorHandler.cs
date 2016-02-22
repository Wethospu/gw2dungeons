using System;
using System.Text;

namespace DataCreator.Utility
{
  /// <summary>
  /// Warning/error message generation and printing.
  /// </summary>
  public class ErrorHandler
  {

    // Currently processed line number, line and file.
    // These are needed for ShowWarning to show a proper generic message.
    private static int _lineNumber;
    private static string _line = "";
    public static string CurrentFile = "";
    // Count warnings to give a total amount at the end.
    public static int WarningCounter;

    /// <summary>
    /// Sets up the warning system so it can show correct warning messages.
    /// </summary>
    static public void InitializeWarningSystem(int lineNumber, string lineBeingProcessed)
    {
      _lineNumber = lineNumber;
      _line = lineBeingProcessed;
    }

    /// <summary>
    /// Prints a generic warning message. Additional message can be given.
    /// </summary>
    // Requires correct line number, current file and line to work properly.
    static public void ShowWarning(string additionalMessage = "")
    {
      string message;
      if (_lineNumber < 0)
      {
        message = additionalMessage;
      }
      else
      {
        message = "(" + _lineNumber + ") " + CurrentFile + ": Warning in line \"" + _line + "\"";
        if (additionalMessage.Length > 0)
          message += ":\n" + additionalMessage;
      }
      ShowWarningMessage(message);
    }

    /// <summary>
    /// Prints a given warning message.
    /// </summary>
    static public void ShowWarningMessage(string message)
    {
      WarningCounter++;
      if (message.Length == 0)
        Console.Error.WriteLine("Critical program error.");
      Console.Error.WriteLine(message);
      Console.Error.WriteLine("");
    }

    /// <summary>
    /// Returns a message which says how warnings were counted.
    /// </summary>
    static public string WarningCounterMessage()
    {
      if (WarningCounter == 0)
        return "";
      return " with" + WarningCounter + " warnings";
    }
  }
}
