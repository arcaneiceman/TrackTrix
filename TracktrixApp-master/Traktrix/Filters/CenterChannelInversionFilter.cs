using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traktrix.Filters
{
    class CenterChannelInversionFilter : Filter
    {

        public static byte[] FilterTheBuffer(byte[] input)
        {
            MemoryStream ByteStream1 = new MemoryStream();
            MemoryStream ByteStream2 = new MemoryStream();
            //splitting channels
            for (int i = 0; i < input.Length; i = i + 4)
            {
                    ByteStream1.WriteByte(input[i]);
                    ByteStream1.WriteByte(input[i + 1]);
                    ByteStream2.WriteByte(input[i + 2]);
                    ByteStream2.WriteByte(input[i + 3]);
            }
            Byte[] Channel1 = ByteStream1.ToArray();
            Byte[] Channel2 = ByteStream2.ToArray();
            //inverting Channel2
            int temp;
            for (int i = 0; i < Channel2.Length; i++)
            {
                temp = (int)Channel2[i] & 0xff;
                Channel2[i] = (byte)(temp * (-1));
            }
            //putting it back into place
            int count = 0;
            int temp1, temp2;
            MemoryStream OutputStream = new MemoryStream();
            for (int i = 0; i < input.Length; i = i + 4)
            {
                    temp1 = ((int)Channel1[count]) + ((int)Channel2[count]);
                    temp2 = ((int)Channel1[count + 1]) + ((int)Channel2[count + 1]);
                    OutputStream.WriteByte((byte)temp1);
                    OutputStream.WriteByte((byte)temp2);
                    OutputStream.WriteByte((byte)temp1);
                    OutputStream.WriteByte((byte)temp2);
                    count = count + 2;              
            }
            byte[] result = OutputStream.ToArray();
            OutputStream.Dispose();
            ByteStream1.Dispose();
            ByteStream2.Dispose();
            return result;
        }

    }
}
