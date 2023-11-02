using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Data;

namespace Backup
{
    public class Program
    {
        private static string _root = $"D:\\SQL Backup";
        public static void Main()
        {
            Console.WriteLine("Starting Backup\n");
            //
            Procedure.BackupDataBases();

            var folders = from folder
                          in Directory.EnumerateDirectories(_root)
                          select folder;
            
            foreach (var folder in folders)
            {
                DailyCleanup(folder);
            }

            Console.WriteLine("\n\nBackup finished");
        }

        public static void DailyCleanup(string path)
        {
            var files = from file
                        in Directory.EnumerateFiles(path)
                        select file;

            DateTime today = DateTime.Today;
            for( int i = 0; i < 10; i++ )
            {
                DateTime startDate = today.AddDays( -i );

                var daysFiles = from file
                                in files
                                where (
                                    File.GetCreationTime( file ).Year == startDate.Year
                                    &&
                                    File.GetCreationTime( file ).Month == startDate.Month
                                    &&
                                    File.GetCreationTime( file ).Day == startDate.Day
                                )
                                select file;

                if (daysFiles.Any())
                {
                    string oldestFile = files.First();
                    foreach (var file in daysFiles)
                    {
                        if (Directory.GetLastWriteTime(file) > Directory.GetLastWriteTime(oldestFile))
                        {
                            oldestFile = file;
                        }
                    }

                    var toDelete = from file
                                   in daysFiles
                                   where file != oldestFile
                                   select file;

                    foreach(var file in toDelete)
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        public static void FolderCleanUp()
        {
            DateTime today = DateTime.Today;
            DateTime tenDays = today.AddDays(-10);

            List<string> backupFiles = new List<string>();

            // LIST OUT THE BACK UP SUBFOLDERS
            var folders = from folder
                          in Directory.EnumerateDirectories(_root)
                          select folder;

            foreach (var folder in folders)
            {
                Console.WriteLine(folder);
                // CHECK FOLDERS FOR THE NEWEST FILE
                var files = from file
                            in Directory.EnumerateFiles(folder)
                            where Directory.GetCreationTime(file) >= today
                            select file;

                // COMPARE ALL FILES AGAINST THE FIRST
                var max = files.First();

                foreach (var file in files)
                {
                    // NEWER FOLDERS ARE ASSIGNED THE MAX REF
                    if (Directory.GetLastWriteTime(max) <= Directory.GetLastWriteTime(file))
                    {
                        max = file;
                    }
                }

                // RETURN MAX
                Console.WriteLine($"{max} - {Directory.GetLastWriteTime(max)}");

                var toDelete = from file
                               in Directory.EnumerateFiles(folder)
                               where file != max
                               select file;

                // DELETE THE OTHER BAK FILES
                foreach (var file in toDelete)
                {
                    Console.WriteLine($"Deleting: {file}");
                    File.Delete(file);
                }

                Console.WriteLine("\n\n");
            }
        }
    }
}

