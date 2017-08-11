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

        const String LOG_FILENAME = "NTP.log";

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
        public DateTime retrieveCurrentTime()
        {
            //setTime("10:10");
            //setDate("01.02.2017");

            this.Log("Start retrieving time");

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
                this.Log("Time and Date retrieved: "+ currentTime.Day + "-" + currentTime.Month + "-" + currentTime.Year + " " + currentTime.Hour + ":" + currentTime.Minute);

                //time retrieved successfully

                //setting time
                this.Log("Start setting time");
                setTime(currentTime.Hour + ":" + currentTime.Minute);
                DateTime dt = DateTime.Now;
                this.Log("System time: " + dt.Hour + ":" + dt.Minute);
                
                //setting date
                this.Log("Start setting date");
                setDate(currentTime.Day + "-" + currentTime.Month + "-" + currentTime.Year);
                dt = DateTime.Now;
                this.Log("System date: " + dt.Year + "-" + dt.Month + "-" + dt.Day);

                //exit the service
                this.Log("Stopping the service");
                this.Stop();
            }
            catch
            {
                //time could not be retrieved => wait some time and retry again
                this.Log("Something went wrong => wait for 10s and try again");
                var t = Task.Run(() => {
                    //wait for 10s                    
                    System.Threading.Thread.Sleep(10000);
                }).ContinueWith((param) => {
                    //try again (on the UI thread)
                    this.Log("Trying again");
                    this.performSync();
                });
            }
        }

        /// <summary>
        /// Write the content to the log file. First it will check if the logging in enabled in the config file
        /// </summary>
        /// <param name="content">Content of the file</param>
        public void Log(String content) {
            if (Settings.Default.Log_Active){
                DateTime dt = DateTime.Now;
                System.IO.File.AppendAllText(LOG_FILENAME, dt.ToString() + ": " + content + "\n");
            }
        }

        /// <summary>
        /// Clear the content of the LOG file
        /// </summary>
        public void ClearLog()
        {
            if (Settings.Default.Log_Active)
            {
                System.IO.File.WriteAllText(LOG_FILENAME,"");
            }
        }

        /// <summary>
        /// On service start => perform the date and time sync
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            //clear the content of the log file first
            this.ClearLog();
            this.Log("Service started");

            //apply the startup delay (defaul 20s)
            this.Log("waiting for the startup delay " + Settings.Default.Startup_Delay + "ms");
            var t = Task.Run(() => {
                //wait for 10s
                System.Threading.Thread.Sleep(Settings.Default.Startup_Delay);
            }).ContinueWith((param) => {
                //try again (on the UI thread)
                this.Log("Starting the sync process");
                this.performSync();
            });
        }

        protected override void OnStop()
        {
            //on stop
        }
        
    }
}
