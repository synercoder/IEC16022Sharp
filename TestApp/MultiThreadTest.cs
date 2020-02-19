using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IEC16022Sharp;
using System.Drawing;


namespace TestApp
{
    internal class MultiThreadTest
    {

        public static void Exec()
        {
            Console.WriteLine("\n***MultiThreadTest***");
            int paralleThread = 5;
            Thread[] t = new Thread[paralleThread];

            for (int i = 0; i < paralleThread; i++)
            {
                t[i] = new Thread(new ThreadStart(Test4_ThreadRun));
                t[i].Name = "Thrd_" + i.ToString("000");
                t[i].IsBackground = true;
                t[i].Start();
            }

            for (int i = 0; i < paralleThread; i++)
                t[i].Join();

        }


        private static void Test4_ThreadRun()
        {
            Console.WriteLine(Thread.CurrentThread.Name + " started");
            for (int i = 0; i < 1000; i++)
            {
                DataMatrix dm1 = new DataMatrix(
                    "Lorem ipsum dolor sit amet, consectetur adipisicing elit, " +
                    "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua");
                Bitmap bmp = dm1.Image;
            }
            Console.WriteLine(Thread.CurrentThread.Name + " end");
        }


    }
}
