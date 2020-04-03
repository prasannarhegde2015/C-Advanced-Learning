using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Weatherford.Core.Logging;
using Weatherford.Core.Logging.Desktop;
using Weatherford.DynaCardLibrary.API.Enums;
using Weatherford.POP.APIClient;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Interfaces;
using Weatherford.POP.Utils;

namespace Weatherford.POP.Scheduler.Jobs
{
    public class Program
    {
        #region  Ctrl-C Handler
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        public delegate bool HandlerRoutine(CtrlTypes CtrlType);
        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            Trace.WriteLine("** Received termination event : " + ctrlType.ToString());
            return true;
        }
        #endregion


        internal class ServiceFactory
        {
            private ClientServiceFactory _clientFactory;

            private bool _useHTTPS;

            internal ServiceFactory(string hostname, bool useHTTPS)
            {
                _clientFactory = new ClientServiceFactory(hostname);
                _useHTTPS = useHTTPS;
            }

            internal T GetService<T>()
            {
                return _clientFactory.GetService<T>(null, _useHTTPS);
            }

            internal int? DefaultTimeout
            {
                get { return _clientFactory.DefaultTimeout; }
                set { _clientFactory.DefaultTimeout = value; }
            }
        }

        #region Global Variables and properties

        internal class Args
        {
            internal const string UpdateDynaCards = "-runAnalysis";
            internal const string GetIBMData = "-getIBMData";
            internal const string GenIBMData = "-m";
            internal const string UpdateMTBFTable = "-runMTBFCalc";
            internal const string UpdateMostCommonEventTypesTable = "-getMostCommonEventTypes";
            internal const string InsertScenarioScheduleDailyRun = "-insertScenarioScheduleDailyRun";
            internal const string RunScenarioScheduler = "-runScenarioScheduler";
            internal const string RunAllocations = "-runAllocationProduction";
            internal const string RunIntelligentAlarmCheckForESPWells = "-runIntelligentAlarmCheckESP";
            internal const string RunIntelligentAlarmCheckForPCPWells = "-runIntelligentAlarmCheckPCP";
            internal const string RunAllocationsTimeSpan = "-runAllocationTimeSpan";
            internal const string RunProductionAllocations = "-runProductionAllocation";
            internal const string RunProdAllocationsTimeSpan = "-runProdAllocationTimeSpan";
            internal const string RunIntelligentAlarmCheckForGLWells = "-runIntelligentAlarmCheckGL";
            internal const string RunRunLatestPlungerLiftCycleCheck = "-runLatestPlungerLiftCycleCheck";
            internal const string RunIntelligentAlarmSendForRRLWells = "-runIntelligentAlarmSendRRL";
            internal const string RunIntelligentAlarmCheckForPGLWells = "-runIntelligentAlarmCheckPGL";
            internal const string RunAutonomousControlForRRLWells = "-runAutonomousControlRRL";
            internal const string RunWellRunStatusDerivation = "-runWellRunStatusDerivation";
            internal const string RunWAGCycleCheck = "-runWAGCycleCheck";
            internal const string GetAWTData = "-getAWtData";
            internal const string RunJobEconomicAnalysis = "-runEconomicAnalysisAndDPICalculation";
            //internal const string GetAWTDataTimeSpan = "-getAWtDataTimeSpan";
            internal const string UpdateDowntime = "-updateDowntime";
            internal const string UpdateAllDowntime = "-updateLostProductionAllDowntime";
            internal const string BuildDowntimeCache = "-rebuildDowntimeWellCache";
            internal const string UseHTTPS = "-https";
            internal const string DataConnectionKey = "-dataConnectionKey";
            internal const string RenameCardBlobEntries = "-renameCardBlobEntries";
            internal const string UpdateOperatingEnvelopeForPCP = "-UpdateOperatingEnvelopeForPCP";
            internal const string RunAutomaticDownTimeCalculations = "-runAutomaticDownTimeCalculations";
            internal const string RunAllocationForWellName = "-wellNameForTimeSpan";
        }

        private static readonly int s_groupSize = 50;

        private const int MiliSecondsInASecond = 1000;

        public static string Hostname
        {
            get { return ConfigurationManager.AppSettings.Get("Server"); }
        }

        public static int TimeoutInSeconds
        {
            get
            {
                if (int.TryParse(ConfigurationManager.AppSettings["TimeoutInSeconds"], out int timeoutInSeconds))
                    return timeoutInSeconds;
                else
                    return 300; //By default returning 300 seconds
            }
        }

        private static ServiceFactory ClientFactory { get; set; }

        private static LogMessageSource logger;

        private const int MaxTimestampEntriesForDynacard = 20;

        #endregion

        #region Main Program
        public static void Main(string[] args)
        {
            Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            LogMessageRouterSettings.Load(appConfig);
            logger = new LogMessageSource(LogMessageCategory.General, LogMessagePriority.Medium);

            var argMgr = new ArgumentManager();
            argMgr.SetDefaultHelpArgs();

            argMgr.AddParameter(Args.UpdateDynaCards, "Check for the latest scanned cards and run analysis on the newly scanned cards", true);
            argMgr.AddParameter(Args.GetIBMData, "Get IBM failure prediction data", false);
            argMgr.AddHiddenParameter(Args.GenIBMData, "Generate failure prediction data", false);
            argMgr.AddParameter(Args.UpdateMTBFTable, "Call the server method that populates data necessary for historical MTBF calculation", false);
            argMgr.AddParameter(Args.UpdateMostCommonEventTypesTable, "Call the server method that populates data most commonly used event types", false);
            argMgr.AddParameter(Args.InsertScenarioScheduleDailyRun, "Call the server method that inserts daily run scenarios into the scheduler", false);
            argMgr.AddParameter(Args.RunScenarioScheduler, "Call the server method that runs the scenarios in the scheduler", false);
            argMgr.AddParameter(Args.RunAllocations, "Call the server method that runs the allocations calculation", false);
            argMgr.AddParameter(Args.RunIntelligentAlarmCheckForESPWells, "Call the server method that runs intelligent alarm check for ESP wells", false);
            argMgr.AddParameter(Args.RunIntelligentAlarmCheckForPCPWells, "Call the server method that runs intelligent alarm check for PCP wells", false);
            argMgr.AddParameter(Args.RunAllocationsTimeSpan, "Call the server method that runs the allocations calculation with time span setting", false);
            argMgr.AddParameter(Args.RunProductionAllocations, "Call the server method that runs the production allocations calculation", false);
            argMgr.AddParameter(Args.RunProdAllocationsTimeSpan, "Call the server method that runs the production allocations calculation with time span setting", false);
            argMgr.AddParameter(Args.RunIntelligentAlarmCheckForGLWells, "Call the server method that runs intelligent alarm check for GL wells", false);
            argMgr.AddParameter(Args.RunRunLatestPlungerLiftCycleCheck, "Run the latest PGL cycle check", true);
            argMgr.AddParameter(Args.RunIntelligentAlarmSendForRRLWells, "Call the server method that runs intelligent alarm check for RRL wells", false);
            argMgr.AddParameter(Args.RunIntelligentAlarmCheckForPGLWells, "Call the server method that runs intelligent alarm check for PGL wells", false);
            argMgr.AddParameter(Args.RunAutonomousControlForRRLWells, "Call the server method that runs idle time optimization for RRL wells", false);
            argMgr.AddParameter(Args.RunWellRunStatusDerivation, "Call the server method that derives the well run status", false);
            argMgr.AddParameter(Args.RunWAGCycleCheck, "Run the latest WAG cycle check", true);
            argMgr.AddParameter(Args.GetAWTData, "Get well test records from CygNet VHS::AWT for yesterday", false);
            argMgr.AddParameter(Args.RunJobEconomicAnalysis, "Call the server method that executes economic analysis", false);
            //argMgr.AddParameter(Args.GetAWTDataTimeSpan, "Get well test records from CygNet VHS::AWT with time span setting", false);
            argMgr.AddParameter(Args.UpdateDowntime, "Update current downtime records' lost oil, water, and gas fields", false);
            argMgr.AddParameter(Args.UpdateAllDowntime, "Update all downtime records' lost oil, water, and gas fields", false);
            argMgr.AddHiddenParameter(Args.BuildDowntimeCache, "Rebuild the well to downtime table.  Intended for use after an initial downtime load.", false);
            argMgr.AddHiddenParameter(Args.DataConnectionKey, "Used to filer RRL Analysis to only run against a specific data connection.  Intended for services set-up of scheduled jobs.", true);
            argMgr.AddHiddenParameter(Args.RunAllocationForWellName, "Used to Tun Against a Specific Well", true);
            argMgr.AddParameter(Args.UseHTTPS, "Use HTTPS for the connection to the server", false);
            argMgr.AddParameter(Args.RenameCardBlobEntries, "Rename card blob entries based on repository site, CVS Name and facility id", false);
            argMgr.AddParameter(Args.UpdateOperatingEnvelopeForPCP, "Updates Operating Envelope for the PCP Wells", false);
            argMgr.AddParameter(Args.RunAutomaticDownTimeCalculations, "Call the server method that automatically calculates the DownTime for All Wells.", false);
            argMgr.Parse(args);

            if (Environment.UserInteractive)
            {
                // Capture Ctrl-C.
                SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);
            }

            do
            {
                if (argMgr.GetUnknownArgs().Count > 0)
                {
                    var bob = new StringBuilder();
                    bob.AppendLine("Unexpected arguments: " + string.Join(", ", argMgr.GetUnknownArgs()));
                    bob.AppendLine(argMgr.ShowUsage());
                    logger.Log(bob.ToString(), LogMessageCategory.Exception);
                    break;
                }

                if (argMgr.GetSetArgCount() == 0)
                {
                    logger.Log(" " + Environment.NewLine + "No expected arguments set." + argMgr.ShowUsage());
                    break;
                }

                if (argMgr.IsHelpRequested())
                {
                    logger.Log(" " + argMgr.ShowUsage());
                    break;
                }

                try
                {
                    bool useHTTPS = ConfigurationManager.AppSettings["UseHTTPS"] == "true" || argMgr.ArgumentIsSet(Args.UseHTTPS);
                    if (useHTTPS)
                    {
                        logger.Log("HTTPS is enabled.");
                    }
                    ClientFactory = new ServiceFactory(Hostname, useHTTPS);
                    ClientFactory.DefaultTimeout = MiliSecondsInASecond * TimeoutInSeconds;
                    logger.Log("Authenticating...", LogMessageCategory.General, LogMessagePriority.Low);
                    var tokenServiceClient = ClientFactory.GetService<ITokenService>();
                    AuthenticatedUserDTO user = tokenServiceClient.CreateToken();
                    logger.LogFmt("Successfully authenticated user: {0}.", "", LogMessageCategory.General, LogMessagePriority.Low, user.Name);

                    if (user != null)
                    {
                        if (argMgr.ArgumentIsSet(Args.UpdateDynaCards))
                        {
                            DateTime timestamp;
                            int? noOfDays = null;
                            int? dataConnectionPrimaryKey = null;

                            string value = argMgr.GetParameterValue(Args.UpdateDynaCards);

                            if (value != null)
                            {
                                if (int.TryParse(value, out int result))
                                {
                                    noOfDays = result;
                                }
                            }

                            if (argMgr.ArgumentIsSet(Args.DataConnectionKey))
                            {
                                value = argMgr.GetParameterValue(Args.DataConnectionKey);
                                if (value != null)
                                {
                                    if (int.TryParse(value, out int result))
                                    {
                                        dataConnectionPrimaryKey = result;
                                    }
                                    else
                                    {
                                        logger.Log("Unable to find entered Data Connection Key, running Dyna Card Analysis for all wells.");
                                    }
                                }
                                else
                                    logger.Log("No valid Data Connection Key, running Dyna Card Analysis for all wells.");
                            }

                            timestamp = GetLastDynacardTimestamp(noOfDays);
                            UpdateDynacardsTask(timestamp, dataConnectionPrimaryKey);
                        }

                        if (argMgr.ArgumentIsSet(Args.GetIBMData))
                        {
                            bool genMock = argMgr.ArgumentIsSet(Args.GenIBMData);
                            GetIBMDataTask(genMock);
                        }

                        if (argMgr.ArgumentIsSet(Args.UpdateMTBFTable))
                        {
                            logger.Log("Attempting to update MTBF table.");
                            var wellMTBFServiceClient = ClientFactory.GetService<IWellMTBFService>();
                            wellMTBFServiceClient.GenerateMTBFData();
                        }

                        if (argMgr.ArgumentIsSet(Args.UpdateMostCommonEventTypesTable))
                        {
                            logger.Log("Attempting to update MostCommonEventType table.");
                            string msg; msg = "";
                            var JobServiceClient = ClientFactory.GetService<IJobAndEventService>();
                            JobServiceClient.RepopulateMostCommonEventType();
                            if (msg != "")
                            {
                                logger.Log(msg);
                            }
                            else
                            {
                                logger.Log("Completed successfully.");
                            }
                        }


                        if (argMgr.ArgumentIsSet(Args.InsertScenarioScheduleDailyRun))
                        {
                            logger.Log("Attempting to insert Scenario Schedule Daily Run records.");
                            var surfaceNetworkClient = ClientFactory.GetService<ISurfaceNetworkService>();
                            surfaceNetworkClient.InsertSNScenarioScheduleDailyRuns();
                        }


                        if (argMgr.ArgumentIsSet(Args.RunScenarioScheduler))
                        {
                            logger.Log("Attempting to run Scenario Scheduler.");
                            var surfaceNetworkClient = ClientFactory.GetService<ISurfaceNetworkService>();
                            surfaceNetworkClient.RunScheduledOptimizationJobs();
                            surfaceNetworkClient.RunScheduledOptimizationPFJobs();
                        }


                        if (argMgr.ArgumentIsSet(Args.RunAllocations))
                        {
                            RunAllocationTask();
                        }

                        if (argMgr.ArgumentIsSet(Args.GetAWTData))
                        {
                            GetAWTDataTask();
                        }

                        //if (argMgr.ArgumentIsSet(Args.GetAWTDataTimeSpan))
                        //{
                        //    GetAWTDataTimeSpanTask();
                        //}

                        if (argMgr.ArgumentIsSet(Args.RunAllocationsTimeSpan))
                        {
                            string wlname = null;
                            if (argMgr.ArgumentIsSet(Args.RunAllocationForWellName))
                            {
                                wlname = argMgr.GetParameterValue(Args.RunAllocationForWellName);
                            }
                                RunAllocationForMultipleDays(wlname);
                        }

                        if (argMgr.ArgumentIsSet(Args.RunProductionAllocations))
                        {
                            RunProductionAllocationTask();
                        }

                        if (argMgr.ArgumentIsSet(Args.RunProdAllocationsTimeSpan))
                        {
                            RunProdAllocationForMultipleDays();
                        }

                        if (argMgr.ArgumentIsSet(Args.RunIntelligentAlarmCheckForESPWells))
                        {
                            ESPIntelligentAlarmsTask();
                        }

                        if (argMgr.ArgumentIsSet(Args.RunIntelligentAlarmCheckForPCPWells))
                        {
                            PCPIntelligentAlarmsTask();
                        }

                        if (argMgr.ArgumentIsSet(Args.RunIntelligentAlarmCheckForPGLWells))
                        {
                            PGLIntelligentAlarmsTask();
                        }

                        if (argMgr.ArgumentIsSet(Args.RunIntelligentAlarmCheckForGLWells))
                        {
                            GLIntelligentAlarmsTask();
                        }

                        if (argMgr.ArgumentIsSet(Args.RunRunLatestPlungerLiftCycleCheck))
                        {
                            // Get the number of historical days to process. This is provided in the command line.
                            string value = argMgr.GetParameterValue(Args.RunRunLatestPlungerLiftCycleCheck);

                            // If no argument was provided, default to 7 days of history.
                            value = value ?? "7";

                            if (!int.TryParse(value, out int lookBackDays))
                                logger.LogFmt("Invalid number of historical lookback days: {0}", value, LogMessageCategory.Exception, LogMessagePriority.High);
                            else
                                PGLLatestCycleCheck(value);
                        }

                        if (argMgr.ArgumentIsSet(Args.RunIntelligentAlarmSendForRRLWells))
                        {
                            RRLSendIntelligentAlarmsTask();
                        }

                        if (argMgr.ArgumentIsSet(Args.RunAutonomousControlForRRLWells))
                        {
                            RRLIdleTimeOptimizationTask();
                        }

                        if (argMgr.ArgumentIsSet(Args.RunWellRunStatusDerivation))
                        {
                            WellRunStatusDerivationTask();
                        }

                        if (argMgr.ArgumentIsSet(Args.RunWAGCycleCheck))
                        {
                            WAGLatestCycleCheck();
                        }

                        if (argMgr.ArgumentIsSet(Args.RunJobEconomicAnalysis))
                        {
                            JobEconomicAnalysisTask();
                        }

                        if (argMgr.ArgumentIsSet(Args.UpdateDowntime))
                        {
                            var surveillanceService = ClientFactory.GetService<ISurveillanceService>();
                            surveillanceService.UpdateCurrentDowntimeRecords();
                        }

                        if (argMgr.ArgumentIsSet(Args.UpdateAllDowntime))
                        {
                            var surveillanceService = ClientFactory.GetService<ISurveillanceService>();
                            surveillanceService.ReCalculateLostProduction(null);
                        }

                        if (argMgr.ArgumentIsSet(Args.BuildDowntimeCache))
                        {
                            var surveillanceService = ClientFactory.GetService<ISurveillanceService>();
                            surveillanceService.BuildWellCacheDownTime(null);
                        }

                        if (argMgr.ArgumentIsSet(Args.RenameCardBlobEntries))
                        {
                            RenameDynacardBlobEntries();
                        }

                        if (argMgr.ArgumentIsSet(Args.UpdateOperatingEnvelopeForPCP))
                        {
                            UpdatePCPOperatingEnvelope();
                        }

                        if (argMgr.ArgumentIsSet(Args.RunAutomaticDownTimeCalculations))
                        {
                            CalculateDownTimeForAllWells();
                        }

                    }
                    else
                    {
                        logger.LogFmt("Failed to authenticate user. Operation aborting.", "",
                           LogMessageCategory.General, LogMessagePriority.Low, user.Name);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogFmt("Encountered an error while running Scheduler job: {0}", ex.ToString(), LogMessageCategory.Exception, LogMessagePriority.High, ex.Message);
                }
            } while (false);
            LogMessageRouter.Instance.Terminate();
        }

        #endregion

        #region ESPIntelligentAlarmsTask
        private static void ESPIntelligentAlarmsTask()
        {
            logger.Log("Attempting to run intelligent alarm check for ESP wells.");
            var intelligentAlarmServiceClient = ClientFactory.GetService<IIntelligentAlarmService>();
            var wellServiceClient = ClientFactory.GetService<IWellService>();

            WellDTO[] wells = wellServiceClient.GetAllWells()
                .Where(wl => wl.WellType == WellTypeId.ESP && wl.DataConnection != null && wl.FacilityId != null).ToArray();

            if (wells.Count() == 0)
            {
                logger.Log("No configured ESP wells.", LogMessageCategory.General, LogMessagePriority.Low);
            }
            else
            {
                foreach (var well in wells)
                {
                    if (well.DataConnection.ScadaSourceType == ScadaSourceType.CygNet)
                    {
                        try
                        {
                            logger.LogFmt("Running intelligent alarm check for ESP well: {0}", LogMessageCategory.General, LogMessagePriority.Low, well.Name);
                            intelligentAlarmServiceClient.IntelligentAlarmCheckForSingleWellNonRRL(well.Id.ToString());
                        }
                        catch (Exception ex)
                        {
                            logger.LogFmt("Intelligent alarm check for ESP Well: {0} failed. Exception: {1}",
                                LogMessageCategory.Exception, LogMessagePriority.High, well.Name, ex.Message);
                            continue;
                        }
                    }
                }
            }
        }
        #endregion

        #region PCPIntelligentAlarmsTask
        private static void PCPIntelligentAlarmsTask()
        {
            logger.Log("Attempting to run intelligent alarm check for PCP wells.");
            var intelligentAlarmServiceClient = ClientFactory.GetService<IIntelligentAlarmService>();
            var wellServiceClient = ClientFactory.GetService<IWellService>();

            WellDTO[] wells = wellServiceClient.GetAllWells()
                .Where(wl => wl.WellType == WellTypeId.PCP && wl.DataConnection != null && wl.FacilityId != null).ToArray();

            if (wells.Count() == 0)
            {
                logger.Log("No configured PCP wells.", LogMessageCategory.General, LogMessagePriority.Low);
            }
            else
            {
                foreach (var well in wells)
                {
                    if (well.DataConnection.ScadaSourceType == ScadaSourceType.CygNet)
                    {
                        try
                        {
                            logger.LogFmt("Running intelligent alarm check for PCP well: {0}", LogMessageCategory.General, LogMessagePriority.Low, well.Name);
                            intelligentAlarmServiceClient.IntelligentAlarmCheckForSingleWellNonRRL(well.Id.ToString());
                        }
                        catch (Exception ex)
                        {
                            logger.LogFmt("Intelligent alarm check for PCP Well: {0} failed. Exception: {1}",
                                LogMessageCategory.Exception, LogMessagePriority.High, well.Name, ex.Message);
                            continue;
                        }
                    }
                }
            }
        }
        #endregion

        #region PGL Cycle Processing
        /// <summary>
        /// Process the plunger lift production data collected for each plunger lift cycle in the target date range.
        /// </summary>
        /// <param name="lookBackDays">the number of days of history to process if no records exist for a well</param>
        private static void PGLLatestCycleCheck(string lookBackDays)
        {
            logger.Log("Attempting to run latest PGL cycle check.");
            IWellService wellServiceClient = ClientFactory.GetService<IWellService>();

            // Get all PGL wells from the database
            WellDTO[] wells = wellServiceClient.GetAllWells().Where(wl => wl.WellType == WellTypeId.PLift && wl.FacilityId != null).ToArray();

            if (wells.Count() == 0)
            {
                logger.Log("No configured PGL wells.", LogMessageCategory.General, LogMessagePriority.Low);
            }
            else
            {
                var surveillanceServiceClient = ClientFactory.GetService<ISurveillanceService>();
                foreach (var well in wells)
                {
                    if (well.DataConnection.ScadaSourceType == ScadaSourceType.CygNet)
                    {
                        surveillanceServiceClient.GetLatestPGLCycleData(well, lookBackDays);

                        // For testing just to the first well.
                        // break;
                    }
                }
            }
        }
        #endregion

        #region WAG Cycle Check Processing 
        private static void WAGLatestCycleCheck()
        {
            logger.Log("Attempting to run latest WAG cycle check.");
            IWellService wellServiceClient = ClientFactory.GetService<IWellService>();

            // Get all WAG wells from the database
            WellDTO[] wells = wellServiceClient.GetAllWells().Where(wl => wl.WellType == WellTypeId.WGInj && wl.FacilityId != null).ToArray();

            if (wells.Count() == 0)
            {
                logger.Log("No configured WAG wells.", LogMessageCategory.General, LogMessagePriority.Low);
            }
            else
            {
                var surveillanceServiceClient = ClientFactory.GetService<ISurveillanceService>();
                foreach (var well in wells)
                {
                    if (well.DataConnection.ScadaSourceType == ScadaSourceType.CygNet)
                    {
                        try
                        {
                            logger.LogFmt("Running Scheduler for WAG well: {0}", LogMessageCategory.General, LogMessagePriority.Low, well.Name);
                            surveillanceServiceClient.UpdateLatestWAGCycleData(well);
                        }
                        catch (Exception ex)
                        {
                            logger.LogFmt("Scheduler for WAG well: {0} failed. Exception: {1}",
                                LogMessageCategory.Exception, LogMessagePriority.High, well.Name, ex.Message);
                            continue;
                        }

                    }
                }
            }
        }
        #endregion

        #region PGLIntelligentAlarmsTask
        private static void PGLIntelligentAlarmsTask()
        {
            logger.Log("Attempting to run intelligent alarm check for PGL wells.");

            IIntelligentAlarmService intelligentAlarmServiceClient = ClientFactory.GetService<IIntelligentAlarmService>();
            IWellService wellServiceClient = ClientFactory.GetService<IWellService>();

            WellDTO[] wells = wellServiceClient.GetAllWells().Where(wl => wl.WellType == WellTypeId.PLift && wl.FacilityId != null).ToArray();

            if (wells.Count() == 0)
            {
                logger.Log("No configured PGL wells.", LogMessageCategory.General, LogMessagePriority.Low);
            }
            else
            {
                foreach (var well in wells)
                {
                    try
                    {
                        logger.LogFmt("Running intelligent alarm check for PGL well: {0}", LogMessageCategory.General, LogMessagePriority.Low, well.Name);
                        intelligentAlarmServiceClient.IntelligentAlarmCheckForSingleWellNonRRL(well.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        logger.LogFmt("Intelligent alarm check for PGL Well: {0} failed. Exception: {1}",
                            LogMessageCategory.Exception, LogMessagePriority.High, well.Name, ex.Message);
                        continue;
                    }
                }
            }
        }
        #endregion


        #region GLIntelligentAlarmsTask
        private static void GLIntelligentAlarmsTask()
        {
            logger.Log("Attempting to run intelligent alarm check for GL wells.");

            IIntelligentAlarmService intelligentAlarmServiceClient = ClientFactory.GetService<IIntelligentAlarmService>();
            IWellService wellServiceClient = ClientFactory.GetService<IWellService>();

            WellDTO[] wells = wellServiceClient.GetAllWells().Where(wl => wl.WellType == WellTypeId.GLift && wl.FacilityId != null).ToArray();

            if (wells.Count() == 0)
            {
                logger.Log("No configured GL wells.", LogMessageCategory.General, LogMessagePriority.Low);
            }
            else
            {
                foreach (var well in wells)
                {
                    try
                    {
                        logger.LogFmt("Running intelligent alarm check for GL well: {0}", LogMessageCategory.General, LogMessagePriority.Low, well.Name);
                        intelligentAlarmServiceClient.IntelligentAlarmCheckForSingleWellGL(well.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        logger.LogFmt("Intelligent alarm check for GL Well: {0} failed. Exception: {1}",
                            LogMessageCategory.Exception, LogMessagePriority.High, well.Name, ex.Message);
                        continue;
                    }
                }
            }
        }
        #endregion

        #region RRLSendIntelligentAlarmsTask
        /// <summary>
        /// Step 1: Read all wells from db and filter out only RRL wells with valid data connection.
        /// Step 2: Iterate through each RRL well.
        /// Step 3: Get current values of Intelligent Alarm data group from device.
        /// Step 4: Invoke ScanAndAnalyzeDynacardWithUnits API for only current card. This will collect  
        ///         latest current card, perform analysis and populate the analysis report into the db.
        /// Step 5: Invoke GetDownholeAnalysisReport API to fetch the analysis report. The report will
        ///         give us current values of intelligent points.
        /// Step 6: Update current values of intelligent alarm points into set point dto.
        /// Setp 7: Get active intelligent alarms for a well id from db.
        /// Step 8: Update Alarm File to update/ reset the occurance of alarms.
        /// Step 9: Update set point dto with alarm values and host alarms
        /// Step 10: Send updated set point dto to remote device.
        /// Step 11: Reset Alarm file to reset the alarm occurance that have reached limit.
        /// </summary>

        // These are currently supported type intelligent alarms in the CygNet
        // data group for WellPilotRPOC device. Later, it will be grown as per
        // future requirement.
        private static List<string> SupportedAlarmTypes = new List<string>()
        {
            SettingServiceStringConstants.HIGH_GEARBOX_TORQUE,
            SettingServiceStringConstants.GEARBOX_UNBALANCE,
            SettingServiceStringConstants.HIGH_ROD_STRESS
        };

        private static void RRLSendIntelligentAlarmsTask()
        {
            logger.Log("Starting to send RRL intelligent alarms in RRL device.");

            IDynacardService dynacardServiceClient = ClientFactory.GetService<IDynacardService>();
            IWellService wellServiceClient = ClientFactory.GetService<IWellService>();
            IIntelligentAlarmService intelligentAlarmServiceClient = ClientFactory.GetService<IIntelligentAlarmService>();
            ISurveillanceService surveillanceServiceClient = ClientFactory.GetService<ISurveillanceService>();
            ISettingService settingService = ClientFactory.GetService<ISettingService>();

            WellDTO[] wells = wellServiceClient.GetAllWells().Where(wl => wl.WellType == WellTypeId.RRL && wl.FacilityId != null).ToArray();

            // In the edge device, there will be only one RRL well. However, keep it generic.
            var reqWells = wells.Where(w => w.DataConnection != null);
            //var well = wells.Where(w => w.FacilityId == "RPOC_0001").FirstOrDefault();

            // We are going to scan only the current card.
            string cardType = ((int)CardType.Current).ToString();


            if (wells.Count() == 0)
            {
                logger.Log("No RRL wells found in db.", LogMessageCategory.General, LogMessagePriority.Low);
            }
            else
            {
                foreach (var well in reqWells)
                {
                    try
                    {
                        string status = "success";
                        string wellId = well.Id.ToString();

                        // Get the analysis method from the WellSettings database.
                        WellSettingDTO[] analysisSettings = settingService.GetWellSettingsByWellIdAndCategory(wellId, ((int)SettingCategory.Analysis).ToString());
                        IEnumerable<WellSettingDTO> optionDto = analysisSettings.SkipWhile(dto => dto.Setting.Name != SettingServiceStringConstants.ANALYSIS_METHOD).Take(1);
                        WellSettingDTO analysisMethodDTO = optionDto.FirstOrDefault();
                        string dhCardSource = analysisMethodDTO.StringValue;
                        logger.Log($"Get Intelligent Alarm analysis method for '{well.FacilityId}' is '{dhCardSource}'.", LogMessageCategory.General, LogMessagePriority.Low);
                        if (dhCardSource.ToUpper() == "GIBBS")
                            dhCardSource = ((int)DownholeCardSource.CalculatedGibbs).ToString();
                        else
                            dhCardSource = ((int)DownholeCardSource.CalculatedEverittJennings).ToString();

                        // Get current values for RRL Intelligent dg.
                        logger.Log($"Get Intelligent Alarm data group data for '{well.FacilityId}' facility.", LogMessageCategory.General, LogMessagePriority.Low);
                        SetPointsDTO rrlIADTO = surveillanceServiceClient.GetSetPointsParamsForSingleWell(wellId, "RRLIAlms");

                        if (rrlIADTO == null || rrlIADTO.SetPointsList.Count == 0)
                        {
                            logger.Log($"Invalid Intelligent Alarm data group found for '{well.FacilityId}' facility.", LogMessageCategory.General, LogMessagePriority.High);
                            continue;
                        }
                        else
                        {
                            logger.Log($"Get Intelligent Alarm data group data for '{well.FacilityId}' facility succeeded.", LogMessageCategory.General, LogMessagePriority.Low);
                        }

                        // Scan and anaylyze the current card. This API will also set any intelligent alarms in db.
                        logger.Log($"Scan and analyze current card for well '{wellId}'.", LogMessageCategory.General, LogMessagePriority.Low);
                        DynacardEntryAndUnitsDTO[] dynaCardEntry = dynacardServiceClient.ScanAndAnalyzeDynacardWithUnits(wellId, cardType, "runAnalysis");
                        if (dynaCardEntry == null)
                        {
                            logger.Log($"Invalid Current card data for '{well.FacilityId}' facility.", LogMessageCategory.General, LogMessagePriority.High);
                            continue;
                        }
                        else
                        {
                            logger.Log($"Scan and analyze current card for well id'{wellId}' succeeded.", LogMessageCategory.General, LogMessagePriority.Low);
                        }

                        // Get analysis report to help with the current values of intelligent points.
                        var cardTicks = dynaCardEntry[0].Value.TimestampInTicks;

                        logger.Log($"Get Downhole Analysis report for well '{wellId}'.", LogMessageCategory.General, LogMessagePriority.Low);
                        var analysisReport = dynacardServiceClient.GetDownholeAnalysisReport(wellId, cardType, cardTicks, dhCardSource);
                        if (analysisReport == null || analysisReport.PumpingUnitDataDTO == null || analysisReport.RodTaperDTO == null)
                        {
                            logger.Log($"Invalid downhole analysis report for well '{wellId}'.", LogMessageCategory.General, LogMessagePriority.High);
                        }
                        else
                        {
                            logger.Log($"Get Downhole Analysis report for well '{wellId}' succeeded.", LogMessageCategory.General, LogMessagePriority.Low);
                        }

                        // Set current values of intelligent alarm points.
                        UpdateCurrentValuesInSetPointDTO(rrlIADTO, analysisReport, settingService);

                        // Get active intelligent alarm list.
                        logger.Log($"Get active intelligent alarms for well '{wellId}'.", LogMessageCategory.General, LogMessagePriority.Low);
                        CurrentAlarmDTO[] activeAlarmList = intelligentAlarmServiceClient.GetActiveIntelligentAlarmsByWellId(wellId, WellTypeId.RRL.ToString());
                        if (activeAlarmList == null)
                        {
                            // No need to go further, just update the current intelligent point values into device.
                            surveillanceServiceClient.SendSetPointsParamsForSingleWell(rrlIADTO, "false");
                            UpdateOrResetAlarmFile(wellId, null, out status);
                            logger.Log($"No active intelligent alarms were fetched for well '{wellId}'.", LogMessageCategory.General, LogMessagePriority.Low);
                            continue;
                        }

                        // Update Alarm file based on fetched active alarms.
                        if (!UpdateOrResetAlarmFile(wellId, activeAlarmList, out status))
                        {
                            logger.Log(status, LogMessageCategory.General, LogMessagePriority.High);
                            return;
                        }

                        // We need to send the updated setpoint dto to device as many times as we have
                        // number of the active alarms that are supported by CygNet, so that we can set
                        // the host alarm sequentially.
                        CurrentAlarmDTO alarm = null;
                        bool spDgUpdated = false;
                        foreach (var alarmType in SupportedAlarmTypes)
                        {
                            alarm = activeAlarmList.Where(a => a.AlarmType.AlarmType == alarmType).FirstOrDefault();
                            if (alarm != null && UpdateAlarmValuesInSetPointDTO(alarm, rrlIADTO))
                            {
                                spDgUpdated = true;
                                logger.Log($"Send intelligent alarm value for '{alarmType}' to '{well.FacilityId} facility'.", LogMessageCategory.General, LogMessagePriority.Low);
                                string result = surveillanceServiceClient.SendSetPointsParamsForSingleWell(rrlIADTO, "false");
                                if (result.ToLower().Equals("success"))
                                {
                                    logger.Log($"Send intelligent alarm value for '{alarmType}' to '{well.FacilityId} facility succeeded'.", LogMessageCategory.General, LogMessagePriority.Low);
                                }
                                else
                                {
                                    logger.Log($"Send intelligent alarm value for '{alarmType}' to '{well.FacilityId} facility FAILED'.", LogMessageCategory.General, LogMessagePriority.High);
                                }
                            }
                        }

                        // Check if we have updated set point dg for at least once in order to make
                        // sure that intelligent point current values are sent to device.
                        if (!spDgUpdated)
                        {
                            logger.Log($"Send intelligent point current values for '{well.FacilityId} facility'.", LogMessageCategory.General, LogMessagePriority.Low);
                            var result = surveillanceServiceClient.SendSetPointsParamsForSingleWell(rrlIADTO, "false");
                            if (result.ToLower().Equals("success"))
                            {
                                logger.Log($"Send intelligent point current values for '{well.FacilityId} facility succeeded'.", LogMessageCategory.General, LogMessagePriority.Low);
                            }
                            else
                            {
                                logger.Log($"Send intelligent point current values for '{well.FacilityId} facility FAILED'.", LogMessageCategory.General, LogMessagePriority.High);
                            }
                        }

                        // Reset alarm occurance in the Alarm file.
                        UpdateOrResetAlarmFile(wellId, null, out status);
                    }
                    catch (Exception ex)
                    {
                        logger.LogFmt("Intelligent alarm send check for RRL Well: {0} failed. Exception: {1}",
                            LogMessageCategory.Exception, LogMessagePriority.High, well.Name, ex.Message);
                        continue;
                    }
                }
            }
        }

        private static bool UpdateAlarmValuesInSetPointDTO(CurrentAlarmDTO alarm, SetPointsDTO rrlIADTO)
        {
            var alarmType = alarm.AlarmType.AlarmType;
            long occurance = GetAlarmOccurance(rrlIADTO.WellId, alarmType);
            if (occurance < 3)
            {
                // We will send the intelligent alarm value to device only when it has occured 3 times in a row.
                return false;
            }
            else
            {
                var setPoint = rrlIADTO.SetPointsList.Where(sp => sp.Description == alarmType).FirstOrDefault();
                if (setPoint == null)
                    return false;

                setPoint.Value = alarm.NumericValue?.ToString(CultureInfo.InvariantCulture) ?? alarm.StringValue;

                // Get host alarm value
                int haVal = GetHostAlarmValue(alarmType);
                if (haVal > 0)
                {
                    var hostAlarmSP = rrlIADTO.SetPointsList.Where(sp => sp.Description == "Host Alarm").FirstOrDefault();
                    if (hostAlarmSP != null)
                    {
                        hostAlarmSP.Value = haVal.ToString();
                    }
                }
                return true;
            }
        }

        private static int GetHostAlarmValue(string alarmType)
        {
            int hostAlarmVal = -1;
            switch (alarmType)
            {
                case SettingServiceStringConstants.HIGH_GEARBOX_TORQUE:
                    hostAlarmVal = 0;
                    break;

                case SettingServiceStringConstants.GEARBOX_UNBALANCE:
                    hostAlarmVal = 4;
                    break;

                case SettingServiceStringConstants.HIGH_ROD_STRESS:
                    hostAlarmVal = 8;
                    break;
            }

            return hostAlarmVal;
        }

        private static long GetAlarmOccurance(long wellId, string alarmType)
        {
            long alarmOccurance = -1;
            string path = ConfigurationManager.AppSettings.Get("AlarmFile");
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);

                if (lines.Length == 0)
                    return alarmOccurance;

                string[] Fields = lines.First().Split(',');

                string wellLine = (from rec in lines
                                   where rec.Split(',')[0] == wellId.ToString()
                                   select rec).FirstOrDefault();

                if (string.IsNullOrEmpty(wellLine))
                    return alarmOccurance;

                string[] FieldVals = wellLine.Split(',');
                int idx = Array.IndexOf(Fields, Fields.Where(x => x == alarmType).FirstOrDefault());
                alarmOccurance = long.Parse(FieldVals[idx]);
                return alarmOccurance;
            }
            else
            {
                Console.WriteLine($"File path '{path}' not found for Alarm File.");
                return 0;
            }
        }

        private static bool UpdateOrResetAlarmFile(string wellId, CurrentAlarmDTO[] activeAlarmList, out string status)
        {
            status = "success";
            bool resetFile = activeAlarmList == null;
            string path = ConfigurationManager.AppSettings.Get("AlarmFile");
            List<string> lines = new List<string>();
            if (File.Exists(path))
            {
                List<string> records = File.ReadAllLines(path).ToList();
                string headerLine = "WellId,High Rod Stress,High Gearbox Torque,Gearbox Unbalance";

                if (records.Count > 0)
                    headerLine = records.First();

                lines.Add(headerLine);

                string templateLine = (from rec in records
                                       where rec.Split(',')[0] == "-1"
                                       select rec).FirstOrDefault();

                if (templateLine.IsNullOrEmpty())
                    templateLine = "-1,0,0,0";

                lines.Add(templateLine);

                string wellLine = (from rec in records
                                   where rec.Split(',')[0] == wellId
                                   select rec).FirstOrDefault();

                List<string> wellLines = records.Skip(2).ToList();
                if (wellLine.IsNullOrEmpty())
                {
                    string[] tempVals = templateLine.Split(',');
                    tempVals[0] = wellId;
                    string updatedLine1 = string.Join(",", tempVals);
                    wellLines.Add(updatedLine1);
                }

                foreach (var line in wellLines)
                {
                    string[] CurrentVals = line.Split(',');
                    if (CurrentVals[0] != wellId)
                    {
                        lines.Add(line);
                        continue;
                    }

                    string[] Fields = headerLine.Split(',');

                    long[] FieldVals = new long[Fields.Count()];
                    for (int ii = 0; ii < CurrentVals.Length; ii++)
                    {
                        FieldVals[ii] = long.Parse(CurrentVals[ii]);
                    }
                    CurrentAlarmDTO activeAlarm = null;
                    string updatedLine = string.Empty;
                    for (int i = 0; i < Fields.Count(); i++)
                    {
                        string field = Fields[i];
                        switch (field)
                        {
                            case "WellId":
                                FieldVals[i] = long.Parse(wellId);
                                break;

                            case SettingServiceStringConstants.HIGH_ROD_STRESS:
                                if (!resetFile)
                                {
                                    activeAlarm = activeAlarmList.Where(a => a.AlarmType.AlarmType == SettingServiceStringConstants.HIGH_ROD_STRESS).FirstOrDefault();
                                    FieldVals[i] = GetUpdatedAlarmOccurance(CurrentVals, FieldVals, activeAlarm, i);
                                }
                                else if (resetFile && long.Parse(CurrentVals[i]) == 3)
                                {
                                    // Alarm occurance reached limit. Reset its occurance.
                                    FieldVals[i] = 0;
                                }
                                break;

                            case SettingServiceStringConstants.HIGH_GEARBOX_TORQUE:
                                if (!resetFile)
                                {
                                    activeAlarm = activeAlarmList.Where(a => a.AlarmType.AlarmType == SettingServiceStringConstants.HIGH_GEARBOX_TORQUE).FirstOrDefault();
                                    FieldVals[i] = GetUpdatedAlarmOccurance(CurrentVals, FieldVals, activeAlarm, i);
                                }
                                else if (resetFile && long.Parse(CurrentVals[i]) == 3)
                                {
                                    // Alarm occurance reached limit. Reset its occurance.
                                    FieldVals[i] = 0;
                                }
                                break;

                            case SettingServiceStringConstants.GEARBOX_UNBALANCE:
                                if (!resetFile)
                                {
                                    activeAlarm = activeAlarmList.Where(a => a.AlarmType.AlarmType == SettingServiceStringConstants.GEARBOX_UNBALANCE).FirstOrDefault();
                                    FieldVals[i] = GetUpdatedAlarmOccurance(CurrentVals, FieldVals, activeAlarm, i);
                                }
                                else if (resetFile && long.Parse(CurrentVals[i]) == 3)
                                {
                                    // Alarm occurance reached limit. Reset its occurance.
                                    FieldVals[i] = 0;
                                }
                                break;
                        }
                    }

                    updatedLine = string.Join(",", FieldVals);
                    lines.Add(updatedLine);
                }

                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    foreach (string line in lines)
                    {
                        writer.WriteLine(line);
                    }
                }

                return true;
            }
            else
            {
                status = $"Alarm File '{path}' not found";
                return false;
            }
        }

        private static long GetUpdatedAlarmOccurance(string[] CurrentVals, long[] FieldVals, CurrentAlarmDTO activeAlarm, int index)
        {
            long value = 0;
            if (activeAlarm != null && long.Parse(CurrentVals[index]) < 3)
            {
                // Alarm is ON. Increment its occurance.
                value = long.Parse(CurrentVals[index]) + 1;
            }
            else
            {
                // Alarm is OFF. Reset its occurance.
                value = 0;
            }

            return value;
        }

        private static void UpdateCurrentValuesInSetPointDTO(SetPointsDTO spDTO, AnalysisReportDTO analysisReport, ISettingService settingService)
        {
            if (analysisReport == null || analysisReport.PumpingUnitDataDTO == null || analysisReport.RodTaperDTO == null)
            {
                // Use the existing value in the dg
                return;
            }


            const string prefix = "Cur. ";

            var setPointList = spDTO.SetPointsList;
            foreach (var sp in setPointList)
            {
                switch (sp.Description)
                {
                    case prefix + SettingServiceStringConstants.HIGH_GEARBOX_TORQUE:
                        var gearboxTorque = analysisReport.PumpingUnitDataDTO.Value.ExistingTorqueLoading.ToString();
                        sp.Value = gearboxTorque.IsNullOrEmpty() ? sp.Value : gearboxTorque;
                        break;

                    case prefix + SettingServiceStringConstants.GEARBOX_UNBALANCE:
                        var gearboxUnbal = (analysisReport.PumpingUnitDataDTO.Value.MaximumTorqueUpLoading - analysisReport.PumpingUnitDataDTO.Value.MaximumTorqueDownLoading).ToString();
                        sp.Value = gearboxUnbal.IsNullOrEmpty() ? sp.Value : gearboxUnbal;
                        break;

                    case prefix + SettingServiceStringConstants.HIGH_ROD_STRESS:
                        var highRodStress = analysisReport.RodTaperDTO.Values.OrderByDescending(en => en.RodLoading).First().RodLoading.ToString();
                        sp.Value = highRodStress.IsNullOrEmpty() ? sp.Value : highRodStress;
                        break;

                    case prefix + SettingServiceStringConstants.CAL_PIP:
                        if (settingService.GetSystemSettingByName(SettingServiceStringConstants.SEND_CPIP_TO_CONTROLLER)?.NumericValue.Value > 0)
                        {
                            var calPIP = analysisReport.PumpingUnitDataDTO.Value.CalculatedPumpIntakePressure.Value.ToString();
                            sp.Value = calPIP.IsNullOrEmpty() ? sp.Value : calPIP;
                        }
                        break;
                }
            }
        }

        #endregion

        #region RenameDynacardBlobEntries
        private static void RenameDynacardBlobEntries()
        {
            var wellServiceClient = ClientFactory.GetService<IWellService>();
            logger.Log("Retrieve all the RRL wells which are mapped to CygNet SCADA");
            WellDTO[] wells = wellServiceClient.GetAllWells()?.Where(o => o.WellType == WellTypeId.RRL && o.DataConnection != null && o.DataConnection.ScadaSourceType == ScadaSourceType.CygNet).ToArray();
            logger.LogFmt("Total count of RRL Wells mapped with CygNet SCADA are: {0}.", "",
                    LogMessageCategory.General, LogMessagePriority.Low, wells.Count());

            var dynacardServiceClient = ClientFactory.GetService<IDynacardService>();
            foreach (var well in wells)
            {
                foreach (CardType cardType in Enum.GetValues(typeof(CardType)))
                {
                    if (cardType == CardType.UnknownType)
                    {
                        continue;
                    }

                    try
                    {
                        logger.Log($"Started renaming {well.Name} card blob entries for {cardType.ToString()} card type.");
                        dynacardServiceClient.RenameDynacardBlobEntries(well.Id.ToString(), cardType.ToString());
                        logger.Log($"Completed renaming {well.Name} card blob entries for {cardType.ToString()} card type.");
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Error in renaming {well.Name} card blob entries for {cardType.ToString()} card type - {ex.Message}");
                        continue;
                    }
                }
            }
        }
        #endregion // RenameDynacardBlobEntries

        private static void UpdateDynacardsTask(DateTime dtTimestamp, int? dataConnectionPrimaryKey)
        {
            var wellServiceClient = ClientFactory.GetService<IWellService>();
            var settingService = ClientFactory.GetService<ISettingService>();
            var wellStatusClient = ClientFactory.GetService<ISurveillanceService>();

            logger.Log("Retrieve all the RRL wells which are mapped to SCADA");
            WellDTO[] wells = wellServiceClient.GetAllWells()?.Where(o => o.WellType == WellTypeId.RRL && o.DataConnection != null).ToArray();
            logger.LogFmt("Total count of RRL Wells: {0}", "",
                    LogMessageCategory.General, LogMessagePriority.Low, wells.Count());

            logger.Log($"Timestamp being used for searching CygNet in this run:{dtTimestamp}", LogMessageCategory.Debug);

            // Look for the LINK_SCAN_TO_ANALYSIS Settng from the System settings to see if the flag has been set or not.
            //     If it is set, we will run analysis on all the cards being pulled from CygNet if not we will only add the cards to Dyncard Library service 
            //     and not worry about running analysis on them.
            logger.Log("Reading the System Setting: LINK_SCAN_TO_ANALYSIS", LogMessageCategory.Debug);
            bool runAnalysisForAllCards = (settingService.GetSystemSettingByName(SettingServiceStringConstants.LINK_SCAN_TO_ANALYSIS)?.NumericValue ?? 0.0) != 0.0;
            if (runAnalysisForAllCards)
            {
                logger.Log("LINK_SCAN_TO_ANALYSIS setting is set, analysis and intelligent alarms check operations will be performed.");
            }
            else
            {
                logger.Log("LINK_SCAN_TO_ANALYSIS setting is not set, analysis and intelligent alarms check operations will not be performed.");
            }

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 10;

            if(int.TryParse(ConfigurationManager.AppSettings["MaxThreads"], out int maxThreadsInt))
                options.MaxDegreeOfParallelism = maxThreadsInt;

            // Start fetching the cards and check if they exists in the Dynacard library service, if they are not present then start adding them to the Dynacard service, run analysis if LINK_SCAN_TO_ANALYSIS is set also update the intelligent alarms if present.
            if (wells.Count() > 0)
            {
                Parallel.ForEach(wells, options, well =>
                {
                    if (well.DataConnection.ScadaSourceType != ScadaSourceType.CygNet)
                    {
                        return;
                    }

                    if (dataConnectionPrimaryKey != null && well.DataConnection.Id != dataConnectionPrimaryKey)
                    {
                        return;
                    }

                WellDowntimeDTO downtime = wellStatusClient.GetDownTimeForWell(well.Id.ToString());

                    if (downtime != null && downtime.Id > 0 && downtime.DownTimeStatus == "OFF")
                    {
                        logger.Log($"{well.Name} - Well is out of service, skipping analysis. ", "",
                            LogMessageCategory.General, LogMessagePriority.Low);
                        return;
                    }

                    DateTime? lastsuccessfulTransaction = DTOExtensions.FromISO8601Nullable(wellStatusClient.GetLastSuccessfulTxForWell(well.Id.ToString()));
                    if (string.IsNullOrEmpty(lastsuccessfulTransaction?.ToString()))
                    {
                        logger.Log($"{well.Name} - Unable to read last successfull transaction information. ", "",
                             LogMessageCategory.General, LogMessagePriority.Low);
                        return;
                    }

                    var dynacardServiceClient = ClientFactory.GetService<IDynacardService>();
                    Dictionary<string, List<Tuple<long, long>>> ddsCards = dynacardServiceClient.GetAvailableDynacardsTimeStampsFromDDSByCardType(well.Id.ToString(), dtTimestamp.Ticks.ToString());
                    logger.Log($"{well.Name}: Comparing with cards in Dynacard Library.", LogMessageCategory.General, LogMessagePriority.Low);

                    Tuple<long, long> intelligentAlarmCard = null;
                    CardType intelligentAlarmCardType = CardType.UnknownType;
                    if (lastsuccessfulTransaction?.AddHours(24) > DateTime.Now)
                    {
                        string runTimeStr = wellStatusClient.GetRunTimeYesterdayForSingleWell(well.Id.ToString());
                        if (double.TryParse(runTimeStr, out double runTime))
                        {
                            intelligentAlarmCardType = runTime == 24.00 ? CardType.Current : CardType.Full;
                            string intelligentAlarmCardTypeStr = intelligentAlarmCardType.ToString();
                            if (ddsCards?.ContainsKey(intelligentAlarmCardTypeStr) == true)
                            {
                                intelligentAlarmCard = ddsCards[intelligentAlarmCardTypeStr].OrderByDescending(t => t.Item1).FirstOrDefault();
                            }
                            logger.Log($"{well.Name} run time yesterday: {runTime} using card type {intelligentAlarmCardType} {(intelligentAlarmCard != null ? $"found new matching card with timestamp {intelligentAlarmCard.Item1}" : "found new matching card")}.",
                                       LogMessageCategory.General, LogMessagePriority.Low);
                        }
                    }
                    if (intelligentAlarmCard == null)
                    {
                        logger.Log($"{well.Name}: no card found to use to update intelligent alarms.");
                    }

                    foreach (var card in ddsCards)
                    {
                        if (!Enum.TryParse(card.Key, out CardType cardType))
                        {
                            continue;
                        }

                        if (card.Value?.Count == 0)
                        {
                            logger.Log($"{well.Name}: No new {cardType} cards in CygNet since {dtTimestamp}", LogMessageCategory.Debug);
                        }

                        foreach (Tuple<long, long> timestamp in card.Value)
                        {
                            try
                            {
                                DynaCardEntryDTO dynacard = dynacardServiceClient.GetDynacard(well.Id.ToString(), timestamp.Item1.ToString(), ((int)cardType).ToString());
                                if (dynacard.ErrorMessage.ToLower() == "success")
                                {
                                    logger.LogFmt("{0}: Exists - [Timestamp: {1}, CardType: {2}]", LogMessageCategory.General, LogMessagePriority.Low, well.Name, timestamp.Item1.ToString(), cardType.ToString());
                                    continue;
                                }
                                logger.LogFmt("{0}: Adding card to Dyncard library [Timestamp: {1}, CardType: {2}]", LogMessageCategory.General, LogMessagePriority.Low, well.Name, timestamp.ToString(), cardType.ToString());
                                dynacard = dynacardServiceClient.AddCardFromDDSToDynacardLibrary(well.Id.ToString(), ((int)cardType).ToString(), timestamp.Item2.ToString());
                                if (dynacard.ErrorMessage.ToLower() == "success")
                                {
                                    logger.LogFmt("{0}: Card successfully added. [Timestamp: {1}, CardType: {2}]", LogMessageCategory.General, LogMessagePriority.Low, well.Name, timestamp.ToString(), ((int)cardType).ToString());

                                    bool isIntelligentAlarmCard = intelligentAlarmCard == timestamp;
                                    if (runAnalysisForAllCards || isIntelligentAlarmCard)
                                    {
                                        if (isIntelligentAlarmCard)
                                        {
                                            logger.Log(well.Name + ": Intelligent alarms will be calculated using this card.");
                                        }
                                        dynacard = dynacardServiceClient.AnalyzeSelectedSurfaceCard(well.Id.ToString(), dynacard.TimestampInTicks, ((int)cardType).ToString(), isIntelligentAlarmCard ? "true" : "false");
                                        if (dynacard.ErrorMessage.ToLower() == "success")
                                        {
                                            logger.LogFmt("{0}: {1}. Successfully ran analysis.", "",
                                             LogMessageCategory.General, LogMessagePriority.Low, well.Name, cardType);
                                        }
                                        else
                                        {
                                            logger.LogFmt("{0}: {1} ", "",
                                             LogMessageCategory.General, LogMessagePriority.Low, well.Name, dynacard.ErrorMessage);
                                        }
                                    }
                                }
                                else
                                {
                                    logger.LogFmt("{wellname}: Exception while saving card to Dynacard Library. [Timestamp: {timestamp}, CardType: {cardType.ToString()}]", LogMessageCategory.General, LogMessagePriority.Low, well.Name, timestamp.ToString(), ((int)cardType).ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogFmt("{0}: Error while adding card to card library.",
                                        ex.ToString(), LogMessageCategory.Exception, LogMessagePriority.High, well.Name, ex.Message);
                            }
                        }
                    }
                });
            }
        }

        private static DateTime GetLastDynacardTimestamp(int? noOfDays)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "RunAnalayis_Task_Log.txt";
            int nonNullNumberOfDays = (noOfDays == null) ? 7 : (int)noOfDays;

            logger.Log($"Looking for RunAnalayis_Task_Log.txt in {AppDomain.CurrentDomain.BaseDirectory}", LogMessageCategory.Debug);
            if (!File.Exists(filePath))
            {
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                }
                return DateTime.Now.AddDays(-nonNullNumberOfDays);
            }

            List<string> allTimeStamps = File.ReadAllLines(filePath).ToList();
            DateTime lastRunTime = DateTime.Now.AddDays(-nonNullNumberOfDays);
            if (allTimeStamps.Count > 0)
            {
                if (DateTime.TryParse(allTimeStamps[0], out lastRunTime) == false)
                {
                    lastRunTime = DateTime.Now.AddDays(-nonNullNumberOfDays);
                }
            }
            allTimeStamps.Insert(0, DateTime.Now.ToString());

            if (allTimeStamps.Count > MaxTimestampEntriesForDynacard)
            {
                allTimeStamps.RemoveAt(MaxTimestampEntriesForDynacard);
            }
            File.Delete(filePath);
            File.WriteAllLines(filePath, allTimeStamps);

            return (noOfDays == null) ? lastRunTime : DateTime.Now.AddDays(-nonNullNumberOfDays);
        }

        #region GetIBMDataTask
        private static void GetIBMDataTask(bool genMock)
        {
            logger.Log("Retrieve failure prediction data...", LogMessageCategory.General, LogMessagePriority.Low);
            var wellMTBFServiceClient = ClientFactory.GetService<IWellMTBFService>();
            var wellServiceClient = ClientFactory.GetService<IWellService>();
            long[] wellIds = wellServiceClient.GetAllWells().Select(wl => wl.Id).OrderBy(id => id).ToArray();
            int groups = wellIds.Length / s_groupSize + (wellIds.Length % s_groupSize == 0 ? 0 : 1);
            for (int i = 0; i < groups; i++)
            {
                int iStart = i * s_groupSize;
                int iLength = (wellIds.Length - iStart >= s_groupSize) ? s_groupSize : (wellIds.Length - iStart);
                long[] subWellIds = new long[iLength];
                Array.Copy(wellIds, iStart, subWellIds, 0, iLength);
                long[] failedIds = wellMTBFServiceClient.GetFailurePredictionDataOnDemand(subWellIds, genMock ? "1" : "0");
                logger.Log(string.Format(" Round {0} : {1}/{2} wells Succeeded. ", (i + 1), (iLength - failedIds.Length), iLength));
                if (failedIds.Length > 0)
                {
                    string strIds = string.Join(",", failedIds.Select(id => id.ToString()).ToArray());
                    logger.Log(string.Format(" Round {0} : Failed well ids: {1}", (i + 1), strIds));
                }
            }
        }
        #endregion

        #region RunAllocationTask
        public static void RunAllocationTask()
        {
            logger.Log("Retrieving wells...");
            var wellServiceClient = ClientFactory.GetService<IWellService>();
            WellDTO[] wells = wellServiceClient.GetAllWells()?.Where(o => (
                o.WellType == WellTypeId.GLift ||
                o.WellType == WellTypeId.ESP ||
                o.WellType == WellTypeId.PCP ||
                o.WellType == WellTypeId.NF ||
                o.WellType == WellTypeId.PLift ||
                o.WellType == WellTypeId.RRL ||
                o.WellType == WellTypeId.GInj ||
            o.WellType == WellTypeId.WInj) && o.FacilityId != null).ToArray();
            logger.LogFmt("Total count of GL, ESP, PCP, NF, PGL, RRL Wells: {0}", "",
                    LogMessageCategory.General, LogMessagePriority.Low, wells.Count());

            if (wells.Count() > 0)
            {
                var settingsServiceClient = ClientFactory.GetService<ISettingService>();
                SystemSettingDTO durationDTO = settingsServiceClient.GetSystemSettingByName(SettingServiceStringConstants.DAILY_AVERAGE_DATA_DURATION);
                double duration = (double)(durationDTO.NumericValue == null ? 0 : (durationDTO.NumericValue));
                logger.LogFmt("DAILY_AVERAGE_DATA_DURATION Value:{0}", LogMessageCategory.General, LogMessagePriority.Low, durationDTO.NumericValue.ToString());

                foreach (WellDTO well in wells)
                {
                    if (well.DataConnection != null)
                    {
                        if ((well.DataConnection.ScadaSourceType == ScadaSourceType.CygNet) || (well.DataConnection.ScadaSourceType == ScadaSourceType.OSIPI))
                        {
                            logger.LogFmt("Running Daily Average calculation for WellId:{0} & WellName: {1}.", LogMessageCategory.General, LogMessagePriority.Low, well.Id, well.Name);
                            try
                            {
                                var surveillanceServiceClient = ClientFactory.GetService<ISurveillanceService>();
                                surveillanceServiceClient.AddDailyAverageFromVHSByDateRange(well.Id.ToString(),
                                    DTOExtensions.ToISO8601(DateTime.Now.Date.AddHours(-duration).ToUniversalTime()),
                                    DTOExtensions.ToISO8601(DateTime.Now.Date.AddMilliseconds(-10).ToUniversalTime()));
                            }
                            catch (Exception ex)
                            {
                                logger.LogFmt("Error while running Daily Average calculation for WellId:{0} , WellName: {1}, Exception: {2}.",
                                    LogMessageCategory.General, LogMessagePriority.Low, well.Id, well.Name, ex.Message);
                                continue;
                            }
                        }
                    }
                }
            }
            else
            {
                logger.Log("No wells found to run RunAllocationTask task.", LogMessageCategory.General, LogMessagePriority.Low);
            }
        }
        #endregion

        #region GetAWTDataTask
        public static void GetAWTDataTask()
        {
            logger.Log("Retrieving wells...");
            var wellServiceClient = ClientFactory.GetService<IWellService>();
            WellDTO[] wells = wellServiceClient.GetAllWells()?.Where(o => (
                o.WellType == WellTypeId.GLift ||
                o.WellType == WellTypeId.ESP ||
                o.WellType == WellTypeId.NF ||
                o.WellType == WellTypeId.PLift ||
                o.WellType == WellTypeId.RRL ||
                o.WellType == WellTypeId.GInj ||
            o.WellType == WellTypeId.WInj) && o.FacilityId != null).ToArray();
            logger.LogFmt("Total count of GL, ESP, NF, PGL, RRL Wells: {0}", "",
                    LogMessageCategory.General, LogMessagePriority.Low, wells.Count());

            if (wells.Count() > 0)
            {
                var settingsServiceClient = ClientFactory.GetService<ISettingService>();
                SystemSettingDTO durationDTO = settingsServiceClient.GetSystemSettingByName(SettingServiceStringConstants.TIME_SPAN_AWT_DATA_RETRIEVE);
                double duration = (double)(durationDTO.NumericValue == null ? 0 : (durationDTO.NumericValue));
                logger.LogFmt("DAILY_AVERAGE_DATA_DURATION Value:{0}", LogMessageCategory.General, LogMessagePriority.Low, durationDTO.NumericValue.ToString());

                foreach (WellDTO well in wells)
                {
                    if (well.DataConnection != null)
                    {
                        if ((well.DataConnection.ScadaSourceType == ScadaSourceType.CygNet) || (well.DataConnection.ScadaSourceType == ScadaSourceType.OSIPI))
                        {
                            logger.LogFmt("Getting Well Test Data from VHS::AWT for WellId:{0} & WellName: {1}.", LogMessageCategory.General, LogMessagePriority.Low, well.Id, well.Name);
                            try
                            {
                                var surveillanceServiceClient = ClientFactory.GetService<ISurveillanceService>();
                                surveillanceServiceClient.AddWellTestRecordsFromAWTByDataRange(well.Id.ToString(),
                                    DTOExtensions.ToISO8601(DateTime.Now.Date.AddDays(-duration).ToUniversalTime()),
                                    DTOExtensions.ToISO8601(DateTime.Now.Date.AddMilliseconds(-10).ToUniversalTime()));
                            }
                            catch (Exception ex)
                            {
                                logger.LogFmt("Error while getting well test data from VHS::AWT for WellId:{0} , WellName: {1}, Exception: {2}.",
                                    LogMessageCategory.General, LogMessagePriority.Low, well.Id, well.Name, ex.Message);
                                continue;
                            }
                        }
                    }
                }
            }
            else
            {
                logger.Log("No wells found to run GetAWTDataTask task.", LogMessageCategory.General, LogMessagePriority.Low);
            }
        }

        //public static void GetAWTDataTimeSpanTask()
        //{
        //    logger.Log("Retrieving wells...");
        //    var wellServiceClient = ClientFactory.GetService<IWellService>();
        //    WellDTO[] wells = wellServiceClient.GetAllWells()?.Where(o => (
        //        o.WellType == WellTypeId.GLift ||
        //        o.WellType == WellTypeId.ESP ||
        //        o.WellType == WellTypeId.NF ||
        //        o.WellType == WellTypeId.PLift ||
        //        o.WellType == WellTypeId.RRL ||
        //        o.WellType == WellTypeId.GInj ||
        //    o.WellType == WellTypeId.WInj) && o.FacilityId != null).ToArray();
        //    logger.LogFmt("Total count of GL, ESP, NF, PGL, RRL Wells: {0}", "",
        //            LogMessageCategory.General, LogMessagePriority.Low, wells.Count());
        //    var surveillanceServiceClient = ClientFactory.GetService<ISurveillanceService>();

        //    if (wells.Count() > 0)
        //    {
        //        var settingsServiceClient = ClientFactory.GetService<ISettingService>();
        //        SystemSettingDTO durationDTO = settingsServiceClient.GetSystemSettingByName(SettingServiceStringConstants.TIME_SPAN_AWT_DATA_RETRIEVE);
        //        double duration = (double)(durationDTO.NumericValue == null ? 0 : (durationDTO.NumericValue));
        //        logger.LogFmt("DAILY_AVERAGE_DATA_DURATION Value:{0}", LogMessageCategory.General, LogMessagePriority.Low, durationDTO.NumericValue.ToString());

        //        foreach (WellDTO well in wells)
        //        {
        //            if (well.DataConnection != null)
        //            {
        //                if ((well.DataConnection.ScadaSourceType == ScadaSourceType.CygNet) || (well.DataConnection.ScadaSourceType == ScadaSourceType.OSIPI))
        //                {
        //                    logger.LogFmt("Getting Well Test Data from VHS::AWT for WellId:{0} & WellName: {1}.", LogMessageCategory.General, LogMessagePriority.Low, well.Id, well.Name);

        //                    try
        //                    {
        //                        for (int i = (int)duration; i > 0; i--)
        //                        {
        //                            string startDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddHours(-i * 24).ToUniversalTime());
        //                            string endDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddHours(-(i - 1) * 24).AddMilliseconds(-10).ToUniversalTime());
        //                            logger.LogFmt("Performing calculation for WellName: {0}, Start Date: {1}, End Date: {2}", LogMessageCategory.General, LogMessagePriority.Low, well.Name, startDate, endDate);
        //                            surveillanceServiceClient.AddWellTestRecordsFromAWTByDataRange(well.Id.ToString(), startDate, endDate);
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        logger.LogFmt("Error while getting well test data from VHS::AWT for WellId:{0} , WellName: {1}, Exception: {2}.",
        //                            LogMessageCategory.General, LogMessagePriority.Low, well.Id, well.Name, ex.Message);
        //                        continue;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        logger.Log("No wells found to run GetAWTDataTimeSpan task.", LogMessageCategory.General, LogMessagePriority.Low);
        //    }
        //}

        #endregion

        #region RunAllocationForMultipleDays
        public static void RunAllocationForMultipleDays(string wellname)
        {
            logger.Log("Retrieving wells...");
            var wellServiceClient = ClientFactory.GetService<IWellService>();
            WellDTO[] wells = wellServiceClient.GetAllWells()?.Where(o => (
                o.WellType == WellTypeId.GLift ||
                o.WellType == WellTypeId.ESP ||
                o.WellType == WellTypeId.PCP ||
                o.WellType == WellTypeId.NF ||
                o.WellType == WellTypeId.PLift ||
                o.WellType == WellTypeId.RRL ||
                o.WellType == WellTypeId.GInj ||
            o.WellType == WellTypeId.WInj) && o.FacilityId != null).ToArray();
            logger.LogFmt("Total count of GL, ESP, PCP, NF, PGL, RRL Wells: {0}", "",
                    LogMessageCategory.General, LogMessagePriority.Low, wells.Count());
            if (wellname != null)
            {
                wells = wells.Where(x => x.Name == wellname).ToArray();
            }

                if (wells.Count() > 0)
            {
                var settingsServiceClient = ClientFactory.GetService<ISettingService>();
                SystemSettingDTO durationDTO = settingsServiceClient.GetSystemSettingByName(SettingServiceStringConstants.TIME_SPAN_DAILY_TRENDS);
                int duration = (int)(durationDTO.NumericValue == null ? 0 : (durationDTO.NumericValue));
                logger.LogFmt("TIME_SPAN_DAILY_TRENDS Value:{0}", LogMessageCategory.General, LogMessagePriority.Low, durationDTO.NumericValue);

                var surveillanceServiceClient = ClientFactory.GetService<ISurveillanceService>();
                foreach (WellDTO well in wells)
                {
                    if (well.DataConnection != null)
                    {
                        if ((well.DataConnection.ScadaSourceType == ScadaSourceType.CygNet && well.FacilityId != null) ||
                            (well.DataConnection.ScadaSourceType == ScadaSourceType.OSIPI) || (well.DataConnection.ScadaSourceType == ScadaSourceType.Concentrator))
                        {
                            logger.LogFmt("Trend data calculation for WellId:{0}, WellName: {1}, Duration: {2}", LogMessageCategory.General, LogMessagePriority.Low, well.Id, well.Name, duration.ToString());
                            try
                            {
                                for (int i = duration; i > 0; i--)
                                {
                                    string startDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddHours(-i * 24).ToUniversalTime());
                                    string endDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddHours(-(i - 1) * 24).AddMilliseconds(-10).ToUniversalTime());
                                    logger.LogFmt("Performing calculation for WellName: {0}, Start Date: {1}, End Date: {2}", LogMessageCategory.General, LogMessagePriority.Low, well.Name, startDate, endDate);
                                    surveillanceServiceClient.AddDailyAverageFromVHSByDateRange(well.Id.ToString(), startDate, endDate);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogFmt("Error while running Daily Average calculation for WellId:{0} , WellName: {1}, Exception: {2}.", LogMessageCategory.Critical, LogMessagePriority.High, well.Id, well.Name, ex.Message);
                                continue;
                            }
                        }
                        else
                        {
                            logger.LogFmt("Skipping Trend data calculation for WellId:{0}, WellName: {1}, Incorrect Well Information.", LogMessageCategory.General, LogMessagePriority.Low, well.Id, well.Name);
                        }
                    }
                }
            }
            else
            {
                logger.Log("No wells found to run RunAllocationForMultipleDays task.", LogMessageCategory.General, LogMessagePriority.Low);
            }
        }
        #endregion

        #region RunProductionAllocation
        private static void RunProductionAllocationTask()
        {
            var wellAllocationServiceClient = ClientFactory.GetService<IWellAllocationService>();

            var startDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddHours(-24).ToUniversalTime());
            var endDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddMilliseconds(-10).ToUniversalTime());

            ProductionType phaseType = ProductionType.Unknown;
            foreach (ProductionType prodType in Enum.GetValues(typeof(ProductionType)))
            {
                if (prodType == ProductionType.Unknown)
                    continue;

                phaseType = prodType;
                try
                {
                    logger.LogFmt("Saving allocated rate for the wells for phase type: {0}, Start Date: {1}, End Date: {2}", LogMessageCategory.General, LogMessagePriority.Low, phaseType.ToString(), startDate, endDate);
                    wellAllocationServiceClient.SaveAllocationRatesInWellDailyAverage(phaseType.ToString(), startDate, endDate);
                }
                catch (Exception ex)
                {
                    logger.LogFmt("Error while saving allocation rates for phase type {0} during {1} and {2}. Exception: {3}.", LogMessageCategory.Critical, LogMessagePriority.High, phaseType.ToString(), startDate, endDate, ex.Message);
                    continue;
                }
            }
        }
        #endregion


        #region RunProdAllocationForMultipleDays
        private static void RunProdAllocationForMultipleDays()
        {
            var settingsServiceClient = ClientFactory.GetService<ISettingService>();
            SystemSettingDTO durationDTO = settingsServiceClient.GetSystemSettingByName(SettingServiceStringConstants.TIME_SPAN_DAILY_TRENDS);
            int duration = (int)(durationDTO.NumericValue == null ? 0 : (durationDTO.NumericValue));
            logger.LogFmt("TIME_SPAN_DAILY_TRENDS Value:{0}", LogMessageCategory.General, LogMessagePriority.Low, durationDTO.NumericValue);

            var wellAllocationServiceClient = ClientFactory.GetService<IWellAllocationService>();
            for (int i = duration; i > 0; i--)
            {
                string startDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddHours(-i * 24).ToUniversalTime());
                string endDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddHours(-(i - 1) * 24).AddMilliseconds(-10).ToUniversalTime());
                logger.LogFmt("Saving allocated rate for the wells in all hierarchies, Start Date: {0}, End Date: {1}", LogMessageCategory.General, LogMessagePriority.Low, startDate, endDate);
                ProductionType phaseType = ProductionType.Unknown;
                foreach (ProductionType prodType in Enum.GetValues(typeof(ProductionType)))
                {
                    if (prodType == ProductionType.Unknown)
                        continue;

                    phaseType = prodType;
                    try
                    {
                        logger.LogFmt("Saving allocated rate for the wells for phase type: {0}, Start Date: {1}, End Date: {2}", LogMessageCategory.General, LogMessagePriority.Low, phaseType.ToString(), startDate, endDate);
                        wellAllocationServiceClient.SaveAllocationRatesInWellDailyAverage(phaseType.ToString(), startDate, endDate);
                    }
                    catch (Exception ex)
                    {
                        logger.LogFmt("Error while saving allocation rates for phase type {0} during {1} and {2}. Exception: {3}.", LogMessageCategory.Critical, LogMessagePriority.High, phaseType.ToString(), startDate, endDate, ex.Message);
                        continue;
                    }
                }
            }
        }
        #endregion

        #region RRLIdleTimeOptimization        
        private static void RRLIdleTimeOptimizationTask()
        {
            logger.Log("Retrieving wells...");
            var wellServiceClient = ClientFactory.GetService<IWellService>();
            WellDTO[] wells = wellServiceClient.GetAllWells()?.Where(o => o.WellType == WellTypeId.RRL && o.DataConnection != null).ToArray();
            logger.LogFmt("Total count of RRL Wells: {0}", "",
                    LogMessageCategory.General, LogMessagePriority.Low, wells.Count());

            var alarmServiceClient = ClientFactory.GetService<IAlarmService>();
            var settingServiceClient = ClientFactory.GetService<ISettingService>();
            var surveillanceServiceClient = ClientFactory.GetService<ISurveillanceService>();
            var alarmTypeList = alarmServiceClient.GetAlarmTypes();
            string alarmTypeDescription = DescriptionOverrideAttribute.GetDescription(typeof(EdgeAutonomousQuantity), EdgeAutonomousQuantity.InfProdChange.ToString(), WellTypeId.RRL, null);
            var alarmType = new AlarmTypeDTO
            {
                Id = alarmTypeList.First(en => en.AlarmType == alarmTypeDescription).Id,
                AlarmType = alarmTypeDescription,
            };

            if (wells.Count() > 0)
            {
                ParallelOptions options = new ParallelOptions();
                options.MaxDegreeOfParallelism = 10;

                if (int.TryParse(ConfigurationManager.AppSettings["MaxThreads"], out int maxThreadsInt))
                    options.MaxDegreeOfParallelism = maxThreadsInt;

                Parallel.ForEach(wells, options, well =>
                {
                    try
                    {
                        if (well.DataConnection.ScadaSourceType == ScadaSourceType.CygNet)
                        {
                            CurrentAlarmDTO lastProductionDecreaseAlert = alarmServiceClient.GetActiveAlarmByWellIdAndAlarmType(well.Id.ToString(), alarmType.Id.ToString());
                            //var daysInterval = settingServiceClient.GetSystemSettingByName(SettingServiceStringConstants.DAYS_BETWEEN_IDLE_TIME_OPTIMIZATION_PROCESS).NumericValue;
                            try
                            {
                                //var lastProductionDecreaseAlert = _alarmService.GetAlarmsByWellIdAndAlarmType(wellId, alarmType.Id.ToString())?.OrderByDescending(en => en.Timestamp).FirstOrDefault();
                                var daysInterval = settingServiceClient.GetSystemSettingByName(SettingServiceStringConstants.DAYS_BETWEEN_IDLE_TIME_OPTIMIZATION_PROCESS).NumericValue;
                                if (lastProductionDecreaseAlert != null
                                    && (DateTime.Today.ToUniversalTime().Date - lastProductionDecreaseAlert.StartTime.Date) >= TimeSpan.FromDays((int)daysInterval))
                                {
                                    alarmServiceClient.ClearWellAlarmsByTypeIds(well.Id.ToString(), new[] { alarmType.Id });
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Log($"update alarm information throw exception while running optimizing RRL idle time scheduler job: " + ex.Message, LogMessageCategory.Exception, LogMessagePriority.High);
                            }

                            if (lastProductionDecreaseAlert == null)
                            {
                                surveillanceServiceClient.OptimizingRRLIdleTimeForSingleWell(well.Id.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogFmt("{0} - Encountered error while running RRLIdleTimeOptimizationTask in Scheduler: {1}",
                            ex.ToString(), LogMessageCategory.Exception, LogMessagePriority.High, well.Name, ex.Message);
                    }
                });
            }
        }

        #endregion

        #region WellRunStatusDerivation   
        private static void WellRunStatusDerivationTask()
        {
            logger.Log("Retrieving wells...");
            var wellServiceClient = ClientFactory.GetService<IWellService>();
            WellDTO[] wells = wellServiceClient.GetAllWells()?.Where(o => o.WellType == WellTypeId.ESP || o.WellType == WellTypeId.GLift || o.WellType == WellTypeId.NF && o.DataConnection != null).ToArray();
            logger.LogFmt("Total count of ESP, Gas Lift and Natural Flowing Wells: {0}", "",
                    LogMessageCategory.General, LogMessagePriority.Low, wells.Count());

            var settingServiceClient = ClientFactory.GetService<ISettingService>();
            var surveillanceServiceClient = ClientFactory.GetService<ISurveillanceService>();

            if (wells.Count() > 0)
            {
                ParallelOptions options = new ParallelOptions();
                options.MaxDegreeOfParallelism = 10;

                if (int.TryParse(ConfigurationManager.AppSettings["MaxThreads"], out int maxThreadsInt))
                    options.MaxDegreeOfParallelism = maxThreadsInt;

                Parallel.ForEach(wells, options, well =>
                {
                    try
                    {
                        if (well.DataConnection.ScadaSourceType == ScadaSourceType.CygNet)
                        {
                            surveillanceServiceClient.DerivingRunStatusForSingleWell(well.Id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogFmt("{0} - Encountered error while running WellRunStatusDerivationTask in Scheduler: {1}",
                            ex.ToString(), LogMessageCategory.Exception, LogMessagePriority.High, well.Name, ex.Message);
                    }
                });
            }
        }

        #endregion

        #region JobEconomicAnalysisTask
        private static void JobEconomicAnalysisTask()
        {
            logger.Log("Attempting to run job economic analysis, check for wells.");

            IJobAndEventService jobeconomicAnalysisClient = ClientFactory.GetService<IJobAndEventService>();
            IWellService wellServiceClient = ClientFactory.GetService<IWellService>();

            WellDTO[] wells = wellServiceClient.GetAllWells().ToArray();

            if (wells.Count() == 0)
            {
                logger.Log("No configured wells.", LogMessageCategory.General, LogMessagePriority.Low);
            }
            else
            {
                foreach (var well in wells)
                {
                    try
                    {
                        jobeconomicAnalysisClient.UpdateJobEconomicAnalysisViaScheduler(well.Id.ToString());
                        logger.LogFmt("Completed the Economic Analysis and DPI Recalculation for well: {0}", LogMessageCategory.General, LogMessagePriority.Low, well.Name);
                    }
                    catch (Exception ex)
                    {
                        logger.LogFmt("Running job economic analysis for Well: {0} failed. Exception: {1}",
                            LogMessageCategory.Exception, LogMessagePriority.High, well.Name, ex.Message);
                        continue;
                    }
                }
            }
        }
        #endregion

        #region "PCPOperatingEnvelopeUpdateTask"

        public static void UpdatePCPOperatingEnvelope()
        {
            //logger.Log("Starting to update PCP operating envelope data for each PCP Well.");

            IWellService wellServiceClient = ClientFactory.GetService<IWellService>();
            IWellTestService wellTestServiceClient = ClientFactory.GetService<IWellTestService>();

            WellDTO[] wells = wellServiceClient.GetAllWells().Where(wl => wl.WellType == WellTypeId.PCP).ToArray();
            foreach (var well in wells)
            {
                try
                {
                    string wellId = well.Id.ToString();

                    if (wellTestServiceClient.PreparePCPOperatingEnvelope(wellId))
                    {
                        logger.Log($"Successfully prepared operating envelope for '{well.Name}' well.", LogMessageCategory.General, LogMessagePriority.Low);
                    }
                    else
                    {
                        logger.Log($"Failed to prepare operating envelope for '{well.Name}' well.", LogMessageCategory.General, LogMessagePriority.Low);
                    }
                }
                catch
                {
                }
            }
        }
        #endregion

        #region AutomaticDownTimeCalculationTask

        public static void CalculateDownTimeForAllWells()
        {
            IWellService wellServiceClient = ClientFactory.GetService<IWellService>();
            var surveillanceService = ClientFactory.GetService<ISurveillanceService>();
            var settingService = ClientFactory.GetService<ISettingService>();

            //Check Automatic Downtime Logging is Enabled.
            SystemSettingDTO systemSettingDTO = settingService.GetSystemSettingByName(SettingServiceStringConstants.ENABLE_AUTOMATIC_DOWNTIME_LOGGING);
            bool downtimeLoggingEnabled = Convert.ToBoolean(systemSettingDTO.NumericValue.Value);

            if (!downtimeLoggingEnabled)
            {
                logger.Log($"Automatic Downtime Logging is disabled.", LogMessageCategory.General, LogMessagePriority.Low);
                return;
            }

            WellDTO[] wells = wellServiceClient.GetAllWells().ToArray();
            foreach (var well in wells)
            {
                try
                {
                    if (well.WellType != WellTypeId.GInj && well.WellType != WellTypeId.WInj && well.WellType != WellTypeId.WGInj 
                        && well.WellType != WellTypeId.All && well.WellType != WellTypeId.OT && well.WellType != WellTypeId.Unknown
                        && well.DataConnection.ScadaSourceType == ScadaSourceType.CygNet)
                    {
                        string wellId = well.Id.ToString();

                        if (surveillanceService.CalculateDownTimeForWell(wellId))
                        {
                            logger.Log($"Successfully calculated Downtime for '{well.Name}' well.", LogMessageCategory.General, LogMessagePriority.Low);
                        }
                        else
                        {
                            logger.Log($"Failed to calculate Downtime for '{well.Name}' well.", LogMessageCategory.General, LogMessagePriority.Low);
                        }
                    }
                }
                catch
                {
                }
            }
        }
        #endregion
    }
}
