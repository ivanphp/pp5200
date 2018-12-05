using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSCommLib;
using System.Net;
using System.IO;
using Microsoft.VisualBasic;
using System.Threading;

namespace ConsoleApplication4
{
    class Program
    {
        #region 版面相關設定
        public static byte[] Print = new byte[] { 0x0C };                   // FF Print and return to standard mode (in page mode),印出資料
        public static byte[] ToCenter = new byte[] { 0x1B, 0x61, 0x01 };    // Select justification ESC a 1  (0:Left ,1:Center ,2:Right) ,置中對齊
        public static byte[] ToLeft = new byte[] { 0x1B, 0x61, 0x00 };      // Select justification ESC a 1  (0:Left ,1:Center ,2:Right) ,靠左對齊
        public static byte[] ToRight = new byte[] { 0x1B, 0x61, 0x02 };     // Select justification ESC a 1  (0:Left ,1:Center ,2:Right) ,靠右對其
        public static byte[] CutPaper = new byte[] { 0x1D, 0x56, 0x00 };    // 切紙

        public static byte[] PrintNV = new byte[] { 0x1C, 0x70, 0x01, 0x00 };    // Printer NV-BitMap
        
        public static byte[] linefeed = new byte[] { 0x0A };                // 換行
        public static byte[] Magnify = new byte[] { 0x1B, 0x4D, 0x01 };     // Select character font ESC M n ,
        public static byte[] Initialize = new byte[] { 0x1B, 0x40 };        // Initialize printer  ESC @ ,初始
        public static byte[] Tab = new byte[] { 0x09 };
        #endregion

        #region 字形設定
        public static byte[] TurnSmoothing = new byte[] { 0x1D, 0x62, 0x01 };           // GS b Turn smoothing mode on/off ,可使放大字體減少鋸齒現象
        public static byte[] TurnWhite = new byte[] { 0x1D, 0x42, 0x00 };               // GS B Turn white/black reverse print mode on/off 
        public static byte[] CharacterDoubleSize = new byte[] { 0x1D, 0x21, 0x11 };     // GS ! n (n=0 標準大小,n=1 倍高倍寬) ,倍高倍寬
        public static byte[] CharacterNormalSize = new byte[] { 0x1D, 0x21, 0x00 };     // GS ! n (n=0 標準大小,n=1 倍高倍寬) ,正常
        public static byte[] PageSpacing = new byte[] { 0x1B, 0x33 };                   // ESC 2 ,行距2
        #endregion

        #region BarCode設定
        public static byte[] BarCodeHRI = new byte[] { 0x1D, 0x48, 0x00 };      // GS H n : Select print position of HRI characters (0x00:不列印,0x02:印),資訊是否印在下方
        public static byte[] BarCodeHeight = new byte[] { 0x1D, 0x68, 0x28 };   // GS h n : Set bar code height ,BarCode高度 1cm = 80(10),
        public static byte[] BarCodeWidth = new byte[] { 0x1D, 0x77, 0x01 };    // GS w n : Set bar code width ,因為使用 58mm 紙寬並列印 19 碼, 故 n 只能 = 1 才能列印
        #endregion

        #region 設定列印範圍
        /*
         * GS P = Set horizontal and vertical motion units
         * 設定印表機的x,y方向的基本單位值
         * 列印解析度為203 DPI 
         */
        public static byte[] BasicCalculation = new byte[] { 0x1D, 0x50, 0x00, 0xCB };
        public static byte[] PageMode = new byte[] { 0x1B, 0x4C };                  // ESC L ,選擇Page Mode
        public static byte[] PagePrintDirection = new byte[] { 0x1B, 0x54, 0x31 }; // ESC T n 設定Page Mode的列印方向

        /*
         * ESC W xL xH yL yH dxL dxH dyL dyH
         * xL=00 xH=00 --> 水平方向起印位置在 (xL + xH*256 = 0 + 0*256 = 0 ) 第 0 個點的位置
         * yL=00 yH=00 --> 垂直方向起印位置在 (yL + yH*256 = 0 + 0*256 = 0 ) 第 0 個點的位置
         * dxL dxH --> 水平列印寬度為 (dxL + dxH*256 = 160 + 1*256 = 416 )個點的距離 dxL = A0 dxH =1
         * dyL dyH --> 垂直列印長度為 (dyL + dyH*256 = 168 + 2*256 = 680 )個點的距離 dyL = A8 dyH=2
         * */
        public static byte[] PageModeArea = new byte[] { 0x1B, 0x57, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x01, 0x30, 0x01 };
        #endregion

        #region QRCode用到相關指令
        public static byte[] PrintQRCode = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30 };
        //public static byte[] _sizeQR = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x04 };
        public static byte[] QRCodeSize = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x03 };       //GS ( k pL pH cn fn n : Set the size of module ,QRCode大小 
        public static byte[] QRErrorLevel = { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x31 };     //GS ( k pL pH cn fn n : Select the error correction level ,QRCode錯誤階層
        public static byte[] StoreQRCode = { 0x1D, 0x28, 0x6B, 0x83, 0x00, 0x31, 0x50, 0x30 };
        public static byte[] QRCode_1_X = { 0x1B, 0x24, 0x3C, 0x00 };   //QRCode 1 X軸的位置
        public static byte[] QRCode_2_X = { 0x1B, 0x24, 0xF0, 0x00 };   //QRCode 2 X軸的位置

        public static byte[] End_X = { 0x1B, 0x24, 0x1E, 0x00 };        //電子發票結尾X軸位置
        public static byte[] End_Y = { 0x1D, 0x24, 0x00, 0x01 };       //電子發票結尾Y軸位置
        #endregion
        static void Main(string[] args)
        {


            try
            {
              
                string port = "COM3";


            SerialPort MSComm1 = new SerialPort(port, 19200, Parity.None, 8, StopBits.One);
                MSComm1.Encoding = Encoding.GetEncoding("BIG5");
                MSComm1.Open();
                //MSComm1.Write(CutPaper, 0, CutPaper.Length);
                if (!MSComm1.IsOpen)
                {
                    // 設定 PORT 接收事件
                    MSComm1.Open();
                    // 清空 serial port 的緩存
                    MSComm1.DiscardInBuffer();
                    MSComm1.DiscardOutBuffer();
                    //Thread.Sleep(50);
                }
                MSComm1.Write(Initialize, 0, Initialize.Length);
                //置中
                MSComm1.Write(ToCenter, 0, ToCenter.Length);
                //列印LOGO
                MSComm1.Write(PrintNV, 0, PrintNV.Length);

                MSComm1.Write(linefeed, 0, linefeed.Length);
                byte[] titleContent = System.Text.Encoding.Default.GetBytes("電子發票證明聯" );
                MSComm1.Write(ToCenter, 0, ToCenter.Length);
                MSComm1.Write(TurnSmoothing, 0, TurnSmoothing.Length);
                MSComm1.Write(TurnWhite, 0, TurnWhite.Length);
                MSComm1.Write(CharacterDoubleSize, 0, CharacterDoubleSize.Length);
                MSComm1.Write(titleContent, 0, titleContent.Length);
                MSComm1.Write(linefeed, 0, linefeed.Length);

                //列印空白
                byte[] endContent = System.Text.Encoding.Default.GetBytes(" ");
                MSComm1.Write(endContent, 0, endContent.Length);
                MSComm1.Write(linefeed, 0, linefeed.Length);
                //列印
                MSComm1.Write(Print, 0, Print.Length);
                

                #region 切紙
                
                MSComm1.Write(CutPaper, 0, CutPaper.Length);

                #endregion

                MSComm1.Close();
               
                Console.Read();
                return;



            }
            catch (Exception ex)
            {
                Console.Write(ex.Message + "\r\n");
                Console.Write(ex.StackTrace + "\r\n");
                Console.Read();
            }


        }


    }


}
