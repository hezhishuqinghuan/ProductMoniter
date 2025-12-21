using Modbus.Device;
using ProductMonitor.Models;
using ProductMonitor.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProductMonitor.ViewModels
{
    internal class MainWindowVM:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowVM()
        {

            #region 初始化环境监控数据
            EnvironmentColl = new ObservableCollection<EnvironmentModel>();
            EnvironmentColl.Add(new EnvironmentModel { EnItemName = "光照(Lux)", EnItemValue = 123 });
            EnvironmentColl.Add(new EnvironmentModel { EnItemName = "噪音(db)", EnItemValue = 55 });
            EnvironmentColl.Add(new EnvironmentModel { EnItemName = "温度(℃)", EnItemValue = 80 });
            EnvironmentColl.Add(new EnvironmentModel { EnItemName = "湿度(%)", EnItemValue = 43 });
            EnvironmentColl.Add(new EnvironmentModel { EnItemName = "PM2.5(m³)", EnItemValue = 20 });
            EnvironmentColl.Add(new EnvironmentModel { EnItemName = "硫化氢(PPM)", EnItemValue = 15 });
            EnvironmentColl.Add(new EnvironmentModel { EnItemName = "氮气(PPM)", EnItemValue = 18 });

            //从设备读取数据（异步）
            Task.Run(async () =>
            {
                while (true)
                {
                    using (SerialPort serialPort = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One))
                    {
                        serialPort.Open();
                        Modbus.Device.IModbusMaster master = Modbus.Device.ModbusSerialMaster.CreateRtu(serialPort);

                        ushort[] values = master.ReadHoldingRegisters(1, 0, 7);//从设备地址，寄存器起始地址，寄存器个数
                                                                               //功能码03
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                EnvironmentColl[i].EnItemValue = values[i];
                            }
                        });
                        await Task.Delay(1000);
                    }
                }
            });
            #endregion

            #region 初始化报警列表
            AlarmList = new List<AlarmModel>();
            AlarmList.Add(new AlarmModel { Num = "01", Msg = "设备温度过高", Time = "2025-09-21 18:34:56", Duration = 7 });
            AlarmList.Add(new AlarmModel { Num = "02", Msg = "车间温度过高", Time = "2025-09-21 20:40:55", Duration = 10 });
            AlarmList.Add(new AlarmModel { Num = "03", Msg = "设备转速过快", Time = "2025-09-21 12:24:34", Duration = 12 });
            AlarmList.Add(new AlarmModel { Num = "04", Msg = "设备气压偏低", Time = "2025-09-21 17:34:56", Duration = 90 });
            #endregion

         

            #region 初始化设备监控
            DeviceList = new List<DeviceModel>();
            DeviceList.Add(new DeviceModel { DeviceItem = "电能(Kw.h)", Value = 60.8 });
            DeviceList.Add(new DeviceModel { DeviceItem = "电压(V)", Value = 390 });
            DeviceList.Add(new DeviceModel { DeviceItem = "电流(A)", Value = 5 });
            DeviceList.Add(new DeviceModel { DeviceItem = "压差(kpa)", Value = 13 });
            DeviceList.Add(new DeviceModel { DeviceItem = "温度(℃)", Value = 36 });
            DeviceList.Add(new DeviceModel { DeviceItem = "振动(mm/s)", Value = 4.1 });
            DeviceList.Add(new DeviceModel { DeviceItem = "转速(r/mm)", Value = 2600 });
            DeviceList.Add(new DeviceModel { DeviceItem = "气压(kpa)", Value = 0.5 });


            #endregion

            #region 初始化雷达数据
            RaderList = new List<RaderModel>();
            RaderList.Add(new RaderModel { ItemName = "排烟风扇", Value = 90 });
            RaderList.Add(new RaderModel { ItemName = "客梯", Value = 30.00 });
            RaderList.Add(new RaderModel { ItemName = "供水机", Value = 34.89 });
            RaderList.Add(new RaderModel { ItemName = "喷淋水泵", Value = 69.59 });
            RaderList.Add(new RaderModel { ItemName = "稳压设备", Value = 20 });
            RaderList.Add(new RaderModel { ItemName = "稳压设备2", Value = 65 });

            #endregion

            #region 初始化人员缺岗信息
            StuffOutWorkList = new List<StuffOutWorkModel>();
            StuffOutWorkList.Add(new StuffOutWorkModel { StuffName = "张晓婷", Position = "技术员", OutWorkCount = 123 });
            StuffOutWorkList.Add(new StuffOutWorkModel { StuffName = "李晓", Position = "操作员", OutWorkCount = 23 });
            StuffOutWorkList.Add(new StuffOutWorkModel { StuffName = "王丽福", Position = "技术员", OutWorkCount = 143 });
            StuffOutWorkList.Add(new StuffOutWorkModel { StuffName = "成晨", Position = "统计员", OutWorkCount = 113 });
            StuffOutWorkList.Add(new StuffOutWorkModel { StuffName = "李晓婷", Position = "技术员", OutWorkCount = 12 });

            #endregion

            #region 初始化车间列表
            WorkShopList = new List<WorkShopModel>();
            WorkShopList.Add(new WorkShopModel { WorkShopName = "贴片车间", WorkingCount = 73, WrongCount = 12, WaitCount = 8, StopCount = 0 });
            WorkShopList.Add(new WorkShopModel { WorkShopName = "封片车间", WorkingCount = 33, WrongCount = 12, WaitCount = 8, StopCount = 0 });
            WorkShopList.Add(new WorkShopModel { WorkShopName = "焊接车间", WorkingCount = 51, WrongCount = 12, WaitCount = 8, StopCount = 0 });
            WorkShopList.Add(new WorkShopModel { WorkShopName = "贴片车间", WorkingCount = 65, WrongCount = 12, WaitCount = 8, StopCount = 0 });
            #endregion

            #region 初始化机台列表
            MachineList = new List<MachineModel>();
            Random random = new Random();
            for (int i = 0; i < 20; i++)
            {
                int plan = random.Next(100, 1000);//计划数 随机量
                int finish = random.Next(0, plan);//已完成数
                MachineList.Add(new MachineModel
                {
                    MachineName = "焊接机-" + (i + 1),
                    FinishCount = finish,
                    PlanCount = plan,
                    Statue = "作业中",
                    OrderNo = "H20252234144"
                });
            }

            #endregion

        }





        /// <summary>
        /// 监控用户控件
        /// </summary>
        private UserControl _MonitorUC;

		/// <summary>
		/// 监控用户控件
		/// </summary>
		public UserControl MonitorUC
        {
			get { 
                if (_MonitorUC == null)
                {
                    _MonitorUC = new MonitorUC();
                }
                
                return _MonitorUC; 
            }
			set { 
                _MonitorUC = value; 
                if(PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MonitorUC"));
                }
            
            }
		}
        #region 时间 日期

        /// <summary>
        /// 时间 小时:分钟
        /// </summary>
        public string TimeStr
        {
            get { return DateTime.Now.ToString("HH:mm"); }
           
        }
        /// <summary>
        /// 日期 年-月-日
        /// </summary>
        public string DateStr
        {
            get { return DateTime.Now.ToString("yyyy-MM-dd"); }

        }
        /// <summary>
        /// 星期 
        /// </summary>
        public string WeekStr
        {
            get
            {
                int index = (int)DateTime.Now.DayOfWeek;
                string[] week = new string[7] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
                return week[index];

            }

        }
        #endregion

        #region 计数

        /// <summary>
        /// 机台总数
        /// </summary>
        private string _MachineCount="0298";

        public string MachineCount
        {
            get { return _MachineCount; }
            set { 
                
                _MachineCount= value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MachineCount"));
                }

            }
        }

        /// <summary>
        /// 生产计数
        /// </summary>
        private string _ProductCount = "1825";

        public string ProductCount
        {
            get { return _ProductCount; }
            set
            {

                _ProductCount = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ProductCount"));
                }

            }
        }

        /// <summary>
        /// 不良计数
        /// </summary>
        private string _BadCount = "0312";

        public string BadCount
        {
            get { return _BadCount; }
            set
            {

                BadCount = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("BadCount"));
                }

            }
        }
        #endregion

        #region 环境监控数据
        /// <summary>
        /// 环境监控数据
        /// </summary>
        private ObservableCollection<EnvironmentModel>  _EnvironmentColl;

        public ObservableCollection<EnvironmentModel>  EnvironmentColl
        {
            get { return _EnvironmentColl; }
            set { 
                _EnvironmentColl = value;
                if(PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("EnvironmentColl"));
                }
            }
        }

       


        #endregion

        #region 报警属性
        private List<AlarmModel> _AlarmList;

        public List<AlarmModel> AlarmList
        {
            get { return _AlarmList; }
            set { 
                
                _AlarmList = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AlarmList"));
                }

            }
        }

        #endregion

     

        #region 设备集合

        private List<DeviceModel> _DeviceList;

        public List<DeviceModel> DeviceList
        {
            get { return _DeviceList; }
            set { 
                
                _DeviceList = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("DeviceList"));
                }

            }
        }
        #endregion

        #region 雷达数据属性

        /// <summary>
        /// 雷达
        /// </summary>
        private List<RaderModel> _RaderList;

        public List<RaderModel> RaderList
        {
            get { return _RaderList; }
            set { 
                _RaderList = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("RaderList"));
                }
            }
        }
        #endregion

        #region 缺岗员工属性
        /// <summary>
        /// 缺岗员工
        /// </summary>
        private List<StuffOutWorkModel> _StuffOutWorkList;

        public List<StuffOutWorkModel> StuffOutWorkList
        {
            get { return _StuffOutWorkList; }
            set { 
                
                _StuffOutWorkList = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("StuffOutWorkList"));
                }

            }
        }

        #endregion

        #region 车间属性
        private List<WorkShopModel> _WorkShopList;

        public List<WorkShopModel> WorkShopList
        {
            get { return _WorkShopList; }
            set { 
                _WorkShopList = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("WorkShopList"));
                }

            }
        }

        #endregion

        #region 机台集合属性
        private List<MachineModel> _MachineList;

        public List<MachineModel> MachineList
        {
            get { return _MachineList; }
            set { 
                
                _MachineList = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MachineList"));
                }


            }
        }

        #endregion
    }
}
