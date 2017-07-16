using System;
using System.Threading;
using System.Windows.Forms;

namespace CSharpUtils.Forms
{
	/// <summary>
	/// http://www.albahari.com/threading/part2.aspx#_Rich_Client_Applications
	/// </summary>
	public partial class ProgressForm : Form
	{
		public Action Process, Cancel;
		public Func<bool> OnCancelClick;
		public event Action Complete;
		bool Cancelled = false;
		public object WaitObject = null;

		public ProgressForm()
		{
			InitializeComponent();

			this.OnCancelClick = () => {
				return true;
			};

			Cancel += delegate()
			{
				Cancelled = true;
			};
			Complete += new Action(ProgressForm_Ended);
		}

		void ProgressForm_Ended()
		{
			if (WaitObject != null)
			{
				lock (WaitObject)
				{
					Monitor.Wait(WaitObject);
				}
			}

			for (int n = 0; n < 2; n++ )
			{
				if (!Visible) return;
				try
				{
					BeginInvoke(new Action(delegate()
					{
						/*
						try
						{
						}
						catch
						{
						}
						*/
						//DialogResult property to be set to DialogResult.Cancel
						DialogResult = Cancelled ? DialogResult.Cancel : DialogResult.OK;
						Close();
					}));
				}
				catch
				{
				}
				Thread.Sleep(20);
			}
		}

		public void ExecuteProcess()
		{
			if (Process != null)
			{
				(new Thread(delegate()
				{
					Process();
					if (Complete != null) Complete();
				})).Start();
			}
			//Csono
			ShowDialog();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (this.OnCancelClick())
			{
				buttonCancel.Text = "Closing...";
				buttonCancel.Enabled = false;

				if (Cancel != null) Cancel();
			}
		}

		public void SetOriginDestination(String Origin, String Destination)
		{
			labelOrigin.Text = Origin;
			labelDestination.Text = Destination;
		}

		public void SetStep(double Value, String Details)
		{
			try
			{
				this.progressBar1.Invoke(new Action(delegate()
				{
					try
					{
						this.labelAction.Text = Details;
						this.progressBar1.Minimum = 0;
						this.progressBar1.Maximum = 10000;
						this.progressBar1.Value = (int)(Value * 10000);
					}
					catch (Exception e) {
						Console.Error.WriteLine("Waring: " + e);
					}
				}));
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Waring: " + e);
			}
		}
	}
}
