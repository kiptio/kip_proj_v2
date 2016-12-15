using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace Kip.WinSvc.ApplicationPoolRecycle
{
    /// <summary>
    /// Install.bat:
    /// %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe ApplicationPoolRecycleService.exe
    /// Net Start ApplicationPoolRecycleService
    /// 
    /// Uninstall.bat:
    /// net stop ApplicationPoolRecycleService
    /// %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe /u ApplicationPoolRecycleService.exe
    /// </summary>
    public partial class ApplicationPoolRecycleService : ServiceBase
    {
        private string requestUriString = "http://www.baidu.com/";
        private string apppoolName = "your website appool name";
        
        public ApplicationPoolRecycleService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            new Thread(() =>
            {
                HttpWebRequest request = null;
                HttpStatusCode statusCode = HttpStatusCode.OK;
                HttpStatusCode prexStatusCode = HttpStatusCode.OK;

                while (true)
                {
                    statusCode = HttpStatusCode.OK;

                    try
                    {
                        // 发送Request到目标站点，获取响应状态码
                        request = (HttpWebRequest)WebRequest.Create(requestUriString);
                        request.Method = "GET";
                        request.AllowAutoRedirect = false;

                        using (var response = request.GetResponse() as HttpWebResponse)
                        {
                            statusCode = response.StatusCode;
                        }
                        Logger.WriteToFile("-", false);
                    }
                    catch (WebException we)
                    {
                        statusCode = ((HttpWebResponse)we.Response).StatusCode;
                        Logger.WriteToFile(string.Format("WebException message：[{0}][{1}]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), we.Message));
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToFile(string.Format("Exception message：[{0}]{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ex.Message));
                    }

                    try
                    {
                        // 连续两次出现503的时候，重启IIS
                        if (statusCode == HttpStatusCode.ServiceUnavailable && prexStatusCode == statusCode)
                        {
                            Logger.WriteToFile(string.Format("[{0}]准备重启IIS", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                            ApplicationPoolRecycle();
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    // 标记为前一个状态码
                    prexStatusCode = statusCode;

                    // 线程挂起
                    Thread.Sleep(30000);
                }
            }).Start();
        }

        protected override void OnStop()
        {
            Logger.WriteToFile(string.Format("[{0}]服务停止", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        #region [回收应用程序池]
        private void ApplicationPoolRecycle()
        {
            using (var p = new Process())
            {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();

                p.StandardInput.WriteLine(@"cd %windir%\system32\inetsrv");
                p.StandardInput.WriteLine(@"appcmd recycle apppool /apppool.name:" + apppoolName);

                p.StandardInput.WriteLine("exit");        //不过要记得加上Exit要不然下一行程式执行的时候会当机
                Logger.WriteToFile(p.StandardOutput.ReadToEnd());
            }
        }
        #endregion
    }

    #region [文件日志]
    public static class Logger
    {
        public static void WriteToFile(string content)
        {
            WriteToFile(content, true);
        }

        public static void WriteToFile(string content, bool writeLine)
        {
            string filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "WinService.log");
            using (var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    if (writeLine)
                        streamWriter.WriteLine("\r\n" + content);
                    else
                        streamWriter.Write(content);
                }
            }
        }
    }
    #endregion
}
