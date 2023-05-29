using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ2.Models
{
    public class LineEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public bool isUnderground { get; set; }
        public float r { get; set; }
        public string conductorMaterial { get; set; }
        public string lineType { get; set; }
        public long thermalConstantHeat { get; set; }
        public long firstEnd { get; set; }
        public long secondEnd { get; set; }
        public List<Point> points { get; set; }

        public LineEntity()
        {
            points = new List<Point>();
        }

        public LineEntity(long id, string name, bool isUnderground, float r, string conductorMaterial, string lineType, long thermalConstantHeat, long firstEnd, long secondEnd)
        {
            this.id = id;
            this.name = name;
            this.isUnderground = isUnderground;
            this.r = r;
            this.conductorMaterial = conductorMaterial;
            this.lineType = lineType;
            this.thermalConstantHeat = thermalConstantHeat;
            this.firstEnd = firstEnd;
            this.secondEnd = secondEnd;
            points = new List<Point>();
        }
    }
}
