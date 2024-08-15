


using System.Diagnostics;

namespace PuppetServiceWorker.Modules;


public class ProcessManipulation
{
 





    
public      static void KillProcessesByName(string processName)
    {
        try
        {
            Process[] processes = Process.GetProcessesByName(processName);
            foreach (Process process in processes)
            {
                process.Kill();
                Console.WriteLine($"Killed process: {process.ProcessName} (ID: {process.Id})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}