using System;
using System.IO;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;

namespace ChkApiLog
{

    class Options
    {
        #region Public Properties

        [Option('l', "log", Required = true, HelpText = "Log file to be processed.")]
        public string LogFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file. Default is <logfile>.summary.txt")]
        public string OutputFile { get; set; }

        [Option('i', "excludeisconnected", Required = false, HelpText = "Excludes reporting instances of MAP27API_IsRadioConnected")]
        public bool ExcludeIsRadioConnected { get; set; }

        [Option('c', "combine", Required = false, HelpText = "Combined command start and end reports")]
        public bool CombineCommandStartEnd { get; set; }

        [Option('f', "reportfailures", Required = false, HelpText = "Report only failures")]
        public bool ReportOnlyFailures { get; set; }

        #endregion Public Properties

        #region Public Methods

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        #endregion Public Methods
    }

    class Program
    {
        #region Private Methods

        private static void Analyse(string logFile, StreamWriter s_OutputStream, bool excludeIsRadioConnected, bool reportOnlyFailures, bool combineCommandStartEnd)
        {
            if (!File.Exists(logFile))
            {
                throw new Exception(string.Format("Log file {0} does not exist", logFile));
            }

            Console.WriteLine("Analysing {0}", logFile);

            using (FileStream stream = File.OpenRead(logFile))
            using (TextReader reader = new StreamReader(stream))
            {
                long lineCount = 0;
                double logStartTime = 0;
                string lineIn = reader.ReadLine();
                if (lineIn != null)
                {
                    string logTime = lineIn.Substring(0, s_TimestampLength);
                    logStartTime = ParseTimestamp(logTime);
                }

                ++lineCount;

                string currentMap27ApiCommand = string.Empty;
                string commandStartTime = string.Empty;

                while (lineIn != null)
                {
                    // Line looks like:
                    // 15:26:23.612  MAP27API_SetExternalOutput: 5a90020 result 0
                    if (lineIn.Length >= s_TimestampLength)
                    {
                        string trimmedLine = lineIn.Substring(s_TimestampLength).Trim();

                        if (trimmedLine.StartsWith(s_Map27ApiHeader, StringComparison.Ordinal))
                        {
                            if (excludeIsRadioConnected &&
                                String.Compare(trimmedLine, s_Map27ApiHeader.Length, s_IsRadioConnected, 0, s_IsRadioConnected.Length) == 0)
                            {
                                lineIn = reader.ReadLine();
                                ++lineCount;
                                continue;
                            }

                            Match m = s_Map27ApiCommandRegex.Match(trimmedLine);

                            if (m.Success)
                            {
                                string logTime = lineIn.Substring(0, s_TimestampLength);
                                Group commandGroup = m.Groups["Command"];
                                if (commandGroup != null && commandGroup.Success)
                                {
                                    string map27ApiCommand = commandGroup.Value;

                                    Group resultGroup = m.Groups["Result"];
                                    if (resultGroup != null && resultGroup.Success)
                                    {
                                        // Found a command completion 
                                        if (currentMap27ApiCommand == map27ApiCommand)
                                        {
                                            bool show = true;
                                            bool failure = false;

                                            int result = 0;
                                            if (int.TryParse(resultGroup.Value, out result))
                                            {
                                                failure = result != 0;
                                            }

                                            if (reportOnlyFailures)
                                            {
                                                show = failure;
                                            }

                                            if (show)
                                            {
                                                s_OutputStream.WriteLine("{0} {1:0000.000000} {2} Result: {3} Started {4} Duration {5:.000000}s {6}",
                                                    logTime,
                                                    ParseTimestamp(logTime) - logStartTime,
                                                    map27ApiCommand,
                                                    resultGroup.Value,
                                                    commandStartTime,
                                                    ParseTimestamp(logTime) - ParseTimestamp(commandStartTime),
                                                    failure ? "[FAIL]" : string.Empty);

                                                Console.WriteLine("{0} {1:0000.000000} {2} Result: {3} Started {4} Duration {5:.000000}s {6}",
                                                    logTime,
                                                    ParseTimestamp(logTime) - logStartTime,
                                                    map27ApiCommand,
                                                    resultGroup.Value,
                                                    commandStartTime,
                                                    ParseTimestamp(logTime) - ParseTimestamp(commandStartTime),
                                                    failure ? "[FAIL]" : string.Empty);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Found the start of a command
                                        currentMap27ApiCommand = map27ApiCommand;
                                        commandStartTime = logTime;

                                        if(!reportOnlyFailures && !combineCommandStartEnd)
                                        {
                                            s_OutputStream.WriteLine("{0} {1:0000.000000} {2}",
                                                logTime,
                                                ParseTimestamp(logTime) - logStartTime,
                                                map27ApiCommand);

                                            Console.WriteLine("{0} {1:0000.000000} {2}",
                                                logTime,
                                                ParseTimestamp(logTime) - logStartTime,
                                                map27ApiCommand);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        s_OutputStream.WriteLine("[Error] Failed to parse line: {0} '{1}'", lineCount, lineIn);
                        Console.WriteLine("[Error] Failed to parse line: {0} '{1}'", lineCount, lineIn);
                    }

                    lineIn = reader.ReadLine();
                    ++lineCount;
                }

                Console.WriteLine("Processed {0} lines", lineCount);
            }
        }

        private const string s_Map27ApiHeader = "MAP27API_";
        private const string s_IsRadioConnected = "IsRadioConnected";

        private const int s_TimestampLength = 12;

        private static double ParseTimestamp(string value)
        {
            // Timestamp looks like:
            // 15:27:03.181
            return TimeSpan.Parse(value).TotalSeconds;
        }

        private static Regex s_Map27ApiCommandRegex = new Regex(
              @"MAP27API_(?<Command>\w+)(?::\s+[a-fA-F0-9]+)?(?:\s+result\s+(?<Result>\d+))?",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );


        static void Main(string[] args)
        {
            try
            {
                Options theOptions = new Options();
                if (CommandLine.Parser.Default.ParseArguments(args, theOptions))
                {
                    string outputFile = theOptions.OutputFile;
                    if (String.IsNullOrEmpty(outputFile))
                    {
                        outputFile = theOptions.LogFile + ".summary.txt";
                    }

                    Console.WriteLine("Writing to output {0}", outputFile);

                    s_OutputStream = File.CreateText(outputFile);

                    Analyse(theOptions.LogFile, s_OutputStream, theOptions.ExcludeIsRadioConnected, theOptions.ReportOnlyFailures, theOptions.CombineCommandStartEnd);
                }
            }
            catch (Exception ex)
            {
                Exception theException = ex;
                while (theException != null)
                {
                    Console.WriteLine(theException);
                    theException = theException.InnerException;
                }
            }

            if (s_OutputStream != null)
            {
                s_OutputStream.Dispose();
            }
        }

        #endregion Private Methods

        #region Private Fields

        private static StreamWriter s_OutputStream;

        #endregion Private Fields

    }
}
