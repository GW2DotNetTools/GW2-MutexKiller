using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
namespace GW2MutexKiller 
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string MutexName = "AN-Mutex-Window-Guild Wars 2";
            string ProcessNameOriginal = "gw2";
            string ProcessNameTest = "Gw2FeaturePublicTestTiny";

            try
            {
                Process process = null;
                if (Process.GetProcessesByName(ProcessNameOriginal).Count() == 1)
                {
                    process = Process.GetProcessesByName(ProcessNameOriginal)[0];
                }
                else
                {
                    process = Process.GetProcessesByName(ProcessNameTest)[0];
                }

                List<Win32API.SYSTEM_HANDLE_INFORMATION> handles = new List<Win32API.SYSTEM_HANDLE_INFORMATION>();
                for (int i = 0; i < 99; i++)
                {
                    string mutex = "\\Sessions\\" + i + "\\BaseNamedObjects\\" + MutexName;
                    Console.WriteLine("Searching in " + mutex);
                    handles = Win32Processes.GetHandles(process, "Mutant", mutex);
                    if (handles.Count > 0)
                    { 
                        break; 
                    }
                }
               
                foreach (var handle in handles)
                {
                    IntPtr ipHandle = IntPtr.Zero;
                    if (!Win32API.DuplicateHandle(Process.GetProcessById(handle.ProcessID).Handle, handle.Handle, Win32API.GetCurrentProcess(), out ipHandle, 0, false, Win32API.DUPLICATE_CLOSE_SOURCE))
                    {
                        Console.WriteLine("DuplicateHandle() failed, error = {0}", Marshal.GetLastWin32Error()); 
                    }

                    Console.WriteLine("Mutex was killed");
                }

                for (int i = 3; i > 0; i--)
                {
                    Console.WriteLine("Closing in..." + i);
                    Thread.Sleep(1000);
                }

                return;
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("The process is not currently running.");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("The Mutex '{0}' was not found.", MutexName);
            }

            Console.WriteLine("Press any key to close.");
            Console.ReadLine();
        }
    }
}