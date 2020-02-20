/*
 *
 * (IEC16022Sharp DataMatrix bar code generation lib)
 * 
 * FastBWBmp: a class for direct creation of 1 bit/pixel BMP file
 * (c) 2007 Fabrizio Accatino <fhtino@yahoo.com>
 * (c) 2020 Gerard Gunnewijk <gerard.gunnewijk@live.nl>
 * 
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

// Many informations about BMP format from http://www.fortunecity.com/skyscraper/windows/364/bmpffrmt.html

using System.IO;

namespace IEC16022Sharp
{
    public class FastBWBmp
    {
        private readonly int _width;
        private readonly int _height;
        private readonly byte[,] _dots;
        private readonly byte[] _pixelData;
        private readonly byte[] _fileBytes;

        public FastBWBmp(byte[,] dots)
        {
            _width = dots.GetLength(1);
            _height = dots.GetLength(0);
            _dots = dots;
            _pixelData = _convertTo1BitPixelData();
            _fileBytes = _buildFileBytes();
        }

        /// <summary>
        /// Get the byte array of the bmp file data
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return _fileBytes;
        }

        /// <summary>
        /// Save bmp to file
        /// </summary>
        public void Save(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(_fileBytes, 0, _fileBytes.Length);
            }
        }

        /// <summary>
        /// Save bmp to stream
        /// </summary>
        public void Save(Stream strm)
        {
            strm.Write(_fileBytes, 0, _fileBytes.Length);
        }

        private byte[] _convertTo1BitPixelData()
        {
            int rows = _dots.GetLength(0);
            int cols = _dots.GetLength(1);

            // intero superiore
            int bytesPerRow = (cols / 8) + ( cols % 8 == 0 ? 0 : 1 );
            // arrotonda sempre a multipli di 4 bytes
            if (bytesPerRow % 4 > 0)
                bytesPerRow += 4 - (bytesPerRow % 4);

            // Alloca spazio per i pixel
            byte[] bytes = new byte[bytesPerRow * rows];

            // Ciclo allocazione dot --> pixel
            for (int r = 0; r < rows; r++)
            {
                // Idea iniziale:  ogni byte è composto da 8 dot
                // (il problema è controllare di non sfondare la matrice dots)
                //
                //for (int c = 0; c < cols; c = c + 8)
                //{
                //    bytes[r * bytesPerRow + c / 8] = (byte)(
                //        (dots[r, c] & 1) |
                //        (dots[r, c + 1] & 1) << 1 |
                //        (dots[r, c + 2] & 1) << 2 |
                //        (dots[r, c + 3] & 1) << 3 |
                //        (dots[r, c + 4] & 1) << 4 |
                //        (dots[r, c + 5] & 1) << 5 |
                //        (dots[r, c + 6] & 1) << 6 |
                //        (dots[r, c + 7] & 1) << 7
                //        );
                //}

                // Nuova versione  (performance ???)
                for (int c = 0; c < cols; c++)
                {
                    // Attenzione: le righe dell'immagine sono memorizzate nell'ordine inverso sul file Bmp
                    bytes[(( rows - r - 1 ) * bytesPerRow) + (c / 8)] = (byte)
                         (
                             bytes[(( rows - r - 1 ) * bytesPerRow) + (c / 8)] |
                             ( ( _dots[r, c] & 1 ) << ( 7 - (c % 8) ) )
                         );
                }
            }

            return bytes;
        }

        private byte[] _buildFileBytes()
        {
            var fileHeader = new BitmapFileHeader
            {
                BfOffBits = 14 + 40 + ( 2 * 4 )   // BITMAPFILEHEADER + BITMAPINFOHEADER + 2 * RGBQUAD
            };
            fileHeader.BfSize = fileHeader.BfOffBits + (uint)_pixelData.Length; // dataLength + headersLength
            var fileHeaderBytes = fileHeader.ToByteArray();

            var infoHeader = new BitmapInfoHeader
            {
                Width = (uint)_width,
                Height = (uint)_height,
                BitCount = 1,
                SizeImage = (uint)_pixelData.Length,
                XPelsPerMeter = 3780,
                YPelsPerMeter = 3780
            };
            var infoHeaderBytes = infoHeader.ToByteArray();

            var black = new RgbQuad(0, 0, 0);
            var blackBytes = black.ToByteArray();

            var white = new RgbQuad(255, 255, 255);
            var whiteBytes = white.ToByteArray();

            // Scrittura dati
            using (var outStream = new MemoryStream())
            {
                outStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);
                outStream.Write(infoHeaderBytes, 0, infoHeaderBytes.Length);
                outStream.Write(whiteBytes, 0, whiteBytes.Length);
                outStream.Write(blackBytes, 0, blackBytes.Length);
                outStream.Write(_pixelData, 0, _pixelData.Length);
                return outStream.ToArray();
            }
        }

        private static byte[] _intTo2Bytes(ushort i)
        {
            return new byte[] {
                (byte)(i & 255),
                (byte)((i>>8) & 255)
            };
        }

        private static byte[] _intTo4Bytes(uint i)
        {
            return new byte[] {
                (byte)(i & 255),
                (byte)((i>>8) & 255),
                (byte)((i>>16) & 255),
                (byte)((i>>24) & 255)
            };
        }

        private class BitmapFileHeader
        {
            private readonly ushort _bfType = 19778;   // "BM"
            private readonly ushort _bfReserved1 = 0;  // must always be set to zero.
            private readonly ushort _bfReserved2 = 0;  // must always be set to zero.

            public uint BfSize;            // specifies the size of the file in bytes.
            public uint BfOffBits;         // specifies the offset from the beginning of the file to the bitmap data.

            public byte[] ToByteArray()
            {
                byte[] b = new byte[14];
                _intTo2Bytes(_bfType).CopyTo(b, 0);
                _intTo4Bytes(BfSize).CopyTo(b, 2);
                _intTo2Bytes(_bfReserved1).CopyTo(b, 2 + 4);
                _intTo2Bytes(_bfReserved2).CopyTo(b, 2 + 4 + 2);
                _intTo4Bytes(BfOffBits).CopyTo(b, 2 + 4 + 2 + 2);
                return b;
            }
        }

        private class BitmapInfoHeader
        {
            private readonly uint _compression = 0;   // Specifies the type of compression, usually set to zero (no compression).
            private readonly uint _clrUsed = 0;       // specifies the number of colors used in the bitmap, if set to zero the number of colors is calculated using the biBitCount member.
            private readonly uint _clrImportant = 0;  // specifies the number of color that are 'important' for the bitmap, if set to zero, all colors are important.
            private readonly ushort _planes = 1;	  // specifies the number of planes of the target device, must be set to zero.

            public uint Size = 40;          // specifies the size of the BITMAPINFOHEADER structure, in bytes.
            public uint Width = 0;	        // specifies the width of the image, in pixels.
            public uint Height = 0;	        // specifies the height of the image, in pixels.
            public ushort BitCount = 8;       // specifies the number of bits per pixel.
            public uint SizeImage = 0;	    // specifies the size of the image data, in bytes. If there is no compression, it is valid to set this member to zero.
            public uint XPelsPerMeter = 0;	// specifies the the horizontal pixels per meter on the designated targer device, usually set to zero.
            public uint YPelsPerMeter = 0;  // specifies the the vertical pixels per meter on the designated targer device, usually set to zero.

            public byte[] ToByteArray()
            {
                byte[] b = new byte[4 + 4 + 4 + 2 + 2 + 4 + 4 + 4 + 4 + 4 + 4];
                _intTo4Bytes(Size).CopyTo(b, 0);
                _intTo4Bytes(Width).CopyTo(b, 0 + 4);
                _intTo4Bytes(Height).CopyTo(b, 4 + 4);
                _intTo2Bytes(_planes).CopyTo(b, 8 + 4);
                _intTo2Bytes(BitCount).CopyTo(b, 12 + 2);
                _intTo4Bytes(_compression).CopyTo(b, 14 + 2);
                _intTo4Bytes(SizeImage).CopyTo(b, 16 + 4);
                _intTo4Bytes(XPelsPerMeter).CopyTo(b, 20 + 4);
                _intTo4Bytes(YPelsPerMeter).CopyTo(b, 24 + 4);
                _intTo4Bytes(_clrUsed).CopyTo(b, 28 + 4);
                _intTo4Bytes(_clrImportant).CopyTo(b, 32 + 4);
                return b;
            }
        }

        private class RgbQuad
        {
            public byte Blue = 0;       // specifies the blue part of the color.
            public byte Green = 0;	   // specifies the green part of the color.
            public byte Red = 0;        // specifies the red part of the color.
            private readonly byte _reserved = 0;  // must always be set to zero.

            public RgbQuad(byte red, byte green, byte blue)
            {
                Blue = blue;
                Green = green;
                Red = red;
            }

            public byte[] ToByteArray()
            {
                // Attenzione: l'ordine non è RGB ma BGR + 1 byte riservato (a 0)
                return new byte[] { Blue, Green, Red, _reserved };
            }
        }
    }
}
