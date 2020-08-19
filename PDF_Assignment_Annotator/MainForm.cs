using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using PDF_Annotation;
using SharpUpdate;

namespace PDF_Assignment_Annotator
{    
    public partial class MainForm : Form
    {
        private SharpUpdater updater;
        AboutForm aboutForm;
        public MainForm()
        {
            InitializeComponent();
            Image[] images = new Image[] {
                Properties.Resources.correct,
                Properties.Resources.wrong,
                Properties.Resources.correct_selected,
                Properties.Resources.wrong_selected };
            Annotator.Initiate(this, pictureBox1, images);
            updater = new SharpUpdater(Assembly.GetExecutingAssembly(), this, new Uri("https://raw.githubusercontent.com/DinushaMS/Assignment-Annotator-V2/master/PDF_Assignment_Annotator/bin/Release/update.xml"));
            //this.Text = ProductName + " - " + ProductVersion;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            RunOpernFileDialog();                
        }

        private void RunOpernFileDialog()
        {
            Annotator.isSaved = false;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PDF Files | *.pdf";
            ofd.DefaultExt = "pdf";
            DialogResult response = ofd.ShowDialog();
            Data.fileName = Path.GetFileName(ofd.FileName);
            if (response == DialogResult.OK)
            {
                OpenFile(ofd.FileName);
                Annotator.isWelcomePage = false;
            }
        }

        private void OpenFile(string filePath)
        {
            Data.panelWidth = panel1.Width;
            Data.sourcePDFpath = filePath;
            Data.sourceImageDirpath = ".//Images";
            //this.Text = $"PDF Assignment Grader - {DATA.fileName}*";
            Console.WriteLine(filePath);
            Console.WriteLine(Path.GetDirectoryName(filePath));
            try
            {
                Annotator.ticks = new List<Annotation>();
                Annotator.pages = new List<Pages>();
                Data.OriginalImages = new List<Bitmap>();
                PDF.ToImage(Data.sourcePDFpath, Data.sourceImageDirpath, "page", ImageFormat.Jpeg);
                
                Data.finalScore = 0;
                for (int i = 0; i < Data.pageCount; i++)
                {
                    Pages p = new Pages();
                    Annotator.pages.Add(p);
                }
                ViewPage();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {

            }
        }
        private void ViewPage()
        {
            vScrollBar1.Maximum = Data.OriginalImages[Annotator.pageIndex].Height - panel1.Height;
            //txtPage.Text = $"{Annotator.pageIndex + 1} of {Data.pageCount}";
            Annotator.pbPage.Location = new Point(0, 0);
            vScrollBar1.Value = 0;
            Annotator.ticks.Clear();
            if (Annotator.pages[Annotator.pageIndex].tick != null)
                Annotator.ticks.AddRange(Annotator.pages[Annotator.pageIndex].tick);
            Annotator.Draw();
        }

        private void pbPage_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 30;
            Console.WriteLine($"Hit{numberOfTextLinesToMove}");
            if (vScrollBar1.Value - numberOfTextLinesToMove >= 0 && vScrollBar1.Value - numberOfTextLinesToMove <= vScrollBar1.Maximum)
            {
                vScrollBar1.Value -= numberOfTextLinesToMove;
                Annotator.Scroll(vScrollBar1.Value);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Data.fileName = Path.GetFileName(sfd.FileName);
                Data.savePDFpath = sfd.FileName;
                PDF.Export();
                //this.Text = $"PDF Assignment Grader - {Path.GetFileName(DATA.fileName)}";
            }
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            Annotator.Scroll(vScrollBar1.Value);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string WelcomeFilePath = "Stamps\\Original.pdf";
            if (File.Exists(WelcomeFilePath))
            {
                OpenFile(WelcomeFilePath);
            }
        }

        private void btnCheckForUpdates_Click(object sender, EventArgs e)
        {
            updater.DoUpdate();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            if (aboutForm == null)
            {
                aboutForm = new AboutForm();
                aboutForm.Show();
            }
            else
                aboutForm.Activate();
            
        }
    }
}
