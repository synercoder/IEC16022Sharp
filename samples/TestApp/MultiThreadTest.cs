using System;
using System.Threading;
using IEC16022Sharp;

namespace TestApp
{
    internal class MultiThreadTest
    {
        public static void Exec()
        {
            Console.WriteLine("\n***MultiThreadTest***");
            int paralleThread = 5;
            var t = new Thread[paralleThread];

            for (int i = 0; i < paralleThread; i++)
            {
                t[i] = new Thread(new ThreadStart(_test4ThreadRun))
                {
                    Name = "Thrd_" + i.ToString("000"),
                    IsBackground = true
                };
                t[i].Start();
            }

            for (int i = 0; i < paralleThread; i++)
                t[i].Join();

        }


        private static void _test4ThreadRun()
        {
            Console.WriteLine(Thread.CurrentThread.Name + " started");
            for (int i = 0; i < 1000; i++)
            {
                var dm1 = new DataMatrix(
                    "Lorem ipsum dolor sit amet, consectetur adipisicing elit, " +
                    "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua");
                _ = dm1.GetBitmap();
            }
            Console.WriteLine(Thread.CurrentThread.Name + " end");
        }


    }
}
