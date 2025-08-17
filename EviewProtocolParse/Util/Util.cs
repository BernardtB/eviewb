using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EviewProtocolParse.Util
{
    public class Util
    {
        static public UInt16 ToUint16(byte[] data, int offset)
        {
            if (data.Length < offset + 2) return 0;
            UInt16 value = 0;
            value = data[offset + 1];
            value <<= 8;
            value += data[offset];
            return value;
        }
        static public UInt32 ToUint32(byte[] data, int offset)
        {
            if (data.Length < offset + 4) return 0;
            UInt32 value = 0;
            value = data[offset];
            value += ((UInt32)data[offset + 1]) << 8;
            value += (UInt32)data[offset + 2] << 16;
            value += (UInt32)data[offset + 3] << 24;
            return value;
        }

        static public byte[] StringToHex(string content, out string outString)
        {

            content = content.ToUpper();
            content = content.Replace("\r\n", " ");
            List<byte> mList = new List<byte>();
            outString = "";

            if (false == IsHex(content)) { return null; }
            int index = 0;
            string _value = "";
            for (int i = 0; i < content.Length; i++)
            {
                switch (index)
                {
                    case 0:
                        {
                            if (content[i] == ' ') continue;
                            _value = content[i].ToString();
                            index++;
                            break;
                        }
                    case 1:
                        {
                            if (content[i] != ' ')
                            {
                                _value += content[i].ToString();
                            }
                            byte value = 0;
                            if (false == byte.TryParse(_value.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier, null, out value)) return null;
                            mList.Add(value);
                            index = 0;
                            outString += _value.ToString() + " ";
                            break;
                        }
                    default:
                        return null;

                }


            }
            return mList.ToArray();
        }

        static public bool IsHex(string conten)
        {
            foreach (var item in conten)
            {
                if (item >= '0' && item <= '9') continue;
                if (item >= 'a' && item <= 'f') continue;
                if (item >= 'A' && item <= 'F') continue;
                if (item == ' ') continue;
                return false;
            }
            return true;
        }


        static public string HexToString(byte[] data, int offset, int size)
        {
            if (data.Length < size + offset) return "";
            string _str = "";
            string temp;
            for (int i = offset; i < offset + size; i++)
            {
                temp = Convert.ToString(data[i], 16).ToUpper();
                _str += temp.PadLeft(2, '0');
                _str += " ";
            }
            return _str;
        }
        static public string MacToString(byte[] data, int offset)
        {
            if (data.Length < 6 + offset) return "";
            string _str = "";
            string temp;
            int i = 0;
            for (i = offset; i < offset + 5; i++)
            {
                temp = Convert.ToString(data[i], 16).ToUpper();
                _str += temp.PadLeft(2, '0');
                _str += ":";
            }

            temp = Convert.ToString(data[i], 16).ToUpper();
            _str += temp.PadLeft(2, '0');
            return _str;
        }


        static public string HexToString(byte data)
        {
            byte[] value = new byte[1];
            value[0] = data;
            return HexToString(value, 0, 1);
        }
        static public string HexToString(UInt16 data)
        {
            byte[] value = new byte[2];
            value[0] = (byte)(data >> 8);
            value[1] = (byte)(data);
            return HexToString(value, 0, 2);
        }
        static public string HexToString(UInt32 data)
        {
            byte[] value = new byte[4];
            value[0] = (byte)(data >> 24);
            value[1] = (byte)(data >> 16);
            value[2] = (byte)(data >> 8);
            value[3] = (byte)(data);
            return HexToString(value, 0, 4);
        }


        static public string Space(int count)
        {
            string _str = "";
            for (int i = 0; i < count; i++)
            {
                _str += " ";
            }
            return _str;
        }

        static public string FormtShowNoEnter(string tile, string content, int space, int nameMaxLen)
        {
            string str = Space(space);
            str += tile.PadRight(nameMaxLen, ' ') + ":";
            str += content;
            return str;
        }

        static public string FormtShow(string tile, string content, int space, int nameMaxLen)
        {
            return FormtShowNoEnter(tile, content, space, nameMaxLen) + "\r\n";
        }



    }
}
