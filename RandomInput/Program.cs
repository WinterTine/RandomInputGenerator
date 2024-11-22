// Input list: M1 M2 W A S D E R Space
// M1 M2 E R Space small inpt
// W A S D med inpt
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using WindowsInput;

namespace RandomInput
{
    class Program
    {
        private static Thread worker;
        private static bool doLoop = false;
        private static IntPtr targetWindowHandle;
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr point);

        static void Main(string[] args)
        {
            Console.WriteLine("Select the process you want to target (Apps only):");

            Process[] processes = Process.GetProcesses();
            var apps = new List<Process>();

            for (int i = 0; i < processes.Length; i++)
            {
                // Filter only processes with a main window handle
                if (processes[i].MainWindowHandle != IntPtr.Zero)
                {
                    apps.Add(processes[i]);
                    Console.WriteLine($"{apps.Count}: {processes[i].ProcessName} (PID: {processes[i].Id})");
                }
            }

            if (apps.Count == 0)
            {
                Console.WriteLine("No active apps found. Exiting.");
                return;
            }

            Console.WriteLine("Enter the number corresponding to the app:");
            if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= apps.Count)
            {
                Process selectedProcess = apps[selection - 1];
                targetWindowHandle = selectedProcess.MainWindowHandle;

                if (targetWindowHandle == IntPtr.Zero)
                {
                    Console.WriteLine("The selected app does not have a main window. Exiting.");
                    return;
                }

                Console.WriteLine($"Targeting app: {selectedProcess.ProcessName} (PID: {selectedProcess.Id})");
                Console.WriteLine("Press 'P' to toggle input simulation on/off. Press 'Q' to quit.");
                while (true)
                {
                   
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true).Key;
                        if (key == ConsoleKey.P)
                        {

                            if (doLoop)
                            {
                                Console.WriteLine("The application has been paused.");
                                doLoop = false;
                                worker.Join();
                            }
                            else
                            {
                                doLoop = true;
                                worker = new Thread(doRandomKeystroke);
                                worker.Start();
                            }
                          
                        }
                        else if (key == ConsoleKey.Q){
                            doLoop = false;
                            worker.Join();
                            break;
                        }

                    }
                }
            }
            Console.WriteLine("Press a key to exit...");
            Console.ReadKey();
           
        }
        static void doRandomKeystroke()
        {
            InputSimulator isim = new InputSimulator();
            Random randkey = new Random();
            while (doLoop)
            {
                if (!SetForegroundWindow(targetWindowHandle))
                {
                    Console.WriteLine("Failed to set the target window to foreground. Stopping.");
                    doLoop = false;
                    return;
                }
                Thread.Sleep(200);
                int key = randkey.Next(1, 19);
                if (key >= 1 && key < 6)
                {
                    Console.WriteLine("Sending M1");
                    isim.Mouse.LeftButtonDown();
                    Thread.Sleep(50);
                    isim.Mouse.LeftButtonUp();
                }
                if (key >= 6 && key < 11)
                {
                    Console.WriteLine("Sending M2");
                    isim.Mouse.RightButtonDown();
                    Thread.Sleep(50);
                    isim.Mouse.RightButtonUp();
                }
                if (key == 11 || key == 12)
                {
                    Console.WriteLine("Sending E");
                    isim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_E);
                    Thread.Sleep(50);
                    isim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_E);
                }
                if (key == 13)
                {
                    Console.WriteLine("Sending R");
                    isim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_R);
                    Thread.Sleep(50);
                    isim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_R);
                }
                if (key == 14)
                {
                    Console.WriteLine("Sending SPACE");
                    isim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.SPACE);
                    Thread.Sleep(50);
                    isim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.SPACE);
                }
                if (key == 15)
                {
                    Console.WriteLine("Sending W");
                    isim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_W);
                    Thread.Sleep(500);
                    isim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_W);
                }
                if (key == 16)
                {
                    Console.WriteLine("Sending A");
                    isim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_A);
                    Thread.Sleep(500);
                    isim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_A);
                }
                if (key == 17)
                {
                    Console.WriteLine("Sending S");
                    isim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_S);
                    Thread.Sleep(500);
                    isim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_S);

                }
                if (key == 18)
                {
                    Console.WriteLine("Sending D");
                    isim.Keyboard.KeyDown(WindowsInput.Native.VirtualKeyCode.VK_D);
                    Thread.Sleep(500);
                    isim.Keyboard.KeyUp(WindowsInput.Native.VirtualKeyCode.VK_D);
                }
            }
            
            

        }

        }
}