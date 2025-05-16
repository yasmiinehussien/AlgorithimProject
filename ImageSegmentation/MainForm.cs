using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Threading.Tasks;

namespace ImageTemplate
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        string filePath = @"G:\Algorithim\[TEMPLATE] ImageSegmentation\ImageSegmentation\OurOutput.txt";
        public void write_in_file(List<int> comps)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($" {comps.Count}");
                    writer.WriteLine();

                    foreach (int x in comps)
                    {
                        writer.WriteLine($" {x}");
                    }

                    Console.WriteLine($"Segmentation report saved to: {filePath}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error writing report: {e.Message}");
            }
        }





        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private async void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value;

            Stopwatch timer = Stopwatch.StartNew();

            RGBPixel[,] resultImage = null;
            List<int> compsCopy = null;

            try
            {
                await Task.Run(() =>
                {
                    ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
                    ImageOperations.get_regions(35000, ImageMatrix);
                    resultImage = ImageOperations.Visulaiztaion(ImageMatrix);
                    compsCopy = new List<int>(ImageOperations.comps);
                });
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("?? Out of Memory!\nTry using a smaller image or lower value of k.", "Memory Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            timer.Stop();
            long time = timer.ElapsedMilliseconds;

            // Save results to file
            write_in_file(compsCopy);

            // Display final image on pictureBox2
            ImageOperations.DisplayImage(resultImage, pictureBox2);

            MessageBox.Show($"Execution Time: {time} ms", "Processing Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



    }
}