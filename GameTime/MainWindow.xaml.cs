using System;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;

namespace GameTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {

            InitializeComponent();

            Data data = new Data();

            ProcessLogger procLog = new ProcessLogger(data);

        }

    }
        

}