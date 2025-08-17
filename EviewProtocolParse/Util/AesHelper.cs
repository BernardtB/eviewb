using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EviewProtocolParse.Util
{
    class AesHelper
    {

        /// <summary>
        /// AES CBC 128 加密
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="data"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static byte[] AesCBC128Decrypt(byte[] key, byte[] iv, byte[] encrypted, PaddingMode mode)
        {
            try
            {

                using (RijndaelManaged cipher = new RijndaelManaged())
                {
                    cipher.Mode = CipherMode.CBC;
                    cipher.Padding = mode;
                    cipher.KeySize = 128;
                    cipher.BlockSize = 128;
                    cipher.Key = key;
                    cipher.IV = iv;

                    List<byte> lstBytes = encrypted.ToList();


                    using (ICryptoTransform decryptor = cipher.CreateDecryptor())
                    {
                        using (MemoryStream msDecrypt = new MemoryStream(lstBytes.ToArray()))
                        {
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] read = new byte[200];
                                MemoryStream P_MemoryStream_temp =//创建内存流对象
                     new MemoryStream();

                                int i;

                                while ((i = csDecrypt.Read(//使用while循环得到解密数据
                       read, 0, read.Length)) > 0)//(1\从当前流中读取200个字节-并将它们存储在 P_bt_temp 中 2\P_bt_temp 中的字节偏移量-从该偏移量开始存储从当前流中读取的数据 3\读入缓冲区中的总字节数)
                                {
                                    P_MemoryStream_temp.Write(//将解密后的数据放入内存流
                                        read, 0, i);
                                }


                                return P_MemoryStream_temp.ToArray();
                            }
                        }
                    }
                }
            }catch
            {
                return new byte[0];
            }
        }


        /// <summary>
        /// AES CBC 128 加密
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="data"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static byte[] AesCBC128Encrypt(byte[] key, byte[] iv, byte[] data, PaddingMode mode)
        {
            byte[] encrypted;
            using (RijndaelManaged cipher = new RijndaelManaged())
            {

                cipher.Mode = CipherMode.CBC;
                cipher.Padding = mode;
                cipher.KeySize = 128;
                cipher.BlockSize = 128;
                cipher.Key = key;
                cipher.IV = iv;
                using (ICryptoTransform encryptor = cipher.CreateEncryptor())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream writer = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            writer.Write(data, 0, data.Length);
                            writer.FlushFinalBlock();
                            encrypted = ms.ToArray();


                            return encrypted;
                        }
                    }
                }

            }
        }
    }





}
