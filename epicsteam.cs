using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class EpicSteam
{
    public static List<Process> GetChildren(Process epicProcess)
    {
        List<Process> gameProcesses = new List<Process>();
        ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(
            "SELECT * " +
            "FROM Win32_Process " +
            "WHERE ParentProcessId=" + epicProcess.Id);
        ManagementObjectCollection epicChildren = searcher.Get();
        if (epicChildren.Count > 0)
        {
            foreach (var item in epicChildren)
            {
                UInt32 childProcessId = (UInt32)item["ProcessId"];
                if ((int)childProcessId != Process.GetCurrentProcess().Id)
                {
                    Process childProcess = Process.GetProcessById((int)childProcessId);
                    if (!childProcess.ProcessName.StartsWith("Epic"))
                    {
                        // This is most likely the/a game process
                        gameProcesses.Add(childProcess);
                    }
                }
            }
        }
        return gameProcesses;
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            System.Console.WriteLine("ERROR: Needs launch URL as parameter");
            return;
        }

        // TODO: Removing the silent argument from the URL may lead to the launcher not being minimized.
        //       That would help with a more clean shutdown.
        var epicUrl = args[0];

        var ps = new ProcessStartInfo(epicUrl)
        {
            UseShellExecute = true,
            Verb = "open",
        };

        System.Console.WriteLine($"Starting url: {epicUrl}");
        var epicProcess = Process.Start(ps);

        // Try to find a running game process from the list of the launchers children
        // It may take quite some time for the game to launch if:
        // - EpicLauncher needs an update
        // - Cloud sync has been interupted last time (e.g. needs to complete before launch)
        List<Process> gameProcesses = new List<Process>();
        for (int i = 0; i < 120; i++)
        {
            System.Console.WriteLine($"Waiting for game to start ({i})...");
            Thread.Sleep(1000);
            gameProcesses = GetChildren(epicProcess);
            if (gameProcesses.Count > 0)
            {
                // Found at least one process looking like the game process we are looking for
                break;
            }
        }

        if (gameProcesses.Count == 0) {
            System.Console.WriteLine("Unable to find any game process");
            return;
        }

        foreach (var process in gameProcesses)
        {
            System.Console.WriteLine($"Game [{process.ProcessName}] started.");
            process.WaitForExit();
            System.Console.WriteLine($"Game [{process.ProcessName}] exited.");
        }

        System.Console.WriteLine("All potential game processes exited, waiting 30s for cloud sync to complete...");
        Thread.Sleep(30 * 1000);
        for (int i = 0; i < 3; i++) {
            System.Console.WriteLine($"Trying to gracefully terminate EpicLauncher ({i})...");
            // This seems to only work of the window is actually visible.
            // The launcher will refuse to terminate if a cloud-sync process is still running
            epicProcess.CloseMainWindow();
            // Wait another 10s for epic launcher to terminate
            epicProcess.WaitForExit(10 * 1000);
        }

        // Kill the launcher if it is still running
        if (!epicProcess.HasExited)
        {
            System.Console.WriteLine("EpicLauncher still running, sending kill signal");
            epicProcess.Kill();
            epicProcess.WaitForExit();
        }
        epicProcess.Close();
    }
}