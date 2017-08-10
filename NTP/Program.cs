using GuerrillaNtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Collections;
using System.Diagnostics;

namespace NTP
{
    class Program
    {
        /// <summary>
        /// The is the main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            bool install = false, uninstall = false, console = false, rethrow = false;
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-i":
                    case "-install":
                        install = true; break;
                    case "-u":
                    case "-uninstall":
                        uninstall = true; break;
                    case "-c":
                    case "-console":
                        console = true; break;
                    default:
                        Console.Error.WriteLine("Argument notexpected: " + arg);
                        break;
                }
            }

            if (uninstall)
            {
                //uninstalling service
                Install(true, args);
            }
            else if (install)
            {
                //installing service
                Install(false, args);
            }
            else
            {
                //normal start => 
                if (Environment.UserInteractive)
                {
                    //user interactive mode
                    var svc = new SyncDateTime();
                    var type = svc.GetType();
                    var start = ((System.Reflection.MethodInfo[])((System.Reflection.TypeInfo)type).DeclaredMethods).SingleOrDefault(x => x.Name == "OnStart");
                    start.Invoke(svc, new[] { args });
                    
                    Console.WriteLine("Services started");
                    Console.ReadLine();

                    var stop = ((System.Reflection.MethodInfo[])((System.Reflection.TypeInfo)type).DeclaredMethods).SingleOrDefault(x => x.Name == "OnStop");
                    stop.Invoke(svc, null);
                }
                else
                {
                    //non-interactive mode
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new SyncDateTime()
                    };
                    ServiceBase.Run(ServicesToRun);

                }

            }
        }

        /// <summary>
        /// Installing/Uninstalling the service
        /// </summary>
        /// <param name="undo"></param>
        /// <param name="args"></param>
        public static void Install(bool undo, string[] args)
        {
            try
            {
                Console.WriteLine(undo ? "uninstalling" : "installing");
                using (AssemblyInstaller inst = new AssemblyInstaller(typeof(Program).Assembly, args))
                {
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        if (undo)
                        {
                            inst.Uninstall(state);
                        }
                        else
                        {
                            inst.Install(state);
                            SetRecoveryOptions(SyncDateTime.serviceName);
                            inst.Commit(state);
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch { }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Recovery options
        /// </summary>
        /// <param name="serviceName"></param>
        static void SetRecoveryOptions(string serviceName)
        {
            int exitCode;
            using (var process = new Process())
            {
                var startInfo = process.StartInfo;
                startInfo.FileName = "sc";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                // tell Windows that the service should restart if it fails
                startInfo.Arguments = string.Format("failure \"{0}\" reset= 0 actions= restart/0", serviceName);

                process.Start();
                process.WaitForExit();

                exitCode = process.ExitCode;
            }

            if (exitCode != 0)
                throw new InvalidOperationException(string.Format("sc failure setting process exited with code { 0 }"));
        }
    }
}
