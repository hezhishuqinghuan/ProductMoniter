using Modbus.Device;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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

namespace ProductMonitor.UserControls
{
    /// <summary>
    /// ModbusTest.xaml 的交互逻辑
    /// </summary>
    public partial class ModbusTest : UserControl
    {


        public ModbusTest()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 生成校验码
        /// </summary>
        /// <param name="value"></param>
        /// <param name="poly"></param>
        /// <param name="crcInit"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private List<byte> CRC16(List<byte> value, ushort poly = 0xA001, ushort crcInit = 0xFFFF)
        {
            if (value == null || !value.Any())
            {
                throw new ArgumentException("");
            }
            //运算
            ushort crc = crcInit;
            for (int i = 0; i < value.Count; i++)
            {
                crc = (ushort)(value[i] ^ crc);
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (ushort)((crc << 1) ^ poly) : (ushort)(crc >> 1);

                }

            }
            byte hi = (byte)((crc & 0xFF00 >> 8));//高位置
            byte lo = (byte)((crc & 0x00FF));//低位置

            List<byte> bufer = new List<byte>();
            bufer.AddRange(value);
            bufer.Add(lo);
            bufer.Add(hi);
            return bufer;
        }

        private void BtnReadCoilsStatus(object sender, EventArgs e)
        {
            //1、自己组装报文
            //2、通过modbus组件调用

            #region 自己组装报文 了解 面试
            //ushort startAddr = 0;//线圈起始地址
            //ushort readLen = 6;//长度

            ////组装请求报文
            //List<byte> command = new List<byte>();
            //command.Add(0x01);//1号地址
            //command.Add(0x01);//功能码 读线圈状态

            ////起始地址
            //command.Add(BitConverter.GetBytes(startAddr)[1]);//地址起始高位
            //command.Add(BitConverter.GetBytes(startAddr)[0]);//地址起始高位

            ////读取数据
            //command.Add(BitConverter.GetBytes(readLen)[1]);//读取数量高位
            //command.Add(BitConverter.GetBytes(readLen)[0]);//读取数量低位

            ////CRC
            //command=CRC16(command);

            ////发送Rtu(串口) SerialPort
            //using (SerialPort serialPort=new SerialPort("COM1",9600,Parity.None,8,StopBits.One))
            //{
            //    serialPort.Open();//打开串口
            //    serialPort.Write(command.ToArray(), 0, command.Count);

            //    //接收响应报文并解析
            //    byte[] respBytes = new byte[serialPort.BytesToRead];
            //    serialPort.Read(respBytes, 0, respBytes.Length);//丢数据包 线程

            //    //校验 校验位
            //    List<byte> respList = new List<byte>(respBytes);//respBytes.ToList();
            //    respList.RemoveRange(0, 3);//移除前三位，前三位是地址、功能码、字节数，而我们需要的是第四位的数据
            //    respList.RemoveRange(respList.Count-2, 2);//移除后两位，后两位是校验位

            //    //反转
            //    respList.Reverse();

            //    var respStrList=respList.Select(m => Convert.ToString(m,2)).ToList();

            //    var result = "";
            //    foreach (string item in respStrList) {
            //        result += item.PadLeft(8, '0');

            //    }

            //    //字符串反转
            //    result=new string(result.ToArray().Reverse<char>().ToArray());
            //    result=result.Length>readLen?result.Substring(0,readLen):result;

            //    MessageBox.Show(result);



            #endregion

            #region 通过modbus组件调用读取

            ushort startAddr = 0;//线圈起始地址
            ushort readLen = 6;//长度

            using (SerialPort serialPort = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One))
            {
                serialPort.Open();
                IModbusSerialMaster master = ModbusSerialMaster.CreateRtu((Modbus.IO.IStreamResource)serialPort);
                bool[] result=master.ReadCoils(1,startAddr, readLen);
            }

            #endregion
        }
        
    }
}
