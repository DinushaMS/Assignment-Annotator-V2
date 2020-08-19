using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Annotation
{
    /// <summary>
    /// Annotations
    /// </summary>
    public class Annotation
    {
        public double x { get; set; }
        public double y { get; set; }
        public char type { get; set; }
        public string text { get; set; }
        public bool isSelceted { get; set; }
    }
}
