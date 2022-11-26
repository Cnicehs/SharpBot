using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public static class EncodingExtension
{
    public static byte[] StringToCChar(this Encoding encoding, string str)
    {
        return encoding.GetBytes(str).Append((byte)0).ToArray();
    }

    public static string CCharToString(this Encoding encoding, IntPtr ptr)
    {
        List<byte> bytesList = new List<byte>();
        int i = 0;
        while (true)
        {
            byte b = Marshal.ReadByte(ptr, i++);
            if ((char)b == '\0')
            {
                break;
            }
            bytesList.Add(b);
        }
        return encoding.GetString(bytesList.ToArray());
    }
}
