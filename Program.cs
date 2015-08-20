using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;

namespace TAPChecker
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      StringWriter writer = new StringWriter();
      Parser p = new Parser(with => with.HelpWriter = writer);
      TapCheckerOptions opts = new TapCheckerOptions();
      if (!p.ParseArguments(args, opts))
      {
        Console.WriteLine(writer.ToString());
        return;
      }

      List<string> files = new List<string>();
      if ((opts.Files.Length == 1) && (opts.Files[0] == "*"))
      {
        files.AddRange(Directory.GetFiles(".", "*.tap"));
      }
      else
      {
        files.AddRange(opts.Files.ToArray());
      }

      ConsoleColor currentColor = Console.ForegroundColor;
      TapChecker checker = new TapChecker();
      foreach (var fileToCheck in files)
      {
        List<ReportLine> report = checker.Check(fileToCheck, opts);

        foreach (var reportLine in report)
        {
          switch (reportLine.Status)
          {
            case ReportLineStatus.Info:
              Console.ForegroundColor = currentColor;
              break;
            case ReportLineStatus.Error:
              Console.ForegroundColor = ConsoleColor.Red;
              break;
            case ReportLineStatus.Warning:
              Console.ForegroundColor = ConsoleColor.Yellow;
              break;
          }
          Console.WriteLine(reportLine.Message);
        }
        Console.WriteLine();
      }
      Console.ForegroundColor = currentColor;
      Console.WriteLine("Press enter to exit");
      Console.ReadLine();
    }
  }
}
