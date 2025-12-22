using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductMonitor.Models
{
    /// <summary>
    /// 设备数据模型
    /// </summary>
    internal class DeviceModel:INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 设备监控项名称
        /// </summary>
        private string _DeviceItem;

        public string DeviceItem
        {
            get { return _DeviceItem; }
            set { 
                _DeviceItem = value;
                if(PropertyChanged != null)
                {
                    PropertyChanged(this,new PropertyChangedEventArgs(nameof(DeviceItem)));
                }
            }
        }


        /// <summary>
        /// 值
        /// </summary>
        /// 
        private int _Value;

        public int Value
        {
            get { return _Value; }
            set { 
                _Value = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

       

    }
}
