﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTP
{
    class Program
    {
        static void setDate(string dateInYourSystemFormat)
        {
            var proc = new System.Diagnostics.ProcessStartInfo();
            proc.UseShellExecute = true;
            proc.WorkingDirectory = @"C:\Windows\System32";
            proc.CreateNoWindow = true;
            proc.FileName = @"C:\Windows\System32\cmd.exe";
            proc.Verb = "runas";
            proc.Arguments = "/C date " + dateInYourSystemFormat;
            try
            {
                System.Diagnostics.Process.Start(proc);
            }
            catch
            {

            }
        }
        static void setTime(string timeInYourSystemFormat)
        {
            var proc = new System.Diagnostics.ProcessStartInfo();
            proc.UseShellExecute = true;
            proc.WorkingDirectory = @"C:\Windows\System32";
            proc.CreateNoWindow = true;
            proc.FileName = @"C:\Windows\System32\cmd.exe";
            proc.Verb = "runas";
            proc.Arguments = "/C time " + timeInYourSystemFormat;
            try
            {
                System.Diagnostics.Process.Start(proc);
            }
            catch
            {

            }
        }

        static void Main(string[] args)
        {
            setTime("10:10");
            setDate("01.02.2017");
        }
    }
}
