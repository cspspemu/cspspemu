using System;

namespace CSPspEmu.Core
{
	public abstract class PspPluginImpl
	{
		/// <summary>
		/// 
		/// </summary>
		public abstract PluginInfo PluginInfo { get; }

		/// <summary>
		/// 
		/// </summary>
		public abstract bool IsWorking { get; }

		//public abstract void Start();

		/// <summary>
		/// </summary>
		/// <typeparam name="TType"></typeparam>
		/// <param name="injectContext"></param>
		/// <param name="availablePluginImplementations"></param>
		public static void SelectWorkingPlugin<TType>(InjectContext injectContext, params Type[] availablePluginImplementations) where TType : PspPluginImpl
		{
			foreach (var implementationType in availablePluginImplementations)
			{
				bool isWorking = false;

				try
				{
					isWorking = ((PspPluginImpl)injectContext.GetInstance(implementationType)).IsWorking;
				}
				catch (Exception exception)
				{
					Console.Error.WriteLine(exception);
				}

				if (isWorking)
				{
					// Found a working implementation
					injectContext.SetInstanceType<TType>(implementationType);
					return;
				}
			}

			throw (new Exception("Can't find working type for '" + availablePluginImplementations + "'"));
			//return null;
		}
	}
}
