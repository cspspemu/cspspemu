using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CSharpUtils.Forms;

namespace CSharpUtils.VirtualFileSystem.Utils
{
	public class Synchronizer
	{
		String RemovingFormat = "Removing {0}...";
		String UpdatingFormat = "Updating {0}...";
		String SynchronizingFormat = "Synchronizing {0}...";
		String ConnectingToFormat = "Connecting to {0}...";
		object IsFinishedLockObject;

		public enum SynchronizationMode
		{
			CopyNewFiles = 1,
			UpdateOldFiles = 2,
			DeleteOldFiles = 4,
			ReplaceAllFiles = 8,
			CopyNewAndUpdateOldFiles = CopyNewFiles | UpdateOldFiles,
		}

		public enum ReferenceMode
		{
			Size = 1,
			LastWriteTime = 2,
			//Contents = 4,
			SizeAndLastWriteTime = Size | LastWriteTime,
		}

		public bool Canceling = false;
		FileSystem SourceFileSystem, DestinationFileSystem;
		String SourcePath, DestinationPath;
		SynchronizationMode _SynchronizationMode;
		ReferenceMode _ReferenceMode;
		public event Action<double, String> OnStep;
		public event Action<Exception> OnError;

		public Synchronizer(FileSystem SourceFileSystem, String SourcePath, FileSystem DestinationFileSystem, String DestinationPath, SynchronizationMode _SynchronizationMode, ReferenceMode _ReferenceMode)
		{
			this.SourceFileSystem = SourceFileSystem;
			this.DestinationFileSystem = DestinationFileSystem;
			this.SourcePath = SourcePath;
			this.DestinationPath = DestinationPath;
			this._SynchronizationMode = _SynchronizationMode;
			this._ReferenceMode = _ReferenceMode;

			OnStep += delegate(double Value, String Details) {
				//Console.WriteLine("OnStep: " + Value + "; " + Details);
			};

			OnError += delegate(Exception Exception)
			{
				Console.Error.WriteLine(Exception);
			};
		}

		public void Cancel()
		{
			Canceling = true;
		}

		public static void Synchronize(FileSystem SourceFileSystem, String SourcePath, FileSystem DestinationFileSystem, String DestinationPath, SynchronizationMode _SynchronizationMode, ReferenceMode _ReferenceMode)
		{
			lock (SourceFileSystem)
			{
				lock (DestinationFileSystem)
				{
					var Synchronizer = new Synchronizer(SourceFileSystem, SourcePath, DestinationFileSystem, DestinationPath, _SynchronizationMode, _ReferenceMode);
					Synchronizer.SynchronizeFolder("/");
				}
			}
		}

		protected void CheckCanceling()
		{
			if (Canceling) throw(new OperationCanceledException());
		}

		public void SynchronizeFolder(String Path = "/")
		{
			IsFinishedLockObject = new object();
			try
			{
				Console.WriteLine("Synchronizing folder");
				_SynchronizeFolder(Path);
				CallStep(1.0, "Finished");
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine("Cancelled");
			}
			catch (Exception Exception)
			{
				if (OnError != null) OnError(Exception);
			}
			lock (IsFinishedLockObject)
			{
				Monitor.Pulse(IsFinishedLockObject);
			}
		}

		private void CallStep(double Step, String Details)
		{
			if (OnStep != null) OnStep(Step, Details);
		}

		private static double GetStep(double StepFrom, double StepTo, int subStep, int subSteps)
		{
			Debug.Assert(StepTo >= StepFrom);
			Debug.Assert(subSteps >= subStep);
			return StepFrom + (StepTo - StepFrom) * (double)subStep / (double)subSteps;
		}

		private void _SynchronizeFolder(String Path, double StepFrom = 0.0, double StepTo = 1.0)
		{
			//Console.WriteLine("------------{0}/{1}", StepFrom, StepTo);

			Canceling = false;

			CallStep(StepFrom, String.Format(SynchronizingFormat, Path));

			var SourceFiles = this.SourceFileSystem
				.FindFiles(FileSystem.CombinePath(SourcePath, Path))
				.ToDictionary(FileSystemEntry => FileSystemEntry.Name)
			;
			var DestinationFiles = this.DestinationFileSystem
				.FindFiles(FileSystem.CombinePath(DestinationPath, Path))
				.ToDictionary(FileSystemEntry => FileSystemEntry.Name)
			;
			var FilesToRemove = new LinkedList<FileSystemEntry>();
			var FilesToUpdate = new LinkedList<FileSystemEntry>();
			var FoldersToExplore = new LinkedList<String>();

			// Folders to Explore.
			foreach (var PairSourceFile in SourceFiles)
			{
				CheckCanceling();

				var SourceFile = PairSourceFile.Value;
				var SourceFileName = PairSourceFile.Key;

				if (SourceFile.Type == FileSystemEntry.EntryType.Directory)
				{
					FoldersToExplore.AddLast(FileSystem.CombinePath(Path, SourceFileName));
				}
			}

			foreach (var PairSourceFile in SourceFiles)
			{
				CheckCanceling();

				var SourceFile = PairSourceFile.Value;
				var SourceFileName = PairSourceFile.Key;

				// New file (No contained in the Destination).
				if (!DestinationFiles.ContainsKey(SourceFileName))
				{
					// Add New Files.
					if (_SynchronizationMode.HasFlag(SynchronizationMode.CopyNewFiles))
					{
						FilesToUpdate.AddLast(SourceFile);
					}
				}
				// Existant file (Contained in the Destination)
				else
				{
					FileSystemEntry DestinationFile = DestinationFiles[SourceFileName];

					bool AreEquals = !SourceFile.SpecialFlags.HasFlag(FileSystemEntry.SpecialFlagsTypes.SynchronizeAlways);

					// Check old files for updated.
					if (_SynchronizationMode.HasFlag(SynchronizationMode.UpdateOldFiles))
					{
						if (_ReferenceMode.HasFlag(ReferenceMode.Size) && (SourceFile.Size != DestinationFile.Size))
						{
							AreEquals = false;
						}

						if (_ReferenceMode.HasFlag(ReferenceMode.LastWriteTime) && (SourceFile.Time.LastWriteTime != DestinationFile.Time.LastWriteTime))
						{
							//Console.WriteLine(SourceFile + ":  " + SourceFile.Time.LastWriteTime + " != " + DestinationFile.Time.LastWriteTime);
							AreEquals = false;
						}
					}

					if (!AreEquals)
					{
						FilesToUpdate.AddLast(SourceFile);
					}
				}
			}

			// Delete Old Files.
			if (_SynchronizationMode.HasFlag(SynchronizationMode.DeleteOldFiles))
			{
				throw (new NotImplementedException());
				/*
				foreach (var PairDestinationFile in DestinationFiles)
				{
					CheckCanceling();

					var DestinationFile = PairDestinationFile.Value;
					var DestinationFileName = PairDestinationFile.Key;

					// File was deleted (No contained in the Source).
					if (!SourceFiles.ContainsKey(DestinationFileName))
					{
						FilesToRemove.AddLast(DestinationFile);
					}
				}
				*/
			}

			int step = 0;
			int steps = FilesToUpdate.Count + FilesToRemove.Count + FoldersToExplore.Count;

			foreach (var FileToUpdate in FilesToUpdate)
			{
				String FileToUpdatePathFileName = FileSystem.CombinePath(Path, FileToUpdate.Name);

				CheckCanceling();
				CallStep(GetStep(StepFrom, StepTo, step++, steps), String.Format(UpdatingFormat, FileToUpdatePathFileName));

				if (FileToUpdate.Type == FileSystemEntry.EntryType.Directory)
				{
					Console.WriteLine("Directory: " + FileToUpdatePathFileName);
					CreateFolder(FileToUpdatePathFileName);
				}
				else
				{
					CopyFile(FileToUpdatePathFileName);
				}
			}


			foreach (var FileToRemove in FilesToRemove)
			{
				String FileToRemovePathFileName = FileSystem.CombinePath(Path, FileToRemove.Name);

				CheckCanceling();
				CallStep(GetStep(StepFrom, StepTo, step++, steps), String.Format(RemovingFormat, FileToRemovePathFileName));

				RemoveFile(FileToRemovePathFileName);
			}

			foreach (var FolderName in FoldersToExplore)
			{
				CheckCanceling();

				_SynchronizeFolder(
					FolderName,
					GetStep(StepFrom, StepTo, step + 0, steps),
					GetStep(StepFrom, StepTo, step + 1, steps)
				);

				step++;
			}
		}

		protected void CreateFolder(String PathFileName)
		{
			try
			{
				DestinationFileSystem.CreateDirectory(FileSystem.CombinePath(DestinationPath, PathFileName));
			}
			catch (Exception Exception)
			{
				Console.WriteLine("Error creating folder '{0}' : {1}", FileSystem.CombinePath(DestinationPath, PathFileName), Exception.Message);
			}
		}

		protected void RemoveFile(String PathFileName)
		{
			try
			{
				DestinationFileSystem.DeleteFile(FileSystem.CombinePath(DestinationPath, PathFileName));
			}
			catch (Exception Exception)
			{
				Console.WriteLine("Error deleting file '{0}' : {1}", FileSystem.CombinePath(DestinationPath, PathFileName), Exception.Message);
			}
		}

		protected void CopyFile(String PathFileName)
		{
			using (var SourceStream = SourceFileSystem.OpenFile(FileSystem.CombinePath(DestinationPath, PathFileName), FileMode.Open))
			using (var DestinationStream = DestinationFileSystem.OpenFile(FileSystem.CombinePath(DestinationPath, PathFileName), FileMode.Create))
			{
				SourceStream.CopyToFast(DestinationStream);
			}
		}

		public void ShowProgressForm(Action Start = null, Action Complete = null)
		{
			var ProgressForm = new ProgressForm();
			ProgressForm.WaitObject = this.IsFinishedLockObject;

			this.OnStep += delegate(double Value, String Details)
			{
				ProgressForm.SetStep(Value, Details);
			};

			ProgressForm.SetOriginDestination(
				SourceFileSystem.Title,
				DestinationFileSystem.Title
			);

			this.OnError += delegate(Exception Exception)
			{
				if (!Canceling)
				{
					MessageBox.Show(Exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				}
			};

			ProgressForm.OnCancelClick = delegate()
			{
				return (MessageBox.Show("¿Está seguro de querer cancelar la sincronización?", "Atención", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes);
			};

			if (Complete != null)
			{
				ProgressForm.Complete += Complete;
			}

			ProgressForm.Process = delegate()
			{
				RetrySource:

				try
				{
					CallStep(0, String.Format(ConnectingToFormat, SourceFileSystem.Title));
					SourceFileSystem.TryInitialize();
				}
				catch (Exception Exception)
				{
					Console.WriteLine(Exception);
					if (!Canceling)
					{
						if (MessageBox.Show(Exception.Message + "\n\n" + Exception.StackTrace, "Can't connect to local FileSystem", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2) == DialogResult.Retry)
						{
							goto RetrySource;
						}
					}
				}
			
				RetryDestination:

				try
				{
					CallStep(0, String.Format(ConnectingToFormat, DestinationFileSystem.Title));
					DestinationFileSystem.TryInitialize();
				}
				catch (Exception Exception)
				{
					Console.WriteLine(Exception);
					if (!Canceling)
					{
						if (MessageBox.Show(Exception.Message + "\n\n" + Exception.StackTrace, "Can't connect to remote FileSystem", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2) == DialogResult.Retry)
						{
							goto RetryDestination;
						}
					}
				}

				Console.WriteLine("Started SynchronizeFolder ({0} -> {1})...", SourceFileSystem, DestinationFileSystem);
				this.SynchronizeFolder();
			};

			ProgressForm.Cancel = this.Cancel;

			if (Start != null)
			{
				Start();
			}

			//ProgressForm.ShowDialog();
			ProgressForm.ExecuteProcess();
		}
	}
}
