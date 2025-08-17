using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EviewProtocolParse;
using EviewProtocolParse.EviewProto;
using Microsoft.Win32;

namespace EviewProtocolParse
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {



        public MainWindow()
        {
            InitializeComponent();
            cb_Crypto.SelectedIndex = 0;
        }


        string DiscardLine = "----------------------------------------------------------\r\n";


        bool IsStaicAES()
        {
            return cb_Crypto.SelectedIndex == 1;
        }
        public static int ConvertDateTimeInt(System.DateTime time)

        {

            double intResult = 0;

            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));

            intResult = (time - startTime).TotalSeconds;

            return (int)intResult;

        }

        void ParseABProtocol(byte[] raw)
        {


            txt_Parse.AppendText(DiscardLine);

            txt_Parse.AppendText(EviewProto.EviewProto.HeadToString(raw));
            txt_Parse.AppendText(DiscardLine);
            txt_Parse.AppendText(EviewProto.EviewProto.BodyToString(raw, 8, raw.Length - 8, 0, 15));
            txt_Parse.AppendText(DiscardLine);
        }
        private void Btn_Parse_Click(object sender, RoutedEventArgs e)
        {
            string cont = "";
            byte[] raw = Util.Util.StringToHex(txt_ProtocolData.Text, out cont);
            if (raw == null || raw.Length == 0) return;
            txt_Parse.Text = cont + "\r\n";
            ParseABProtocol(raw);

        }

        private void Btn_GetACK_Click(object sender, RoutedEventArgs e)
        {
            string cont = "";
            byte[] rsp;
            byte[] raw = Util.Util.StringToHex(txt_ProtocolData.Text, out cont);
            txt_Parse.Text = cont + "\r\n";
            txt_Parse.AppendText(DiscardLine);
            EviewProto.EviewProto.Body Body = EviewProto.EviewProto.GetBody(raw, 8, raw.Length - 8, 0, 15);
            if (Body.IsVaild == true)
            {
                foreach (var item in Body.Items)
                {
                    if (item.Commad == (byte)EviewType.Command.Services && item.Type == (byte)EviewType.ServerKey.TimeStamp)
                    {
                        txt_Parse.AppendText("Response timestamp:\r\n");

                        rsp = EviewProto.EviewProto.GetTimestampPacket(raw, (uint)ConvertDateTimeInt(DateTime.Now.ToUniversalTime()));
                        txt_Parse.AppendText(Util.Util.HexToString(rsp, 0, rsp.Length));
                        return;
                    }
                }
            }
            txt_Parse.AppendText("ACK Packet:\r\n");
            rsp = EviewProto.EviewProto.GetAckPacket(raw, 0);
            txt_Parse.AppendText(Util.Util.HexToString(rsp, 0, rsp.Length));

        }


        private void Btn_LoadStaticAes_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();



            openFileDialog.Title = "SelectConfigFile";
            openFileDialog.Filter = "txt文件|*.txt";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "txt";
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"AES"))
            {
                openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + @"AES";
            }
            if (openFileDialog.ShowDialog() == true)
            {
                string txtFile = openFileDialog.FileName;
                Load_StaticAESFile(txtFile);


            }

        }
        byte[] GetAesHexValue(string value)
        {
            byte[] hex = new byte[16];
            value = value.Replace(" ", "");
            if (value.Length != 32) return null;
            for (int i = 0; i < value.Length; i += 2)
            {

                hex[i / 2] = (byte)Convert.ToByte(value.Substring(i, 2), 16);
            }
            return hex;
        }
        public byte[] AESKey { get; set; }
        public byte[] AESIV { get; set; }
        public uint ID { get; set; }


        private void Load_StaticAESFile(string file)
        {

            using (StreamReader sr = new StreamReader(file, Encoding.UTF8))
            {
                while (sr.Peek() > 0)
                {
                    string temp = sr.ReadLine();
                    temp = temp.Replace(" ", "");
                    if (temp[0].Equals('#') == true) continue;
                    Console.WriteLine(temp);

                    string[] _strs = temp.Split('|');
                    if (_strs.Length < 3) continue;

                    int index = 0;
                    uint id = 0;
                    if (uint.TryParse(_strs[index++], out id) == false) continue;
                    string sAesKey = _strs[index++];
                    byte[] bAesKey = GetAesHexValue(sAesKey);
                    if (bAesKey == null) return;


                    string sAesIV = _strs[index++];
                    byte[] bAesIV = GetAesHexValue(sAesIV);
                    if (bAesIV == null) return;
                    txt_AesKey.Text = sAesKey;

                    txt_KeyID.Text = id.ToString();
                    txt_AesIV.Text = sAesIV;

                    AESKey = bAesKey;
                    AESIV = bAesIV;
                    ID = id;
                    return;

                }
            }
        }



        private void Cb_Crypto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            gp_StaticAes.IsEnabled = false;
            if (IsStaicAES())
            {
                gp_StaticAes.IsEnabled = true;
            }

        }



        private void Btn_Encrypt_Click(object sender, RoutedEventArgs e)
        {
            txt_Parse.Clear();
            if (IsStaicAES() == false || ID == 0 || AESIV == null || AESKey == null)
            {
                txt_Parse.AppendText("Please Select Static AES and load the AES file!\r\n");
                return;
            }
            string cont;
            byte[] raw = Util.Util.StringToHex(txt_ProtocolData.Text, out cont);
            EviewProto.EviewProto.Head head = new EviewProto.EviewProto.Head();
            head.Put(raw);
            EviewCryptProto cryptProto = new EviewCryptProto();

            UInt16 cc = 0;
            if (UInt16.TryParse(txt_ControlValue.Text, out cc) == false)
            {
                cc = 1;
                txt_ControlValue.Text = cc.ToString();
            }

            byte[] encrypt = cryptProto.Encrypt(ID | (2 << 24), AESKey, AESIV, cc, head.sequenceID, raw);

            txt_Parse.AppendText(Util.Util.HexToString(encrypt, 0, encrypt.Length));

        }

        private void Btn_Decrypt_Click(object sender, RoutedEventArgs e)
        {
            txt_Parse.Clear();
            if (IsStaicAES() == false || ID == 0 || AESIV == null || AESKey == null)
            {
                txt_Parse.AppendText("Please Select Static AES and load the AES file!\r\n");
                return;
            }
            string cont;
            byte[] raw = Util.Util.StringToHex(txt_ProtocolData.Text, out cont);
            EviewCryptProto cryptProto = new EviewCryptProto();

            EviewCryptProto.DecryptBody body = cryptProto.Decrypt(AESKey, AESIV, raw);

            txt_Parse.Clear();
            txt_Parse.AppendText(cryptProto.HeadToString(raw));
            txt_Parse.AppendText(DiscardLine);
            txt_Parse.AppendText("Head Cal CRC:" + Util.Util.HexToString(body.HeadCRC) + "\r\n");

            txt_Parse.AppendText("Encrypt Len:" + Util.Util.HexToString(body.EncrypBodyLen) + "\r\n");
            txt_Parse.AppendText("Encrypt CRC:" + Util.Util.HexToString(body.EncrypBodyCRC) + "\r\n");

            txt_Parse.AppendText(DiscardLine);
            if (body.Body != null && body.Body.Length>0)
            {

                txt_Parse.AppendText(Util.Util.HexToString(body.Body, 0, body.Body.Length) + "\r\n");
                txt_Parse.AppendText(DiscardLine);
                ParseABProtocol(body.Body);

            }

        }
    }
}
