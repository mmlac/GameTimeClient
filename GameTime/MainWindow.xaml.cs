using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Management;
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
using System.Globalization;
using Test;

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

            //PrintProcesses();

            //UseWMIToGetProcesses();

            PInvokeExample();



        }

        private bool ContainsCaseInsensitive(String paragraph, String word)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(paragraph, word, CompareOptions.IgnoreCase) >= 0;
        }




        private void PrintProcesses()
        {
            List<String> printList = new List<String>();
            Process[] localAll = Process.GetProcesses();
            //Process[] localAll = { Process.GetProcessById(23268) };

            /*TODO: This is filled on startup witheverything that has
                    the graphic libraries loaded and than track only what
                    is loaded afterwards. Maybe even do a 15sec wait 
                    after startup to make sure everything has loaded.

                    Also allow to block these standard apps below. Run a
                    unique on the list after the initial parse.
            */
            String[] ignoreProcessList = {
                "GalaxyClient", "GalaxyClient Helper", "RzSynapse", "devenv",
                "XDesProc", "GameOverlayUI", "Steam", "steamwebhelper",
                "GameplayTimeTracker", "chrome", "firefox", "Dropbox",
                "explorer", "CompanionApp", "procexp64", "notepad++",
                "SkypeApp", "VeraCrypt", "SearchUI", "ShellExperienceHost",
                 "ScriptedSandbox64", "SetPoint", "SystemSettings",
                "SkypeHost", "Microsoft.Photos", "UpdateChecker",
                "ApplicationFrameHost"
                };
            foreach (Process p in localAll)
            {
                //printList.Add(String.Format("ID: {0}  Name: {1}", p.Id, p.ProcessName));
                if (false && ignoreProcessList.Contains(p.ProcessName))
                    continue;
                try
                {
                    List<String> moduleList = new List<String>();
                    ProcessModuleCollection pmc = p.Modules;
                    foreach (ProcessModule pm in pmc)
                    {
                        String name = pm.ModuleName;
                        //printList.Add(String.Format("{0} runs {1}", p.ProcessName, name));
                        if (ContainsCaseInsensitive(name, "d3d") || ContainsCaseInsensitive(name, "opengl"))
                        {
                            moduleList.Add(name);
                        }
                    }
                    if (moduleList.Count > 0)
                        printList.Add(String.Format("{0} is running {1}", p.ProcessName, String.Join(",", moduleList)));
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    Console.WriteLine("W32 exception: {0}  ->  {1}", e.ErrorCode, e.Message);
                }
                catch (Exception e) { }
            }

            for (int i = 0; i < 10; ++i)
            {
                Console.WriteLine();
            }
            Console.WriteLine(printList.Count);
            foreach (String line in printList)
            {
                Console.WriteLine(line);
            }
        }


        private void UseWMIToGetProcesses()
        {
            HashSet<int> handle_list = new HashSet<int>();
            string win32_proc_query_string = "select * from CIM_Process";
            string cim_query_string = "select * from CIM_ProcessExecutable";

            ManagementScope ms = new ManagementScope(@"\\localhost\root\cimv2", null);
            ms.Connect();
            if (ms.IsConnected)
            {
                ObjectQuery cim_query = new ObjectQuery(cim_query_string);
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms, cim_query))
                {
                    using (ManagementObjectCollection searchResult = searcher.Get())
                    {
                        foreach (ManagementObject mo in searchResult)
                        {
                            if (ContainsCaseInsensitive(mo["Antecedent"] + "", "d3d") || ContainsCaseInsensitive((String)mo["Antecedent"], "opengl"))
                            {
                                //foreach (PropertyData pd in mo.Properties)
                                //{
                                //    Console.WriteLine("{0}: {1}   ####    ", pd.Name, mo[pd.Name]);
                                //}

                                String dependent = (String)mo["Dependent"];

                                if (dependent.Contains("\""))
                                {
                                    // Dependent of the form: \\<hostname>\root\cimv2:Win32_Process.Handle="23312"
                                    int first_quot = dependent.IndexOf("\"");
                                    int start = first_quot + 1; //skip the -"- itself
                                    int end = dependent.Count() - 1;
                                    int range = end - start;
                                    String handle = dependent.Substring(start, range);

                                    int handle_i;
                                    if (Int32.TryParse(handle, out handle_i))
                                    {
                                        handle_list.Add(handle_i);
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("Handles: {0}", String.Join(", ", handle_list));
                //Console.WriteLine("Process count:{0}", process_count);

                if (handle_list.Count > 0)
                {
                    int cntr = 0;
                    StringBuilder sb = new StringBuilder(win32_proc_query_string);
                    sb.Append(" WHERE ");
                    bool first = true;
                    foreach (int h in handle_list)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            sb.Append(" OR ");
                        }
                        sb.Append(String.Format("Handle = {0}", h));
                    }

                    Console.WriteLine("QUERY: {0}", sb.ToString());

                    ObjectQuery win32_proc_query = new ObjectQuery(sb.ToString());
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms, win32_proc_query))
                    {
                        using (ManagementObjectCollection searchResult = searcher.Get())
                        {
                            foreach (ManagementObject mo in searchResult)
                            {
                                ++cntr;

                                //foreach (PropertyData pd in mo.Properties)
                                //{
                                //    Console.WriteLine("{0}: {1}   ####    ", pd.Name, mo[pd.Name]);
                                //}

                                Console.WriteLine("Cap: " + mo["Caption"] + "  Desc: " + mo["Description"] + "  ExecPath: " + mo["ExecutablePath"] + " CreatDate: " + mo["CreationDate"]);

                            }
                        }
                    }

                    Console.WriteLine("Counters:  handle_list: {0}  -  cntr: {1}", handle_list.Count(), cntr);
                }

            }

        }






        private void PInvokeExample()
        {
            Program.Foo();
        }





    }

}