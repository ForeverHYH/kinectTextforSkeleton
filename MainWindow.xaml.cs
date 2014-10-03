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
using Microsoft.Kinect;

namespace kinectTextforSkeleton
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinectDevice;
        private readonly Brush[] skeletonBrushes;//绘图笔刷
        private Skeleton[] frameSkeletons;

        public MainWindow()
        {    
            InitializeComponent();
            skeletonBrushes = new Brush[]  { Brushes.Black, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };   
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged; //相当于addTarget,给这个事件（statusChanged）加一个事件侦听函数
            //this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(isConnect);
            this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(k => k.Status == KinectStatus.Connected); //赋值时调用set函数，=后面的相当于value
            //都是返回满足条件的第一个元素，如果没有该元素，则返回null,即返回第一个连接的kinect
            //lambda表达式，匿名函数，FirstOrDefault需要传入函数，所以k => k.Status == KinectStatus.Connected相当于一个没写函数名的函数,等价于函数isConnect
       //  private bool isConnect(Microsoft.Kinect.KinectSensor k)
       //  {
       //     return k.Status == KinectStatus.Connected;
       //  }
        }

       

        public KinectSensor KinectDevice
        {   
            get 
            {
                return this.kinectDevice; 
            }   
            set    
            {       
                if (this.kinectDevice != value)        //value是关键字，相当于set函数里面传进来的参数
                {            //Uninitialize           
                    if (this.kinectDevice != null)           
                    {                
                        this.kinectDevice.Stop();
                        this.kinectDevice.SkeletonFrameReady -= KinectDevice_SkeletonFrameReady;
                        this.kinectDevice.SkeletonStream.Disable(); 
                        this.frameSkeletons = null;            
                    }           
                    this.kinectDevice = value;           
                    //Initialize           
                    if (this.kinectDevice != null)           
                    {               
                        if (this.kinectDevice.Status == KinectStatus.Connected)
                        {                    
                            this.kinectDevice.SkeletonStream.Enable();
                            this.frameSkeletons = new Skeleton[this.kinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                            this.kinectDevice.SkeletonFrameReady += KinectDevice_SkeletonFrameReady;  //如果SkeletonFrameReady了，就添加一个事件侦听
                            this.kinectDevice.Start(); 
                        }            
                    }       
                }   
            }
        }


        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {   
            switch (e.Status)    
            {        
                case KinectStatus.Initializing:
                    break;
                case KinectStatus.Connected:
                    break;
                case KinectStatus.NotPowered:
                    break;
                case KinectStatus.NotReady:
                    break;
                case KinectStatus.DeviceNotGenuine:
                      this.KinectDevice = e.Sensor; 
                      break;        
                case KinectStatus.Disconnected:
                //TODO: Give the user feedback to plug-in a Kinect device.  
                      this.KinectDevice = null;           
                      break;        
                default:            //TODO: Show an error state            
                    break;   
            }
        }


        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {    
            using (SkeletonFrame frame = e.OpenSkeletonFrame()) //using的意义是括号里面的内容只对using后面的大括号有用，出了大括号自动释放内存数据
            {       
                if (frame != null) 
                {           
                    Polyline figure; //折线
                    Brush userBrush; 
                    Skeleton skeleton;  

                    LayoutRoot.Children.Clear(); //清空窗口 
                    frame.CopySkeletonDataTo(this.frameSkeletons);   //把当前的frame的对象的数据放在数组中以便后续遍历

                    for (int i = 0; i < this.frameSkeletons.Length; i++) //length是用户的个数，一个用户对应一个笔刷
                    {               
                        skeleton = this.frameSkeletons[i];

                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)  //如果识别状态是已经识别（能够识别出用户），就执行后续代码
                        {                   
                            userBrush = this.skeletonBrushes[i % this.skeletonBrushes.Length]; //选一个笔刷绘制人体

                            //绘制头和躯干                   
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.Head, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.Spine, 
                                                                  JointType.ShoulderRight, JointType.ShoulderCenter, JointType.HipCenter
                                                                  });
                            LayoutRoot.Children.Add(figure); 

                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipLeft, JointType.HipRight }); 
                            LayoutRoot.Children.Add(figure);     
              
                            //绘制作腿                   
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft }); 
                            LayoutRoot.Children.Add(figure);

                            //绘制右腿                   
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight }); 
                            LayoutRoot.Children.Add(figure); 

                            //绘制左臂                   
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft });
                            LayoutRoot.Children.Add(figure);      
             
                            //绘制右臂                   
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight }); 
                            LayoutRoot.Children.Add(figure);               
                        }           
                    }       
                }   
            }
        }
        
        private Polyline CreateFigure(Skeleton skeleton, Brush brush, JointType[] joints) 
        {
            Polyline figure = new Polyline();

            figure.StrokeThickness = 8; //线宽
            figure.Stroke = brush;   //笔触

            for (int i = 0; i < joints.Length; i++)
            { 
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]])); //坐标转换过后把关键点赋值给折线对象
            } 

            return figure; 
        }

        private Point GetJointPoint(Joint joint)  //舍弃了Z值，三维转换成二维坐标，再适应窗口大小
        {
            DepthImagePoint point = this.kinectDevice.CoordinateMapper.MapSkeletonPointToDepthPoint(joint.Position, this.KinectDevice.DepthStream.Format);
            point.X *= (int)this.LayoutRoot.ActualWidth / KinectDevice.DepthStream.FrameWidth; 
            point.Y *= (int)this.LayoutRoot.ActualHeight / KinectDevice.DepthStream.FrameHeight;

            return new Point(point.X, point.Y); 
        } 
    }
}
