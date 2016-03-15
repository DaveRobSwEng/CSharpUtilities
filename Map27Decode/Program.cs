using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;
using CommandLine.Text;


namespace Map27Decode
{

    class Options
    {
        [Option('l', "log", Required = true, HelpText = "Log file to be processed.")]
        public string LogFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output file. Default is <logfile>.txt")]
        public string OutputFile { get; set; }

        [Option('d', "datadict", Required = false, HelpText = "Data dictionary file")]
        public string DataDictFile { get; set; }

        [Option('r', "retries", Required = false, HelpText = "Report on retries")]
        public bool Retries { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Options theOptions = new Options();
                if (CommandLine.Parser.Default.ParseArguments(args, theOptions))
                {
                    LogDecoder decoder = new LogDecoder();
                    decoder.ShowRetries = theOptions.Retries;
                    decoder.OnSignalDecoded += Decoder_OnSignalDecoded;
                    decoder.OnOutOfFrameBytes += Decoder_OnOutOfFrameBytes;

                    string outputFile = theOptions.OutputFile;
                    if (String.IsNullOrEmpty(outputFile))
                    {
                        outputFile = theOptions.LogFile + ".txt";

                        Console.WriteLine("Writing to output {0}", outputFile);
                    }

                    if (theOptions.Retries)
                    {
                        Console.WriteLine("Analysing retries");
                    }

                    s_OutputStream = File.CreateText(outputFile);

                    string dataDictFile = theOptions.DataDictFile;
                    if (String.IsNullOrEmpty(dataDictFile))
                    {
                        string assemblyPath = Assembly.GetEntryAssembly().Location;
                        dataDictFile = Path.Combine(Path.GetDirectoryName(assemblyPath), "map27dict.xml");

                        Console.WriteLine("Using default dictionary {0}", dataDictFile);
                    }

                    decoder.Decode(theOptions.LogFile, dataDictFile);
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

        private static void Decoder_OnOutOfFrameBytes(string signalText)
        {
            s_OutputStream.WriteLine(signalText);
        }

        private static StreamWriter s_OutputStream;

        private static void Decoder_OnSignalDecoded(string signalText)
        {
            s_OutputStream.WriteLine(signalText);
        }
    }
}
