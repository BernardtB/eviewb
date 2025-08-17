using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EviewProtocolParse.Util;

namespace EviewProtocolParse.EviewProto
{
    class EviewProto
    {


        public static string BodyToString(byte[] data, int offset, int size,int spaceOffset,int nameMaxLen)
        {
            string str = "";
            Body mBody = new Body();
            mBody.Put(data, offset, size);
            if (mBody.IsVaild == false) return null;
            int space = 2+ spaceOffset;
            int namecount = nameMaxLen;
            str += Util.Util.FormtShow("CRC", Util.Util.HexToString(mBody.CRC), space, namecount);
            str += Util.Util.FormtShow("Length", Util.Util.HexToString(mBody.Length), space, namecount);
            str += Util.Util.FormtShow("Command", Util.Util.HexToString(mBody.Comand), space, namecount);

            str += Util.Util.FormtShow("Command", EnumHelper.GetDescription((EviewType.Command)mBody.Comand), space, namecount);
            string DiscardLine = "----------------------------------------------------------\r\n";

            foreach (var item in mBody.Items)
            {
                str += DiscardLine;
                str += EviewType.Parse(item, space, namecount);  

            }



            return str;
        }
        public static Body GetBody(byte[] data, int offset, int size, int spaceOffset, int nameMaxLen)
        {
            Body mBody = new Body();
            mBody.Put(data, offset, size);
            return mBody;
        }
        public static byte[] GetAckPacket(byte[] data, byte respone)
        {
            ACKPack pack = new ACKPack();
            return pack.GetRaw(data, respone);
        }

        public static byte[] GetTimestampPacket(byte[] data, UInt32 UTC)
        {
            TimestampPack pack = new TimestampPack();
            return pack.GetRaw(data, UTC);
        }


        public struct Body
        {
            public byte Comand { get; set; }

            public List<EviewType> Items { get; set; }


            public UInt16 CRC { get; set; }
            public UInt16 Length { get; set; }
            public bool IsVaild { get; set; }

            public bool Put(byte[] data, int offset, int size)
            {
                if (data.Length < offset + size) return false;

                byte[] body = new byte[size];
                Array.Copy(data, offset, body, 0, size);
                CRC = Util.CRCHelper.Crc16Compute(body);
                Length = (UInt16)size;
                int index = 0;
                Comand = body[index++];
                Items = new List<EviewType>();
                EviewType item;
                for (int i = 1; i < size; )
                {
                    if (body.Length < i + 2) break;
                    byte len = body[i];
                    byte key = body[i + 1];
                    if (body.Length < i + 1 + len) break;

                    if (len != 0)
                    {
                        byte[] src = new byte[len + 1];
                        Array.Copy(body, i , src, 0, len +1);
                        if (len > 1)
                        {                   
                            byte[] value = new byte[len - 1];
                            Array.Copy(body, i + 2, value, 0, len - 1);
                            item = new EviewType(Comand, key, src, value, null);
                        }
                        else
                            item = new EviewType(Comand, key, src, null, null);
                        Items.Add(item);
                    }
                    i += len + 1;
                }
                IsVaild = true;
                return IsVaild;
            }





        }

        #region 头部解释

        public struct Head
        {
            public bool IsVaild { get; set; }
            public byte Magic { get; set; }
            private byte properites;
            public byte Properites
            {
                get
                {
                    return properites;
                }
                set
                {
                    properites = value;
                    Version = (byte)(value & 0xf);
                    ACK = (value & 0x10) > 0;
                    ERR = (value & 0x20) > 0;
                    Encyption = (byte)(value >> 6);
                }
            }
            public byte Version { get; set; }

            public bool ACK { get; set; }
            public bool ERR { get; set; }

            public byte Encyption { get; set; }
            public UInt16 Length { get; set; }

            public UInt16 CRC { get; set; }

            public UInt16 sequenceID { get; set; }


            public bool Put(byte[] data)
            {
                if (data.Length < 8) return false;
                int index = 0;
                Magic = data[index++];
                Properites = data[index++];
                Length = Util.Util.ToUint16(data, index);
                index += 2;
                CRC = Util.Util.ToUint16(data, index);
                index += 2;
                sequenceID = Util.Util.ToUint16(data, index);
                index += 2;
                IsVaild = true;
                return IsVaild;
            }
        }
        public static string HeadToString(byte[] data)
        {
            string str = "";
            Head mHead = new Head();


            mHead.Put(data);
            if (mHead.IsVaild == false) return null;
            str += Util.Util.HexToString(data, 0, 8);
            str += "\r\n";
            int space = 2;
            int namecount = 12;
            str += Util.Util.FormtShow("Head", Util.Util.HexToString(mHead.Magic), space, namecount);


            str += Util.Util.FormtShow("properties", Util.Util.HexToString(mHead.Properites), space, namecount);

            str += Util.Util.Space(space) + "[" + "\r\n";

            str += Util.Util.FormtShow("Encryption", Util.Util.HexToString(mHead.Encyption), space + 2, namecount);

            str += Util.Util.FormtShow("ERR", mHead.ERR.ToString(), space + 2, namecount);
            str += Util.Util.FormtShow("ACK", mHead.ACK.ToString(), space + 2, namecount);

            str += Util.Util.FormtShow("Version", Util.Util.HexToString(mHead.Version), space + 2, namecount);


            str += Util.Util.Space(space) + "]" + "\r\n";

            //length
            str += Util.Util.FormtShow("Length", Util.Util.HexToString(mHead.Length), space, namecount);


            //crc
            str += Util.Util.FormtShow("CRC", Util.Util.HexToString(mHead.CRC), space, namecount);

            //Sequence id
            str += Util.Util.FormtShow("Sequence id", Util.Util.HexToString(mHead.sequenceID), space, namecount);
            return str;
        }
        #endregion

        #region ACK Packet 产生
        public struct ACKPack
        {

            public byte[] GetRaw(byte[] head, byte respone)
            {
                Head mHead = new Head();
                mHead.Put(head);
                List<byte> value = new List<byte>();
                if (mHead.IsVaild == false) return value.ToArray();
                byte[] content = new byte[3];
                content[0] = 0x7f;
                content[1] = 0x01;
                content[2] = respone;


                value.Add(mHead.Magic);
                value.Add(0);//properies

                value.Add(3);//len
                value.Add(0);

                //crc
                UInt16 crc = Util.CRCHelper.Crc16Compute(content);
                value.Add((byte)(crc & 0xff));
                value.Add((byte)((crc >> 8) & 0xff));

                //sequence id
                value.Add((byte)(mHead.sequenceID & 0xff));
                value.Add((byte)((mHead.sequenceID >> 8) & 0xff));

                foreach (var item in content)
                {
                    value.Add(item);
                }

                return value.ToArray();
            }

        }


        public struct TimestampPack
        {

            public byte[] GetRaw(byte[] head, uint utc)
            {
                //AB 00 07 00 E2 36 01 00     03 05 12 40 61 48 5F
                Head mHead = new Head();
                mHead.Put(head);
                List<byte> value = new List<byte>();
                if (mHead.IsVaild == false) return value.ToArray();
                byte[] content = new byte[7];
                int index = 0;
                content[index++] = 0x03;
                content[index++] = 0x5;
                content[index++] = 0x12;
                content[index++] = (byte)(utc&0xff);
                content[index++] = (byte)((utc>>8)&0xff);
                content[index++] = (byte)((utc>>16)&0xff);
                content[index++] = (byte)((utc>>24)&0xff);




                value.Add(mHead.Magic);
                value.Add(0);//properies

                value.Add((byte)(content.Length&0xff));//len
                value.Add((byte)((content.Length>>8)&0xff));//len


                //crc
                UInt16 crc = Util.CRCHelper.Crc16Compute(content);
                value.Add((byte)(crc & 0xff));
                value.Add((byte)((crc >> 8) & 0xff));

                //sequence id
                value.Add((byte)(mHead.sequenceID & 0xff));
                value.Add((byte)((mHead.sequenceID >> 8) & 0xff));

                foreach (var item in content)
                {
                    value.Add(item);
                }

                return value.ToArray();
            }

        }
        #endregion
    }
}
