using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfYuanshenHelper.Common;

namespace WpfYuanshenHelper
{
    
    public class NotifyIconViewModel
    {
        /// <summary>
        /// 如果窗口没显示，就显示窗口
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    //CanExecuteFunc = () => Application.Current.MainWindow.Visibility == Visibility.Hidden,
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow.Visibility = Visibility.Visible;
                    }
                };
            }
        }
        /// <summary>
        /// 隐藏窗口
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Application.Current.MainWindow.Visibility = Visibility.Hidden,
                    //CanExecuteFunc = () => Application.Current.MainWindow.Visibility != Visibility.Hidden
                };
            }
        }
        /// <summary>
        /// 编辑Cookie
        /// </summary>
        public ICommand ExitCookieCommand
        {
            get
            {
                return new DelegateCommand 
                { 
                    CommandAction = () =>
                    {
                        string Path = $"{System.Environment.CurrentDirectory}" + "\\cookie.txt";//这个在debug根目录里，把cookie复制粘贴到这个文档中即可 多个要回车隔开
                        if (!File.Exists(Path))     // 返回bool类型，存在返回true，不存在返回false
                        {
                            File.Create(Path);      //不存在则创建文件
                        }
                        System.Diagnostics.Process.Start("explorer.exe", Path);
                    }
                };
            }
        }
        /// <summary>
        /// 关闭软件
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }
    }
}
