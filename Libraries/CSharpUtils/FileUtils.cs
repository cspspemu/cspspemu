using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		static public void CopyTree(String SourcePath, String DestinationPath)
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

        static public String GetExecutableFilePath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
        }

        static public String GetExecutableDirectoryPath()
        {
            return System.IO.Path.GetDirectoryName(GetExecutableFilePath());
        }
	}
}
