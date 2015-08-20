using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;


namespace TAPChecker
{
  internal class TapCheckerOptions
  {
    [OptionArray('c', "check", Required = true, HelpText = "File(s) to check")]
    public string[] Files { get; set; }

    [Option('f', "fix", Required = false, HelpText = "Fix errors found in TAP files.")]
    public bool Fix { get; set; }

    [Option('l', "longpause", Required = false, HelpText = "Details the long pauses in the TAP files")]
    public bool LongPauseDetail { get; set; }



    [HelpOption]
    public string GetUsage()
    {
      return HelpText.AutoBuild(this,
        (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
    }
  }
}
