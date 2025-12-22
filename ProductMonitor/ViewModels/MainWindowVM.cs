using Modbus.Device;
using ProductMonitor.Database;
using ProductMonitor.Models;
using ProductMonitor.Services;
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

        public ObservableCollection<EnvironmentModel> EnvironmentColl { get; set; }
        public ObservableCollection<DeviceModel> DeviceColl { get; set; }

        public ObservableCollection<AlarmModel> AlarmList { get; set; }

        private CancellationTokenSource _modbusCts;

        // 添加计数器（类字段）
        private int _snapshotCounter = 0;
        private const int SNAPSHOT_INTERVAL = 10; // 每10秒存一次

        public MainWindowVM()
        {
           

            #region 初始化环境监控数据
            EnvironmentColl = new ObservableCollection<EnvironmentModel>
    {
        new EnvironmentModel { EnItemName = "光照(Lux)", EnItemValue = 0 },
        new EnvironmentModel { EnItemName = "噪音(db)", EnItemValue = 0 },
        new EnvironmentModel { EnItemName = "温度(℃)", EnItemValue = 0 },
        new EnvironmentModel { EnItemName = "湿度(%)", EnItemValue = 0 },
        new EnvironmentModel { EnItemName = "PM2.5(m³)", EnItemValue = 0 },
        new EnvironmentModel { EnItemName = "硫化氢(PPM)", EnItemValue = 0 },
        new EnvironmentModel { EnItemName = "氮气(PPM)", EnItemValue = 0 }
    };
            #endregion

            #region 初始化报警列表
            AlarmList = new ObservableCollection<AlarmModel>();
            #endregion



            #region 初始化设备监控
            DeviceColl = new ObservableCollection<DeviceModel>
    {
        new DeviceModel { DeviceItem = "总电能(KW)", Value = 0 },
        new DeviceModel { DeviceItem = "电压(V)", Value = 0 },
        new DeviceModel { DeviceItem = "转速(r/min)", Value = 0 },
        new DeviceModel { DeviceItem = "气压(MPa)", Value = 0 },
        new DeviceModel { DeviceItem = "流量(m³/h)", Value = 0 },
        new DeviceModel { DeviceItem = "频率(Hz)", Value = 0 },
        new DeviceModel { DeviceItem = "功率(W)", Value = 0 }
    };

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

            #region 启动Modbus服务并订阅事件
            _modbusCts = new CancellationTokenSource();
            var modbusService = new ModbusDataService();

            modbusService.OnEnvironmentDataUpdated += HandleEnvironmentData;
            modbusService.OnDeviceDataUpdated += HandleDeviceData;

            Task.Run(() => modbusService.StartReadingAsync(_modbusCts.Token));
            #endregion

        }

        #region 处理环境数据
        private void HandleEnvironmentData(List<int> values)
        {
            //1、更新界面
            for (int i = 0; i < values.Count && i < EnvironmentColl.Count; i++)
            {
                EnvironmentColl[i].EnItemValue = values[i];
            }
            // 2. 报警检查（按 index 对应）
            if (values.Count > 2) // 温度
            {
                int temp = values[2];
                if (temp > 30)
                {
                    TriggerAlarm(
                        alarmType: "温度过高",
                        actualValue: $"{temp}℃",
                        triggerTime: DateTime.Now,
                        duration: 10,
                        fullMessage: $"温度过高 ({temp}℃ > 30℃)"
                    );
                }
            }

            if (values.Count > 4) // PM2.5
            {
                int pm25 = values[4];
                if (pm25 > 95)
                {
                    TriggerAlarm(
                        alarmType: "PM2.5超标",
                        actualValue: $"{pm25}",
                        triggerTime: DateTime.Now,
                        duration: 10,
                        fullMessage: $"PM2.5超标 ({pm25} > 95)"
                    );
                }
            }

        }
        #endregion


        #region 处理设备数据更新
        private void HandleDeviceData(List<int> values)
        {
            //1、更新界面
            for (int i = 0; i < values.Count && i < DeviceColl.Count; i++)
            {
                DeviceColl[i].Value = values[i];
            }

            // 2. 检查设备报警
            if (values.Count > 1) // 电压 (V)
            {
                int voltage = values[1];
                if (voltage > 240)
                {
                    TriggerAlarm(
                        alarmType: "电压过高",
                        actualValue: $"{voltage}V",
                        triggerTime: DateTime.Now,
                        duration: 10,
                        fullMessage: $"电压过高 ({voltage}V > 240V)"
                    );
                }
                else if (voltage < 200)
                {
                    TriggerAlarm(
                        alarmType: "电压过低",
                        actualValue: $"{voltage}V",
                        triggerTime: DateTime.Now,
                        duration: 10,
                        fullMessage: $"电压过低 ({voltage}V < 200V)"
                    );
                }
            }

            if (values.Count > 2) // 转速 (r/min)
            {
                int speed = values[2];
                if (speed > 1100)
                {
                    TriggerAlarm(
                        alarmType: "转速过快",
                        actualValue: $"{speed}r/min",
                        triggerTime: DateTime.Now,
                        duration: 10,
                        fullMessage: $"转速过快 ({speed}r/min > 1100r/min)"
                    );
                }
            }

            if (++_snapshotCounter >= SNAPSHOT_INTERVAL)
            {
                _snapshotCounter = 0;
                if (values.Count >= 7)
                {
                    FactoryDbContext.SaveDeviceSnapshot(
                        values[0], // 总电能
                        values[1], // 电压
                        values[2], // 转速
                        values[3], // 气压
                        values[4], // 流量
                        values[5], // 频率
                        values[6]  // 功率
                    );
                }
            }


        }
        #endregion

        // 


        //报警处理
        /// <summary>
        /// 触发报警（UI 显示简化版，数据库存储完整版）
        /// </summary>
        private void TriggerAlarm(
            string alarmType,
            string actualValue,
            DateTime triggerTime,
            int duration,
            string fullMessage = null)
        {
            string uiMsg = $"{alarmType} ({actualValue})";
            string dbMsg = fullMessage ?? uiMsg;

            string num = (AlarmList.Count + 1).ToString("D2");
            string timeStr = triggerTime.ToString("yyyy-MM-dd HH:mm:ss");

            // 插入到顶部（最新在最前）
            AlarmList.Insert(0, new AlarmModel
            {
                Num = num,
                Msg = uiMsg,
                Time = timeStr,
                Duration = duration
            });

            // 保存完整日志到数据库
            FactoryDbContext.SaveAlarm(dbMsg, triggerTime, duration);
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
