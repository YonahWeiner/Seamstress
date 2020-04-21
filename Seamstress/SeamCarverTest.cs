using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Seamstress
{
    class SeamCarverTest
    {
        public void Run()
        {
            // define setup variables
            // define number of iterations
            int numberOfCuts = 5;
            string imageName = "Giza";


            // open image
            Console.WriteLine("Loading Image");
            Mat originalMat = CvInvoke.Imread(PathInfo.resources_directory + @"\"+imageName+".jpg");

            SeamCarver sc = new SeamCarver(originalMat);

            for(int i =0; i < numberOfCuts; i++)
            {
                sc.RemoveVerticalSeam();
            }

            sc.ResizableImageMat.Save(PathInfo.output_directory + "\\"+imageName+ numberOfCuts +".jpg");

        }


    }
}
