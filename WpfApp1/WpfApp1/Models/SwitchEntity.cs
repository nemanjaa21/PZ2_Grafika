using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PZ2.Models
{
    public class SwitchEntity : PowerEntity
    {
        public string status { get; set; }

        public SwitchEntity()
        {
            boja = Brushes.Blue;
        }
    }
}
