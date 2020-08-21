using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.Drawing.Imaging;
using System.IO;
using O2S.Components.PDFRender4NET;
using iText.IO.Util;
using System.Reflection;

namespace PDF_Annotation
{
    public class PDF
    {
        static PdfDocument pdfDoc;
        public static readonly int iWidth = 70;
        public static readonly int iHeight = 50;
        static int fontSize = 14;
        public static double fScoreOrigin_x = 0;
        public static double fScoreOrigin_y = 0;
        public static bool Export()
        {
            Pages[] pages = Annotator.pages.ToArray();
            pdfDoc = new PdfDocument(new PdfReader(Data.sourcePDFpath), new PdfWriter(Data.savePDFpath));
            Document document = new Document(pdfDoc);

            // Load image from disk
            string imgC = "Stamps\\correct.png";
            string imgW = "Stamps\\wrong.png";
            ImageData correctSign = ImageDataFactory.Create(imgC);
            ImageData wrongSign = ImageDataFactory.Create(imgW);
            //Create layout image object and provide parameters. Page number = 1
            for (int i = 1; i <= Data.pageCount; i++)
            {
                iText.Kernel.Geom.Rectangle mediabox = pdfDoc.GetPage(i).GetPageSize();
                int actualWidth = (int)mediabox.GetWidth();
                int actualHeight = (int)mediabox.GetHeight();
                int img_w;
                int img_h;
                int px, py;
                if (pages[i - 1].tick != null)
                {
                    img_w = (iWidth * actualWidth) / Data.OriginalImages[i - 1].Width;
                    img_h = (iHeight * actualHeight) / Data.OriginalImages[i - 1].Height;
                    foreach (var item in pages[i - 1].tick)
                    {
                        px = Convert.ToInt32(item.x * actualWidth);
                        py = actualHeight - Convert.ToInt32(item.y * actualHeight) - img_h / 2;
                        iText.Layout.Element.Image image;
                        int numLines = item.text.Split('\n').Length * 14;
                        Text scoreText = new Text($"({item.text})").SetFontColor(ColorConstants.RED).SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)).SetFontSize(fontSize);
                        Text commentText = new Text($"{item.text}").SetFontColor(ColorConstants.RED).SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA)).SetFontSize(fontSize);
                        if (item.type == 'C')
                            image = new iText.Layout.Element.Image(correctSign).ScaleAbsolute(img_w, img_h).SetFixedPosition(i, px, py);
                        else if (item.type == 'I')
                            image = new iText.Layout.Element.Image(wrongSign).ScaleAbsolute(img_w, img_h).SetFixedPosition(i, px, py);
                        else
                            image = null;
                        // This adds the image to the page
                        if (image != null)
                        {
                            document.Add(image);
                            if (item.text != "")
                            {
                                Paragraph p1 = new Paragraph(scoreText).SetFixedPosition(i, px + img_w, py, 50).SetFontSize(fontSize);
                                document.Add(p1);
                            }
                        }
                        else
                        {
                            Paragraph p1 = new Paragraph(commentText).SetFixedPosition(i, px, py - numLines, 200).SetFontSize(fontSize);
                            p1.SetFixedLeading(Convert.ToInt32(fontSize * 1.2));
                            document.Add(p1);
                        }

                    }
                }
                if (i == 1)
                {
                    px = Convert.ToInt32(fScoreOrigin_x * actualWidth) + 10;
                    py = actualHeight - Convert.ToInt32((fScoreOrigin_y * actualHeight) + (fontSize * 2.5));

                    Text finalText = new Text($"{Data.finalScore}\n-----\n{Data.maxScore}").SetFontColor(ColorConstants.RED).SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA));
                    Paragraph p2 = new Paragraph(finalText).SetFixedPosition(1, px, py, 50).SetFontSize(fontSize);
                    p2.SetFixedLeading(Convert.ToInt32((fontSize * 10) / 12));
                    document.Add(p2);
                }
            }

            document.Close();
            MessageBox.Show("File saved successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        public static bool ToImage(string pdfInputPath, string imageOutputPath, string imageName, ImageFormat imageFormat)
        {
            PDFFile pdfFile = PDFFile.Open(pdfInputPath);
            Data.pageCount = pdfFile.PageCount;
            int startPageNum = 1;
            int endPageNum = Data.pageCount;
            Console.WriteLine(imageOutputPath);

            if (!Directory.Exists(Data.sourceImageDirpath))
            {
                Directory.CreateDirectory(Data.sourceImageDirpath);
            }
            else
            {
                Directory.Delete(Data.sourceImageDirpath, true);
                Directory.CreateDirectory(Data.sourceImageDirpath);
            }
            // validate pageNum
            if (startPageNum <= 0)
            {
                startPageNum = 1;
            }

            if (endPageNum > pdfFile.PageCount)
            {
                endPageNum = pdfFile.PageCount;
            }

            if (startPageNum > endPageNum)
            {
                startPageNum = endPageNum;
                endPageNum = startPageNum;
            }

            Data.OriginalImages = new List<Bitmap>();
            // start to convert each page
            for (int i = startPageNum; i <= endPageNum; i++)
            {
                float pageWidth = (float)(pdfFile.GetPageSize(i - 1).Width / 72);
                //float pageHeight = (float)(pdfFile.GetPageSize(i - 1).Height / 72);
                float resolution = Data.panelWidth / pageWidth;
                //Bitmap pageImage = pdfFile.GetPageImage(i - 1, 56 * (int)definition);
                Bitmap pageImage = pdfFile.GetPageImage(i - 1, resolution);
                //if (!Directory.Exists(imageOutputPath))
                //    Directory.CreateDirectory(imageOutputPath + "\\images");
                pageImage.Save(imageOutputPath + "\\" + imageName + i.ToString() + "." + imageFormat.ToString(), imageFormat);
                string imgPath = imageOutputPath + "\\" + imageName + i.ToString() + "." + imageFormat.ToString();
                Data.OriginalImages.Add(new Bitmap(GetCopyImage(imgPath)));
                File.Delete(imgPath);

                //Console.WriteLine($"page {i} converted, width:{Data.OriginalImages[i - 1].Width} height:{Data.OriginalImages[i - 1].Height}");
            }

            pdfFile.Dispose();
            //Data.OriginalImages.Clear();
            //File.Delete(imageOutputPath + "\\" + imageName + "1." + imageFormat.ToString());
            return true;
        }

        private static System.Drawing.Image GetCopyImage(string path)
        {
            using (var im = System.Drawing.Image.FromFile(path))
            {
                Bitmap bm = new Bitmap(im);
                return bm;
            }
        }
    }
}
