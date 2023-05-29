using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PZ2.Models
{
    public class PowerEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public string tooltip { get; set; }
        public SolidColorBrush boja { get; set; }

        public PowerEntity()
        {
        }

        public PowerEntity(long id, string name, double x, double y, string tooltip, SolidColorBrush boja)
        {
            this.id = id;
            this.name = name;
            this.x = x;
            this.y = y;
            this.tooltip = tooltip;
            this.boja = boja;
        }
    }
}
