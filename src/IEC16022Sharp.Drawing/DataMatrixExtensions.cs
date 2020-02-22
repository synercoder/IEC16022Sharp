using System.Drawing;
using System.Drawing.Imaging;

namespace IEC16022Sharp
{
    /// <summary>
    /// Extension class for <see cref="DataMatrix"/>
    /// </summary>
    public static class DataMatrixExtensions
    {
        /// <summary>
        /// Get an Image (.NET) of the datamatrix
        /// </summary>
        /// <param name="dataMatrix">The datamatrix that will be used to get the image</param>
        /// <returns>The image of the datamatrix</returns>
        public static Bitmap GetBitmap(this DataMatrix dataMatrix)
        {
            // Nota: questo codice lavora solo sulle immagini 24bit

            int width = dataMatrix.PixelArray.GetLength(0);
            int height = dataMatrix.PixelArray.GetLength(1);
            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            var ptr = bmpData.Scan0;

            //int bytes = bmp.Width * bmp.Height * 3;   // BUG: la larghezza è data dallo Stride e non da bmpWidth
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int idx = ( ( height - y - 1 ) * bmpData.Stride ) + ( x * 3 );

                    if (dataMatrix.PixelArray[x, y] == BarColor.White)
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

            return bmp;
        }
    }
}
