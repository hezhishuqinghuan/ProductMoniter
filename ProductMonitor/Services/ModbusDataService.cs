using Modbus.Device;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ProductMonitor.Services
{
    public class ModbusDataService
    {
        private readonly string _portName = "COM1";
        private readonly int _baudRate = 9600;
        private readonly byte _slaveId = 1;
        private readonly ushort _startAddress = 0;
        private readonly ushort _totalRegisters = 14;//0~13

        //事件：通知环境数据更新
        public event Action<List<int>> OnEnvironmentDataUpdated;
        //事件：通知设备数据更新
        public event Action<List <int>> OnDeviceDataUpdated;

        public async Task StartReadingAsync(CancellationToken cancellationToken)
        {
            int readIndex = 0;
            while(!cancellationToken.IsCancellationRequested) {
                try
                {
                    using (var serialPort = new SerialPort(_portName, _baudRate, Parity.None, 8, StopBits.One))
                    {
                        serialPort.Open();
                        Modbus.Device.IModbusMaster master = Modbus.Device.ModbusSerialMaster.CreateRtu(serialPort);

                        //一次性读取14个寄存器
                        ushort[] rawlValues = master.ReadHoldingRegisters(_slaveId, _startAddress, _totalRegisters);

                        //分离环境数据（0~6）
                        var envValues=new List<int>();
                        for (int i = 0; i < 7; i++)
                        {
                            int baseVal = rawlValues[i];
                            int fluctuation = 0;
                            switch (i)
                            {
                                case 0: fluctuation = (int)(50 * Math.Sin(readIndex / 10.0)); baseVal = Math.Max(100, Math.Min(500, baseVal + fluctuation)); break;
                                case 1: fluctuation = (int)(5 * Math.Sin(readIndex / 7.0)); baseVal = Math.Max(50, Math.Min(70, baseVal + fluctuation)); break;

                                // 温度：允许短暂超限
                                case 2:
                                    fluctuation = (int)(3 * Math.Sin(readIndex / 5.0));
                                    int tempOverride = (readIndex % 60 == 0) ? 32 : (baseVal + fluctuation);
                                    baseVal = Math.Max(20, Math.Min(40, tempOverride)); // 上限提到 40，让 32 通过
                                    break;

                                case 3: fluctuation = (int)(10 * Math.Sin(readIndex / 8.0)); baseVal = Math.Max(40, Math.Min(80, baseVal + fluctuation)); break;
                                case 4:
                                    fluctuation = (int)(20 * Math.Sin(readIndex / 6.0));
                                    int pmOverride = (readIndex % 70 == 0) ? 96 : (baseVal + fluctuation);
                                    baseVal = Math.Max(10, Math.Min(100, pmOverride)); 
                                    break;

                                case 5: fluctuation = (int)(5 * Math.Sin(readIndex / 9.0)); baseVal = Math.Max(10, Math.Min(30, baseVal + fluctuation)); break;
                                case 6: fluctuation = (int)(3 * Math.Sin(readIndex / 4.0)); baseVal = Math.Max(15, Math.Min(25, baseVal + fluctuation)); break;
                            }
                            envValues.Add(baseVal);
                        }

                        //分离设备数据（7~13）
                        var devValues= new List<int>();
                        for (int i = 7; i < 14; i++)
                        {
                            int baseVal=rawlValues[i];
                            int fluctuation = 0;
                            switch (i - 7) //映射为0-6
                            {
                                case 0: fluctuation = (int)(100 * Math.Sin(readIndex / 12.0)); baseVal = Math.Max(1000, Math.Min(5000, baseVal + fluctuation)); break; // 电能

                                // 电压：
                                case 1:
                                    fluctuation = (int)(10 * Math.Sin(readIndex / 11.0));
                                    int voltOverride = (readIndex % 30 == 0) ? 250 : (baseVal + fluctuation);
                                    baseVal = Math.Max(190, Math.Min(260, voltOverride)); // 让 250 通过
                                    break;

                                case 2: 
                                    fluctuation = (int)(50 * Math.Sin(readIndex / 13.0));
                                    int speedOverride = (readIndex % 20 == 0) ? 1210 : (baseVal + fluctuation);
                                    baseVal = Math.Max(800, Math.Min(1220, speedOverride));
                                    break; // 转速
                                case 3: fluctuation = (int)(5 * Math.Sin(readIndex / 10.0)); baseVal = Math.Max(50, Math.Min(80, baseVal + fluctuation)); break; // 气压
                                case 4: fluctuation = (int)(20 * Math.Sin(readIndex / 9.0)); baseVal = Math.Max(30, Math.Min(60, baseVal + fluctuation)); break; // 流量
                                case 5: fluctuation = (int)(2 * Math.Sin(readIndex / 8.0)); baseVal = Math.Max(48, Math.Min(52, baseVal + fluctuation)); break; // 频率
                                case 6: fluctuation = (int)(30 * Math.Sin(readIndex / 7.0)); baseVal = Math.Max(800, Math.Min(1500, baseVal + fluctuation)); break; // 功率
                            }
                            devValues.Add(baseVal);
                        }

                        //回到UI线程更新
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            OnEnvironmentDataUpdated?.Invoke(envValues);
                            OnDeviceDataUpdated?.Invoke(devValues);

                        });

                        readIndex++;
                        await Task.Delay(1000,cancellationToken);

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Modbus 通信失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    await Task.Delay(3000, cancellationToken);

                }
            }
        }


    }
}
