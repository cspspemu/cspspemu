using System;
using System.Collections.Generic;
using System.IO;

namespace CSharpUtils
{
    public class FileUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SourcePath"></param>
        /// <param name="DestinationPath"></param>
        /// <see cref="http://stackoverflow.com/questions/58744/best-way-to-copy-the-entire-contents-of-a-directory-in-c"/>
        public static void CopyTree(String SourcePath, String DestinationPath)
        {
            // Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));
            }

            // Copy all the files
            foreach (string newPath in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath));
            }
        }

        public static String GetExecutableFilePath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
        }

        public static String GetExecutableDirectoryPath()
        {
            return System.IO.Path.GetDirectoryName(GetExecutableFilePath());
        }

        static private readonly HashSet<string> _AppendStreams = new HashSet<string>();

        public static void CreateAndAppendStream(string Path, Stream InputStream)
        {
            if (!_AppendStreams.Contains(Path))
            {
                try
                {
                    File.Delete(Path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                _AppendStreams.Add(Path);
            }

            using (var OutputStream = File.OpenWrite(Path))
            {
                OutputStream.Position = OutputStream.Length;
                InputStream.Slice().CopyToFast(OutputStream);
            }
            //throw new NotImplementedException();
        }
    }
}