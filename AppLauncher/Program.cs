using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CryptoZ.Tools;

namespace CryptoZ
{
    // AppLauncher
    class Program
    {
        static bool exitApp = false;
        static string thisExeFilepath, thisExePath;
        static string tickerStorageExe, tickerProducerExe;

        static List<Process> startedProcesses = new List<Process>();


        static void Main(string[] args)
        {
            DisplayWelcomeMessage();

            InitializePaths();

            while (exitApp == false)
            {
                DisplayMenu();
                var choice = Console.ReadLine();
                ProcessChoice(choice);
            }

            CleanUp();
        }

        static void DisplayWelcomeMessage()
        {
            Console.WriteLine("\n=== WELCOME TO CRYPTOZ APP LAUNCHER ===\n");
            Console.WriteLine("This .NET Core app provides a menu to launch other CryptoZ apps.\n");
        }

        static void DisplayMenu()
        {
            Console.WriteLine();
            Console.WriteLine("1. Start Ticker Consumer (display messages)");
            Console.WriteLine("2. Start Ticker Consumer (display 1-char code for each messsage)");
            Console.WriteLine("3. Start Ticker Consumer (store data in .csv files)");
            Console.WriteLine("4. Start Ticker Consumer (store data in SQL database)");
            Console.WriteLine("5. Run Ticker Producer for 5 minutes");
            Console.WriteLine("6. Start Ticker Producer (runs indefinitely)");
            Console.WriteLine("9. Exit");
            Console.WriteLine();
            Console.Write("Enter choice: ");
        }

        static void InitializePaths()
        {
            thisExeFilepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            thisExePath = System.IO.Path.GetDirectoryName(thisExeFilepath);
            tickerStorageExe = FileTools.GetProjectExeFilepathReleaseOrDebug(thisExePath, "TickerStorage", "TickerStorage.exe", "netcoreapp3.1", "Consumers");
            tickerProducerExe = FileTools.GetProjectExeFilepathReleaseOrDebug(thisExePath, "TickerProducer", "TickerProducer.exe", "netcoreapp3.1", "Producers");
        }

        static void ProcessChoice(string choice)
        {
            if (choice == "1")
            {
                StartConsoleApp(tickerStorageExe, "display");
            }
            else if (choice == "2")
            {
                StartConsoleApp(tickerStorageExe, "code");
            }
            else if (choice == "3")
            {
                StartConsoleApp(tickerStorageExe, "csv");
            }
            else if (choice == "4")
            {
                StartConsoleApp(tickerStorageExe, "sql");
            }
            else if (choice == "5")
            {
                StartConsoleApp(tickerProducerExe, "all 5");
            }
            else if (choice == "6")
            {
                StartConsoleApp(tickerProducerExe, "all");
            }
            else if (choice == "9")
            {
                exitApp = true;
            }
            else
            {
                Console.WriteLine($"Unrecognized choice 'choice'.");
            }
        }

        static void ConfigureProcess(Process proc, string filepath, string args)
        {
            proc.StartInfo.FileName = filepath;     //"csc.exe";
            proc.StartInfo.Arguments = args;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            //proc.StartInfo.RedirectStandardOutput = true;
            //proc.StartInfo.CreateNoWindow = false;
        }

        // Run a Console App in a new window
        static void StartConsoleApp(string filepath, string args)
        {
            Process proc = new Process();
            ConfigureProcess(proc, filepath, args);
            bool success = proc.Start();
            if (success) startedProcesses.Add(proc);
            // The following would only apply if we were creating processes with
            // RedirectStandardOutput set to true:
            //Console.WriteLine(proc.StandardOutput.ReadToEnd());
            //proc.WaitForExit();
        }

        static void CleanUp()
        {
            DisposeProcesses();
        }

        static void DisposeProcesses()
        {
            foreach (var p in startedProcesses)
            {
                p.Kill(true);
                p.Dispose();
            }
        }


    } // class

} // namespace
