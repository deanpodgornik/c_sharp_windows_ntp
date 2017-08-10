using GuerrillaNtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NTP
{
    class SyncDateTime : ServiceBase
    {
        public static String serviceName = "deanpodgornik.NTP";

        public SyncDateTime()
        {
            
        }

        /// <summary>
        /// Set date of the system
        /// </summary>
        /// <param name="dateInYourSystemFormat">Format: dd-mm-yy</param>
        public void setDate(string dateInYourSystemFormat)
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
        public void setTime(string timeInYourSystemFormat)
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
        static DateTime retrieveCurrentTime()
        {
            //setTime("10:10");
            //setDate("01.02.2017");

            // perform only one query to get the offset
            TimeSpan offset;
            using (var ntp = new NtpClient(Settings.Default.NTP_Server))
            {
                ntp.Timeout = TimeSpan.FromSeconds(5);
                var ntpResponse = ntp.Query();
                offset = ntpResponse.CorrectionOffset;
            }

            //add UTC offset to achive the locat time 
            var UTC_offset = new TimeSpan(Settings.Default.UTC_Offset, 0, 0);

            // use the offset throughout your app
            return DateTime.UtcNow + offset + UTC_offset;
        }

        /// <summary>
        /// Retrive the current time and date => apply it to the OS
        /// </summary>
        public void performSync() {
            try
            {
                //on start
                var currentTime = retrieveCurrentTime();

                //time retrieved successfully

                //setting time
                setTime(currentTime.Hour + ":" + currentTime.Minute);
                //setting date
                setDate(currentTime.Day + "-" + currentTime.Month + "-" + currentTime.Year);

                //exit the service
                this.Stop();
            }
            catch
            {
                //time could not be retrieved => wait some time and retry again
                
                var t = Task.Run(() => {
                    //wait for 10s
                    System.Threading.Thread.Sleep(10000);
                }).ContinueWith((param) => {
                    //try again (on the UI thread)
                    this.performSync();
                });
            }
        }

        /// <summary>
        /// On service start => perform the date and time sync
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            this.performSync();

        }

        protected override void OnStop()
        {
            //on stop
        }
        
    }
}
