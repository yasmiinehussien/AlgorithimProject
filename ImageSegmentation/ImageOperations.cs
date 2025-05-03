using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

///Algorithms Project
///Intelligent Scissors
///

namespace ImageTemplate
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }

    public class Edge
    {
        public double XEdge1, YEdge1;
        public double XEdge2, YEdge2;

        public double weight;
        public Edge(double x1, double y1, double x2, double y2, double weight)
        {
            XEdge1 = x1;
            YEdge1 = y1;
            XEdge2 = x2; 
            YEdge2 = y2;
            this.weight = weight;
        }
    }




    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }


        //static Dictionary<RGBPixel, List<RGBPixelD>> neighbours;
        //List<int> edges;

        static Dictionary<(int,int), List<(int, int)>> neighbours = new Dictionary<(int,int), List<(int, int)>>();


        /* static Dictionary<(int,int),  lIst<(int,int),Edge> > 
         * 
         * 
         * A --> (B-->dataEdge)
         * 
         * 
         * 
         */


        static Dictionary<(int, int), List<Edge>> Rededges = new Dictionary< (int, int), List<Edge> >();
        static Dictionary<(int, int), List<Edge>> Blueedges = new Dictionary<(int, int), List<Edge>>();
        static Dictionary<(int, int), List<Edge>> Greenedges = new Dictionary<(int, int), List<Edge>>();


        public static RGBPixel[,] calc_neighbours(string ImagePath)
        {
            RGBPixel[,] Imagee = OpenImage(ImagePath);
            int image_height = GetHeight(Imagee);
            int image_width = GetWidth(Imagee);


            Imagee = GaussianFilter1D(Imagee,5 , 0.8);


            for (int i = 0; i < image_height; i++) // row by row
            {

                for (int j = 0; j < image_width; j++)
                {
                    RGBPixel pixel = new RGBPixel();

                    pixel = Imagee[ i, j];
                    

                    if (!neighbours.ContainsKey((i,j)))
                        neighbours[(i,j)] = new List<(int,int)>();

                   

                    if (i == 0 && j == 0)
                    {
                        neighbours[(i,j)].Add((i, j+1 ));
                        neighbours[(i,j)].Add((i+1, j));
                        neighbours[(i,j)].Add((i + 1, j+1));

                       

                    }
                    else if (i == 0 && j == image_width-1)
                    {
                        neighbours[(i, j)].Add((i, j - 1));
                        neighbours[(i, j)].Add((i + 1, j));
                        neighbours[(i, j)].Add((i + 1, j - 1));



                    }
                    else if (i == image_height - 1 && j == 0 )
                    {
                        neighbours[(i, j)].Add((i, j + 1));
                        neighbours[(i, j)].Add((i - 1, j));
                        neighbours[(i, j)].Add((i - 1, j + 1));

                      


                    }
                    else if(i==image_height-1 && j==image_width-1)
                    {
                        neighbours[(i, j)].Add( (i, j - 1));
                        neighbours[(i, j)].Add( (i - 1, j));
                        neighbours[(i, j)].Add( (i - 1, j - 1));

                      

                    }
                    else if(i == 0)
                    {
                        neighbours[(i, j)].Add( (i, j - 1));
                        neighbours[(i, j)].Add( (i, j + 1));

                        neighbours[(i, j)].Add( (i + 1, j));

                        neighbours[(i, j)].Add( (i + 1, j - 1));
                        neighbours[(i, j)].Add( (i + 1, j + 1));


                      

                    }
                    else if (i == image_height-1)
                    {
                        neighbours[(i, j)].Add(( i, j - 1));
                        neighbours[(i, j)].Add( (i, j + 1));

                        neighbours[(i, j)].Add( (i - 1, j));

                        neighbours[(i, j)].Add( (i - 1, j - 1));
                        neighbours[(i, j)].Add(( i - 1, j + 1));



                    }
                    else if (j == 0)
                    {
                        neighbours[(i, j)].Add(( i-1, j ));
                        neighbours[(i, j)].Add( (i+1 , j ));

                        neighbours[(i, j)].Add( (i, j+1));

                        neighbours[(i, j)].Add( (i - 1, j + 1));
                        neighbours[(i, j)].Add((i + 1, j + 1));




                    }
                    else if (j == image_width-1)
                    {
                        neighbours[(i, j)].Add(( i - 1, j));
                        neighbours[(i, j)].Add( (i + 1, j));

                        neighbours[(i, j)].Add(( i, j - 1));

                        neighbours[(i, j)].Add( (i - 1, j - 1));
                        neighbours[(i, j)].Add( (i + 1, j - 1));



                    }
                    else
                    {

                        neighbours[(i, j)].Add( (i + 1, j));
                        neighbours[(i, j)].Add( (i - 1, j));

                        neighbours[(i, j)].Add( (i, j - 1));
                        neighbours[(i, j)].Add( (i , j + 1));

                        neighbours[(i, j)].Add(( i - 1, j - 1));
                        neighbours[(i, j)].Add( (i - 1, j+1));

                        neighbours[(i, j)].Add(( i + 1, j + 1));
                        neighbours[(i, j)].Add( (i+1, j - 1));



                     

                    }


                }
            }



            return Imagee;


        }

        public static void calcEdges(string ImagePath)
        {
            RGBPixel[,] Image = calc_neighbours(ImagePath);
            
            /*
             * A---> B,c
             * B--> A,D
             * 
             * 
             * content Edges
             * A--> B,c
             * 
             * 
             * 
             * 
             * 
             */


            foreach (var pixel in neighbours) //access key of Main pixel 
            {
                int i = pixel.Key.Item1; 
                int j = pixel.Key.Item2;

                foreach (var neighbor in pixel.Value)  // access item in each list 
                {
                    int ni = neighbor.Item1;  // neighbour i
                    int nj = neighbor.Item2;  // neighbour j

                    if(Rededges.ContainsKey((ni,nj))) // to avoid store edge twice 
                    {
                        continue;
                    }


                    if (!Rededges.ContainsKey((i, j)))
                    {
                        Rededges[(i, j)] = new List<Edge>();
                        Greenedges[(i, j)] = new List<Edge>();
                        Blueedges[(i, j)] = new List<Edge>();
                    }


                    double redWeight = Math.Abs(Image[i, j].red - Image[ni, nj].red);
                    double greenWeight = Math.Abs(Image[i, j].green - Image[ni, nj].green);
                    double blueWeight = Math.Abs(Image[i, j].blue - Image[ni, nj].blue);

                    Rededges[(i,j)].Add(new Edge(i, j, ni, nj, redWeight));
                    Blueedges[(i, j)].Add(new Edge(i, j, ni, nj, greenWeight));
                    Greenedges[(i, j)].Add(new Edge(i, j, ni, nj, blueWeight));
                }
            }
        }

    }
}
