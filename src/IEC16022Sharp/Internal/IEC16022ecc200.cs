/*
 *
 * IEC16022Sharp DataMatrix bar code generation lib
 *   Fabrizio Accatino
 * 
 *   C# porting of:
 *      IEC16022 bar code generation
 *      Adrian Kennard, Andrews & Arnold Ltd
 *      with help from Cliff Hones on the RS coding
 *      (C version currently maintained by Stefan Schmidt)
 * 
 * (c) 2004 Adrian Kennard, Andrews & Arnold Ltd
 * (c) 2006 Stefan Schmidt <stefan@datenfreihafen.org>
 * (c) 2007 Fabrizio Accatino <fhtino@yahoo.com>
 * (c) 2020 Gerard Gunnewijk <gerard.gunnewijk@live.nl>
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301 USA
 *
 */

namespace IEC16022Sharp
{
    internal class IEC16022ecc200
    {
        private const int MAXBARCODE = 3116;

        private readonly char[] _encchr = "ACTXEB".ToCharArray();
        private readonly Ecc200matrix_s[] _ecc200matrix = new Ecc200matrix_s[] {
            new Ecc200matrix_s(10, 10, 10, 10, 3, 3, 5),	//
            new Ecc200matrix_s(12, 12, 12, 12, 5, 5, 7),	//
            new Ecc200matrix_s(8, 18, 8, 18, 5, 5, 7),	//
            new Ecc200matrix_s(14, 14, 14, 14, 8, 8, 10),	//
            new Ecc200matrix_s(8, 32, 8, 16, 10, 10, 11),	//
            new Ecc200matrix_s(16, 16, 16, 16, 12, 12, 12),	//
            new Ecc200matrix_s(12, 26, 12, 26, 16, 16, 14),	//
            new Ecc200matrix_s(18, 18, 18, 18, 18, 18, 14),	//
            new Ecc200matrix_s(20, 20, 20, 20, 22, 22, 18),	//
            new Ecc200matrix_s(12, 36, 12, 18, 22, 22, 18),	//
            new Ecc200matrix_s(22, 22, 22, 22, 30, 30, 20),	//
            new Ecc200matrix_s(16, 36, 16, 18, 32, 32, 24),	//
            new Ecc200matrix_s(24, 24, 24, 24, 36, 36, 24),	//
            new Ecc200matrix_s(26, 26, 26, 26, 44, 44, 28),	//
            new Ecc200matrix_s(16, 48, 16, 24, 49, 49, 28),	//
            new Ecc200matrix_s(32, 32, 16, 16, 62, 62, 36),	//
            new Ecc200matrix_s(36, 36, 18, 18, 86, 86, 42),	//
            new Ecc200matrix_s(40, 40, 20, 20, 114, 114, 48),	//
            new Ecc200matrix_s(44, 44, 22, 22, 144, 144, 56),	//
            new Ecc200matrix_s(48, 48, 24, 24, 174, 174, 68),	//
            new Ecc200matrix_s(52, 52, 26, 26, 204, 102, 42),	//
            new Ecc200matrix_s(64, 64, 16, 16, 280, 140, 56),	//
            new Ecc200matrix_s(72, 72, 18, 18, 368, 92, 36),	//
            new Ecc200matrix_s(80, 80, 20, 20, 456, 114, 48),	//
            new Ecc200matrix_s(88, 88, 22, 22, 576, 144, 56),	//
            new Ecc200matrix_s(96, 96, 24, 24, 696, 174, 68),	//
            new Ecc200matrix_s(104, 104, 26, 26, 816, 136, 56),	//
            new Ecc200matrix_s(120, 120, 20, 20, 1050, 175, 68),	//
            new Ecc200matrix_s(132, 132, 22, 22, 1304, 163, 62),	//
            new Ecc200matrix_s(144, 144, 24, 24, 1558, 156, 62)	// 156*4+155*2
        };
        private readonly byte[,] _switchcost = new byte[6, 6]  {
            {0, 1, 1, 1, 1, 2},	// From E_ASCII
            {1, 0, 2, 2, 2, 3},	// From E_C40
            {1, 2, 0, 2, 2, 3},	// From E_TEXT
            {1, 2, 2, 0, 2, 3},	// From E_X12
            {1, 2, 2, 2, 0, 3},	// From E_EDIFACT
            {0, 1, 1, 1, 1, 0}	// From E_BINARY
        };

        public string ErrorMessage { get; private set; } = null;

        public byte[] Iec16022ecc200(ref int wptr, ref int hptr, ref byte[] encodingptr, byte[] barcode, ref int lenp, ref int maxp, ref int eccp)
        {
            if (barcode.Length == 0 || barcode.Length > MAXBARCODE)
                throw new System.ArgumentOutOfRangeException(nameof(barcode), $"Barcode data must be a valid length, bigger than zero and lower than {MAXBARCODE}");

            byte[] binary = new byte[3000];	// encoded raw data and ecc to place in barcode
            int W = 0, H = 0;
            byte[] encoding = null;
            byte[] grid;

            int matrix_IDX;    //struct ecc200matrix_s *matrix;

            //memset(binary, 0, sizeof(binary));

            if (encodingptr != null)
                encoding = encodingptr;
            if (wptr != 0)
                W = wptr;
            if (hptr != 0)
                H = hptr;

            // encoding
            if (W != 0)
            {
                // known size

                // in C era:  for (matrix = ecc200matrix; matrix->W && (matrix->W != W || matrix->H != H); matrix++) ;
                for (matrix_IDX = 0; _ecc200matrix[matrix_IDX].W != 0 && ( _ecc200matrix[matrix_IDX].W != W || _ecc200matrix[matrix_IDX].H != H ); matrix_IDX++) ;

                if (!( _ecc200matrix[matrix_IDX].W != 0 ))
                {
                    ErrorMessage = "Invalid size " + W + "x" + H;
                    return null;
                }
                if (encoding == null)
                {
                    int len = 0;
                    byte[] e = _encmake(barcode, ref len, (char)1);
                    if (e != null && len != _ecc200matrix[matrix_IDX].Bytes)
                    {
                        // try not an exact fit
                        e = _encmake(barcode, ref len, (char)0);
                        if (len > _ecc200matrix[matrix_IDX].Bytes)
                        {
                            ErrorMessage = "Cannot make barcode fit " + W + "x" + H;
                            return null;
                        }
                    }
                    encoding = e;
                }
            }
            else
            {
                // find a suitable encoding
                int dummyint = 0;
                if (encoding == null)
                    encoding = _encmake(barcode, ref dummyint, (char)1);

                if (encoding != null)
                {
                    // find one that fits chosen encoding
                    matrix_IDX = 0;
                    for (; _ecc200matrix[matrix_IDX].W != 0; matrix_IDX++)
                    {
                        int dummyInt2 = 0;
                        char rv = _ecc200encode(binary, _ecc200matrix[matrix_IDX].Bytes, barcode, encoding, ref dummyInt2);
                        if (!( rv == (char)0 ))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    int len = 0;
                    byte[] e;
                    e = _encmake(barcode, ref len, (char)1);
                    for (matrix_IDX = 0; _ecc200matrix[matrix_IDX].W != 0 && _ecc200matrix[matrix_IDX].Bytes != len; matrix_IDX++) ;
                    if (e != null && !( _ecc200matrix[matrix_IDX].W != 0 ))
                    {
                        // try for non exact fit
                        e = _encmake(barcode, ref len, (char)0);
                        for (matrix_IDX = 0; _ecc200matrix[matrix_IDX].W != 0 && _ecc200matrix[matrix_IDX].Bytes < len; matrix_IDX++) ;
                    }
                    encoding = e;
                }
                if (!( _ecc200matrix[matrix_IDX].W != 0 ))
                {
                    ErrorMessage = "Cannot find suitable size, barcode too long";
                    return null;
                }
                W = _ecc200matrix[matrix_IDX].W;
                H = _ecc200matrix[matrix_IDX].H;
            }

            if (!( _ecc200encode(binary, _ecc200matrix[matrix_IDX].Bytes, barcode, encoding, ref lenp) != 0 ))
            {
                ErrorMessage = "Barcode too long for " + W + "x" + H;
                return null;
            }

            // ecc code
            _ecc200(binary, _ecc200matrix[matrix_IDX].Bytes, _ecc200matrix[matrix_IDX].Datablock, _ecc200matrix[matrix_IDX].RSblock);

            {
                // placement
                int x, y, NC, NR;
                int[] places;

                NC = W - ( 2 * ( W / _ecc200matrix[matrix_IDX].FW ) );
                NR = H - ( 2 * ( H / _ecc200matrix[matrix_IDX].FH ) );

                places = new int[NC * NR];   // safemalloc(NC * NR * sizeof(int));

                _ecc200placement(places, NR, NC);
                grid = new byte[W * H];      // safemalloc(W * H);

                for (y = 0; y < H; y += _ecc200matrix[matrix_IDX].FH)
                {
                    for (x = 0; x < W; x++)
                        grid[( y * W ) + x] = 1;
                    for (x = 0; x < W; x += 2)
                        grid[( ( y + _ecc200matrix[matrix_IDX].FH - 1 ) * W ) + x] = 1;
                }
                for (x = 0; x < W; x += _ecc200matrix[matrix_IDX].FW)
                {
                    for (y = 0; y < H; y++)
                        grid[( y * W ) + x] = 1;
                    for (y = 0; y < H; y += 2)
                        grid[( y * W ) + x + _ecc200matrix[matrix_IDX].FW - 1] = 1;
                }
                for (y = 0; y < NR; y++)
                {
                    for (x = 0; x < NC; x++)
                    {
                        int v = places[( ( NR - y - 1 ) * NC ) + x];
                        if (v == 1 || ( v > 7 && ( binary[( v >> 3 ) - 1] & ( 1 << ( v & 7 ) ) ) != 0 ))
                            grid[( ( 1 + y + ( 2 * ( y / ( _ecc200matrix[matrix_IDX].FH - 2 ) ) ) ) * W ) +
                                1 + x + ( 2 * ( x / ( _ecc200matrix[matrix_IDX].FW - 2 ) ) )] = 1;
                    }
                }
            }

            wptr = W;
            hptr = H;
            encodingptr = encoding;
            maxp = _ecc200matrix[matrix_IDX].Bytes;
            eccp = ( _ecc200matrix[matrix_IDX].Bytes + 2 ) / _ecc200matrix[matrix_IDX].Datablock * _ecc200matrix[matrix_IDX].RSblock;

            return grid;
        }

        private bool _isdigit(byte b)
        {
            return ( b >= '0' && b <= '9' );
        }

        private bool _isupper(byte b)
        {
            return ( b >= 'A' && b <= 'Z' );
        }

        private bool _islower(byte b)
        {
            return ( b >= 'a' && b <= 'z' );
        }

        private char _tolower(byte b)
        {
            return char.ToLower((char)b);
        }

        private void _ecc200placementbit(int[] array, int nr, int nc, int r, int c, int p, char b)
        {
            if (r < 0)
            {
                r += nr;
                c += 4 - ( ( nr + 4 ) % 8 );
            }
            if (c < 0)
            {
                c += nc;
                r += 4 - ( ( nc + 4 ) % 8 );
            }
            array[( r * nc ) + c] = ( p << 3 ) + b;
        }

        private void _ecc200placementblock(int[] array, int nr, int nc, int r, int c, int p)
        {
            _ecc200placementbit(array, nr, nc, r - 2, c - 2, p, (char)7);
            _ecc200placementbit(array, nr, nc, r - 2, c - 1, p, (char)6);
            _ecc200placementbit(array, nr, nc, r - 1, c - 2, p, (char)5);
            _ecc200placementbit(array, nr, nc, r - 1, c - 1, p, (char)4);
            _ecc200placementbit(array, nr, nc, r - 1, c - 0, p, (char)3);
            _ecc200placementbit(array, nr, nc, r - 0, c - 2, p, (char)2);
            _ecc200placementbit(array, nr, nc, r - 0, c - 1, p, (char)1);
            _ecc200placementbit(array, nr, nc, r - 0, c - 0, p, (char)0);
        }

        private void _ecc200placementcornerA(int[] array, int nr, int nc, int p)
        {
            _ecc200placementbit(array, nr, nc, nr - 1, 0, p, (char)7);
            _ecc200placementbit(array, nr, nc, nr - 1, 1, p, (char)6);
            _ecc200placementbit(array, nr, nc, nr - 1, 2, p, (char)5);
            _ecc200placementbit(array, nr, nc, 0, nc - 2, p, (char)4);
            _ecc200placementbit(array, nr, nc, 0, nc - 1, p, (char)3);
            _ecc200placementbit(array, nr, nc, 1, nc - 1, p, (char)2);
            _ecc200placementbit(array, nr, nc, 2, nc - 1, p, (char)1);
            _ecc200placementbit(array, nr, nc, 3, nc - 1, p, (char)0);
        }

        private void _ecc200placementcornerB(int[] array, int nr, int nc, int p)
        {
            _ecc200placementbit(array, nr, nc, nr - 3, 0, p, (char)7);
            _ecc200placementbit(array, nr, nc, nr - 2, 0, p, (char)6);
            _ecc200placementbit(array, nr, nc, nr - 1, 0, p, (char)5);
            _ecc200placementbit(array, nr, nc, 0, nc - 4, p, (char)4);
            _ecc200placementbit(array, nr, nc, 0, nc - 3, p, (char)3);
            _ecc200placementbit(array, nr, nc, 0, nc - 2, p, (char)2);
            _ecc200placementbit(array, nr, nc, 0, nc - 1, p, (char)1);
            _ecc200placementbit(array, nr, nc, 1, nc - 1, p, (char)0);
        }

        private void _ecc200placementcornerC(int[] array, int nr, int nc, int p)
        {
            _ecc200placementbit(array, nr, nc, nr - 3, 0, p, (char)7);
            _ecc200placementbit(array, nr, nc, nr - 2, 0, p, (char)6);
            _ecc200placementbit(array, nr, nc, nr - 1, 0, p, (char)5);
            _ecc200placementbit(array, nr, nc, 0, nc - 2, p, (char)4);
            _ecc200placementbit(array, nr, nc, 0, nc - 1, p, (char)3);
            _ecc200placementbit(array, nr, nc, 1, nc - 1, p, (char)2);
            _ecc200placementbit(array, nr, nc, 2, nc - 1, p, (char)1);
            _ecc200placementbit(array, nr, nc, 3, nc - 1, p, (char)0);
        }

        private void _ecc200placementcornerD(int[] array, int nr, int nc, int p)
        {
            _ecc200placementbit(array, nr, nc, nr - 1, 0, p, (char)7);
            _ecc200placementbit(array, nr, nc, nr - 1, nc - 1, p, (char)6);
            _ecc200placementbit(array, nr, nc, 0, nc - 3, p, (char)5);
            _ecc200placementbit(array, nr, nc, 0, nc - 2, p, (char)4);
            _ecc200placementbit(array, nr, nc, 0, nc - 1, p, (char)3);
            _ecc200placementbit(array, nr, nc, 1, nc - 3, p, (char)2);
            _ecc200placementbit(array, nr, nc, 1, nc - 2, p, (char)1);
            _ecc200placementbit(array, nr, nc, 1, nc - 1, p, (char)0);
        }

        private void _ecc200placement(int[] array, int nr, int nc)
        {
            int r, c, p;
            // invalidate
            for (r = 0; r < nr; r++)
                for (c = 0; c < nc; c++)
                    array[( r * nc ) + c] = 0;
            // start
            p = 1;
            r = 4;
            c = 0;
            do
            {
                // check corner
                if (r == nr && !( c != 0 ))
                    _ecc200placementcornerA(array, nr, nc, p++);
                if (( r == nr - 2 ) && !( c != 0 ) && ( ( nc % 4 ) != 0 ))
                    _ecc200placementcornerB(array, nr, nc, p++);
                if (r == nr - 2 && !( c != 0 ) && ( nc % 8 ) == 4)
                    _ecc200placementcornerC(array, nr, nc, p++);
                if (r == nr + 4 && c == 2 && !( ( nc % 8 ) != 0 ))
                    _ecc200placementcornerD(array, nr, nc, p++);
                // up/right
                do
                {
                    if (r < nr && c >= 0 && !( array[( r * nc ) + c] != 0 ))
                        _ecc200placementblock(array, nr, nc, r, c, p++);
                    r -= 2;
                    c += 2;
                }
                while (r >= 0 && c < nc);
                r++;
                c += 3;
                // down/left
                do
                {
                    if (r >= 0 && c < nc && !( array[( r * nc ) + c] != 0 ))
                        _ecc200placementblock(array, nr, nc, r, c, p++);
                    r += 2;
                    c -= 2;
                }
                while (r < nr && c >= 0);
                r += 3;
                c++;
            }
            while (r < nr || c < nc);
            // unfilled corner
            if (!( array[( nr * nc ) - 1] != 0 ))
                array[( nr * nc ) - 1] = array[( nr * nc ) - nc - 2] = 1;
        }

        private void _ecc200(byte[] binary, int bytes, int datablock, int rsblock)
        {
            int blocks = ( bytes + 2 ) / datablock, b;

            var rsObj = new ReedSol(0x12d, rsblock, 1);
            for (b = 0; b < blocks; b++)
            {
                byte[] buf = new byte[256];
                byte[] ecc = new byte[256];
                int n, p = 0;
                for (n = b; n < bytes; n += blocks)
                    buf[p++] = binary[n];
                rsObj.RsEncode(p, buf, out ecc);
                p = rsblock - 1;	// comes back reversed
                for (n = b; n < rsblock * blocks; n += blocks)
                    binary[bytes + n] = ecc[p--];
            }
        }

        private char _ecc200encode(byte[] t, int tl, byte[] s, byte[] encoding, ref int lenp)
        {
            char enc = 'a';		// start in ASCII encoding mode
            int tp = 0, sp = 0;
            int sl = s.Length;
            if (encoding.Length < sl)
            {
                ErrorMessage = "Encoding string too short";
                return (char)0;
            }
            // do the encoding
            while (sp < sl && tp < tl)
            {
                char newenc = enc;	// suggest new encoding
                if (( tl - tp <= 1 && ( enc == 'c' || enc == 't' ) ) || ( tl - tp <= 2 && enc == 'x' ))
                    enc = 'a';	// auto revert to ASCII

                newenc = _tolower(encoding[sp]);    // in C was: newenc = tolower(encoding[sp]);

                switch (newenc)
                {	// encode character
                    case 'c':	// C40
                    case 't':	// Text
                    case 'x':	// X12
                        {
                            char[] Out = new char[6];
                            char p = (char)0;

                            string s2 = "!\"#$%&'()*+,-./:;<=>?@[\\]_";
                            string s3 = "";
                            string e = "";

                            if (newenc == 'c')
                            {
                                e = " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                                s3 = "`abcdefghijklmnopqrstuvwxyz{|}~\0177";
                            }
                            if (newenc == 't')
                            {
                                e = " 0123456789abcdefghijklmnopqrstuvwxyz";
                                s3 = "`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~\0177";
                            }
                            if (newenc == 'x')
                                e = " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ\r*>";
                            do
                            {
                                char c = (char)s[sp++];
                                //char* w;
                                if (( c & 0x80 ) != 0)
                                {
                                    if (newenc == 'x')
                                    {
                                        ErrorMessage = "Cannot encode char 0x" + c + "X in X12";
                                        return (char)0;
                                    }
                                    c &= (char)0x7f;
                                    Out[p++] = (char)1;
                                    Out[p++] = (char)30;
                                }
                                //w = strchr(e, c);

                                int w_idx = e.IndexOf(c);

                                if (w_idx >= 0)
                                    Out[p++] = (char)( ( w_idx + 3 ) % 40 );  //Out[p++] = ((w - e) + 3) % 40;
                                else
                                {
                                    if (newenc == 'x')
                                    {
                                        ErrorMessage = "Cannot encode char 0x" + c + " in X12";
                                        return (char)0;
                                    }
                                    if (c < 32)
                                    {
                                        // shift 1
                                        Out[p++] = (char)0;
                                        Out[p++] = c;
                                    }
                                    else
                                    {
                                        w_idx = s2.IndexOf(c);   // strchr(s2, c);
                                        if (w_idx >= 0)
                                        {
                                            // shift 2
                                            Out[p++] = (char)1;
                                            Out[p++] = (char)w_idx;  // (w - s2);
                                        }
                                        else
                                        {
                                            w_idx = s3.IndexOf(c);  //w = strchr(s3, c);

                                            if (w_idx >= 0)
                                            {
                                                Out[p++] = (char)2;
                                                Out[p++] = (char)w_idx;
                                            }
                                            else
                                            {
                                                ErrorMessage = "Could not encode 0x" + c + "X, should not happen";
                                                return (char)0;
                                            }
                                        }
                                    }
                                }
                                if (p == 2 && tp + 2 == tl && sp == sl)
                                    Out[p++] = (char)0;	// shift 1 pad at end
                                while (p >= 3)
                                {
                                    int v =
                                        ( Out[0] * 1600 ) +
                                        ( Out[1] * 40 ) + Out[2] + 1;
                                    if (enc != newenc)
                                    {
                                        if (enc == 'c' || enc == 't' || enc == 'x')
                                            t[tp++] = 254;	// escape C40/text/X12
                                        else if (enc == 'x')
                                            t[tp++] = 0x7C;	// escape EDIFACT
                                        if (newenc == 'c')
                                            t[tp++] = 230;
                                        if (newenc == 't')
                                            t[tp++] = 239;
                                        if (newenc == 'x')
                                            t[tp++] = 238;
                                        enc = newenc;
                                    }
                                    t[tp++] = (byte)( v >> 8 );
                                    t[tp++] = (byte)( v & 0xFF );
                                    p -= (char)3;
                                    Out[0] = Out[3];
                                    Out[1] = Out[4];
                                    Out[2] = Out[5];
                                }
                            }
                            while (( p != 0 ) && ( sp < sl ));


                            break;
                        }

                    case 'e':
                        {
                            // EDIFACT
                            byte[] Out = new byte[4];
                            byte p = 0;
                            if (enc != newenc)
                            {	// can only be from C40/Text/X12
                                t[tp++] = 254;
                                enc = 'a';
                            }
                            while (sp < sl && _tolower(encoding[sp]) == 'e'
                                   && p < 4)
                                Out[p++] = s[sp++];
                            if (p < 4)
                            {
                                Out[p++] = 0x1F;
                                enc = 'a';
                            }	// termination
                            t[tp] = (byte)( ( s[0] & 0x3F ) << 2 );
                            t[tp++] |= (byte)( ( s[1] & 0x30 ) >> 4 );
                            t[tp] = (byte)( ( s[1] & 0x0F ) << 4 );
                            if (p == 2)
                                tp++;
                            else
                            {
                                t[tp++] |= (byte)( ( s[2] & 0x3C ) >> 2 );
                                t[tp] = (byte)( ( s[2] & 0x03 ) << 6 );
                                t[tp++] |= (byte)( s[3] & 0x3F );
                            }
                            break;
                        }

                    case 'a':	// ASCII
                        {
                            if (enc != newenc)
                            {
                                if (enc == 'c' || enc == 't' || enc == 'x')
                                    t[tp++] = 254;	// escape C40/text/X12
                                else
                                    t[tp++] = 0x7C;	// escape EDIFACT
                            }
                            enc = 'a';
                            if (sl - sp >= 2 && _isdigit(s[sp]) && _isdigit(s[sp + 1]))
                            {
                                t[tp++] = (byte)( ( ( s[sp] - '0' ) * 10 ) + s[sp + 1] - '0' + 130 );
                                sp += 2;
                            }
                            else if (s[sp] > 127)
                            {
                                t[tp++] = 235;
                                t[tp++] = (byte)( s[sp++] - 127 );
                            }
                            else
                                t[tp++] = (byte)( s[sp++] + 1 );

                            break;
                        }

                    case 'b':	// Binary
                        {
                            int l = 0;	// how much to encode
                            if (encoding != null)
                            {
                                int p;
                                for (p = sp; p < sl && _tolower(encoding[p]) == 'b'; p++)
                                    l++;
                            }
                            t[tp++] = 231;	// base256
                            if (l < 250)
                                t[tp++] = (byte)l;
                            else
                            {
                                t[tp++] = (byte)( 249 + ( l / 250 ) );
                                t[tp++] = (byte)( l % 250 );
                            }
                            while (l-- != 0 && tp < tl)
                            {
                                t[tp] = (byte)( s[sp++] + ( ( tp + 1 ) * 149 % 255 ) + 1 );	// see annex H
                                tp++;
                            }
                            enc = 'a';	// reverse to ASCII at end
                            break;
                        }

                    default:
                        ErrorMessage = "Unknown encoding: " + newenc;
                        return (char)0;	// failed
                }
            }

            if (lenp != 0)
                lenp = tp;
            if (tp < tl && enc != 'a')
            {
                if (enc == 'c' || enc == 'x' || enc == 't')
                    t[tp++] = 254;	// escape X12/C40/Text
                else
                    t[tp++] = 0x7C;	// escape EDIFACT
            }
            if (tp < tl)
                t[tp++] = 129;	// pad
            while (tp < tl)
            {	// more padding
                int v = 129 + ( ( tp + 1 ) * 149 % 253 ) + 1;	// see Annex H
                if (v > 254)
                    v -= 254;
                t[tp++] = (byte)v;
            }
            if (tp > tl || sp < sl)
                return (char)0;	// did not fit
            /*
             * for (tp = 0; tp < tl; tp++) fprintf (stderr, "%02X ", t[tp]); \
             * fprintf (stderr, "\n");
             */
            return (char)1;		// OK 
        }

        private byte[] _encmake(byte[] s, ref int lenp, char exact)
        {
            int l = s.Length;
            int p = l;
            char e;

            var enc = new TempStruct1[MAXBARCODE, (int)EncType.E_MAX];

            //memset(&enc, 0, sizeof(enc));

            while (p-- > 0)
            {
                char b = (char)0;
                char sub;
                int sl, tl, bl, t;
                // consider each encoding from this point
                // ASCII
                sl = tl = 1;
                if (_isdigit(s[p]) && p + 1 < l && _isdigit(s[p + 1]))
                    sl = 2;	// double digit
                else if (( s[p] & 0x80 ) != 0)
                    tl = 2;	// high shifted
                bl = 0;
                if (p + sl < l)
                    for (e = (char)0; e < (int)EncType.E_MAX; e++)
                        if (( enc[p + sl, e].T != 0 ) && ( ( t = enc[p + sl, e].T + _switchcost[(int)EncType.E_ASCII, e] ) < bl || !( bl != 0 ) ))
                        {
                            bl = t;
                            b = e;
                        }
                enc[p, (int)EncType.E_ASCII].T = (short)( tl + bl );
                enc[p, (int)EncType.E_ASCII].S = (short)sl;
                if (( bl != 0 ) && b == (int)EncType.E_ASCII)
                    enc[p, b].S += enc[p + sl, b].S;
                // C40
                sub = (char)0;
                tl = 0;
                sl = 0;

                do
                {
                    byte c = s[p + sl++];
                    if (( c & 0x80 ) != 0)
                    {	// shift + upper
                        sub += (char)2;
                        c &= 0x7F;
                    }
                    if (c != ' ' && !_isdigit(c) && !_isupper(c))
                        sub++;	// shift
                    sub++;
                    while (sub >= 3)
                    {
                        sub -= (char)3;
                        tl += 2;
                    }
                } while (( sub != 0 ) && ( p + sl < l ));
                if (( exact != 0 ) && sub == 2 && p + sl == l)
                {
                    // special case, can encode last block with shift 0 at end (Is this 
                    // valid when not end of target buffer?)
                    sub = (char)0;
                    tl += 2;
                }
                if (!( sub != 0 ))
                {	// can encode C40
                    bl = 0;
                    if (p + sl < l)
                        for (e = (char)0; e < (int)EncType.E_MAX; e++)
                            if (( enc[p + sl, e].T != 0 ) && ( ( t = enc[p + sl, e].T + _switchcost[(int)EncType.E_C40, e] ) < bl || !( bl != 0 ) ))
                            {
                                bl = t;
                                b = e;
                            }
                    if (( exact != 0 ) && enc[p + sl, (int)EncType.E_ASCII].T == 1 && 1 < bl)
                    {
                        // special case, switch to ASCII for last bytes
                        bl = 1;
                        b = (char)EncType.E_ASCII;
                    }
                    enc[p, (int)EncType.E_C40].T = (short)( tl + bl );
                    enc[p, (int)EncType.E_C40].S = (short)sl;
                    if (bl != 0 && b == (int)EncType.E_C40)
                        enc[p, b].S += enc[p + sl, b].S;
                }
                // Text
                sub = (char)0;
                tl = 0;
                sl = 0;
                do
                {
                    byte c = s[p + sl++];
                    if (( c & 0x80 ) != 0)
                    {	// shift + upper
                        sub += (char)2;
                        c &= 0x7F;
                    }
                    if (c != ' ' && !_isdigit(c) && !_islower(c))
                        sub++;	// shift
                    sub++;
                    while (sub >= 3)
                    {
                        sub -= (char)3;
                        tl += 2;
                    }
                } while (sub != 0 && ( p + sl < l ));
                if (exact != 0 && sub == 2 && p + sl == l)
                {
                    // special case, can encode last block with shift 0 at end (Is this 
                    // valid when not end of target buffer?)
                    sub = (char)0;
                    tl += 2;
                }
                if (!( sub != 0 ) && ( sl != 0 ))
                {	// can encode Text
                    bl = 0;
                    if (p + sl < l)
                        for (e = (char)0; e < (int)EncType.E_MAX; e++)
                            if (enc[p + sl, e].T != 0
                                &&
                                ( ( t =
                                  enc[p + sl, e].T +
                                  _switchcost[(int)EncType.E_TEXT, e] ) < bl
                                 || !( bl != 0 ) ))
                            {
                                bl = t;
                                b = e;
                            }
                    if (exact != 0 && enc[p + sl, (int)EncType.E_ASCII].T == 1 && 1 < bl)
                    {	// special case, switch to ASCII for last bytes
                        bl = 1;
                        b = (char)EncType.E_ASCII;
                    }
                    enc[p, (int)EncType.E_TEXT].T = (short)( tl + bl );
                    enc[p, (int)EncType.E_TEXT].S = (short)sl;
                    if (bl != 0 && b == (int)EncType.E_TEXT)
                        enc[p, b].S += enc[p + sl, b].S;
                }
                // X12
                sub = (char)0;
                tl = 0;
                sl = 0;
                do
                {
                    byte c = s[p + sl++];
                    if (c != 13 && c != '*' && c != '>' && c != ' '
                        && !_isdigit(c) && !_isupper(c))
                    {
                        sl = 0;
                        break;
                    }
                    sub++;
                    while (sub >= 3)
                    {
                        sub -= (char)3;
                        tl += 2;
                    }
                } while (sub != 0 && p + sl < l);
                if (!( sub != 0 ) && ( sl != 0 ))
                {	// can encode X12
                    bl = 0;
                    if (p + sl < l)
                        for (e = (char)0; e < (int)EncType.E_MAX; e++)
                            if (enc[p + sl, e].T != 0 && ( ( t = enc[p + sl, e].T + _switchcost[(int)EncType.E_X12, e] ) < bl || !( bl != 0 ) ))
                            {
                                bl = t;
                                b = e;
                            }
                    if (exact != 0 && enc[p + sl, (int)EncType.E_ASCII].T == 1 && 1 < bl)
                    {
                        // special case, switch to ASCII for last bytes
                        bl = 1;
                        b = (char)EncType.E_ASCII;
                    }
                    enc[p, (int)EncType.E_X12].T = (short)( tl + bl );
                    enc[p, (int)EncType.E_X12].S = (short)sl;
                    if (bl != 0 && b == (int)EncType.E_X12)
                        enc[p, b].S += enc[p + sl, b].S;
                }
                // EDIFACT
                sl = bl = 0;
                if (s[p + 0] >= 32 && s[p + 0] <= 94)
                {	// can encode 1
                    char bs = (char)0;
                    if (p + 1 == l && ( !( bl != 0 ) || bl < 2 ))
                    {
                        bl = 2;
                        bs = (char)1;
                    }
                    else
                        for (e = (char)0; e < (int)EncType.E_MAX; e++)
                            if (e != (int)EncType.E_EDIFACT && enc[p + 1, e].T != 0 && ( ( t = 2 + enc[p + 1, e].T + _switchcost[(int)EncType.E_ASCII, e] ) < bl || !( bl != 0 ) ))	// E_ASCII as allowed for unlatch
                            {
                                bs = (char)1;
                                bl = t;
                                b = e;
                            }
                    if (p + 1 < l && s[p + 1] >= 32 && s[p + 1] <= 94)
                    {	// can encode 2
                        if (p + 2 == l && ( !( bl != 0 ) || bl < 2 ))
                        {
                            bl = 3;
                            bs = (char)2;
                        }
                        else
                            for (e = (char)0; e < (int)EncType.E_MAX; e++)
                                if (e != (int)EncType.E_EDIFACT
                                    && enc[p + 2, e].T != 0
                                    &&
                                    ( ( t =
                                      3 + enc[p + 2, e].T +
                                      _switchcost[(int)EncType.E_ASCII, e] )
                                     < bl || !( bl != 0 ) ))	// E_ASCII as allowed for unlatch
                                {
                                    bs = (char)2;
                                    bl = t;
                                    b = e;
                                }
                        if (p + 2 < l && s[p + 2] >= 32 && s[p + 2] <= 94)
                        {	// can encode 3
                            if (p + 3 == l && ( !( bl != 0 ) || bl < 3 ))
                            {
                                bl = 3;
                                bs = (char)3;
                            }
                            else
                                for (e = (char)0; e < (int)EncType.E_MAX; e++)
                                    if (e != (int)EncType.E_EDIFACT
                                        && enc[p + 3, e].T != 0
                                        && ( ( t = 3 + enc[p + 3, e].T + _switchcost[(int)EncType.E_ASCII, e] ) < bl || !( bl != 0 ) ))	// E_ASCII as allowed for unlatch
                                    {
                                        bs = (char)3;
                                        bl = t;
                                        b = e;
                                    }
                            if (p + 4 < l && s[p + 3] >= 32 && s[p + 3] <= 94)
                            {	// can encode 4
                                if (p + 4 == l && ( !( bl != 0 ) || bl < 3 ))
                                {
                                    bl = 3;
                                    bs = (char)4;
                                }
                                else
                                {
                                    for (e = (char)0; e < (int)EncType.E_MAX;
                                         e++)
                                        if (enc[p + 4, e].T != 0 && ( ( t = 3 + enc[p + 4, e].T + _switchcost[(int)EncType.E_EDIFACT, e] ) < bl || !( bl != 0 ) ))
                                        {
                                            bs = (char)4;
                                            bl = t;
                                            b = e;
                                        }
                                    if (exact != 0
                                        && enc[p + 4, (int)EncType.E_ASCII].T != 0
                                        && enc[p + 4, (int)EncType.E_ASCII].
                                        T <= 2
                                        && ( t = 3 + enc[p + 4, (int)EncType.E_ASCII].T ) < bl)
                                    {
                                        // special case, switch to ASCII for last 1 ot two bytes
                                        bs = (char)4;
                                        bl = t;
                                        b = (char)EncType.E_ASCII;
                                    }
                                }
                            }
                        }
                    }
                    enc[p, (int)EncType.E_EDIFACT].T = (short)bl;
                    enc[p, (int)EncType.E_EDIFACT].S = (short)bs;
                    if (bl != 0 && b == (int)EncType.E_EDIFACT)
                        enc[p, b].S += enc[p + bs, b].S;
                }
                // Binary
                bl = 0;
                for (e = (char)0; e < (int)EncType.E_MAX; e++)
                    if (enc[p + 1, e].T != 0
                        &&
                        ( ( t =
                          enc[p + 1, e].T + _switchcost[(int)EncType.E_BINARY, e] +
                          ( ( e == (int)EncType.E_BINARY
                        && enc[p + 1, e].T == 249 ) ? 1 : 0 ) )
                         < bl || !( bl != 0 ) ))
                    {
                        bl = t;
                        b = e;
                    }
                enc[p, (int)EncType.E_BINARY].T = (short)( 1 + bl );
                enc[p, (int)EncType.E_BINARY].S = (short)1;
                if (bl != 0 && b == (int)EncType.E_BINARY)
                    enc[p, b].S += enc[p + 1, b].S;
            }

            // VECCHIA
            byte[] encoding = new byte[l + 1];
            p = 0;
            {
                char cur = (char)EncType.E_ASCII;	// starts ASCII
                while (p < l)
                {
                    int t = 0;
                    int m = 0;
                    char b = (char)0;
                    for (e = (char)0; e < (int)EncType.E_MAX; e++)
                        if (enc[p, e].T != 0
                            && ( ( t = enc[p, e].T + _switchcost[cur, e] ) <
                            m || ( t == m && e == cur ) || !( m != 0 ) ))
                        {
                            b = e;
                            m = t;
                        }
                    cur = b;
                    m = enc[p, b].S;

                    if (!( p != 0 ) && lenp != 0)
                        lenp = enc[p, b].T;

                    while (p < l && m-- != 0)
                        encoding[p++] = (byte)_encchr[b];
                }
            }
            encoding[p] = 0;
            return encoding;
        }

        private struct TempStruct1
        {
            // number of bytes of source that can be encoded in a row at this point
            // using this encoding mode
            public short S;
            // number of bytes of target generated encoding from this point to end if
            // already in this encoding mode
            public short T;
        }
    }
}
