using EviewProtocolParse.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EviewProtocolParse.EviewProto
{
    class EviewCryptProto
    {
        #region Head Info
        public struct Head
        {
            //0xA5
            public byte Header { get; set; }
            public byte Properties { get; set; }

            //加密后的
            public UInt16 Length { get; set; }

            public UInt16 CRCAes { get; set; }
            public UInt16 SN { get; set; }

            public uint KeyId { get; set; }

            public UInt16 ControlCode { get; set; }

            public UInt16 HeadCRC { get; set; }
            public bool Put(byte[] data)
            {
                if (data.Length < 12) return false;
                int index = 0;
                Header = data[index++];
                Properties = data[index++];
                Length = Util.Util.ToUint16(data, index); index += 2;
                CRCAes = Util.Util.ToUint16(data, index); index += 2;
                SN = Util.Util.ToUint16(data, index); index += 2;
                KeyId = Util.Util.ToUint32(data, index); index += 4;
                ControlCode = Util.Util.ToUint16(data, index); index += 2;
                HeadCRC = Util.Util.ToUint16(data, index); index += 2;
                UInt16 crc = Util.CRCHelper.Crc16Compute(GetRaw(), 0, GetRaw().Length - 2);
                if (HeadCRC != crc) return false;
                return true;
            }
            public byte[] GetRaw()
            {
                List<byte> raw = new List<byte>();
                raw.Add(Header);       
                raw.Add(Properties);
                raw.Add((byte)(Length & 0xff));
                raw.Add((byte)((Length >> 8) & 0xff));

                raw.Add((byte)(CRCAes & 0xff));
                raw.Add((byte)((CRCAes >> 8) & 0xff));

                raw.Add((byte)(SN & 0xff));
                raw.Add((byte)((SN >> 8) & 0xff));

                raw.Add((byte)(KeyId & 0xff));
                raw.Add((byte)((KeyId >> 8) & 0xff));
                raw.Add((byte)((KeyId >> 16) & 0xff));
                raw.Add((byte)((KeyId >> 24) & 0xff));

                raw.Add((byte)(ControlCode & 0xff));
                raw.Add((byte)((ControlCode >> 8) & 0xff));

                raw.Add((byte)(HeadCRC & 0xff));
                raw.Add((byte)((HeadCRC >> 8) & 0xff));
                return raw.ToArray();
            }
        }
        public string HeadToString(byte[] data)
        {
            string str = "";
            Head mHead = new Head();
            if(mHead.Put(data) == false)return str;
            str += Util.Util.HexToString(data, 0, mHead.GetRaw().Length);
            str += "\r\n";
            int space = 2;
            int namecount = 12;
            str += Util.Util.FormtShow("Head", Util.Util.HexToString(mHead.Header), space, namecount);
            str += Util.Util.FormtShow("properties", Util.Util.HexToString(mHead.Properties), space, namecount);
            str += Util.Util.FormtShow("Length", Util.Util.HexToString(mHead.Length), space, namecount);
            str += Util.Util.FormtShow("AesCRC", Util.Util.HexToString(mHead.CRCAes), space, namecount);
            str += Util.Util.FormtShow("sequence id", Util.Util.HexToString(mHead.SN), space, namecount);

            str += Util.Util.FormtShow("Key id", Util.Util.HexToString(mHead.KeyId ), space, namecount);
            str += Util.Util.FormtShow("type", Util.Util.HexToString((mHead.KeyId>>24) & 0xffffff), space + 2, namecount);
            str += Util.Util.FormtShow("ID", Util.Util.HexToString(mHead.KeyId&0xffffff), space+2, namecount);

            str += Util.Util.FormtShow("Control Code", Util.Util.HexToString(mHead.ControlCode), space, namecount);
            str += Util.Util.FormtShow("Head CRC", Util.Util.HexToString(mHead.HeadCRC), space, namecount);




            return str;
        }


        #endregion


        #region Encryption
        public byte[] Encrypt(uint keyID, byte[] aesKey, byte[] aesIV, UInt16 ControlCode, UInt16 sn, byte[] data)
        {
            List<byte> encrypt = new List<byte>();

            if (keyID == 0 || aesKey == null || aesIV == null || aesIV.Length != 16 || aesIV.Length != 16 || data == null) return encrypt.ToArray();

            Head mHead = new Head();
            byte[] Body = AesHelper.AesCBC128Encrypt(aesKey, aesIV, data, System.Security.Cryptography.PaddingMode.PKCS7);
            if (Body.Length < 1) return null;
            mHead.Header = 0xa5;
            mHead.Length = (UInt16)Body.Length;
            mHead.CRCAes = CRCHelper.Crc16Compute(Body);
            mHead.SN = sn;
            mHead.KeyId = keyID;
            mHead.ControlCode = ControlCode;
            mHead.HeadCRC = CRCHelper.Crc16Compute(mHead.GetRaw(), 0, mHead.GetRaw().Length - 2);

            encrypt.AddRange(mHead.GetRaw());
            encrypt.AddRange(Body);

            return encrypt.ToArray();
        }

        #endregion

        #region Decrypt 
        public struct DecryptBody
        {
            public Head HeadInfo;
            public byte[] Body;
            public bool IsVaild;
            public string OutErr;

            public UInt16 HeadCRC;

            public UInt16 EncrypBodyLen;
            public UInt16 EncrypBodyCRC;
            public UInt16 DecrypBodyLen;
        }


        public DecryptBody Decrypt(byte[] aesKey, byte[] aesIV, byte[] data)
        {
            DecryptBody decrypt = new DecryptBody();
            decrypt.IsVaild = false;
            if (data.Length < 12) return decrypt;
            int index = 0;
            if (false == decrypt.HeadInfo.Put(data)) return decrypt;
            index += decrypt.HeadInfo.GetRaw().Length;

            byte[] body = new byte[data.Length - index];
            Array.Copy(data, index, body, 0, data.Length - index);
            decrypt.HeadCRC = CRCHelper.Crc16Compute(decrypt.HeadInfo.GetRaw(), 0, decrypt.HeadInfo.GetRaw().Length - 2);
            decrypt.EncrypBodyLen = (UInt16)body.Length;
            decrypt.EncrypBodyCRC = CRCHelper.Crc16Compute(body);

            decrypt.Body = AesHelper.AesCBC128Decrypt(aesKey, aesIV, body, System.Security.Cryptography.PaddingMode.PKCS7);
            decrypt.DecrypBodyLen = (UInt16)decrypt.Body.Length;
            return decrypt;
        }
        #endregion


    }
}
