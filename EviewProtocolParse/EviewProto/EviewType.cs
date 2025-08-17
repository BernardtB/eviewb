using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EviewProtocolParse.EviewProto
{
    public class EviewType
    {

        #region 枚举类型 Command
        public enum Command
        {
            [Description("Data Command")]
            Data = 1,
            [Description("Configuration Command")]
            Configuration,
            [Description("Services Command")]
            Services,
            [Description("System Control Command")]
            SystemControl,
            [Description("Firmware Update Command")]
            FirmwareUpdate = 0x7e,
            [Description("Response  Command")]
            Response = 0x7f,
        }

        public enum DataKey
        {
            [Description("Device ID")]
            DeviceID = 0x1,
            [Description("Alarm Code")]
            AlarmCode = 0x2,
            [Description("Historical data completed")]
            HistoricalDataCompleted = 0x10,
            [Description("Historical data ")]
            HistoricalData = 0x11,
            [Description("Single locating data ")]
            SingleLocation = 0x12,
            [Description("Continue locating data ")]
            ContinueLocation = 0x13,

            [Description("GPS Location data ")]
            GPSLocation = 0x20,

            [Description("LBS Location data ")]
            LBSLocation = 0x21,

            [Description("WIFI Location data ")]
            WIFILocation = 0x22,

            [Description("BLE Location data ")]
            BLELocation = 0x23,

            [Description("General  data ")]
            GeneralData = 0x24,

            [Description("Call  Records ")]
            CallRecords = 0x25,
            [Description("Beacon Location data ")]
            BeaconLocation = 0x28,

            [Description("Home WIFI Location data ")]
            HomeWifiLocation = 0x2a,

            [Description("G-Sensor  degree ")]
            G_SensorDegree = 0x30,

            [Description("Activity  degree ")]
            ActivityDegree = 0x31,

            [Description("Heart rate data ")]
            HeartRateData = 0x40,
        }

        public enum ServerKey
        {
            [Description("Device ID")]
            DeviceID = 0x1,

            [Description("Time Stamp ")]
            TimeStamp = 0x12,

        }
        #endregion




        public byte Type { get; set; }
        public byte Commad { get; set; }

        public byte[] ArraryData { get; set; }
        public byte[] SourceData { get; set; }
        public object Data { get; set; }
        public object Length { get; set; }

        public EviewType(byte command, byte type, byte[] source, byte[] array, object data)
        {
            Commad = command;
            Type = type;
            Data = data;
            ArraryData = array;
            SourceData = source;
            Length = source.Length - 1;
        }



        #region Data 类
        #region GeneralData
        public struct GeneralData
        {
            public UInt32 UTC { get; set; }


            public bool GPSLoc { get; set; }
            public bool WIFILoc { get; set; }
            public bool LBSLoc { get; set; }
            public bool BLELoc { get; set; }
            public bool InCharging { get; set; }
            public bool ChargingCompleted { get; set; }
            public bool Reboot { get; set; }
            public bool HistoricalData { get; set; }
            public bool AGPS { get; set; }
            public bool Motion { get; set; }
            public byte WorkMode { get; set; }
            public byte GSMRssi { get; set; }
            public byte Battery { get; set; }
            public bool BeaconLoc { get; set; }
            public bool HomeWIFILoc { get; set; }
            private UInt32 status;
            public UInt32 Status
            {
                get
                {
                    return status;
                }
                set
                {
                    status = value;
                    GPSLoc = (value & 0x01) > 0;
                    WIFILoc = (value & 0x02) > 0;
                    LBSLoc = (value & 0x04) > 0;
                    BLELoc = (value & 0x08) > 0;


                    InCharging = (value & 0x10) > 0;
                    HomeWIFILoc = ((value >> 14) & 1)>0;
                    BeaconLoc = ((value >> 11) & 1)>0;
                    ChargingCompleted = (value & 0x20) > 0;
                    Reboot = (value & 0x40) > 0;
                    HistoricalData = (value & 0x80) > 0;

                    WorkMode = (byte)((value >> 16) & 0x7);
                    GSMRssi = (byte)((value >> 19) & 0x1f);
                    Battery = (byte)((value >> 24) & 0xff);


                }
            }

            public string ParseString(EviewType item, int spaceOffse, int nameMaxLen)
            {
                string str = "";
                if ((Command)item.Commad != Command.Data) return str;
                if ((DataKey)item.Type != DataKey.GeneralData) return str;
                if (item.ArraryData.Length < 8) return str;



                int index = 0;
                UTC = Util.Util.ToUint32(item.ArraryData, index);
                index += 4;
                Status = Util.Util.ToUint32(item.ArraryData, index);
                index += 4;

                System.DateTime dateTime = Util.UTCToTime.ToTime((double)UTC, 0);
                str += Util.Util.FormtShow("UTC", dateTime.ToString("yyyy-MM-dd HH:mm:ss"), spaceOffse, nameMaxLen);
                str += Util.Util.FormtShow("Status", Util.Util.HexToString(Status), spaceOffse, nameMaxLen);


                str += Util.Util.Space(spaceOffse) + "[" + "\r\n";
                int space = spaceOffse + 2;
                int namecount = nameMaxLen;
                str += Util.Util.FormtShow("GSP Loc", GPSLoc.ToString(), space, namecount);
                str += Util.Util.FormtShow("WIFI Loc", WIFILoc.ToString(), space, namecount);
                str += Util.Util.FormtShow("LBS Loc", LBSLoc.ToString(), space, namecount);
                str += Util.Util.FormtShow("BLE Loc", BLELoc.ToString(), space, namecount);
                str += Util.Util.FormtShow("Beacon Loc", BeaconLoc.ToString(), space, namecount);
                str += Util.Util.FormtShow("HomeWIFILoc Loc", HomeWIFILoc.ToString(), space, namecount);

                str += Util.Util.FormtShow("In Charging", InCharging.ToString(), space, namecount);
                str += Util.Util.FormtShow("Charged", ChargingCompleted.ToString(), space, namecount);
                str += Util.Util.FormtShow("Reboot", Reboot.ToString(), space, namecount);
                str += Util.Util.FormtShow("Historical", HistoricalData.ToString(), space, namecount);
                str += Util.Util.FormtShow("AGPS", AGPS.ToString(), space, namecount);
                str += Util.Util.FormtShow("Work mode", WorkMode.ToString(), space, namecount);
                str += Util.Util.FormtShow("GSM Rssi", GSMRssi.ToString(), space, namecount);
                str += Util.Util.FormtShow("Battery ", Battery.ToString() + "%", space, namecount);


                str += Util.Util.Space(spaceOffse) + "]" + "\r\n";

                return str;
            }
        }
        #endregion

        #region DeviceID
        public struct DeviceID
        {
            public string IMEI { get; set; }
            public string ParseString(EviewType item, int spaceOffse, int nameMaxLen)
            {
                string str = "";
                if ((Command)item.Commad != Command.Data) return str;
                if ((DataKey)item.Type != DataKey.DeviceID) return str;
                IMEI = System.Text.Encoding.Default.GetString(item.ArraryData);
                str += Util.Util.FormtShow("IMEI", IMEI, spaceOffse, nameMaxLen);
                return str;
            }
        }
        #endregion

        #region GPSLocation
        public struct GPSLocation
        {
            public Int32 Latitude { get; set; }
            public Int32 Longitude { get; set; }

            public UInt16 Speed { get; set; }

            public UInt16 Direction { get; set; }

            public Int16 Altitude { get; set; }

            public UInt16 Hop { get; set; }

            public UInt32 Mileage { get; set; }

            public byte NumberofStatellites { get; set; }

            public string ParseString(EviewType item, int spaceOffse, int nameMaxLen)
            {
                string str = "";
                if ((Command)item.Commad != Command.Data) return str;
                if ((DataKey)item.Type != DataKey.GPSLocation) return str;
                if (item.ArraryData.Length < 0x15) return str;
                int index = 0;
                Latitude = (Int32)Util.Util.ToUint32(item.ArraryData, index);
                index += 4;
                Longitude = (Int32)Util.Util.ToUint32(item.ArraryData, index);
                index += 4;
                Speed = Util.Util.ToUint16(item.ArraryData, index);
                index += 2;
                Direction = Util.Util.ToUint16(item.ArraryData, index);
                index += 2;

                Altitude = (Int16)Util.Util.ToUint16(item.ArraryData, index);
                index += 2;
                Hop = Util.Util.ToUint16(item.ArraryData, index);
                index += 2;
                Mileage = Util.Util.ToUint32(item.ArraryData, index);
                index += 4;
                NumberofStatellites = item.ArraryData[index];
                index++;

                double f = Latitude;
                f = f / 10000000;
                str += Util.Util.FormtShow("Latitude", f.ToString(), spaceOffse, nameMaxLen);
                f = Longitude;
                f = f / 10000000;
                str += Util.Util.FormtShow("Longitude", f.ToString(), spaceOffse, nameMaxLen);
                str += Util.Util.FormtShow("Speed", Speed.ToString() + " km/h", spaceOffse, nameMaxLen);
                str += Util.Util.FormtShow("Direction", Direction.ToString(), spaceOffse, nameMaxLen);
                str += Util.Util.FormtShow("Altitude", Altitude.ToString(), spaceOffse, nameMaxLen);


                f = Hop;
                f = f / 10;
                str += Util.Util.FormtShow("Hop", f.ToString(), spaceOffse, nameMaxLen);
                str += Util.Util.FormtShow("Mileage", Mileage.ToString(), spaceOffse, nameMaxLen);
                str += Util.Util.FormtShow("Num of state", NumberofStatellites.ToString(), spaceOffse, nameMaxLen);
                return str;
            }
        }
        #endregion

        #region BLELocation
        public struct BLELocation
        {
            public UInt32 Latitude { get; set; }
            public UInt32 Longitude { get; set; }
            public String Address { get; set; }
            public byte[] Mac { get; set; }
            public string ParseString(EviewType item, int spaceOffse, int nameMaxLen)
            {
                string str = "";
                if ((Command)item.Commad != Command.Data) return str;
                if ((DataKey)item.Type != DataKey.BLELocation) return str;
                if (item.ArraryData.Length < 0x6) return str;




                int index = 0;
                Mac = new byte[6];
                Array.Copy(item.ArraryData, index, Mac, 0, 6);
                index += 6;
                if (item.ArraryData.Length > 0xe)
                {
                    Latitude = Util.Util.ToUint32(item.ArraryData, index);
                    index += 4;
                    Longitude = Util.Util.ToUint32(item.ArraryData, index);
                    index += 4;

                    if (item.ArraryData.Length > 0xf)
                    {

                        Address = System.Text.Encoding.Default.GetString(item.ArraryData, index, item.ArraryData.Length - index);
                    }
                    else
                    {
                        Address = "";
                    }
                }
                else
                {
                    Latitude = 0;
                    Longitude = 0;
                }




                str += Util.Util.FormtShow("Mac", Util.Util.MacToString(Mac, 0), spaceOffse, nameMaxLen);
                double f = Latitude;
                f = f / 10000000;
                str += Util.Util.FormtShow("Latitude", f.ToString(), spaceOffse, nameMaxLen);
                f = Longitude;
                f = f / 10000000;
                str += Util.Util.FormtShow("Longitude", f.ToString(), spaceOffse, nameMaxLen);

                str += Util.Util.FormtShow("Address", Address, spaceOffse, nameMaxLen);
                return str;
            }
        }
        #endregion

        #region BeaconLocation
        public struct BeaconLocation
        {
            public byte Flag { get; set; }
            public byte[] Mac { get; set; }
            public sbyte Rssi { get; set; }
            public sbyte Rssi1M { get; set; }


            public UInt32 Latitude { get; set; }
            public UInt32 Longitude { get; set; }
            public String Address { get; set; }

            public string ParseString(EviewType item, int spaceOffse, int nameMaxLen)
            {
                string str = "";
                if ((Command)item.Commad != Command.Data) return str;
                if ((DataKey)item.Type != DataKey.BeaconLocation) return str;
                if (item.ArraryData.Length < 18) return str;




                int index = 0;
                Flag = item.ArraryData[index++];
                Mac = new byte[6];
                Array.Copy(item.ArraryData, index, Mac, 0, 6);
                index += 6;
                Rssi = (sbyte)item.ArraryData[index++];
                Rssi1M = (sbyte)item.ArraryData[index++];
                Latitude = Util.Util.ToUint32(item.ArraryData, index);
                index += 4;
                Longitude = Util.Util.ToUint32(item.ArraryData, index);
                index += 4;

                if (item.ArraryData.Length > 19)
                {
                    Address = System.Text.Encoding.Default.GetString(item.ArraryData, index, item.ArraryData.Length - index);
                }
                else
                {
                    Address = "";
                }


                str += Util.Util.FormtShow("Flag", Util.Util.HexToString(Flag), spaceOffse, nameMaxLen);


                str += Util.Util.FormtShow("Mac", Util.Util.MacToString(Mac, 0), spaceOffse, nameMaxLen);
                str += Util.Util.FormtShow("Rssi", Rssi.ToString(), spaceOffse, nameMaxLen);
                str += Util.Util.FormtShow("Rssi", Rssi1M.ToString(), spaceOffse, nameMaxLen);

                double f = Latitude;
                f = f / 10000000;
                str += Util.Util.FormtShow("Latitude", f.ToString(), spaceOffse, nameMaxLen);
                f = Longitude;
                f = f / 10000000;
                str += Util.Util.FormtShow("Longitude", f.ToString(), spaceOffse, nameMaxLen);

                str += Util.Util.FormtShow("Address", Address, spaceOffse, nameMaxLen);
                return str;
            }
        }
        #endregion

        #region WifiLocation
        public struct WifiInfo
        {
            public sbyte RSSI { get; set; }
            public byte[] Mac { get; set; }
        }
        public struct WifiLocation
        {

            public List<WifiInfo> Wifi { get; set; }
            public string ParseString(EviewType item, int spaceOffse, int nameMaxLen)
            {

                string str = "";
                Wifi = new List<WifiInfo>();
                if ((Command)item.Commad != Command.Data) return str;
                if ((DataKey)item.Type != DataKey.WIFILocation) return str;
                if (item.ArraryData.Length % 7 != 0) return str;
                for (int i = 0; i < item.ArraryData.Length;)
                {
                    WifiInfo info = new WifiInfo();
                    info.RSSI = (sbyte)item.ArraryData[i];
                    info.Mac = new byte[6];
                    //       Array.Copy(item.ArraryData, i+1, info.Mac, 0, 6);
                    for (int j = 0; j < 6; j++)
                    {
                        info.Mac[j] = item.ArraryData[j + i + 1];
                    }

                    Wifi.Add(info);
                    i += 7;
                }

                foreach (var wifi in Wifi)
                {

                    str += Util.Util.FormtShowNoEnter("Mac", Util.Util.MacToString(wifi.Mac, 0), spaceOffse, nameMaxLen);
                    str += "    Rssi:" + wifi.RSSI.ToString() + "\r\n";

                }
                return str;
            }
        }
        #endregion

        #region HomeWifiLocation
        public struct HomeWifiLocation
        {
            public byte Flag { get; set; }
            public byte[] Mac { get; set; }
            public sbyte Rssi { get; set; }

            public UInt32 Latitude { get; set; }
            public UInt32 Longitude { get; set; }
            public String Address { get; set; }

            public string ParseString(EviewType item, int spaceOffse, int nameMaxLen)
            {
                string str = "";
                if ((Command)item.Commad != Command.Data) return str;
                if (item.ArraryData.Length < 17) return str;




                int index = 0;
                Flag = item.ArraryData[index++];
                Mac = new byte[6];
                Array.Copy(item.ArraryData, index, Mac, 0, 6);
                index += 6;
                Rssi = (sbyte)item.ArraryData[index++];
                Latitude = Util.Util.ToUint32(item.ArraryData, index);
                index += 4;
                Longitude = Util.Util.ToUint32(item.ArraryData, index);
                index += 4;

                if (item.ArraryData.Length > 18)
                {
                    Address = System.Text.Encoding.Default.GetString(item.ArraryData, index, item.ArraryData.Length - index);
                }
                else
                {
                    Address = "";
                }


                str += Util.Util.FormtShow("Flag", Util.Util.HexToString(Flag), spaceOffse, nameMaxLen);


                str += Util.Util.FormtShow("Mac", Util.Util.MacToString(Mac, 0), spaceOffse, nameMaxLen);
                str += Util.Util.FormtShow("Rssi", Rssi.ToString(), spaceOffse, nameMaxLen);
                double f = Latitude;
                f = f / 10000000;
                str += Util.Util.FormtShow("Latitude", f.ToString(), spaceOffse, nameMaxLen);
                f = Longitude;
                f = f / 10000000;
                str += Util.Util.FormtShow("Longitude", f.ToString(), spaceOffse, nameMaxLen);

                str += Util.Util.FormtShow("Address", Address, spaceOffse, nameMaxLen);
                return str;
            }
        }
        #endregion


        #region LBSLocation
        public struct LBSInfo
        {

            public byte RXL { get; set; }
            public UInt16 LAC { get; set; }
            public UInt16 CellID { get; set; }

        }
        public struct LBSLocation
        {
            public UInt16 MCC { get; set; }
            public byte MNC { get; set; }
            public List<LBSInfo> LBS { get; set; }
            public string ParseString(EviewType item, int spaceOffse, int nameMaxLen)
            {

                string str = "";
                LBS = new List<LBSInfo>();
                int len = item.ArraryData.Length;
                len -= 3;
                int index = 0;
                if ((Command)item.Commad != Command.Data) return str;
                if ((DataKey)item.Type != DataKey.LBSLocation) return str;
                if (len % 5 != 0) return str;
                MCC = Util.Util.ToUint16(item.ArraryData, index);
                index += 2;
                MNC = item.ArraryData[index];
                index++;

                for (int i = 3; i < item.ArraryData.Length;)
                {
                    LBSInfo info = new LBSInfo();
                    info.RXL = item.ArraryData[i];
                    info.LAC = Util.Util.ToUint16(item.ArraryData, i + 1);
                    info.CellID = Util.Util.ToUint16(item.ArraryData, i + 3);
                    LBS.Add(info);
                    i += 5;
                }

                str += Util.Util.FormtShow("MCC", MCC.ToString(), spaceOffse, nameMaxLen);
                str += Util.Util.FormtShow("MNC", MNC.ToString(), spaceOffse, nameMaxLen);


                foreach (var lbs in LBS)
                {

                    str += Util.Util.FormtShowNoEnter("RXL", lbs.RXL.ToString(), spaceOffse, nameMaxLen);
                    str += Util.Util.FormtShowNoEnter("LAC", lbs.LAC.ToString(), 2, 5);
                    str += Util.Util.FormtShow("CellID", lbs.CellID.ToString(), 2, 5);

                }
                return str;
            }
        }
        #endregion
        #region Activity
        public struct ActivityInfo
        {
            public UInt32 UTC { get; set; }
            public UInt32 Value { get; set; }
        }

        public struct Activity
        {
            public List<ActivityInfo> AcitivityList { get; set; }
            public string ParseString(EviewType item, int spaceOffse, int nameMaxLen)
            {

                string str = "";
                AcitivityList = new List<ActivityInfo>();
                int len = item.ArraryData.Length;

                int index = 0;
                if ((Command)item.Commad != Command.Data) return str;
                if ((DataKey)item.Type != DataKey.ActivityDegree) return str;
                if (len % 8 != 0) return str;

                for (int i = 0; i < item.ArraryData.Length;)
                {
                    ActivityInfo info = new ActivityInfo();
                    info.UTC = Util.Util.ToUint32(item.ArraryData, i);
                    info.Value = Util.Util.ToUint32(item.ArraryData, i + 4);
                    AcitivityList.Add(info);
                    i += 8;
                }

                foreach (var act in AcitivityList)
                {
                    System.DateTime dateTime = Util.UTCToTime.ToTime((double)act.UTC, 0);
                    str += Util.Util.FormtShowNoEnter("UTC", dateTime.ToString("yyyy-MM-dd HH:mm:ss"), spaceOffse, nameMaxLen);

                    str += Util.Util.FormtShow("Value", act.Value.ToString(), spaceOffse, nameMaxLen);

                }
                return str;
            }
        }
        #endregion

        #region AlarmCode
        public struct AlarmCode
        {
            public UInt32 UTC { get; set; }
            private UInt32 code;
            public UInt32 Code
            {
                get
                {
                    return code;
                }
                set
                {
                    code = value;
                    BatteryLow = (value & 0x01) > 0;
                    OverSpeedAlert = (value & 0x02) > 0;
                    FallDownAlert = (value & 0x04) > 0;
                    TiltAlert = (value & 0x08) > 0;
                    GEO1Alert = (value & 0x10) > 0;
                    GEO2Alert = (value & 0x20) > 0;
                    GEO3Alert = (value & 0x40) > 0;
                    GEO4Alert = (value & 0x80) > 0;

                    PowerOffAlert = (value & 0x0100) > 0;
                    PowerOnAlert = (value & 0x0200) > 0;
                    MotionAlert = (value & 0x0400) > 0;
                    NoMotionAlert = (value & 0x0800) > 0;

                    SOSKeyAlert = (value & 0x1000) > 0;
                    Side1Alert = (value & 0x2000) > 0;
                    Side2Alert = (value & 0x4000) > 0;



                }
            }

            public bool BatteryLow { get; set; }
            public bool OverSpeedAlert { get; set; }

            public bool FallDownAlert { get; set; }

            public bool TiltAlert { get; set; }
            public bool GEO1Alert { get; set; }
            public bool GEO2Alert { get; set; }
            public bool GEO3Alert { get; set; }
            public bool GEO4Alert { get; set; }
            public bool PowerOffAlert { get; set; }
            public bool PowerOnAlert { get; set; }
            public bool MotionAlert { get; set; }
            public bool NoMotionAlert { get; set; }
            public bool SOSKeyAlert { get; set; }
            public bool Side1Alert { get; set; }
            public bool Side2Alert { get; set; }


            public string ParseString(EviewType item, int spaceOffse, int nameMaxLen)
            {
                string str = "";
                if ((Command)item.Commad != Command.Data) return str;
                if ((DataKey)item.Type != DataKey.AlarmCode) return str;
                if (item.ArraryData.Length < 4) return str;
                int index = 0;
                Code = Util.Util.ToUint32(item.ArraryData, index);
                index += 4;
                if (item.ArraryData.Length < 8)
                {
                    UTC = 0;
                }
                else
                {
                    UTC = Util.Util.ToUint32(item.ArraryData, index);
                    index += 4;
                }

                str += Util.Util.FormtShow("Status", Util.Util.HexToString(Code), spaceOffse, nameMaxLen);


                str += Util.Util.Space(spaceOffse) + "[" + "\r\n";
                int space = spaceOffse + 2;
                int namecount = nameMaxLen;
                str += Util.Util.FormtShow("Battery Low", BatteryLow.ToString(), space, namecount);
                str += Util.Util.FormtShow("Over speed Alert", OverSpeedAlert.ToString(), space, namecount);
                str += Util.Util.FormtShow("Falldown Alert", FallDownAlert.ToString(), space, namecount);
                str += Util.Util.FormtShow("Tilt Alert", TiltAlert.ToString(), space, namecount);

                str += Util.Util.FormtShow("GEO 1 Alert", GEO1Alert.ToString(), space, namecount);
                str += Util.Util.FormtShow("GEO 2 Alert", GEO2Alert.ToString(), space, namecount);
                str += Util.Util.FormtShow("GEO 3 Alert", GEO3Alert.ToString(), space, namecount);
                str += Util.Util.FormtShow("GEO 4 Alert", GEO4Alert.ToString(), space, namecount);

                str += Util.Util.FormtShow("Power off Alert", PowerOffAlert.ToString(), space, namecount);
                str += Util.Util.FormtShow("Power on Alert", PowerOnAlert.ToString(), space, namecount);

                str += Util.Util.FormtShow("Motion Alert", MotionAlert.ToString(), space, namecount);
                str += Util.Util.FormtShow("No Motion Alert", NoMotionAlert.ToString(), space, namecount);
                str += Util.Util.FormtShow("SOS Key Alert", SOSKeyAlert.ToString(), space, namecount);
                str += Util.Util.FormtShow("Side 1 Alert", Side1Alert.ToString(), space, namecount);
                str += Util.Util.FormtShow("Side 2 Alert", Side2Alert.ToString(), space, namecount);




                str += Util.Util.Space(spaceOffse) + "]" + "\r\n";


                if (UTC > 0)
                {
                    System.DateTime dateTime = Util.UTCToTime.ToTime((double)UTC, 0);
                    str += Util.Util.FormtShow("UTC", dateTime.ToString("yyyy-MM-dd HH:mm:ss"), spaceOffse, nameMaxLen);
                }
                return str;
            }
        }
        #endregion



        #endregion
        delegate string DelParseKeyString(EviewType item, int spaceOffse, int nameMaxLen);
        private static string ParseData(EviewType item, int spaceOffse, int nameMaxLen)
        {
            DelParseKeyString delParseKeyString = null;
            string str = "";
            if ((Command)item.Commad != Command.Data) return str;
            str += Util.Util.FormtShow("Length", item.Length.ToString(), spaceOffse, nameMaxLen);
            str += Util.Util.FormtShow("Key", Util.EnumHelper.GetDescription((DataKey)item.Type), spaceOffse, nameMaxLen);
            switch ((DataKey)item.Type)
            {
                case DataKey.DeviceID:
                    DeviceID device = new DeviceID();
                    delParseKeyString = device.ParseString;
                    break;
                case DataKey.GeneralData:
                    GeneralData generalData = new GeneralData();
                    delParseKeyString = generalData.ParseString;
                    break;
                case DataKey.AlarmCode:
                    AlarmCode alarmCode = new AlarmCode();
                    delParseKeyString = alarmCode.ParseString;
                    break;
                case DataKey.WIFILocation:
                    WifiLocation wifiLocation = new WifiLocation();
                    delParseKeyString = wifiLocation.ParseString;
                    break;
                case DataKey.GPSLocation:
                    GPSLocation gpsLoc = new GPSLocation();
                    delParseKeyString = gpsLoc.ParseString;
                    break;

                case DataKey.LBSLocation:
                    LBSLocation lbs = new LBSLocation();
                    delParseKeyString = lbs.ParseString;
                    break;

                case DataKey.BLELocation:
                    BLELocation ble = new BLELocation();
                    delParseKeyString = ble.ParseString;
                    break;
                case DataKey.BeaconLocation:
                    BeaconLocation beacon = new BeaconLocation();
                    delParseKeyString = beacon.ParseString;
                    break;
                case DataKey.HomeWifiLocation:
                    HomeWifiLocation homewifi = new HomeWifiLocation();
                    delParseKeyString = homewifi.ParseString;
                    break;
                case DataKey.ActivityDegree:
                    Activity activity = new Activity();
                    delParseKeyString = activity.ParseString;
                    break;
            }

            if (delParseKeyString != null)
            {
                str += delParseKeyString(item, spaceOffse, nameMaxLen);
            }
            return str;
        }


        public static string Parse(EviewType item, int spaceOffse, int nameMaxLen)
        {

            int space = spaceOffse;


            string str = "";
            str += Util.Util.Space(space);
            str += Util.Util.HexToString(item.SourceData, 0, item.SourceData.Length);
            str += "\r\n";

            Command cmd = (Command)item.Commad;
            switch (cmd)
            {
                case Command.Data:
                    str += ParseData(item, spaceOffse, nameMaxLen);
                    break;
            }
            str += "\r\n";
            return str;
        }

    }




}
