using IEC16022Sharp;
using System;
using System.Drawing.Imaging;
using System.IO;

namespace TestApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            DateTime tstart = DateTime.Now;

            // Simple test
            _simpleDataMatrix();
            _resizeDataMatrix();
            _bigDataMatrix();

            // Important: for a real speed test, run the test outside Visul Studio
            _speedTest(0);
            _speedTest(1);
            _speedTest(2);

            // FastBmp
            _fastBmpOne();
            _fastBmpTwo();

            // Multithread
            MultiThreadTest.Exec();

            // A known issue
            KnownIssue.Exec();

            Console.WriteLine("***END***");
            Console.WriteLine("\n\nElapsed time: " + DateTime.Now.Subtract(tstart).TotalSeconds);
            Console.WriteLine("\n\nPress a key to exit");
            Console.ReadKey();
        }

        private static void _simpleDataMatrix()
        {
            string outFileName = "simple.png";
            var dm = new DataMatrix("Sabry and Ely");
            dm.GetBitmap().Save(outFileName, ImageFormat.Png);
            Console.WriteLine("DataMatrix: {0} W:{1} H:{2} ", outFileName, dm.Width, dm.Height);
        }

        private static void _resizeDataMatrix()
        {
            string outFileName = "resize.png";
            var dm = new DataMatrix("Sabry and Ely");
            DMImgUtility.SimpleResizeBmp(dm.GetBitmap(), 10, 50).Save(outFileName, ImageFormat.Png);
            Console.WriteLine("DataMatrix: {0} W:{1} H:{2} ", outFileName, dm.Width, dm.Height);
        }

        private static void _bigDataMatrix()
        {
            string outFileName = "big.png";
            var dm = new DataMatrix(
                "Lorem ipsum dolor sit amet, consectetur adipisicing elit, " +
                "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua");
            DMImgUtility.SimpleResizeBmp(dm.GetBitmap(), 10, 50).Save(outFileName, ImageFormat.Png);
            Console.WriteLine("DataMatrix: {0} W:{1} H:{2} ", outFileName, dm.Width, dm.Height);
        }

        private static void _speedTest(int mode)
        {
            int matrixNum = 1000;
            DateTime tstart = DateTime.Now;
            for (int i = 0; i < matrixNum; i++)
            {
                DataMatrix dm = null;

                switch (mode)
                {
                    case 0: dm = new DataMatrix("This is a test - IEC16022Sharp"); break;
                    case 1: dm = new DataMatrix("This is a test - IEC16022Sharp", EncodingType.Ascii); break;
                    case 2: dm = new DataMatrix("This is a test - IEC16022Sharp", 22, 22, EncodingType.Ascii); break;
                }
                using (var ms = new MemoryStream())
                    dm.GetBitmap().Save(ms, ImageFormat.Bmp);
            }
            double elapsedTime = DateTime.Now.Subtract(tstart).TotalSeconds;
            Console.WriteLine("SpeedTest mode [{0}] : {1} matrix - {2} matrix/sec", mode, matrixNum, ( matrixNum / elapsedTime ).ToString(".00"));
        }

        private static void _fastBmpOne()
        {
            var dm = new DataMatrix("Sabry and Ely", 16, 16);
            dm.FastBmp.Save("fastbmp.bmp");
            Console.WriteLine("FastBmpOne: fastbmp.bmp");
        }

        private static void _fastBmpTwo()
        {
            int matrixNum = 10000;
            DateTime tstart = DateTime.Now;
            for (int i = 0; i < matrixNum; i++)
            {
                var dm = new DataMatrix("This is a test - IEC16022Sharp", 22, 22, EncodingType.Ascii);
                using (var ms = new MemoryStream())
                    dm.FastBmp.Save(ms);
            }
            double elapsedTime = DateTime.Now.Subtract(tstart).TotalSeconds;
            Console.WriteLine("SpeedTest FastBmp: {0} matrix/sec", ( matrixNum / elapsedTime ).ToString(".00"));
        }
    }
}
