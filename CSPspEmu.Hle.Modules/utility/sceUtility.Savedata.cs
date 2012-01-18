using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Vfs;

namespace CSPspEmu.Hle.Modules.utility
{
	unsafe public partial class sceUtility
	{
		/// <summary>
		/// Saves or Load savedata to/from the passed structure
		/// After having called this continue calling sceUtilitySavedataGetStatus to
		/// check if the operation is completed
		/// </summary>
		/// <param name="Params">Savedata parameters</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0x50C4CD57, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUtilitySavedataInitStart(SceUtilitySavedataParam* Params)
		{
			//Params->DataBufPointer
			Params->Base.Result = 0;

			var SavePathFolder = "ms0:/PSP/SAVEDATA/" + Params->GameName + Params->SaveName;
			var SaveDataBin = SavePathFolder + "/DATA.BIN";
			var SaveIcon0 = SavePathFolder + "/ICON0.PNG";
			var SavePic1 = SavePathFolder + "/PIC1.PNG";

			Action<PspUtilitySavedataFileData, String> Save = (PspUtilitySavedataFileData PspUtilitySavedataFileData, String FileName) =>
			{
				if (PspUtilitySavedataFileData.Used)
				{
					HleState.HleIoManager.HleIoWrapper.WriteBytes(
						FileName,
						PspMemory.ReadBytes(PspUtilitySavedataFileData.BufferPointer, PspUtilitySavedataFileData.Size)
					);
				}
			};

			try
			{
				switch (Params->Mode)
				{
					case PspUtilitySavedataMode.Autoload:
					case PspUtilitySavedataMode.Load:
						try
						{
							PspMemory.WriteBytes(
								Params->DataBufPointer,
								HleState.HleIoManager.HleIoWrapper.ReadBytes(SaveDataBin)
							);

							Params->Base.Result = SceKernelErrors.ERROR_OK;
						}
						catch (IOException)
						{
							throw (new SceKernelException(SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_DATA));
						}
						catch (Exception)
						{
							throw (new SceKernelException(SceKernelErrors.ERROR_SAVEDATA_LOAD_ACCESS_ERROR));
						}
						break;
					case PspUtilitySavedataMode.Autosave:
					case PspUtilitySavedataMode.Save:
						try
						{
							HleState.HleIoManager.HleIoWrapper.Mkdir(SavePathFolder, SceMode.All);

							HleState.HleIoManager.HleIoWrapper.WriteBytes(
								SaveDataBin,
								PspMemory.ReadBytes(Params->DataBufPointer, Params->DataSize)
							);

							Save(Params->Icon0FileData, SaveIcon0);
							Save(Params->Pic1FileData, SavePic1);
							//Save(Params->SfoParam, SavePic1);
						}
						catch (Exception Exception)
						{
							Console.Error.WriteLine(Exception);
							throw (new SceKernelException(SceKernelErrors.ERROR_SAVEDATA_SAVE_ACCESS_ERROR));
						}
						break;
					default:
						Console.Error.WriteLine("sceUtilitySavedataInitStart: Unsupported mode: " + Params->Mode);
						break;
				}
				//throw(new NotImplementedException());

				Params->Base.Result = SceKernelErrors.ERROR_OK;
			}
			catch (SceKernelException SceKernelException)
			{
				Params->Base.Result = SceKernelException.SceKernelError;
			}
			finally
			{
				CurrentDialogStep = DialogStepEnum.SUCCESS;
			}

			
			return 0;
		}

		/// <summary>
		/// Shutdown the savedata utility. after calling this continue calling
		/// ::sceUtilitySavedataGetStatus to check when it has shutdown
		/// </summary>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0x9790B33C, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUtilitySavedataShutdownStart()
		{
			//throw(new NotImplementedException());
			CurrentDialogStep = DialogStepEnum.SHUTDOWN;
			return 0;
		}

		/// <summary>
		/// Refresh status of the savedata function
		/// </summary>
		/// <param name="unknown">unknown, pass 1</param>
		[HlePspFunction(NID = 0xD4B95FFB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceUtilitySavedataUpdate(int unknown)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Check the current status of the saving/loading/shutdown process
		/// Continue calling this to check current status of the process
		/// before calling this call also sceUtilitySavedataUpdate
		/// </summary>
		/// <returns>
		///		2 if the process is still being processed.
		///		3 on save/load success, then you can call sceUtilitySavedataShutdownStart.
		///		4 on complete shutdown.
		/// </returns>
		[HlePspFunction(NID = 0x8874DBE0, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public DialogStepEnum sceUtilitySavedataGetStatus()
		{
			try
			{
				return CurrentDialogStep;
			}
			finally
			{
				if (CurrentDialogStep == DialogStepEnum.SHUTDOWN) {
					CurrentDialogStep = DialogStepEnum.NONE;
				}
			}
		}
	}
}
