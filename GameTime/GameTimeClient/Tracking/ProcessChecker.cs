using GameTimeClient.Tracking.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;



namespace GameTimeClient.Tracking
{
    /// <summary>
    ///    This class contains logic to interoparate with processes and
    ///    their modules.
    /// </summary>
    class ProcessChecker
    {
        /// <summary>
        ///     Wrapper to Win32 API EnumProcesses
        /// </summary>
        /// <param name="processIds">ProcessIds returned</param>
        /// <param name="arraySizeBytes">Size of the processIds array</param>
        /// <param name="bytesCopied">
        ///     Number of bytes copied to processIds
        /// </param>
        /// <returns>
        ///     Bool indicating whether the operation was successful
        /// </returns>
        [DllImport("psapi")]
        private static extern bool EnumProcesses(
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)]
                [In][Out] UInt32[] processIds,
            UInt32 arraySizeBytes,
            [MarshalAs(UnmanagedType.U4)] out UInt32 bytesCopied);



        /// <summary>
        ///     Enum for selecting which processes should be enumerated
        ///     by EnumProcessModulesEx
        /// </summary>
        private enum DwFilterFlag : uint
        {
            // This is the default one app would get without any flag.
            LIST_MODULES_DEFAULT = 0x0,
            // list 32bit modules in the target process.   
            LIST_MODULES_32BIT = 0x01,
            // list all 64bit modules. 32bit exe will be stripped off.  
            LIST_MODULES_64BIT = 0x02,
            // list all the modules
            LIST_MODULES_ALL = (LIST_MODULES_32BIT | LIST_MODULES_64BIT)
        }


        /// <summary>
        ///     Lists all modules that a given process has loaded.
        /// </summary>
        /// <param name="hProcess">Handle to the process</param>
        /// <param name="lphModule">
        ///     Array that receives a list of module handles
        /// </param>
        /// <param name="cb">Size of the lbhModule array, in bytes</param>
        /// <param name="lpcbNeeded">
        ///     The number of bytes required to store all module handlers in 
        ///     the lphModuleArray
        /// </param>
        /// <param name="dwff">The filter criteria</param>
        /// <returns></returns>
        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool EnumProcessModulesEx(
            IntPtr hProcess,
            [Out] IntPtr lphModule,
            UInt32 cb,
            [MarshalAs(UnmanagedType.U4)] out UInt32 lpcbNeeded,
            DwFilterFlag dwff);


        /// <summary>
        ///   Retrieves the fully qualified path for the file containing the 
        ///   specified module  
        /// </summary>
        /// <param name="hProcess">
        ///     A handle to the process that contains the module. The handle 
        ///     must have the PROCESS_QUERY_INFORMATION and PROCESS_VM_READ 
        ///     access rights.
        /// </param>
        /// <param name="hModule">
        ///     A handle to the module. If this parameter is NULL, 
        ///     GetModuleFileNameEx returns the path of the executable file of 
        ///     the process specified in hProcess
        /// </param>
        /// <param name="lpBaseName">
        ///     A pointer to a buffer that receives the fully qualified path to 
        ///     the module. If the size of the file name is larger than the 
        ///     value of the nSize parameter, the function succeeds but the 
        ///     file name is truncated and null-terminated.
        /// </param>
        /// <param name="nSize">
        ///     The size of the lpFilename buffer, in characters.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value specifies the length 
        ///     of the string copied to the buffer.
        ///     If the function fails, the return value is zero.
        ///     To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(
            IntPtr hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In] [MarshalAs(UnmanagedType.U4)] int nSize);



        /// <summary>
        ///     Retrieves the name of the executable file for the specified 
        ///     process.    
        /// </summary>
        /// <param name="hProcess">
        ///     A handle to the process. The handle must have the 
        ///     PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION 
        ///     access right.
        /// </param>
        /// <param name="lpImageFileName">
        ///     A pointer to a buffer that receives the full path to the 
        ///     executable file.
        /// </param>
        /// <param name="nSize">
        ///     The size of the lpImageFileName buffer, in characters.
        /// </param>
        /// <returns></returns>
        [DllImport("psapi.dll")]
        private static extern uint GetProcessImageFileName(
            IntPtr hProcess,
            [Out] StringBuilder lpImageFileName,
            [In] [MarshalAs(UnmanagedType.U4)] int nSize);


        /// <summary>
        ///     Defines the Process and Security Access Rights
        /// </summary>
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }


        /// <summary>
        ///     Opens an existing local process object.
        /// </summary>
        /// <param name="processAccess">
        ///     The access to the process object. This access right is checked 
        ///     against the security descriptor for the process. This parameter 
        ///     can be one or more of the process access rights.
        ///     If the caller has enabled the SeDebugPrivilege privilege, the 
        ///     requested access is granted regardless of the contents of the 
        ///     security descriptor.
        /// </param>
        /// <param name="bInheritHandle">
        ///     If this value is TRUE, processes created by this process will 
        ///     inherit the handle. Otherwise, the processes do not inherit 
        ///     this handle.
        /// </param>
        /// <param name="processId">
        ///     The identifier of the local process to be opened
        /// </param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            UInt32 processId
        );


        /// <summary>
        ///     Base list of processes that use DirectX or OpenGL but should
        ///     always be ignored.
        /// </summary>
        private static String[] ignoreProcessList =
        {
            "Steam.exe","GalaxyClient.exe","GalaxyClient Helper.exe",
            "GameplayTimeTracker.exe", "Dropbox.exe", "steamwebhelper.exe",
            "devenv.exe", "RzSynapse.exe", "chrome.exe", "devenv.exe",
            "GameOverlayUI.exe", "XDesProc.exe", "firefox.exe", "explorer.exe",
            "procexp64.exe", "notepad++.exe", "SkypeApp.exe", "VeraCrypt.exe",
            "SearchUI.exe", "ShellExperienceHost.exe", "ScriptedSandbox64.exe",
            "SetPoint.exe", "SystemSettings.exe", "SkypeHost.exe",
            "Microsoft.Photos.exe", "UpdateChecker.exe",
            "ApplicationFrameHost.exe"
        };




        /// <summary>
        ///     Get the process names that load D3D or OpenGL
        /// </summary>
        /// <param name="ignoreProcs">
        ///     Process names that should be excluded
        /// </param>
        /// <returns>List of process names</returns>
        public static List<String> Get3DProcessNames(List<String> ignoreProcs)
        {
            List<UInt32> procIds = ListProcesses();
            List<String> proc3DList = Filter3DProcesses(procIds, ignoreProcs);
#if DEBUG
            Console.WriteLine("Found {0} processes.", procIds.Count);
            Console.WriteLine("Found {0} 3D processes", proc3DList.Count);
#endif
            return proc3DList;
        }


        /// <summary>
        ///     List all currently running processes
        /// </summary>
        /// <returns>List of process IDs</returns>
        private static List<UInt32> ListProcesses()
        {
            List<UInt32> proclist = new List<UInt32>();
            UInt32 arraySize = 1024;
            UInt32 arrayBytesSize = arraySize * sizeof(UInt32);
            UInt32[] processIds = new UInt32[arraySize];
            UInt32 bytesCopied;

            bool success = EnumProcesses(
                processIds,
                arrayBytesSize,
                out bytesCopied);

            //Console.WriteLine("success={0}", success);
            //Console.WriteLine("bytesCopied={0}", bytesCopied);

            if (!success)
            {
                Console.WriteLine("Boo!");
                return proclist;
            }
            if (0 == bytesCopied)
            {
                Console.WriteLine("Nobody home!");
                return proclist;
            }

            UInt32 numIdsCopied = bytesCopied >> 2; ;

            if (0 != (bytesCopied & 3))
            {
                UInt32 partialDwordBytes = bytesCopied & 3;

                Console.WriteLine(
                    "EnumProcesses copied {0} and {1}/4th DWORDS" +
                    "...  Please ask it for the other {2}/4th DWORD",
                    numIdsCopied, partialDwordBytes, 4 - partialDwordBytes);
                return proclist;
            }

            for (UInt32 index = 0; index < numIdsCopied; index++)
            {
                //Console.WriteLine("ProcessIds[{0}] = {1}", index, processIds[index]);
                proclist.Add(processIds[index]);
            }
            return proclist;
        }


        /// <summary>
        ///     Get a list of process executable names that have loaded either
        ///     DirectX or OpenGL modules.
        ///     Ignore processes on the default ignore list and ones passed
        ///     in as ignoreProcs parameter.
        /// </summary>
        /// <param name="procIds">Process IDs to scan modules of</param>
        /// <param name="ignoreProcs">
        ///     List of ignored process executable names
        /// </param>
        /// <returns>List of process executable names</returns>
        private static List<String> Filter3DProcesses(
            List<UInt32> procIds,
            List<String> ignoreProcs)
        {
            List<String> procList = new List<String>();
            foreach (UInt32 pid in procIds)
            {
                //
                // Get process handle from processId
                //

                IntPtr procPtr = OpenProcess(
                    ProcessAccessFlags.QueryInformation | 
                    ProcessAccessFlags.VirtualMemoryRead, 
                    false, 
                    pid);

                if (IntPtr.Zero == procPtr)
                {
                    //Console.WriteLine("Failed to resolve Process with ID: {0}", pid);
                    continue;
                }


                //
                // Verify the process is not on ignore list
                //

                StringBuilder procName = new StringBuilder(2000);
                GetProcessImageFileName(procPtr, procName, 2000);
                String processName = CutPath(procName.ToString());

                if (ignoreProcessList.Contains(processName) ||
                    ignoreProcs.Contains(processName))
                    continue;


#if DEBUG
                //list all found modules when debugging
                List < String > modNames = new List<String>();
#endif
                bool haveFound3DModule = false;

                // Setting up the variable for the second argument 
                // for EnumProcessModules
                IntPtr[] hMods = new IntPtr[1024];

                // Don't forget to free this later
                GCHandle gch = GCHandle.Alloc(hMods, GCHandleType.Pinned); 
                IntPtr pModules = gch.AddrOfPinnedObject();

                int intPtrSize = (Marshal.SizeOf(typeof(IntPtr)));

                // Setting up the rest of the parameters for EnumProcessModules
                uint uiSize = (uint)(intPtrSize * (hMods.Length));

                uint cbNeeded = 0;

                bool enumproc = EnumProcessModulesEx(
                    procPtr, 
                    pModules, 
                    uiSize, 
                    out cbNeeded, 
                    DwFilterFlag.LIST_MODULES_ALL);

                if (enumproc == true)
                {
                    Int32 uiTotalNumberofModules = 
                        (Int32)(cbNeeded / intPtrSize);

                    for (int i = 0; i < (int)uiTotalNumberofModules; i++)
                    {
                        StringBuilder strbld = new StringBuilder(1024);

                        GetModuleFileNameEx(procPtr, 
                            hMods[i], 
                            strbld, 
                            (int)(strbld.Capacity));

                        String module = CutPath(strbld.ToString());
                        if (Util.ContainsCaseInsensitive(module, "d3d") ||
                            Util.ContainsCaseInsensitive(module, "opengl"))
                        {
                            haveFound3DModule = true;
#if DEBUG
                            modNames.Add(module);
#else
                            //skip processing remaining modules if not debugging
                            i = (int)uiTotalNumberofModules;
#endif
                        }

                    }         

                    if (haveFound3DModule)
                    {
                        procList.Add(processName);
#if DEBUG
                        Console.WriteLine("File: {0}  Modules: {1}", 
                            processName, String.Join(",", modNames));
                        Console.WriteLine();
#endif
                    }
                }

                // Must free the GCHandle object
                gch.Free();
            }

            return procList;

        }


        


        /// <summary>
        ///     Only return the last path element.
        ///     I.e. foo\bar\baz.exe will return baz.exe
        /// </summary>
        /// <param name="path">Path to be cut</param>
        /// <returns>Last path element</returns>
        private static String CutPath(String path)
        {
            int last = path.LastIndexOf("\\");
            return path.Substring(last + 1, path.Length - last - 1);
        }



    }
}