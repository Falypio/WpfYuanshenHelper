
using WpfYuanshenHelper;
using WpfYuanshenHelper.Client;
using WpfYuanshenHelper.Constant;
using WpfYuanshenHelper.Entities;
using WpfYuanshenHelper.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfYuanshenHelper.Common;
using System.IO;

namespace WpfYuanshenHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new NotifyIconViewModel();
            txtLog.Text = "启动循环：每天早上7点自动签到" + "\r\n" + 
                "签到一次：点击一次签到一次" + "\r\n" + 
                "最小化：隐藏在系统托盘" + "\r\n"+
                "编辑Cookie:打开写有Cookie的记事本，然后去米友社去找到你对应的Cookie,复制粘贴到记事本上，多个就用回车隔开" + "\r\n";
        }
        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                LoadDisplay("启动米友社签到系统！开始定时签到");
                //DoTask("");
                SetTaskAtFixedTime();
            });
        }
        /// <summary>
        /// 自己签到一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOne_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                BtnTwoAsync();
            });
        }
        /// <summary>
        /// 执行签到
        /// </summary>
        private async void BtnTwoAsync()
        {
            string[] args = new string[0];
            //执行功能...
            LoadDisplay("开始签到");
            //取文本中的cookie
            
            string Path = $"{System.Environment.CurrentDirectory}" + "\\cookie.txt";//这个在debug根目录里，把cookie复制粘贴到这个文档中即可 多个要回车隔开
            if (!File.Exists(Path))// 返回bool类型，存在返回true，不存在返回false                                     
            {
                File.Create(Path);//不存在则创建文件
            }
            string[] lines = System.IO.File.ReadAllLines(Path);
            args = lines.ToArray();
            if (args.Length <= 0)
            {
                throw new InvalidOperationException("获取参数不对");
            }
            await Execute(lines);
        }
        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.MainWindow.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClsoe_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        #region 计时器 
        /// <summary>
        /// //设定定时执行
        /// </summary>
        public void SetTaskAtFixedTime()
        {
            DateTime now = DateTime.Now;
            DateTime oneOClock = DateTime.Today.AddHours(7.0); //晨7：00
            if (now > oneOClock)
            {
                oneOClock = oneOClock.AddDays(1.0);
            }
            int msUntilFour = (int)(oneOClock - now).TotalMilliseconds;

            var t = new System.Threading.Timer(DoTask);
            t.Change(msUntilFour, Timeout.Infinite);
        }
        /// <summary>
        /// //要执行的任务
        /// </summary>
        /// <param name="state"></param>
        private async void DoTask(object state)
        {
            string[] args = new string[0];
            //执行功能...
            LoadDisplay("开始签到");
            //取文本中的cookie
            string Path = $"{System.Environment.CurrentDirectory}" + "\\cookie.txt";//这个在debug根目录里，把cookie复制粘贴到这个文档中即可 多个要回车隔开
            string[] lines = System.IO.File.ReadAllLines(Path);
            args = lines.ToArray();
            if (args.Length <= 0)
            {
                throw new InvalidOperationException("获取参数不对");
            }
            await Execute(lines);
            //再次设定
            SetTaskAtFixedTime();
        }
        #endregion
        /// <summary>
        /// 执行读取角色信息并签到
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task Execute(string[] args)
        {
            try
            {
                int accountNum = 0;
                foreach (string line in args)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    var cookieString = string.Join(' ', line);
                    var cookies = cookieString.Split("#");
                    accountNum++;
                    foreach (var cookie in cookies)
                    {
                        LoadDisplay($"开始签到 账号{accountNum}");

                        var client = new GenShinClient(
                            cookie);

                        var rolesResult =
                            await client.GetExecuteRequest<UserGameRolesEntity>(Config.GetUserGameRolesByCookie,
                                "game_biz=hk4e_cn");

                        //检查第一步获取账号信息
                        rolesResult.CheckOutCodeAndSleep();

                        int accountBindCount = rolesResult.Data.List.Count;

                        LoadDisplay($"账号{accountNum}绑定了{accountBindCount}个角色");

                        for (int i = 0; i < accountBindCount; i++)
                        {
                            LoadDisplay(rolesResult.Data.List[i].ToString());

                            var roles = rolesResult.Data.List[i];

                            var signDayResult = await client.GetExecuteRequest<SignDayEntity>(Config.GetBbsSignRewardInfo,
                                $"act_id={Config.ActId}&region={roles.Region}&uid={roles.GameUid}");

                            //检查第二步是否签到
                            signDayResult.CheckOutCodeAndSleep();

                            LoadDisplay(signDayResult.Data.ToString());

                            var data = new
                            {
                                act_id = Config.ActId,
                                region = roles.Region,
                                uid = roles.GameUid
                            };

                            var signClient = new GenShinClient(cookie, true);

                            var result =
                                await signClient.PostExecuteRequest<SignResultEntity>(Config.PostSignInfo,
                                    jsonContent: new JsonContent(data));

                            LoadDisplay(result.CheckOutCodeAndSleep());
                        }
                    }
                }

                
            }
            catch (GenShinException e)
            {
                LoadDisplay($"请求接口时出现异常{e.Message}");
                throw;
            }
            catch (System.Exception e)
            {
                LoadDisplay($"出现意料以外的异常{e}");
                throw;
            }
        }
        /// <summary>
        /// 输出日志信息
        /// </summary>
        /// <param name="news"></param>
        public void LoadDisplay(string news)
        {
            this.txtLog.Dispatcher.BeginInvoke((Action)delegate
            {
                this.txtLog.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss " + news + "\r\n"));
                if (IsVerticalScrollBarAtBottom)
                {
                    this.txtLog.ScrollToEnd();
                }
            });
        }
        /// <summary>
        /// 初始加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //取文本中的cookie
            string Path = $"{System.Environment.CurrentDirectory}" + "\\cookie.txt";

            string[] lines = System.IO.File.ReadAllLines(Path);
            System.Console.WriteLine("Contents of cookie.txt = ");
            foreach (string line in lines)
            {
                Console.WriteLine("\t" + line);
            }
        }
        /// <summary>
        /// 日志文本框滚动条是否在最下方
        /// true:文本框竖直滚动条在文本框最下面时，可以在文本框后追加日志
        /// false:当用户拖动文本框竖直滚动条，使其不在最下面时，即用户在查看旧日志，此时不添加新日志，
        /// </summary>
        public bool IsVerticalScrollBarAtBottom
        {
            get
            {
                bool atBottom = false;

                this.txtLog.Dispatcher.Invoke((Action)delegate
                {
                    //if (this.txtLog.VerticalScrollBarVisibility != ScrollBarVisibility.Visible)
                    //{
                    //    atBottom= true;
                    //    return;
                    //}
                    double dVer = this.txtLog.VerticalOffset;       //获取竖直滚动条滚动位置
                    double dViewport = this.txtLog.ViewportHeight;  //获取竖直可滚动内容高度
                    double dExtent = this.txtLog.ExtentHeight;      //获取可视区域的高度

                    if (dVer + dViewport >= dExtent)
                    {
                        atBottom = true;
                    }
                    else
                    {
                        atBottom = false;
                    }
                });

                return atBottom;
            }
        }

        #region 系统托盘
        public void SystemTray()
        {
            
        }
        #endregion

        
    }
}
