using GameTime.IO;
using GameTimeClient.Tracking;
using GameTimeClient.Tracking.IO;
using GameTimeClient.Tracking.Utility;
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

        ProcessLogger procLog;
        Thread processLoggerThread;
        Thread processAggregateThread;

        public MainWindow()
        {

            InitializeComponent();

            Storage storage = new Storage();
            GameTimeConnection gtconn = new GameTimeConnection();

            procLog = new ProcessLogger(storage, gtconn);

            processLoggerThread = new Thread(procLog.log3DProcesses);
            //processLoggerThread.Start();

            processAggregateThread = new Thread(procLog.aggregate);
            processAggregateThread.Start();

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


        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("BUTTONED!!!");
        }

    }

}