using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductMonitor.Models
{
    /// <summary>
    /// 机台数据模型
    /// </summary>
    internal class MachineModel
    {
        /// <summary>
        /// 机器名称
        /// </summary>
        public string MachineName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Statue { get; set; }

        /// <summary>
        /// 计划数量
        /// </summary>
        public int PlanCount { get; set; }

        /// <summary>
        /// 完成数量
        /// </summary>
        public int FinishCount { get; set; }

        /// <summary>
        /// 工单编号
        /// </summary>
        public String OrderNo { get; set; }

        /// <summary>
        /// 完成百分比（只读）
        /// </summary>
        public double Percent { 
            get
            {
                return FinishCount * 100.0 / PlanCount;
            }
        }

    }
}
