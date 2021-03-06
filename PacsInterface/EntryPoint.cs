﻿using GUI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace PacsInterface
{
    class EntryPoint
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "PacsInterfaceConsole";
            MainWindow mainWindow = new MainWindow();
            Program program = new Program(mainWindow);

            mainWindow.Show();
            Dispatcher.Run();
        }
    }
}
