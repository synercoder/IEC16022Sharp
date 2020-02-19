using System;
using System.Collections.Generic;
using System.Text;


namespace IEC16022Sharp
{
    internal struct ecc200matrix_s
    {
        public ecc200matrix_s(int h, int w, int fh, int fw, int bytes, int datablock, int rsblock)
        {
            H = h;
            W = w;
            FH = fh;
            FW = fw;
            Bytes = bytes;
            Datablock = datablock;
            RSblock = rsblock;
        }

        public int H;
        public int W;
        public int FH;
        public int FW;
        public int Bytes;
        public int Datablock;
        public int RSblock;
    };
}
