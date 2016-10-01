using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace GameTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ProcessLogger procLog;
        Thread processLoggerThread;

        public MainWindow()
        {

            InitializeComponent();

            Data data = new Data();

            procLog = new ProcessLogger(data);

            processLoggerThread = new Thread(procLog.log3DProcesses);
            processLoggerThread.Start();

        }

    

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if(null != processLoggerThread)
            {
                procLog.requestStop();
            }
        e.Cancel = true;
    }


    }

}