using GameTime.IO;
using GameTimeClient.Tracking;
using GameTimeClient.Tracking.IO;
using GameTimeClient.Tracking.Utility;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace GameTimeClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // The path to the key where Windows looks for startup applications
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
#if DEBUG
        const String APP_NAME = "GametimeClient_DEBUG";
#else
        const String APP_NAME = "GametimeClient";
#endif

        ProcessLogger procLog;
        Thread processLoggerThread;
        Thread processAggregateThread;
        GameTimeConnection gtconn;

        public MainWindow()
        {

            InitializeComponent();


            // Minimize to Tray: http://stackoverflow.com/a/10230672/846655
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("Pixelkit-Gentle-Edges-Game-Controller.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };

            // Start Minimized to Tray
            this.WindowState = WindowState.Minimized;
            this.Hide();


            Storage storage = new Storage();
            gtconn = new GameTimeConnection();

            procLog = new ProcessLogger(storage, gtconn);

            processLoggerThread = new Thread(procLog.log3DProcesses);
            processLoggerThread.Start();

            processAggregateThread = new Thread(procLog.aggregate);
            processAggregateThread.Start();



            // Run on startup setting: http://stackoverflow.com/a/32312270
            // Check to see the current state (running at startup or not)
            if (rkApp.GetValue(APP_NAME) == null)
            {
                // The value doesn't exist, the application is not set to run at startup, Check box
                startupCheckbox.IsChecked = false;
            }
            else
            {
                // The value exists, the application is set to run at startup
                startupCheckbox.IsChecked = true;
            }

        }


        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }



        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (null != processLoggerThread)
            {
                procLog.requestStopLogging();
                procLog.requestStopAggregating();
            }

            //Asked Threads to stop, waiting for them to shut down....

            processLoggerThread.Join(1000);
            processAggregateThread.Join(1000);

            e.Cancel = false;
        }


        private void startupCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            // Add the value in the registry so that the application runs at startup
            rkApp.SetValue(APP_NAME, System.Reflection.Assembly.GetExecutingAssembly().Location);
        }


        private void StartupCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Remove the value from the registry so that the application doesn't start
            rkApp.DeleteValue("StartupWithWindows", false);
        }
    }

}