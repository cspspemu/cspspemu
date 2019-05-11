using System;
using System.Threading;
using System.Windows.Forms;

namespace CSharpPlatform.UI
{
    public class Dialog
    {
        public enum Result
        {
            Yes = 0,
            No = 1,
            Back = 2
        }

        public enum Type
        {
            Message = 0,
            Error = 1,
        }

        public static void ShowDialog(Action<Result> Done, string Message, Type Type)
        {
            new Thread(() =>
            {
                var Value = MessageBox.Show(Message, "PSP", MessageBoxButtons.YesNo,
                    (Type == Dialog.Type.Error) ? MessageBoxIcon.Error : MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                Done((Value == DialogResult.Yes) ? Result.Yes : Result.No);
            })
            {
                IsBackground = true,
            }.Start();
        }
    }
}