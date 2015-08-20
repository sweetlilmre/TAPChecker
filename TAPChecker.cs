using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TAPChecker
{
  public class ReportLine
  {
    public ReportLine(string message, ReportLineStatus status)
    {
      Message = message;
      Status = status;
    }

    public string Message { get; private set; }
    public ReportLineStatus Status { get; private set; }
  }

  public class TapChecker
  {
    public TapChecker()
    {
    }

    private void AddHeaderInfo(TapHeader header, List<ReportLine> report)
    {
      report.Add(new ReportLine(string.Format("Header Magic: {0}", header.Magic), ReportLineStatus.Info));
      report.Add(new ReportLine(string.Format("Version: {0}", header.Version), ReportLineStatus.Info));
      report.Add(new ReportLine(string.Format("Platform: {0}", header.Platform), ReportLineStatus.Info));
      report.Add(new ReportLine(string.Format("Video Standard: {0}", header.VideoStandard), ReportLineStatus.Info));
      report.Add(new ReportLine(string.Format("Data Length: {0}", header.DataLength), ReportLineStatus.Info));
    }

    private void Fix(List<ReportLine> report, string fullFileName, TapHeader header)
    {
      report.Add(new ReportLine("Attempting to fix TAP issues...", ReportLineStatus.Info));
      string backupFileName = Path.ChangeExtension(fullFileName, ".tap.bak");
      FileInfo fi = new FileInfo(backupFileName);
      if (fi.Exists)
      {
        report.Add(new ReportLine("Backup file already exists, skipping fix!", ReportLineStatus.Error));
        return;
      }
      try
      {
        File.Copy(fullFileName, backupFileName);
      }
      catch (Exception e)
      {
        report.Add(new ReportLine(string.Format("Unable to create backup: {0}", e.Message), ReportLineStatus.Error));
        return;
      }
      try
      {
        using (FileStream fs = File.OpenWrite(fullFileName))
        {
          fs.Seek(0, SeekOrigin.Begin);
          BinaryWriter bw = new BinaryWriter(fs);
          header.WriteHeader(bw);
        }
      }
      catch (Exception e)
      {
        report.Add(new ReportLine(string.Format("Unable to fix header: {0}", e.Message), ReportLineStatus.Error));
        return;
      }
      report.Add(new ReportLine(string.Format("TAP {0} fixed", fullFileName), ReportLineStatus.Info));
    }

    internal List<ReportLine> Check(string fullFileName, TapCheckerOptions opts)
    {
      List<ReportLine> report = new List<ReportLine>();
      List<ReportLine> pauses = new List<ReportLine>();

      string fileName = Path.GetFileName(fullFileName);
      report.Add(new ReportLine(String.Format("TAP file: {0}", fileName), ReportLineStatus.Info));

      FileInfo fi = new FileInfo(fullFileName);
      if (!fi.Exists)
      {
        report.Add(new ReportLine("File not found", ReportLineStatus.Error));
        return report;
      }

      TapHeader header = new TapHeader();
      try
      {
        using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(fullFileName)))
        {
          BinaryReader br = new BinaryReader(ms);
          header.ReadHeader(br);
          if (opts.LongPauseDetail && header.Version != TapFormat.VERSION_0_INITIAL)
          {
            UInt32 ticksDiv = (UInt32)(header.VideoStandard == TapVideoStandard.PAL ? 985248 : 1022730);
            
            while (ms.Position < ms.Length)
            {
              if (br.ReadByte() != 0) continue;
              byte data = br.ReadByte();

              UInt32 pause = data;
              data = br.ReadByte();
              pause |= (UInt32)(data << 8);
              data = br.ReadByte();
              pause |= (UInt32)(data << 16);
              pauses.Add(new ReportLine(String.Format("Pause at: {0} for {1} (~ {2:F2}s)", ms.Position - 4, pause, (double) pause / ticksDiv), ReportLineStatus.Info));
            }
          }
        }
      }
      catch (Exception e)
      {
        report.Add(new ReportLine(string.Format("Error reading file: {0}", e.Message), ReportLineStatus.Error));
        return report;
      }

      AddHeaderInfo(header, report);

      UInt32 computedLength = (UInt32)fi.Length - TapHeader.HeaderSize;
      if ((header.Magic == TapHeaderMagic.C64_Magic && header.Platform != TapPlatform.C64) || (header.Magic == TapHeaderMagic.C16_Magic && header.Platform != TapPlatform.C16))
      {
        report.Add(new ReportLine("Header Magic and Platform do not match!", ReportLineStatus.Error));
      }
      else
      {
        report.Add(new ReportLine("Header Magic and Platform match", ReportLineStatus.Info));
      }

      report.AddRange(pauses);

      if (header.DataLength != computedLength)
      {
        report.Add(new ReportLine("TAP Header Data Length INCORRECT!", ReportLineStatus.Warning));
        report.Add(new ReportLine(string.Format("\tHeader reports: {0}", header.DataLength), ReportLineStatus.Warning));
        report.Add(new ReportLine(string.Format("\tComputed Length: {0}", computedLength), ReportLineStatus.Warning));
        report.Add(new ReportLine(string.Format("\tFile is: {0}", computedLength > header.DataLength ? "Longer" : "Shorter"), ReportLineStatus.Warning));
        header.DataLength = computedLength;
        if (opts.Fix)
        {
          Fix(report, fullFileName, header);
        }
      }
      return report;
    }
  }
}
