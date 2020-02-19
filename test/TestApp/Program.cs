using System;
using System.Collections.Generic;
using System.Text;
using IEC16022Sharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


namespace TestApp
{
    class Program
    {

        static void Main(string[] args)
        {
            DateTime tstart = DateTime.Now;

            // Simple test
            SimpleDataMatrix();
            ResizeDataMatrix();
            BigDataMatrix();
            HexPbm();
            OverSize_Error();


            // Important: for a real speed test, run the test outside Visul Studio
            SpeedTest(0);
            SpeedTest(1);
            SpeedTest(2);

            // FastBmp
            FastBmpOne();
            FastBmpTwo();

            // Multithread
            MultiThreadTest.Exec();

            // "self test"
            Test_ReedSol.Exec();

            // A known issue
            KnownIssue.Exec();


            Console.WriteLine("***END***");
            Console.WriteLine("\n\nElapsed time: " + DateTime.Now.Subtract(tstart).TotalSeconds);
            Console.WriteLine("\n\nPress a key to exit");
            Console.ReadKey();
        }



        private static void SimpleDataMatrix()
        {
            string outFileName = "simple.png";
            DataMatrix dm = new DataMatrix("Sabry and Ely");
            dm.Image.Save(outFileName, ImageFormat.Png);
            Console.WriteLine("DataMatrix: {0} W:{1} H:{2} ", outFileName, dm.Width, dm.Height);
        }


        private static void ResizeDataMatrix()
        {
            string outFileName = "resize.png";
            DataMatrix dm = new DataMatrix("Sabry and Ely");
            DMImgUtility.SimpleResizeBmp(dm.Image, 10, 50).Save(outFileName, ImageFormat.Png);
            Console.WriteLine("DataMatrix: {0} W:{1} H:{2} ", outFileName, dm.Width, dm.Height);
        }


        private static void BigDataMatrix()
        {
            string outFileName = "big.png";
            DataMatrix dm = new DataMatrix(
                "Lorem ipsum dolor sit amet, consectetur adipisicing elit, " +
                "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua");
            DMImgUtility.SimpleResizeBmp(dm.Image, 10, 50).Save(outFileName, ImageFormat.Png);
            Console.WriteLine("DataMatrix: {0} W:{1} H:{2} ", outFileName, dm.Width, dm.Height);
        }


        private static void OverSize_Error()
        {
            try
            {
                DataMatrix dm = new DataMatrix(new string('x', 4000));
            }
            catch (DataMatrixException dme)
            {
                Console.WriteLine("DataMatrixException: " + dme.Message + (dme.InnerException != null ? " : " + dme.InnerException.Message : ""));
            }
        }


        private static void SpeedTest(int mode)
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
                MemoryStream ms = new MemoryStream();
                dm.Image.Save(ms, ImageFormat.Bmp);
            }
            double elapsedTime = DateTime.Now.Subtract(tstart).TotalSeconds;
            Console.WriteLine("SpeedTest mode [{0}] : {1} matrix - {2} matrix/sec", mode, matrixNum, (matrixNum / elapsedTime).ToString(".00"));
        }



        private static void FastBmpOne()
        {
            DataMatrix dm = new DataMatrix("Sabry and Ely", 16, 16);
            dm.FastBmp.Save("fastbmp.bmp");
            Console.WriteLine("FastBmpOne: fastbmp.bmp");
        }


        private static void FastBmpTwo()
        {
            int matrixNum = 10000;
            DateTime tstart = DateTime.Now;
            for (int i = 0; i < matrixNum; i++)
            {
                DataMatrix dm = new DataMatrix("This is a test - IEC16022Sharp", 22, 22, EncodingType.Ascii);
                MemoryStream ms = new MemoryStream();
                dm.FastBmp.Save(ms);
            }
            double elapsedTime = DateTime.Now.Subtract(tstart).TotalSeconds;
            Console.WriteLine("SpeedTest FastBmp: {0} matrix/sec", (matrixNum / elapsedTime).ToString(".00"));
        }


        private static void HexPbm()
        {
            DataMatrix dm = new DataMatrix("44", 32, 8);
            Console.WriteLine("HexPBM: " + dm.HexPbm);
            string expectedResult = "AAAAAAAAD99B8EC38676A62A9D07B257C73AD2608A7F8791D8F0\nDC38FFFFFFFF";
            if (dm.HexPbm != expectedResult)
                Console.WriteLine("--> HEXPBM ERROR !!!");
        }

    }
}
