using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace Seamstress
{
	// credit: based on Jillian Silbert's seam carver
    public class SeamCarver
    {
        private static double SMALL = 1e-6;
        public Mat OriginalImageMat { get; private set; }
        public Mat ResizableImageMat { get; private set; }

        public SeamCarver(Mat imageMat)
        {
            OriginalImageMat = imageMat;
            ResizableImageMat = OriginalImageMat.Clone();
        }

        /// Reset the image to the original uncropped image
        public void ResetImage()
        {
            ResizableImageMat = OriginalImageMat.Clone();
        }

        #region Public Methods
        public void RemoveHorizontalSeam() { }
        public void RemoveVerticalSeam() 
        {
            RemoveVerticalSeam(FindVerticleSeam());
        }

        #endregion

        #region Private Methods

        private int[] FindVerticleSeam()
        {
            // Define variables
            int height = ResizableImageMat.Height;
            int width = ResizableImageMat.Width;
            Image<Gray, Byte> energyImage = GetEnergyImage();
            double[,] seamArray = new double[height + 1, width];
            int[] seam = new int[height];

            // Special case where width is 1 (return array of zeroes)
            if (width == 1) return seam;

            #region Enter Values into Seam Array
            // first row is all zeros (array initializes as all zeros)
            // middle rows depend on partial seam values and energy values
            for (int row = 1; row < height; row++)
            {
                // leftmost pixel of each row (2-way minimum. cell above and above to the right)
                seamArray[row, 0] =
                    Math.Min(seamArray[row - 1, 0] + energyImage[row - 1, 0].Intensity,
                            seamArray[row - 1, 1] + energyImage[row - 1, 1].Intensity);

                // inner pixels. Depend on partial seam values and energy values
                for(int col = 1; col < width - 1; col++)
                {
                    // 3 way minimum. The minimum between the 3 cells above/
                    seamArray[row,col] = Math.Min(seamArray[row - 1, col + 1] + energyImage[row - 1, col + 1].Intensity,
                        Math.Min(seamArray[row - 1, col] + energyImage[row - 1, col].Intensity,
                        seamArray[row - 1, col - 1] + energyImage[row - 1, col - 1].Intensity));
                }

                // rightmost pixel of each row (2 way minimum. cell above and above to the  left)
                seamArray[row, width - 1] =
                    Math.Min(seamArray[row - 1, width - 2] + energyImage[row - 1, width - 2].Intensity,
                            seamArray[row - 1, width - 1] + energyImage[row - 1, width - 1].Intensity);
            }

            // last row depends only on the pixel aove it (remember, seam array is length height + 1)
            for(int col = 0; col < width; col++)
            {
                seamArray[height, col] = seamArray[height - 1, col] + energyImage[height - 1, col].Intensity;
            }
            #endregion

            // Find beginning of seam. The beginning is the minumum value of the last row of the seam array
            // it starts searching at 50 and ends 50 before the end because of problems. todo: fix
            int minLoc = 50;
            for (int col = minLoc; col < width - minLoc; col++)
            {
                if (seamArray[height, col] < seamArray[height, minLoc]) 
                    minLoc = col;
                //Console.Write(seamArray[height, col] + " ");
            }
           // Console.WriteLine();

            // Find entire seam from starting point
            seam[height - 1] = minLoc;
            // find seam loc in row above from the last assigned row
            for(int row = height - 2; row >=0; row--)
            {
                minLoc = seam[row + 1];
                for(int col = minLoc - 1; col <= minLoc + 1; col++)
                {
                    if (col < 0 || col >= width) continue;
                    if(Math.Abs(seamArray[row,col] + energyImage[row,col].Intensity - seamArray[row + 1,minLoc]) < SMALL)
                    {
                        // what if none satisfy this condition?
                        seam[row] = col;
                        break;
                    }
                }
            }

            return seam;
        }

        private int[] FindHorizontalSeam()
        {
            return new int[] { };
        }

        private void RemoveVerticalSeam(int[] seam) 
        {
            if (seam == null) throw new NullReferenceException();
            if (seam.Length != ResizableImageMat.Height) throw new Exception("The seam is smaller than the picture.");
            if (ResizableImageMat.Width == 1) 
                throw new Exception("The image was can't be cropped anymore verticaly because the width of the image is 1 pixel.");


            Mat newImageMat = new Mat(ResizableImageMat.Height, ResizableImageMat.Width - 1, DepthType.Cv8U,3);
            Image<Bgr, Byte> newImage = newImageMat.ToImage<Bgr, Byte>();
            Image<Bgr, Byte> ResizableImage = ResizableImageMat.ToImage<Bgr, Byte>();

            bool removed;
            for(int row = 0; row < ResizableImage.Height; row++)
            {
                removed = false;
                for(int col = 0; col < ResizableImage.Width; col++)
                {
                    // at the pixel being removed
                    if(!removed && col == seam[row])
                    {
                        removed = true;
                        continue;
                    }
                    if (!removed)
                        newImage[row, col] = ResizableImage[row, col];
                    else
                        newImage[row, col - 1] = ResizableImage[row, col]; 
                }
            }
            ResizableImageMat = newImage.Mat.Clone();
            newImageMat.Dispose();

        }

        private Image<Gray,Byte> GetEnergyImage()
        {
            // Convert cropped image to gray scale
            Mat energyMatIn = ResizableImageMat.Clone().ToImage<Gray,Byte>().Mat;
            Mat energyMatOut = new Mat();

            // Gradient
            CvInvoke.Sobel(energyMatIn, energyMatOut, DepthType.Cv8U, 1, 1, 1);

            return energyMatOut.ToImage<Gray,Byte>();
            
        }

        #endregion


    }
}
