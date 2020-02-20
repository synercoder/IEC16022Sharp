using System;
using System.Text;

namespace IEC16022Sharp
{
    public class DataMatrix
    {
        private readonly byte[] _data;
        private readonly EncodingType _globalEncoding = EncodingType.NotDef;
        private int _width;
        private int _height;
        private FastBWBmp _bmpBW = null;
        private byte[] _encoding = null;
        private string _hexPbm = null;

        public DataMatrix(string message)
            : this(Encoding.ASCII.GetBytes(message), 0, 0, EncodingType.NotDef)
        { }

        public DataMatrix(string message, EncodingType globalEncoding)
            : this(Encoding.ASCII.GetBytes(message), 0, 0, globalEncoding)
        { }

        public DataMatrix(string message, int w, int h)
            : this(Encoding.ASCII.GetBytes(message), w, h, EncodingType.NotDef)
        { }

        public DataMatrix(string message, int w, int h, EncodingType globalEncoding)
            : this(Encoding.ASCII.GetBytes(message), w, h, globalEncoding)
        { }

        public DataMatrix(byte[] data, int w, int h, EncodingType globalEncoding)
            : this(data, w, h, globalEncoding, null)
        { }

        /// <summary>
        /// Internal costructor
        /// </summary>
        private DataMatrix(byte[] data, int w, int h, EncodingType globalEncoding, byte[] encoding)
        {
            _data = data;
            _width = w;
            _height = h;
            _globalEncoding = globalEncoding;
            _encoding = encoding;

            try
            {
                PixelArray = _build();
            }
            catch (DataMatrixException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DataMatrixException("Internal error", ex);
            }
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
                    int rows = PixelArray.GetLength(1);
                    int cols = PixelArray.GetLength(0);
                    var newDotMatrix = new BarColor[rows, cols];

                    for (int r = 0; r < rows; r++)
                        for (int c = 0; c < cols; c++)
                            newDotMatrix[rows - r - 1, c] = PixelArray[c, r];

                    _bmpBW = new FastBWBmp(newDotMatrix);
                }
                return _bmpBW;
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
                    int rows = PixelArray.GetLength(1);
                    int cols = PixelArray.GetLength(0);

                    int bitCount = 7;
                    int hexDigitCount = 0;
                    byte currentByte = (byte)0;
                    var sb = new StringBuilder();

                    for (int r = rows - 1; r >= 0; r--)
                        for (int c = 0; c < cols; c++)
                        {
                            byte currentBit = (byte)( ((byte)PixelArray[c, r]) << bitCount );
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
        public Immutable2DArray<BarColor> PixelArray { get; }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        private Immutable2DArray<BarColor> _build()
        {
            //byte[] encoding = null;
            int lenp = 0;
            int maxp = 0;
            int eccp = 0;

            // globaEncoding is present
            if (_globalEncoding != EncodingType.NotDef)
            {
                _encoding = new byte[_data.Length + 1];
                byte e = _encodingTypeToByte(_globalEncoding);
                for (int i = 0; i < _encoding.Length; i++)
                    _encoding[i] = e;
                _encoding[_data.Length] = 0;
            }

            // Matrix creation
            var iec16022 = new IEC16022ecc200();
            byte[] array = iec16022.Iec16022ecc200(
                       ref _width,
                       ref _height,
                       ref _encoding,
                       _data.Length,
                       _data,
                       ref lenp,
                       ref maxp,
                       ref eccp);

            if (array == null)
                throw new DataMatrixException("Error building datamatrix: " + iec16022.ErrorMessage);

            var resultArray = new BarColor[_width, _height];
            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                    resultArray[x, y] = array[(_width * y) + x] == 0
                        ? BarColor.Black
                        : BarColor.White;

            return new Immutable2DArray<BarColor>(resultArray);
        }

        /// <summary>
        /// Convert EncodingType enum to internal format (byte)
        /// </summary>
        private byte _encodingTypeToByte(EncodingType et)
        {
            return et switch
            {
                EncodingType.Ascii => (byte)'A',
                EncodingType.Binary => (byte)'B',
                EncodingType.C40 => (byte)'C',
                EncodingType.Edifact => (byte)'E',
                EncodingType.Text => (byte)'T',
                EncodingType.X12 => (byte)'X',
                EncodingType.NotDef => throw new ApplicationException("EncodingType not valid"),
                _ => throw new ApplicationException("Unknown EncodingType")
            };
        }
    }
}
