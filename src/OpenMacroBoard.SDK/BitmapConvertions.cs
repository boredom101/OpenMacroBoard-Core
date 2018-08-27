using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace OpenMacroBoard.SDK
{
    internal unsafe static class BitmapConvertions
    {
        public class TransformationAction
        {
            private UnsafeTransformationAction action;

            private TransformationAction(UnsafeTransformationAction action)
            {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }
        }

        public static TransformationAction Bgr24ToBgr24 { get; }

        static BitmapConvertions()
        {
            Bgr24ToBgr24 = new TransformationAction(Transform_Bgr24ToBgr24);
        }

        public static BitmapData ToRawBgr24(this Bitmap bitmap)
        {
            var w = bitmap.Width;
            var h = bitmap.Height;

            System.Drawing.Imaging.BitmapData data = null;
            try
            {
                data = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, bitmap.PixelFormat);

                var targetFormat = PixelFormats.Bgr24;
                var targetStride = targetFormat.BytesPerPixel * w;

                var targetRawBitmap = new BitmapData(w, h, targetStride, targetFormat);

                fixed (byte* target = targetRawBitmap.data)
                {
                    BitmapTransformation(
                        //width + height
                        w, h,

                        //source info
                        (byte*)data.Scan0, data.Stride, bitmap.PixelFormat.GetBytesPerPixel(),

                        //target info
                        target, targetStride, targetFormat.BytesPerPixel,

                        //transformation
                        FindTransformationForPair(bitmap.PixelFormat, targetFormat)
                    );
                }

                return targetRawBitmap;
            }
            finally
            {
                if (data != null)
                    bitmap.UnlockBits(data);
            }
        }

        private static UnsafeTransformationAction FindTransformationForPair(PixelFormat originalFormat, IPixelFormat targetFormat)
        {
            if (targetFormat != PixelFormats.Bgr24)
                throw new NotImplementedException();

            switch (originalFormat)
            {
                case PixelFormat.Format24bppRgb: return Transform_Bgr24ToBgr24;
                case PixelFormat.Format32bppArgb: return Transform_Brga32ToBgr24;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// This transformation can be used to remap images with different strides
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="sourceStart"></param>
        /// <param name="targetData"></param>
        /// <param name="targetStart"></param>
        private static void Transform_Bgr24ToBgr24(byte* sourceData, int sourceStart, byte* targetData, int targetStart)
        {
            targetData[targetStart + 0] = sourceData[sourceStart + 0];
            targetData[targetStart + 1] = sourceData[sourceStart + 1];
            targetData[targetStart + 2] = sourceData[sourceStart + 2];
        }

        public static void Transform_Brga32ToBgr24(byte* sourceData, int sourceStart, byte* targetData, int targetStart)
        {
            double alpha = (double)sourceData[sourceStart + 3] / 255f;
            targetData[targetStart + 0] = (byte)Math.Round(sourceData[sourceStart + 0] * alpha);
            targetData[targetStart + 1] = (byte)Math.Round(sourceData[sourceStart + 1] * alpha);
            targetData[targetStart + 2] = (byte)Math.Round(sourceData[sourceStart + 2] * alpha);
        }

        public delegate void UnsafeTransformationAction(byte* sourceData, int sourceStart, byte* targetData, int targetStart);

        private static void BitmapTransformation(
            int width, int height,
            byte* srcData, int srcStride, int srcBytePerPixel,
            byte* tarData, int tarStride, int tarBytePerPixel,
            UnsafeTransformationAction tranformationAction
        )
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    var startSource = srcStride * y + x * srcBytePerPixel;
                    var startTarget = tarStride * y + x * tarBytePerPixel;
                    tranformationAction(srcData, startSource, tarData, startTarget);
                }
        }
    }
}
