using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Security;
using System.Xml.Schema;
using System.Collections.Specialized;
using System.Windows.Forms.VisualStyles;

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
        public RGBPixel(byte Red, byte Green, byte Blue)
        {
            red = Red;
            green = Green;
            blue = Blue;
        }
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }

    public class Region
    {
        public int count;
        public RGBPixel color;



    }




    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    /// 

    public class DSU
    {
        public int lenght;
        public int width;
        public double k;
        public List<Tuple<double, int, int>> list = new List<Tuple<double, int, int>>();
        public int[] head;
        public int[] comp_size;
        public double[] internal_diff; // maximum edge in the compounent


        public DSU(double par, int n, int m, List<Tuple<double, int, int>> ls)
        {
            k = par;
            lenght = n;
            width = m;
            list = ls;

            head = new int[n * m];
            comp_size = new int[n * m];

            internal_diff = new double[n * m];

            for (int i = 0; i < n * m; i++)
            {
                head[i] = i;

                comp_size[i] = 1;
                internal_diff[i] = 0;
            }
        }

        public int find_head(int node) // log (n*m)
        {
            if (head[node] == node) return node;

            return head[node] = find_head(head[node]);
        }
        public void merge(double weight, int v1, int v2)
        {

            int head1 = find_head(v1);
            int head2 = find_head(v2);
            if (head1 == head2) return;

            if (comp_size[head2] > comp_size[head1])
            {
                int x = head1;
                head1 = head2;
                head2 = x;
            }

            double thr1 = k / comp_size[head1];
            double thr2 = k / comp_size[head2];
            if (weight > Math.Min(thr1 + internal_diff[head1], thr2 + internal_diff[head2])) return;
            // head1 > head2
            head[head2] = head1;

            comp_size[head1] += comp_size[head2];
            internal_diff[head1] = weight; // sorted
        }

        public void run_DSU()
        {
            int sz = list.Count;
            for (int i = 0; i < sz; i++)
            {
                merge(list[i].Item1, list[i].Item2, list[i].Item3);
            }
            for (int i = 0; i < width * lenght; i++)
            {
                head[i] = find_head(i);
            }
        }


    }

    public class Get_Intersection
    {
        public int length;
        public int width;
        public int[] head_red, head_green, head_blue;
        public int[] union;
        public int[] size_of_comp;

        public Get_Intersection(int n, int m, int[] head_red, int[] head_green, int[] head_blue)
        {
            this.length = n;
            this.width = m;

            this.head_red = head_red;
            this.head_green = head_green;
            this.head_blue = head_blue;

            union = new int[n * m];

            size_of_comp = new int[n * m];

            for (int i = 0; i < n * m; i++)
            {
                union[i] = i;
                size_of_comp[i] = 0;
            }
        }
        public int find_head2(int node) // log (n*m)
        {
            if (union[node] == node) return node;
            return union[node] = find_head2(union[node]);
        }

        public void merge2(int v1, int v2)
        {
            int head1 = find_head2(v1);
            int head2 = find_head2(v2);
            if (head1 == head2) return;
            if (size_of_comp[head2] > size_of_comp[head1])
            {
                int x = head1;
                head1 = head2;
                head2 = x;
            }

            bool red = false, green = false, blue = false;

            if (head_red[head1] == head_red[head2]) red = true;
            if (head_blue[head1] == head_blue[head2]) blue = true;
            if (head_green[head1] == head_green[head2]) green = true;

            if (!red || !green || !blue) return;

            union[head2] = head1;
            size_of_comp[head1] += size_of_comp[head2];
        }

        public int calc_node(int i, int j)
        {
            return i * width + j;
        }
        public bool valid(int x, int y)
        {
            if (x > -1 && x < length && y > -1 && y < width) return true;
            return false;

        }

        public void Execute()
        {
            int[] arr1 = { 1, 0, 0, -1, 1, -1, 1, -1 };
            int[] arr2 = { 0, 1, -1, 0, -1, 1, 1, -1 };

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < width; j++)
                {

                    for (int u = 0; u < 8; u++)
                    {

                        int x = i + arr1[u];
                        int y = j + arr2[u];
                        if (valid(x, y))
                        {
                            int pixel1 = calc_node(i, j);
                            int pixel2 = calc_node(x, y);
                            merge2(pixel1, pixel2);
                        }

                    }
                }
            }

            for (int i = 0; i < width * length; i++)
            {
                union[i] = find_head2(i);
            }


        }





    }
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        /// 
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
        public static Dictionary<int, List<Tuple<int, int>>> segments = new Dictionary<int, List<Tuple<int, int>>>();

        public static Dictionary<int, Region> count = new Dictionary<int, Region>();
        public static List<int> comps = new List<int>();
        static List<Tuple<double, int, int>> list_red = new List<Tuple<double, int, int>>();
        static List<Tuple<double, int, int>> list_green = new List<Tuple<double, int, int>>();
        static List<Tuple<double, int, int>> list_blue = new List<Tuple<double, int, int>>();
        public static int[] Heads;
        static Dictionary<(int, int), List<(int, int)>> neighbours = new Dictionary<(int, int), List<(int, int)>>();

        static Dictionary<(int, int), List<Edge>> Rededges = new Dictionary<(int, int), List<Edge>>();
        static Dictionary<(int, int), List<Edge>> Blueedges = new Dictionary<(int, int), List<Edge>>();
        static Dictionary<(int, int), List<Edge>> Greenedges = new Dictionary<(int, int), List<Edge>>();
        public static int calc_node(int i, int j, int n, int m)
        {
            return i * m + j;
        }

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








        public static RGBPixel[,] Visulaiztaion(RGBPixel[,] image)
        {

            int lenght = image.GetLength(0);
            int width = image.GetLength(1);

            byte red, green, blue;
            int index = 0;
            foreach (Region x in count.Values)
            {
                red = (byte)(((index * 71) % 200) + 56);
                green = (byte)(((index * 73) % 200) + 56);
                blue = (byte)(((index * 79) % 200) + 56);

                x.color = new RGBPixel(red, green, blue);
                index++;
            }

            for (int i = 0; i < lenght; i++)
            {
                for (int j = 0; j < width; j++)
                {

                    int pixel = calc_node(i, j, lenght, width);
                    int parent = Heads[pixel];
                    image[i, j] = count[parent].color;


                }
            }
            return image;
        }

        public static RGBPixel[,] calc_neighbours(RGBPixel[,] Imagee)
        {
            int image_height = GetHeight(Imagee);
            int image_width = GetWidth(Imagee);


            //  Imagee = GaussianFilter1D(Imagee, 5, 0.8);


            for (int i = 0; i < image_height; i++) // row by row
            {

                for (int j = 0; j < image_width; j++)
                {
                    RGBPixel pixel = new RGBPixel();

                    pixel = Imagee[i, j];


                    if (!neighbours.ContainsKey((i, j)))
                        neighbours[(i, j)] = new List<(int, int)>();



                    if (i == 0 && j == 0)
                    {
                        neighbours[(i, j)].Add((i, j + 1));
                        neighbours[(i, j)].Add((i + 1, j));
                        neighbours[(i, j)].Add((i + 1, j + 1));



                    }
                    else if (i == 0 && j == image_width - 1)
                    {
                        neighbours[(i, j)].Add((i, j - 1));
                        neighbours[(i, j)].Add((i + 1, j));
                        neighbours[(i, j)].Add((i + 1, j - 1));



                    }
                    else if (i == image_height - 1 && j == 0)
                    {
                        neighbours[(i, j)].Add((i, j + 1));
                        neighbours[(i, j)].Add((i - 1, j));
                        neighbours[(i, j)].Add((i - 1, j + 1));




                    }
                    else if (i == image_height - 1 && j == image_width - 1)
                    {
                        neighbours[(i, j)].Add((i, j - 1));
                        neighbours[(i, j)].Add((i - 1, j));
                        neighbours[(i, j)].Add((i - 1, j - 1));



                    }
                    else if (i == 0)
                    {
                        neighbours[(i, j)].Add((i, j - 1));
                        neighbours[(i, j)].Add((i, j + 1));

                        neighbours[(i, j)].Add((i + 1, j));

                        neighbours[(i, j)].Add((i + 1, j - 1));
                        neighbours[(i, j)].Add((i + 1, j + 1));




                    }
                    else if (i == image_height - 1)
                    {
                        neighbours[(i, j)].Add((i, j - 1));
                        neighbours[(i, j)].Add((i, j + 1));

                        neighbours[(i, j)].Add((i - 1, j));

                        neighbours[(i, j)].Add((i - 1, j - 1));
                        neighbours[(i, j)].Add((i - 1, j + 1));



                    }
                    else if (j == 0)
                    {
                        neighbours[(i, j)].Add((i - 1, j));
                        neighbours[(i, j)].Add((i + 1, j));

                        neighbours[(i, j)].Add((i, j + 1));

                        neighbours[(i, j)].Add((i - 1, j + 1));
                        neighbours[(i, j)].Add((i + 1, j + 1));




                    }
                    else if (j == image_width - 1)
                    {
                        neighbours[(i, j)].Add((i - 1, j));
                        neighbours[(i, j)].Add((i + 1, j));

                        neighbours[(i, j)].Add((i, j - 1));

                        neighbours[(i, j)].Add((i - 1, j - 1));
                        neighbours[(i, j)].Add((i + 1, j - 1));



                    }
                    else
                    {

                        neighbours[(i, j)].Add((i + 1, j));
                        neighbours[(i, j)].Add((i - 1, j));

                        neighbours[(i, j)].Add((i, j - 1));
                        neighbours[(i, j)].Add((i, j + 1));

                        neighbours[(i, j)].Add((i - 1, j - 1));
                        neighbours[(i, j)].Add((i - 1, j + 1));

                        neighbours[(i, j)].Add((i + 1, j + 1));
                        neighbours[(i, j)].Add((i + 1, j - 1));





                    }


                }
            }



            return Imagee;


        }
        public static void calcEdges(RGBPixel[,] Imagee)
        {
            RGBPixel[,] Image = calc_neighbours(Imagee);
            int lenght = Image.GetLength(0);
            int width = Image.GetLength(1);

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

                    // you make it a directed graph 
                    if (Rededges.ContainsKey((ni, nj))) // to avoid store edge twice 
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

                    Rededges[(i, j)].Add(new Edge(i, j, ni, nj, redWeight));
                    Blueedges[(i, j)].Add(new Edge(i, j, ni, nj, greenWeight));
                    Greenedges[(i, j)].Add(new Edge(i, j, ni, nj, blueWeight));


                    int new_node1 = calc_node(i, j, lenght, width);
                    int new_node2 = calc_node(ni, nj, lenght, width);

                    list_red.Add(Tuple.Create(redWeight, new_node1, new_node2));
                    list_blue.Add(Tuple.Create(blueWeight, new_node1, new_node2));
                    list_green.Add(Tuple.Create(greenWeight, new_node1, new_node2));
                }


            }

        }

        public static void get_regions(double k, RGBPixel[,] Image) // k is a parameter
        {


            int lenght = Image.GetLength(0);
            int width = Image.GetLength(1);
            calcEdges(Image);
            list_red.Sort();
            list_blue.Sort();
            list_green.Sort();

            DSU dsu_red = new DSU(k, lenght, width, list_red);
            dsu_red.run_DSU();
            // vector<vector<int>>comp

            DSU dsu_blue = new DSU(k, lenght, width, list_green);
            dsu_blue.run_DSU();

            DSU dsu_green = new DSU(k, lenght, width, list_blue);
            dsu_green.run_DSU();



            Get_Intersection seg = new Get_Intersection(lenght, width, dsu_red.head, dsu_green.head, dsu_blue.head);
            seg.Execute();


            Heads = seg.union;


            for (int i = 0; i < lenght; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int pixel = calc_node(i, j, lenght, width);
                    //count[pixel]++;
                    int head = Heads[pixel];
                    if (!segments.ContainsKey(head))
                    {
                        segments[head] = new List<Tuple<int, int>>();
                    }
                    if (!count.ContainsKey(head))
                    {
                        count[head] = new Region(); // or new Region { count = 0 } if needed
                    }

                    segments[head].Add(Tuple.Create(i, j));
                    count[head].count++;
                }
            }

            foreach (Region p in count.Values) comps.Add(p.count);
            comps.Sort(); // sz: how many compounents and sz of each compounent
            comps.Reverse();  // in cout , get the comps out desecnding 
            /*  Dictionary<int, List<int>> segments = new Dictionary<int, List<int>>();


               for(int i=0;i<lenght*width;i++)
               {
                   segments[head[i]].Add(i); // each i will go to its comp

               }
              Dictionary<int, List<Tuple<int, int>>> segments = new Dictionary<int, List<Tuple<int, int>>>();
              for (int i = 0; i < lenght; i++)
              {
                  for (int j = 0; j < width; j++)
                  {
                      int pixel = calc_node(i, j, lenght, width);
                      segments[head[pixel]].Add(Tuple.Create(i, j));

                  }
              }

              List<int> comps = new List<int>();
              foreach (List<Tuple<int, int>> p in segments.Values) comps.Add(p.Count);
              comps.Sort();*/








        }





    }
}