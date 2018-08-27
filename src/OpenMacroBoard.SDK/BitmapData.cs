using System;

namespace OpenMacroBoard.SDK
{
    /// <summary>
    /// Contains immutable raw bitmap data with a given format.
    /// </summary>
    public class BitmapData
    {
        internal byte[] data;

        /// <summary>
        /// Gets the pixel width of the bitmap
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the pixel height of the bitmap
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the stride (in bytes)
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// Gets the number of raw bytes that represent this bitmap
        /// </summary>
        public int DataLength { get; }

        /// <summary>
        /// Gets the bitmap format used for this bitmap.
        /// </summary>
        public IPixelFormat Format { get; }

        /// <summary>
        /// Gets a specific byte of the bitmap data
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte this[int index]
        {
            get => data[index];
        }

        internal BitmapData(int width, int height, int stride, IPixelFormat format)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            Stride = stride;
            Format = format ?? throw new ArgumentNullException(nameof(format));

            data = new byte[stride * height];
            DataLength = data.Length;
        }
    }
}
