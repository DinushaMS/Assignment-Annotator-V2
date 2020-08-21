using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDF_Annotation
{
    public static class Annotator
    {
        public static PictureBox pbPage;
        static bool ctScore = false;
        public static int pageIndex = 0;
        static double xf = 0;
        static double yf = 0;
        static char annotationType = 'C';
        static string score = "1";
        public static List<Pages> pages;
        public static List<Annotation> ticks = new List<Annotation>();
        public static bool isWelcomePage = true;

        //bool placingFinal = false;
        static Point hover = new Point(0, 0);
        static Font drawFont = new Font("Arial", 14);
        static Font drawFontL = new Font("Arial", 24);
        static Font textFont = new Font("Arial", 14);
        static SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Red);
        static SolidBrush drawBrush_selected = new SolidBrush(System.Drawing.Color.Blue);
        public static bool isSaved = true;
        static System.Drawing.Image imageFile;
        public static Image correctAnnotation;
        public static Image incorrectAnnotation;
        static Image correctAnnotation_selected;
        static Image incorrectAnnotation_selected;
        static Form parent;
        static ContextMenuStrip cms;
        private static ToolStripMenuItem tsmiPrev = new ToolStripMenuItem();
        private static ToolStripMenuItem tsmiNext = new ToolStripMenuItem();
        private static ToolStripMenuItem tsmiRelocateFinalScore = new ToolStripMenuItem();
        private static ToolStripMenuItem tsmiAnnotateCorrect = new ToolStripMenuItem();
        private static ToolStripMenuItem tsmiAnnotateIncorrect = new ToolStripMenuItem();
        private static ToolStripMenuItem tsmiAnnotateComment = new ToolStripMenuItem();
        private static ToolStripMenuItem tsmiSelectAnnotations = new ToolStripMenuItem();
        private static ToolStripMenuItem tsmiRemoveSelected = new ToolStripMenuItem();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pbPage">
        /// Pass in picture box</param>
        /// <param name="images">
        /// [{correct},{incorrect}{correct_selected}{incorrect_selected}]</param>
        public static void Initiate(Form _parent, PictureBox _pbPage, Image[] images)
        {
            parent = _parent;
            pbPage = _pbPage;
            correctAnnotation = images[0];
            incorrectAnnotation = images[1];
            correctAnnotation_selected = images[2];
            incorrectAnnotation_selected = images[3];
            PopulateCMS();
            pbPage.ContextMenuStrip = cms;
            parent.KeyDown += new KeyEventHandler(parent_KeyDown);
            pbPage.Paint += new PaintEventHandler(pbImage_Paint);
            pbPage.MouseMove += new MouseEventHandler(pbPage_MouseMove);
            pbPage.MouseDown += new MouseEventHandler(pbPage_MouseDown);
        }

        public static void ReleseControls()
        {
            pbPage.Dispose();
        }

        static void PopulateCMS()
        {
            cms = new ContextMenuStrip();
            ToolStripMenuItem[] tsmItems = new ToolStripMenuItem[] {
                tsmiPrev,
                tsmiNext,
                tsmiRelocateFinalScore,
                tsmiAnnotateCorrect,
                tsmiAnnotateIncorrect,
                tsmiAnnotateComment,
                tsmiSelectAnnotations,
                tsmiRemoveSelected
            };
            string[] itemNames = {"Prev",
                "Next",
                "Relocate Final Score",
                "Annotate Correct",
                "Annotate Incorrect",
                "Annotate Comment",
                "Select Annotations",
                "Remove Selected"};
            // 
            // contextMenuStrip1
            // 
            cms.Items.AddRange(tsmItems);
            cms.Name = "Annotation_options";
            cms.AutoSize = true;
            //cms.Size = new Size(181, 48);
            // 
            // helloToolStripMenuItem
            // 
            EventHandler[] evhanld = new EventHandler[]
            {
               Prev,
               Next,
               RelocateFinalScore,
               AnnotateCorrect,
               AnnotateIncorrect,
               AnnotateComment,
               SelectAnnotations,
               RemoveSelected
            };
            for (int i = 0; i < tsmItems.Length; i++)
            {
                tsmItems[i].Name = "tsmi_"+itemNames[i];
                tsmItems[i].Size = new Size(180, 22);
                tsmItems[i].Text = itemNames[i];
                tsmItems[i].Click += evhanld[i];
            }
        }

        private static void RemoveSelected(object sender, EventArgs e)
        {
            DeteleTicks();
        }

        private static void SelectAnnotations(object sender, EventArgs e)
        {
            annotationType = 'S';
        }

        private static void AnnotateComment(object sender, EventArgs e)
        {
            annotationType = 'T';
        }

        private static void AnnotateIncorrect(object sender, EventArgs e)
        {
            score = "0";
            annotationType = 'I';
        }

        private static void AnnotateCorrect(object sender, EventArgs e)
        {
            annotationType = 'C';
        }

        private static void RelocateFinalScore(object sender, EventArgs e)
        {
            annotationType = 'M';
        }

        private static void Next(object sender, EventArgs e)
        {
            if (pageIndex + 1 <= Data.pageCount - 1)
            {
                pageIndex += 1;
                Draw();
            }
        }

        private static void Prev(object sender, EventArgs e)
        {
            if (pageIndex - 1 >= 0)
            {
                pageIndex -= 1;
                Draw();
            }
        }

        private static void pbPage_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Data.OriginalImages != null && !isWelcomePage)
            {
                xf = e.X / (double)Data.OriginalImages[pageIndex].Width;
                yf = e.Y / (double)Data.OriginalImages[pageIndex].Height;
                if (annotationType == 'S')
                {
                    for (int i = 0; i < ticks.Count; i++)
                    {
                        int x = (int)(ticks[i].x * Data.OriginalImages[pageIndex].Width);
                        int y = (int)(ticks[i].y * Data.OriginalImages[pageIndex].Height);
                        if (e.X > x && e.X < x + PDF.iHeight / 2)
                            if (e.Y > y && e.Y < y + PDF.iHeight / 2)
                            {
                                if (!ticks[i].isSelceted)
                                {
                                    ticks[i].isSelceted = true;
                                }
                                else
                                {
                                    ticks[i].isSelceted = false;
                                }
                                break;
                            }
                    }
                }
                else if (annotationType == 'T')
                {
                    //Create an instance of your dialog form
                    CommentForm testDialog = new CommentForm();
                    testDialog.Location = new Point(e.X, e.Y + testDialog.Height / 2);
                    testDialog.StartPosition = FormStartPosition.Manual;

                    // Show testDialog as a modal dialog and determine if DialogResult = OK.
                    if (testDialog.ShowDialog(parent) == DialogResult.OK)
                    {
                        AddTick();
                    }
                }
                else if (annotationType == 'M')
                {
                    PDF.fScoreOrigin_x = xf;
                    PDF.fScoreOrigin_y = yf;
                }
                else
                {
                    AddTick();
                }
                Draw();
            }
        }

        public static void Scroll(int value)
        {
            pbPage.Location = new Point(pbPage.Location.X, -1 * value);
            Draw();
        }

        private static void DeteleTicks()
        {
            ticks = ticks.Where(c => !c.isSelceted).ToList<Annotation>();
            if (pages != null)
            {
                pages[pageIndex].tick = ticks.ToArray();
            }
            CalculateFinalScore();
            Draw();
        }

        private static void CalculateFinalScore()
        {
            Data.finalScore = 0;
            for (int i = 1; i <= Data.pageCount; i++)
            {
                if (pages[i - 1].tick != null)
                {
                    foreach (var item in pages[i - 1].tick)
                    {
                        if (item.text != "" && item.type != 'T')
                        {
                            double val = 0;
                            if (Double.TryParse(item.text, out val))
                            {
                                Data.finalScore += val;
                            }
                            else
                            {
                                MessageBox.Show("Marks must be a valid number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ticks.RemoveAt(ticks.Count - 1);
                            }
                        }
                    }
                }
            }
            if (Data.finalScore > Data.maxScore)
                MessageBox.Show("Total score is over the maximum value!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }

        private static void pbImage_Paint(object sender, PaintEventArgs e)
        {
            if (!isWelcomePage)
            {
                if (annotationType == 'C')
                    e.Graphics.DrawImage(correctAnnotation, hover.X, hover.Y - PDF.iHeight / 2, PDF.iWidth, PDF.iHeight);
                else if (annotationType == 'I')
                    e.Graphics.DrawImage(incorrectAnnotation, hover.X, hover.Y - PDF.iHeight / 2, PDF.iWidth, PDF.iHeight);
                else if (annotationType == 'M')
                {
                    e.Graphics.DrawString($"{Data.finalScore}", drawFontL, drawBrush, hover.X, hover.Y);
                    e.Graphics.DrawString($"------", drawFontL, drawBrush, hover.X, hover.Y + 16);
                    e.Graphics.DrawString($"{Data.maxScore}", drawFontL, drawBrush, hover.X, hover.Y + 40);
                }

                if ((annotationType == 'C' || annotationType == 'I') && score != "")
                    e.Graphics.DrawString($"({score})", drawFontL, drawBrush, hover.X + PDF.iWidth, hover.Y);
            }
        }

        private static void pbPage_MouseMove(object sender, MouseEventArgs e)
        {
            hover.X = e.X;
            hover.Y = e.Y;
            if (annotationType == 'S')
                pbPage.Cursor = Cursors.Hand;
            else if (annotationType == 'T')
                pbPage.Cursor = Cursors.IBeam;
            else if (annotationType == 'C' || annotationType == 'I')
                pbPage.Cursor = Cursors.Cross;
            else
                pbPage.Cursor = Cursors.Default;
            pbPage.Refresh();
        }

        public static void Draw()
        {
            int x = 0, y = 0;

            if (true)//paintEnabled && Data.OriginalImages != null)
            {
                // Create image.
                //imageFile = System.Drawing.Image.FromFile($"{Data.sourceImageDirpath}\\page{pageIndex + 1}.Jpeg");//Data.OriginalImages[pageIndex];//
                imageFile = Data.OriginalImages[pageIndex];
                // Create graphics object for alteration.
                Graphics newGraphics = Graphics.FromImage(imageFile);
                foreach (var item in ticks)
                {
                    x = (int)(item.x * Data.OriginalImages[pageIndex].Width);
                    y = (int)(item.y * Data.OriginalImages[pageIndex].Height - PDF.iHeight / 2);
                    if (!item.isSelceted)
                    {
                        if (item.type == 'C')
                            newGraphics.DrawImage(correctAnnotation, x, y, PDF.iWidth, PDF.iHeight);
                        else if (item.type == 'I')
                            newGraphics.DrawImage(incorrectAnnotation, x, y, PDF.iWidth, PDF.iHeight);
                        else
                            newGraphics.DrawString(item.text, textFont, drawBrush, x, y + PDF.iHeight / 2);
                        if ((item.type == 'C' || item.type == 'I') && item.text != "")
                            newGraphics.DrawString($"({item.text})", drawFont, drawBrush, x + PDF.iWidth, y + PDF.iHeight / 2);
                    }
                    else//if selected
                    {
                        if (item.type == 'C')
                            newGraphics.DrawImage(correctAnnotation_selected, x, y, PDF.iWidth, PDF.iHeight);
                        else if (item.type == 'I')
                            newGraphics.DrawImage(incorrectAnnotation_selected, x, y, PDF.iWidth, PDF.iHeight);
                        else
                            newGraphics.DrawString(item.text, textFont, drawBrush_selected, x, y + PDF.iHeight / 2);
                        if ((item.type == 'C' || item.type == 'I') && item.text != "")
                            newGraphics.DrawString($"({item.text})", drawFont, drawBrush_selected, x + PDF.iWidth, y + PDF.iHeight / 2);
                    }
                }

                if (pageIndex == 0)
                {
                    x = (int)(PDF.fScoreOrigin_x * Data.OriginalImages[pageIndex].Width);
                    y = (int)(PDF.fScoreOrigin_y * Data.OriginalImages[pageIndex].Height);
                    newGraphics.DrawString($"{Data.finalScore}", drawFont, drawBrush, x, y);
                    newGraphics.DrawString($"------", drawFont, drawBrush, x, y + 16);
                    newGraphics.DrawString($"{Data.maxScore}", drawFont, drawBrush, x, y + 40);
                }
                //Data.AnnotatedImages[pageIndex] = (Bitmap)imageFile;
                pbPage.Image = (Bitmap)imageFile;
                newGraphics.Dispose();
                //pbPage.Invalidate();
                pbPage.Refresh();
            }
        }

            private static void parent_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show($"{e.KeyValue},{e.KeyCode},{e.KeyData}");
            int n = e.KeyValue - 48;
            if (!ctScore)
            {
                score = "";
                ctScore = true;
            }
            if (n > -1 && n < 10)
                score += n.ToString();
            if (score.Length > 2)
                score = "";
            
            if (e.KeyCode == Keys.Delete)
            {
                DeteleTicks();
            }
            else if (e.KeyCode == Keys.S)
            {
                annotationType = 'S';
            }
            else if (e.KeyCode == Keys.T)
            {
                annotationType = 'T';
            }
            else if (e.KeyCode == Keys.C)
            {
                annotationType = 'C';
            }
            else if (e.KeyCode == Keys.W)
            {
                score = "0";
                annotationType = 'I';
            }
            pbPage.Refresh();
        }

        private static void AddTick()
        {
            Annotation tick = new Annotation();
            tick.type = annotationType;
            if (annotationType == 'T')
                tick.text = Data.Comment;
            else
            {
                tick.text = score;
                ctScore = false;
            }

            tick.x = xf;
            tick.y = yf;
            tick.isSelceted = false;
            ticks.Add(tick);
            if (pages != null)
            {
                pages[pageIndex].tick = ticks.ToArray();
            }
            CalculateFinalScore();
        }
    }
}
