using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirBnB
{
    public static class ControlExtensions
    {
        public static Task InvokeAsyncCore(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                return Task.Run(() => control.Invoke(action));
            }
            else
            {
                action();
                return Task.CompletedTask;
            }
        }
    }
}