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

namespace ImageTemplate
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        string filePath = @"C:\Users\malak\Music\[TEMPLATE] ImageSegmentation\ImageSegmentation\OurOutput.txt";
        public void write_in_file()
        {

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($" {ImageOperations.comps.Count}");
                    writer.WriteLine(); 

                    // Write sizes of each segment, sorted in descending order
                    foreach (int x in ImageOperations.comps)
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

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);

            Stopwatch timer = Stopwatch.StartNew();

            ImageOperations.get_regions(30000, ImageMatrix);

            RGBPixel[,] ColoredImage = ImageOperations.Visulaiztaion(ImageMatrix);

            timer.Stop();
            long time = timer.ElapsedMilliseconds;

            write_in_file();
            ImageOperations.DisplayImage(ColoredImage, pictureBox2);
            MessageBox.Show($"Execution Time: {time} ms", "Processing Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


    }
}