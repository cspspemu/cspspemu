using System;
using System.Collections.Generic;
using System.IO;

namespace CSharpUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class FileUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        /// <see cref="http://stackoverflow.com/questions/58744/best-way-to-copy-the-entire-contents-of-a-directory-in-c"/>
        public static void CopyTree(string sourcePath, string destinationPath)
        {
            // Now Create all of the directories
            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
            }

            // Copy all the files
            foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetExecutableFilePath() =>
            System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetExecutableDirectoryPath() => Path.GetDirectoryName(GetExecutableFilePath());

        private static readonly HashSet<string> AppendStreams = new HashSet<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="inputStream"></param>
        public static void CreateAndAppendStream(string path, Stream inputStream)
        {
            if (!AppendStreams.Contains(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                AppendStreams.Add(path);
            }

            using (var outputStream = File.OpenWrite(path))
            {
                outputStream.Position = outputStream.Length;
                inputStream.Slice().CopyToFast(outputStream);
            }
            //throw new NotImplementedException();
        }
    }
}