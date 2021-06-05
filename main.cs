using System;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class HelloWorld
{
    private const int WM_DESTROY = 0x2;
    private const int WM_QUIT = 0x12;
    private const int WM_CLOSE = 0x1;
    [DllImport("user32.dll")]
    public static extern int PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            System.Console.WriteLine("ERROR: Needs launch URL and EXE Name");
            return;
        }

        var epicUrl = args[0];
        var exeName = args[1];

        var ps = new ProcessStartInfo(epicUrl)
        {
            UseShellExecute = true,
            Verb = "open",
            WindowStyle = ProcessWindowStyle.Minimized
        };

        System.Console.WriteLine($"Starting url: {epicUrl}");
        var epicProcess = Process.Start(ps);
        System.Console.WriteLine($"epicProcess {epicProcess}");

        var gameProcesses = Process.GetProcessesByName(exeName);
        for (int i = 0; i < 120; i++)
        {
            Thread.Sleep(1000);
            gameProcesses = Process.GetProcessesByName(exeName);
            if (gameProcesses.Length > 0)
            {
                // Found at least one process looking like the game process we are looking for
                break;
            }
        }

        if (gameProcesses.Length != 1)
        {
            System.Console.WriteLine($"Could not find a single process with name: {exeName}");
            return;
        }
        else
        {
            foreach (Process p in gameProcesses)
            {
                System.Console.WriteLine($"gameProcess {p}");
            }
        }

        System.Console.WriteLine("Game started.");
        gameProcesses[0].WaitForExit();
        System.Console.WriteLine("Game exited, waiting 10s for cloud sync...");
        Thread.Sleep(10 * 1000);
        System.Console.WriteLine("Trying to exit epic launcher");

        // This seems to only work of the window is actually visible
        epicProcess.CloseMainWindow();
        // Wait 5s for epic to stop
        epicProcess.WaitForExit(5 * 1000);

        int exitMethod = 1;
        while (!epicProcess.HasExited)
        {
            System.Console.WriteLine($"epic launcher still not gone, trying: {exitMethod}...");
            if (exitMethod == 1)
            {
                PostMessage(epicProcess.MainWindowHandle, WM_DESTROY, 0, 0);
                exitMethod = 2;
            }
            else if (exitMethod == 2)
            {
                PostMessage(epicProcess.MainWindowHandle, WM_QUIT, 0, 0);
                exitMethod = 3;
            }
            else if (exitMethod == 3)
            {
                PostMessage(epicProcess.MainWindowHandle, WM_CLOSE, 0, 0);
                exitMethod = 4;
            }
            else
            {
                System.Console.WriteLine("Ultimately unable to quit epic launcher");
                epicProcess.Kill();
                break;

            }
            // Wait 5s for epic to stop (sync and stuff)
            epicProcess.WaitForExit(5 * 1000);
        }

        epicProcess.Close();
    }
}
