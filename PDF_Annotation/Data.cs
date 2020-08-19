﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_Annotation
{
    public class Data
    {
        public static string fileName { get; set; }
        public static int panelWidth { get; set; }
        public static int pageCount { get; set; }
        public static string sourcePDFpath { get; set; }
        public static string sourceImageDirpath { get; set; }
        public static string savePDFpath { get; set; }

        public static List<Bitmap> OriginalImages { get; set; }

        public static List<int> scrollMax { get; set; }
        public static string Comment { get; set; }

        public static double finalScore = 0;
        public static int maxScore = 100;
    }
}
