﻿using System;
using BinaryObjectScanner;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
#if NET462_OR_GREATER || NETCOREAPP
            // Register the codepages
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif

            // Create progress indicator
            var fileProgress = new Progress<ProtectionProgress>();
            fileProgress.ProgressChanged += Protector.Changed;

            // Get the options from the arguments
            var options = Options.ParseOptions(args);

            // If we have an invalid state
            if (options == null)
            {
                Options.DisplayHelp();
                Console.WriteLine("Press enter to close the program...");
                Console.ReadLine();
                return;
            }

            // Create extractor for all paths
            var extractor = new Extractor(options.Debug);

            // Create printer for all paths
            var printer = new Printer(options.Debug);

            // Create scanner for all paths
            var scanner = new Scanner(
                options.ScanArchives,
                options.ScanContents,
                options.ScanGameEngines,
                options.ScanPackers,
                options.ScanPaths,
                options.Debug,
                fileProgress);

            // Loop through the input paths
            foreach (string inputPath in options.InputPaths)
            {
                // Extraction
                if (options.EnableExtraction)
                    extractor.ExtractPath(inputPath, options.OutputPath);

                // Information printing
                if (options.EnableInformation)
#if NETFRAMEWORK
                    printer.PrintPathInfo(inputPath, false, options.Debug);
#else
                    printer.PrintPathInfo(inputPath, options.Json, options.Debug);
#endif

                // Scanning
                if (options.EnableScanning)
                    Protector.GetAndWriteProtections(scanner, inputPath);
            }

            Console.WriteLine("Press enter to close the program...");
            Console.ReadLine();
        }
    }
}
