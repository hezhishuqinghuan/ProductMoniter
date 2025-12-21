using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductMonitor.Models
{
    internal class EnvironmentModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 环境项名称
        /// </summary>
        private string _EnItemName;

        public string EnItemName
        {
            get { return _EnItemName; }
            set { 
                _EnItemName = value;
                if(PropertyChanged != null)
                {
                    PropertyChanged(this,new PropertyChangedEventArgs(nameof(EnItemName)));
                }
            }
        }


        /// <summary>
        /// 环境项值
        /// </summary>

        private int _EnItemValue;

        public int EnItemValue
        {
            get { return _EnItemValue; }
            set { 
                _EnItemValue = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(EnItemValue)));
                }
            }
        }



    }
}
