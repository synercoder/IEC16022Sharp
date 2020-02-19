/** 
 *
 * IEC16022Sharp DataMatrix bar code generation lib
 * (c) 2007-2008 Fabrizio Accatino <fhtino@yahoo.com>
 * 
 *   HexPbm output by Andrew Francis <acfrancis@gmail.com>
 * 
 *   Core components are based on IEC16022 by Adrian Kennard, Andrews & Arnold Ltd
 *   (C version currently maintained by Stefan Schmidt)
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



using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;



namespace IEC16022Sharp
{
    public class DataMatrix
    {
        private byte[] _data;
        private int _w;
        private int _h;
        private byte[,] _byteArray = null;
        private Bitmap _bmp = null;
        private FastBWBmp _bmpBW = null;
        private EncodingType _globalEncoding = EncodingType.NotDef;
        private byte[] _encoding = null;
        private string _hexPbm = null;



        public DataMatrix(string message)
            : this(Encoding.ASCII.GetBytes(message), 0, 0, EncodingType.NotDef)
        {
        }


        public DataMatrix(string message, EncodingType globalEncoding)
            : this(Encoding.ASCII.GetBytes(message), 0, 0, globalEncoding)
        {
        }


        public DataMatrix(string message, int w, int h)
            : this(Encoding.ASCII.GetBytes(message), w, h, EncodingType.NotDef)
        {
        }


        public DataMatrix(string message, int w, int h, EncodingType globalEncoding)
            : this(Encoding.ASCII.GetBytes(message), w, h, globalEncoding)
        {
        }


        public DataMatrix(byte[] data, int w, int h, EncodingType globalEncoding)
            : this(data, w, h, globalEncoding, null)
        {
        }


        /// <summary>
        /// Internal costructor
        /// </summary>
        private DataMatrix(byte[] data, int w, int h, EncodingType globalEncoding, byte[] encoding)
        {
            _data = data;
            _w = w;
            _h = h;
            _globalEncoding = globalEncoding;
            _encoding = encoding;

            try
            {
                Build();
            }
            catch (DataMatrixException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DataMatrixException("Internal error", ex);
            }
            if (_byteArray == null)
                throw new DataMatrixException("Error creating DataMatrix");
        }


        /// <summary>
        /// Get a bmp file content (fast)
        /// </summary>
        public FastBWBmp FastBmp
        {
            get
            {
                if (_bmpBW == null)
                {
                    int rows = _byteArray.GetLength(1);
                    int cols = _byteArray.GetLength(0);
                    byte[,] newDotMatrix = new byte[rows, cols];

                    for (int r = 0; r < rows; r++)
                        for (int c = 0; c < cols; c++)
                            newDotMatrix[rows - r - 1, c] = _byteArray[c, r] == 0 ? (byte)1 : (byte)0;

                    _bmpBW = new FastBWBmp(newDotMatrix);
                }
                return _bmpBW;
            }
        }


        /// <summary>
        /// Get an Image (.NET) of the datamatrix
        /// </summary>
        public Bitmap Image
        {
            get
            {
                if (_bmp == null)
                    BuildBitmap();
                return _bmp;
            }
        }


        /// <summary>
        /// Get the DataMatrix content as a hex PBM string
        /// </summary>
        public string HexPbm
        {
            get
            {
                if (_hexPbm == null)
                {
                    int rows = _byteArray.GetLength(1);
                    int cols = _byteArray.GetLength(0);

                    int bitCount = 7;
                    int hexDigitCount = 0;
                    byte currentByte = (byte)0;
                    StringBuilder sb = new StringBuilder();

                    for (int r = rows - 1; r >= 0; r--)
                        for (int c = 0; c < cols; c++)
                        {
                            byte currentBit = (byte)(_byteArray[c, r] << bitCount);
                            currentByte |= currentBit;
                            if (bitCount == 0)
                            {
                                sb.AppendFormat("{0:X02}", currentByte);
                                currentByte = (byte)0;
                                bitCount = 7;

                                hexDigitCount += 2;
                                if (hexDigitCount > 50)
                                {
                                    sb.Append("\n");
                                    hexDigitCount = 0;
                                }
                            }
                            else
                            {
                                bitCount--;
                            }
                        }

                    if (bitCount != 7)
                    {
                        sb.AppendFormat("{0:X02}", currentByte);
                    }

                    _hexPbm = sb.ToString();
                }
                return _hexPbm;
            }
        }


        /// <summary>
        /// Get a copy of the pixel matrix
        /// </summary>
        public byte[,] PixelArray { get { return (byte[,])_byteArray.Clone(); } }
        public int W { get { return _w; } }
        public int H { get { return _h; } }



        /// <summary>
        /// ...
        /// </summary>
        private void Build()
        {
            //byte[] encoding = null;
            int lenp = 0;
            int maxp = 0;
            int eccp = 0;


            // globaEncoding is present
            if (_globalEncoding != EncodingType.NotDef)
            {
                _encoding = new byte[_data.Length + 1];
                byte e = EncodingTypeToByte(_globalEncoding);
                for (int i = 0; i < _encoding.Length; i++)
                    _encoding[i] = e;
                _encoding[_data.Length] = 0;
            }


            // Matrix creation
            IEC16022ecc200 iec16022 = new IEC16022ecc200();
            byte[] array = iec16022.iec16022ecc200(
                       ref _w,
                       ref _h,
                       ref _encoding,
                       _data.Length,
                       _data,
                       ref lenp,
                       ref maxp,
                       ref eccp);

            if (array == null)
                throw new DataMatrixException("Error building datamtrix: " + iec16022.ErrorMessage);

            _byteArray = new byte[_w, _h];
            for (int x = 0; x < _w; x++)
                for (int y = 0; y < _h; y++)
                    _byteArray[x, y] = array[_w * y + x];

        }


        /// <summary>
        /// ...
        /// </summary>
        private void BuildBitmap()
        {
            // Nota: questo codice lavora solo sulle immagini 24bit

            int W = _byteArray.GetLength(0);
            int H = _byteArray.GetLength(1);
            Bitmap bmp = new Bitmap(W, H, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, W, H), ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;

            //int bytes = bmp.Width * bmp.Height * 3;   // BUG: la larghezza è data dallo Stride e non da bmpWidth
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            for (int x = 0; x < W; x++)
            {
                for (int y = 0; y < H; y++)
                {
                    int idx = ((H - y - 1) * bmpData.Stride) + x * 3;

                    if (_byteArray[x, y] == 0)
                    {
                        rgbValues[idx] = 255;
                        rgbValues[idx + 1] = 255;
                        rgbValues[idx + 2] = 255;
                    }
                    else
                    {
                        rgbValues[idx] = 0;
                        rgbValues[idx + 1] = 0;
                        rgbValues[idx + 2] = 0;
                    }
                }
            }

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            bmp.UnlockBits(bmpData);

            _bmp = bmp;
        }



        /// <summary>
        /// Convert EncodingType enum to internal format (byte)
        /// </summary>
        private byte EncodingTypeToByte(EncodingType et)
        {
            switch (et)
            {
                case EncodingType.Ascii: return (byte)'A';
                case EncodingType.Binary: return (byte)'B';
                case EncodingType.C40: return (byte)'C';
                case EncodingType.Edifact: return (byte)'E';
                case EncodingType.Text: return (byte)'T';
                case EncodingType.X12: return (byte)'X';
                case EncodingType.NotDef: throw new ApplicationException("EncodingType not valid");
                default: throw new ApplicationException("Unknown EncodingType");
            }
        }


    }
}
