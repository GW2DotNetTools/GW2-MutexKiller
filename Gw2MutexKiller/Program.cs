using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
namespace GW2MutexKiller 
{
    public class Program
    {
        private const string Mutex = "AN-Mutex-Window-Guild Wars 2";
       
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) => HandleException();
            Application.ThreadException += (o, e) => HandleException();

            bool result = Run();
            Console.WriteLine(result ? "Could not find Mutex: " + Mutex : "Please check if GW2 is running!");

            Console.WriteLine("Press any key to close.");
            Console.ReadLine();
        }

        private static void HandleException() 
        {
            Console.WriteLine("Ups! Something went wrong. Please try again.");
            Console.WriteLine("Press any key to close.");
            Console.ReadLine();
            Environment.Exit(0);
        }

        private static bool Run()
        {
            bool runAtLeastOnce = false;
            foreach (var process in Process.GetProcesses().Where(x => x.ProcessName.ToLower().StartsWith("gw2") && !x.ProcessName.ToLower().StartsWith("gw2mutexkiller") ).ToList())
            {
                Console.WriteLine("Checking in Process: " + process.ProcessName);
                runAtLeastOnce = true;
                for (int sessionId = 0; sessionId < 10; sessionId++)
                {
                    RunThroughSession(sessionId, process);
                }
                Console.WriteLine();
            }

            return runAtLeastOnce;
        }

        private static void RunThroughSession(int sessionId, Process process)
        {
            Console.WriteLine(string.Format("Searching for handle: {0}", Mutex));

            List<Win32API.SYSTEM_HANDLE_INFORMATION> handles = Win32Processes.GetHandles(process, "Mutant");
            foreach (var handle in handles)
            {
                string strObjectName2 = Win32Processes.getObjectName(handle, Process.GetProcessById(handle.ProcessID));
                if (!string.IsNullOrWhiteSpace(strObjectName2) && strObjectName2.EndsWith(Mutex))
                {
                    IntPtr ipHandle = IntPtr.Zero;
                    if (Win32API.DuplicateHandle(Process.GetProcessById(handle.ProcessID).Handle, handle.Handle, Win32API.GetCurrentProcess(), out ipHandle, 0, false, Win32API.DUPLICATE_CLOSE_SOURCE))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Mutex was killed");
                        AutoClose();
                    }
                }
            }
        }

        private static void AutoClose()
        {
            for (int j = 3; j > 0; j--)
            {
                Console.WriteLine("Closing in..." + j);
                Thread.Sleep(1000);
            }

            Environment.Exit(0);
        }
    }
}