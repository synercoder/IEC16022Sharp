using IEC16022Sharp;
using System.Drawing.Imaging;

namespace TestApp
{
    internal static class KnownIssue
    {
        public static void Exec()
        {
            string message = "Sabry, Ely e bimbobimba.";

            var dm = new DataMatrix[]{
                new DataMatrix(message),          // NO: the result contains only "Sabry, Ely e bimbobimba"  (last dot was losted!)
                new DataMatrix(message, 20, 20),  // OK
                new DataMatrix(message, EncodingType.Ascii)  // OK
            };

            for (int i = 0; i < dm.Length; i++)
                DMImgUtility.SimpleResizeBmp(dm[i].Image, 10, 100).Save("KnownIssue_" + i + ".png", ImageFormat.Png);
        }
    }
}
