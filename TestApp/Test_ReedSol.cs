using System;
using System.Collections.Generic;
using System.Text;

namespace TestApp
{
    internal class Test_ReedSol
    {
        public static void Exec()
        {
            byte[] inData = new byte[] { 142, 164, 186 };
            byte[] outData;
            byte[] okData = new byte[] { 102, 88, 5, 25, 114 };

            Console.Write("\n\nTesting ReedSol... ");
            
            // ReedSol calculation
            IEC16022Sharp.ReedSol rs = new IEC16022Sharp.ReedSol();
            rs.rs_init_gf(0x12d);
            rs.rs_init_code(5, 1);
            rs.rs_encode(3, inData, out  outData);

            // Output
            for (int i = outData.Length - 1; i >= 0; i--)
                Console.Write(outData[i] + " ");


            // Checks
            if (okData.Length != outData.Length)
                throw new ApplicationException("okData.Length != outData.Length");
            for (int i = 0; i < okData.Length; i++)
                if (outData[i] != okData[i])
                    throw new ApplicationException("outData != okData");

            Console.WriteLine(" --> OK\n");
        }
    }
}
