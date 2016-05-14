using System;
using System.Collections.Generic;
using System.Text;

namespace DataCreator.Utility
{
  /// <summary>
  /// Warning/error message generation and printing.
  /// </summary>
  class ErrorHandler
  {

    // Currently processed line number, line and file.
    // These are needed for ShowWarning to show a proper generic message.
    private static int _lineNumber;
    private static string _line = "";
    public static string CurrentFile = "";
    // Count warnings to give a total amount at the end.
    public static int WarningCounter;
    /// <summary>
    /// Store shown messages to avoid repeating them.
    /// </summary>
    private static HashSet<string> _shownMessages = new HashSet<string>();

    /// <summary>
    /// Resets the warning system to the initial state.
    /// </summary>
    public static void Clear()
    {
      _shownMessages.Clear();
      WarningCounter = 0;
    }

    /// <summary>
    /// Sets up the warning system so it can show correct warning messages.
    /// </summary>
    public static void InitializeWarningSystem(int lineNumber, string lineBeingProcessed)
    {
      _lineNumber = lineNumber;
      _line = lineBeingProcessed;
    }

    /// <summary>
    /// Prints a generic warning message. Additional message can be given.
    /// </summary>
    // Requires correct line number, current file and line to work properly.
    public static void ShowWarning(string additionalMessage = "")
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
    public static void ShowWarningMessage(string message)
    {
      if (message.Length == 0)
        Console.Error.WriteLine("Critical program error.");
      if (_shownMessages.Contains(message))
        return;
      _shownMessages.Add(message);
      WarningCounter++;
      Console.Error.WriteLine(message);
      Console.Error.WriteLine("");
    }

    /// <summary>
    /// Returns a message which says how warnings were counted.
    /// </summary>
    public static string WarningCounterMessage()
    {
      if (WarningCounter == 0)
        return "";
      return " with " + WarningCounter + " warnings";
    }
  }
}
