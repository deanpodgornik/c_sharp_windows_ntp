using GuerrillaNtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTP
{
    class Program
    {
        /// <summary>
        /// Set date of the system
        /// </summary>
        /// <param name="dateInYourSystemFormat">Format: dd-mm-yy</param>
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

        /// <summary>
        /// Set time of the system
        /// </summary>
        /// <param name="timeInYourSystemFormat">Format: mm:ss</param>
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

        /// <summary>
        /// Retrieve the current time from a NTP service
        /// </summary>
        static DateTime retrieveCurrentTime() {
            //setTime("10:10");
            //setDate("01.02.2017");

            // perform only one query to get the offset
            TimeSpan offset;
            using (var ntp = new NtpClient(Settings.Default.NTPServer))
            {
                ntp.Timeout = TimeSpan.FromSeconds(5);
                var ntpResponse = ntp.Query();
                offset = ntpResponse.CorrectionOffset;
            }
            // use the offset throughout your app
            return DateTime.UtcNow + offset;
        }

        static void Main(string[] args)
        {
            
            //TODO: register as service
            
            //TODO: implement retry in case of no internet connection
            
            var currentTime = retrieveCurrentTime();
            //setting time
            setTime(currentTime.Hour+":"+currentTime.Minute);
            //setting date
            setDate(currentTime.Day+"-"+currentTime.Month+"-"+currentTime.Year);
        }
    }
}
