/** 
 *
 * IEC16022Sharp DataMatrix bar code generation lib
 * (c) 2007 Fabrizio Accatino <fhtino@yahoo.com>
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


namespace IEC16022Sharp
{
    public static class DMImgUtility
    {
        /// <summary>
        /// ....
        /// </summary>
        public static Bitmap SimpleResizeBmp(Bitmap inBmp, int resizeFactor, int boderSize)
        {
            int drawAreaW = resizeFactor * inBmp.Width;
            int drawAreaH = resizeFactor * inBmp.Height;
            Bitmap outBmp = new Bitmap(drawAreaW + 2 * boderSize, drawAreaH + 2 * boderSize);
            Graphics g = Graphics.FromImage(outBmp);

            // Imposta parametri per il resizing 
            // (Attenzione: senza PixelOffsetMode a HighQuality viene tagliato un pezzo dell'immagine)
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            // Sfondo bianco
            g.FillRectangle(Brushes.White, 0, 0, outBmp.Width, outBmp.Height);

            // Disegna immagine
            g.DrawImage(inBmp, new Rectangle(boderSize, boderSize, drawAreaW, drawAreaH), 0, 0, inBmp.Width, inBmp.Height, GraphicsUnit.Pixel);

            g.Dispose();
            return outBmp;
        }
    }
}
