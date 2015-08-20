using System;
using System.IO;
using System.Security.Policy;
using System.Text;

namespace TAPChecker
{
  public class TapHeader
  {
    public TapHeader()
    {
    }

    public const UInt32 HeaderSize = 20;
    public const string C64MagicString = "C64-TAPE-RAW";
    public const string C16MagicString = "C16-TAPE-RAW";

    public TapHeaderMagic Magic;
    public TapFormat Version;
    public TapPlatform Platform;
    public TapVideoStandard VideoStandard;
    public byte Reserved;
    public UInt32 DataLength;

    public void ReadHeader(BinaryReader br)
    {
      byte[] tmpMagicBytes = new byte[12];
      br.Read(tmpMagicBytes, 0, 12);
      Magic = TapHeaderMagic.Unknown;

      if (ArrayEquals(tmpMagicBytes, Encoding.ASCII.GetBytes(C64MagicString)))
      {
        Magic = TapHeaderMagic.C64_Magic;
      }
      else if (ArrayEquals(tmpMagicBytes, Encoding.ASCII.GetBytes(C16MagicString)))
      {
        Magic = TapHeaderMagic.C16_Magic;
      }

      Version = (TapFormat) br.ReadByte();
      Platform = (TapPlatform) br.ReadByte();
      VideoStandard = (TapVideoStandard) br.ReadByte();
      Reserved = br.ReadByte();
      DataLength = br.ReadUInt32();
    }

    public void WriteHeader(BinaryWriter bw)
    {
      bw.Write(Magic == TapHeaderMagic.C64_Magic
        ? Encoding.ASCII.GetBytes(C64MagicString)
        : Encoding.ASCII.GetBytes(C16MagicString));
      bw.Write((byte) Version);
      bw.Write((byte) Platform);
      bw.Write((byte) VideoStandard);
      bw.Write((byte) 0);
      bw.Write(DataLength);
    }

    public static bool ArrayEquals(byte[] a1, byte[] a2)
    {
      if (a1 == a2)
      {
        return true;
      }
      if ((a1 != null) && (a2 != null))
      {
        if (a1.Length != a2.Length)
        {
          return false;
        }
        for (int i = 0; i < a1.Length; i++)
        {
          if (a1[i] != a2[i])
          {
            return false;
          }
        }
        return true;
      }
      return false;
    }
  }
}