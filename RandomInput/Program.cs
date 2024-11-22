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
using System.Text.Json;
using Newtonsoft.Json;

namespace RandomInput
{
    public class InputData
    {
        public List<KeyInput> Keys { get; set; }
    }

    public class KeyInput
    {
        public string Key { get; set; }
        public double Chance { get; set; }
    }
    class Program
    {
        private static List<KeyInput> inputs;
        private static Random rand = new Random();
        private static Thread worker;
        private static bool doLoop = false;
        private static IntPtr targetWindowHandle;
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr point);

        static void Main(string[] args)
        {
            inputs = LoadInputs("inputs.json");
            if (inputs == null || inputs.Count == 0)
            {
                Console.WriteLine("No valid inputs found in inputs.json. Exiting.");
                return;
            }
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
        static List<KeyInput> LoadInputs(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {filePath} not found.");
                return null;
            }

            var json = File.ReadAllText(filePath);
            InputData inputData = JsonConvert.DeserializeObject<InputData>(json);

            if (inputData?.Keys == null || !inputData.Keys.Any())
            {
                Console.WriteLine("Invalid or empty inputs.json.");
                return null;
            }

            // Validate probabilities
            double totalChance = inputData.Keys.Sum(k => k.Chance);
            if (Math.Abs(totalChance - 1.0) > 0.0001)
            {
                Console.WriteLine("The chances in inputs.json do not add up to 1.");
                return null;
            }

            return inputData.Keys;
        }
        static void doRandomKeystroke()
        {
            InputSimulator isim = new InputSimulator();

            while (doLoop)
            {
                if (!SetForegroundWindow(targetWindowHandle))
                {
                    Console.WriteLine("Failed to set the target window to foreground. Stopping.");
                    doLoop = false;
                    return;
                }

                Thread.Sleep(200);

                // Randomly select a key based on probabilities
                double randomValue = rand.NextDouble();
                double cumulative = 0;

                foreach (var input in inputs)
                {
                    cumulative += input.Chance;
                    if (randomValue <= cumulative)
                    {
                        Console.WriteLine($"Sending {input.Key}");
                        SimulateKey(isim, input.Key);
                        break;
                    }
                }
            }
        }
        static void SimulateKey(InputSimulator isim, string key)
        {
            if (Enum.TryParse(typeof(WindowsInput.Native.VirtualKeyCode), "VK_" + key.ToUpper(), out var vkCode))
            {
                // Simulate key press if it's a valid VirtualKeyCode
                isim.Keyboard.KeyPress((WindowsInput.Native.VirtualKeyCode)vkCode);
            }
            else if (key.ToUpper() == "M1")
            {
                isim.Mouse.LeftButtonClick();
            }
            else if (key.ToUpper() == "M2")
            {
                isim.Mouse.RightButtonClick();
            }
            else
            {
                Console.WriteLine($"Unrecognized key: {key}. Skipping.");
            }
        }
    }
}