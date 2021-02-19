using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;

namespace CryptoZ.Tools
{
    public static class FileTools
    {
        static string rootFolder = "/cryptoZ";   // default root folder (can be changed with RootFolder property)

        public static string RootFolder
        {
            get
            {
                return rootFolder;
            }
            set
            {
                rootFolder = value;
                EnsurePathExists(rootFolder);
            }
        }


        // Given an IEnumerable of objects and a filepath
        // Write all the objects to a .CSV file (with headers)
        public static void WriteObjectsToCsv<T>(IEnumerable<T> data, string filepath)
        {
            using (var writer = new StreamWriter(filepath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }
        }

        // Given an IEnumerable of strings, a filepath, and a header
        // Write all the strings to a .CSV file (with the given header)
        public static void WriteStringsToCsv(IEnumerable<string> data, string filepath, string singleColumnHeader)
        {
            using (var writer = new StreamWriter(filepath))
            {
                writer.WriteLine(singleColumnHeader);
                data.ToList().ForEach(s => writer.WriteLine(s));
            }
        }

        // Given an exchange name
        // Return the full filepath of the .csv symbols file
        public static string SymbolFilepath(string exchName, bool createIfNotExist = false)
        {
            var symbolPath = Path.Join(rootFolder, "symbols");

            if (createIfNotExist) EnsurePathExists(symbolPath);
            
            var filepath = Path.Join(symbolPath, $"symbols.{exchName}.csv");
            return filepath;
        }

        // Given a subdir (of root) and a file name
        // Return the full filepath of the .csv symbols file
        public static string SubdirFilepath(string subDir, string filename, bool createIfNotExist = false)
        {
            var path = Path.Join(rootFolder, subDir);

            if (createIfNotExist) EnsurePathExists(path);

            var filepath = Path.Join(path, filename);
            return filepath;
        }

        // Create the given path if it does not yet exist
        public static void EnsurePathExists(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to create directory '{path}'.\n   {ex.Message}");
                }
            }
        }

        // Given a starting path and the number of levels up to go
        // Return parent at that level
        public static string GetParent(string startPath, int levels = 1)
        {
            string path = startPath;
            for (int i = 0; i < levels; ++i)
            {
                path = Directory.GetParent(path).FullName;
            }
            return path;
        }

        // Starting with the .exe path of a project in the solution
        // Return the path of the .exe for another project in the solution (Debug folder)
        // where build = ["Debug"|"Release]
        public static string GetProjectExeFilepath(string startingExePath, string projectFolder, string exeFilename, string build = "Debug", string frameworkName = "netcoreapp3.1", string projectSubFolder = null, int levelsUpFromStarting = 4)
        {
            string basePath = GetParent(startingExePath, levelsUpFromStarting);
            string binDir;
            if (string.IsNullOrEmpty(projectSubFolder))
                binDir = Path.Join(basePath, projectFolder, "bin");
            else
                binDir = Path.Join(basePath, projectSubFolder, projectFolder, "bin");
            string debugDir = Path.Join(binDir, build, frameworkName);
            return Path.Join(debugDir, exeFilename);
        }

        // Starting with the .exe path of a project in the solution
        // Return the path of the .exe for another project in the solution (Release if exsts, else Debug)
        // (returns null if neither Release nor Debug version of .exe exists) 
        public static string GetProjectExeFilepathReleaseOrDebug(string startingExePath, string projectFolder, string exeFilename, string frameworkName = "netcoreapp3.1", string projectSubFolder = null, int levelsUpFromStarting = 4)
        {
            string releaseExe = GetProjectExeFilepath(startingExePath, projectFolder, exeFilename, "Release", frameworkName, projectSubFolder, levelsUpFromStarting);
            string debugExe = GetProjectExeFilepath(startingExePath, projectFolder, exeFilename, "Debug", frameworkName, projectSubFolder, levelsUpFromStarting);
            if (File.Exists(releaseExe))
                return releaseExe;
            else if (File.Exists(debugExe))
                return debugExe;
            else
                return null;
        }


    } // class

} // namespace
