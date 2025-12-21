using ProductMonitor.Models;
using System;
using System.Collections.Generic;
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
    /// RaderUC.xaml 的交互逻辑
    /// </summary>
    public partial class RaderUC : UserControl
    {
        public RaderUC()
        {
            InitializeComponent();

            SizeChanged += OnSizeChanged;
        }

        /// <summary>
        /// 窗体大小发生改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Drag();
        }

        //数据源 支持数据绑定 依赖属性


        public List<RaderModel>  ItemSource
        {
            get { return (List<RaderModel>)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register("ItemSource", typeof(List<RaderModel>), typeof(RaderUC));


        /// <summary>
        /// 画图
        /// </summary>
        public void Drag() {  
            
            //判断是否有数据
            if (ItemSource == null || ItemSource.Count == 0)
            {
                return;

            }
            //清除之前画的
            mainCanvas.Children.Clear();
            P1.Points.Clear(); 
            P2.Points.Clear();
            P3.Points.Clear();
            P4.Points.Clear();
            P5.Points.Clear();

            //调整大小（正方形）
            double size = Math.Min(RenderSize.Width, RenderSize.Height);
            LayGrid.Height=size;
            LayGrid.Width=size;

            //半径
            double radius=size/2;
            //步子跨幅
            double step=360.0/ItemSource.Count;

            for (int i = 0; i < ItemSource.Count; i++) {

                double X= (radius - 20) * Math.Cos((step * i - 90) * Math.PI / 180);//x偏移量
                double Y = (radius - 20) * Math.Sin((step * i - 90) * Math.PI / 180);//y偏移量

                //X,Y坐标
                P1.Points.Add(new Point(radius + X, radius + Y));
                P2.Points.Add(new Point(radius + X*0.75, radius + Y*0.75));
                P3.Points.Add(new Point(radius + X*0.5, radius + Y *0.5));
                P4.Points.Add(new Point(radius + X*0.25, radius + Y *0.25));

                //数据多边形
                P5.Points.Add(new Point(radius + X * ItemSource[i].Value * 0.01, radius + Y* ItemSource[i].Value * 0.01));

                //文字处理
                TextBlock txt = new TextBlock();
                txt.Width = 60;
                txt.FontSize = 10;
                txt.TextAlignment = TextAlignment.Center;
                txt.Text = ItemSource[i].ItemName;
                txt.Foreground=new SolidColorBrush(Color.FromArgb(100,255,255,255));
                txt.SetValue(Canvas.LeftProperty, radius + (radius - 10) * Math.Cos((step * i - 90) * Math.PI / 180) - 30);//设置左边边距
                txt.SetValue(Canvas.TopProperty, radius + (radius - 10) * Math.Sin((step * i - 90) * Math.PI / 180) - 7);//设置上边边距

                mainCanvas.Children.Add(txt);

            }
        }


    }
}
