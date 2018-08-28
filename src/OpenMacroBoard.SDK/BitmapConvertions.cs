using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenMacroBoard.SDK
{
    internal static class DefaultBitmapConvertions
    {
        public static void Register()
        {
            BitmapConvertions.RegisterConvertion<Bgra32, Bgr24>(Transform_Brga32ToBgr24);
        }

        private unsafe static void Transform_Bgr24ToBgr24(IntPtr sourceData, int sourceStart, IntPtr targetData, int targetStart)
        {
            var src = (byte*)sourceData;
            var tar = (byte*)targetData;

            tar[targetStart + 0] = src[sourceStart + 0];
            tar[targetStart + 1] = src[sourceStart + 1];
            tar[targetStart + 2] = src[sourceStart + 2];
        }

        private unsafe static void Transform_Brga32ToBgr24(IntPtr sourceData, int sourceStart, IntPtr targetData, int targetStart)
        {
            var src = (byte*)sourceData;
            var tar = (byte*)targetData;

            double alpha = (double)src[sourceStart + 3] / 255f;
            tar[targetStart + 0] = (byte)Math.Round(src[sourceStart + 0] * alpha);
            tar[targetStart + 1] = (byte)Math.Round(src[sourceStart + 1] * alpha);
            tar[targetStart + 2] = (byte)Math.Round(src[sourceStart + 2] * alpha);
        }
    }

    internal class FixStrideConverter
    {
        public FixStrideConverter()
        {

        }

        private unsafe static void Transform_Bgr24ToBgr24(IntPtr sourceData, int sourceStart, IntPtr targetData, int targetStart)
        {
            var src = (byte*)sourceData;
            var tar = (byte*)targetData;

            tar[targetStart + 0] = src[sourceStart + 0];
            tar[targetStart + 1] = src[sourceStart + 1];
            tar[targetStart + 2] = src[sourceStart + 2];
        }
    }

    internal unsafe static class BitmapConvertions
    {
        static BitmapConvertions()
        {
            DefaultBitmapConvertions.Register();
        }

        public static void RegisterConvertion<TSource, TTarget>(TAction convertAction)
            where TSource : IPixelFormat
            where TTarget : IPixelFormat
        {

        }



        public static ITransformationAction Bgr24ToBgr24 { get; } = new TransformationAction(Transform_Bgr24ToBgr24);
        public static ITransformationAction Bgra32ToBgr24 { get; } = new TransformationAction(Transform_Bgr24ToBgr24);

        public interface ITransformationAction { }

        private class TransformationAction : ITransformationAction
        {
            public UnsafeTransformationAction action;

            public TransformationAction(UnsafeTransformationAction action)
            {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }
        }

        public delegate void TAction(IntPtr sourceData, int sourceStart, IntPtr targetData, int targetStart);

        public delegate void UnsafeTransformationAction(byte* sourceData, int sourceStart, byte* targetData, int targetStart);

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

        private static void Transform_Brga32ToBgr24(byte* sourceData, int sourceStart, byte* targetData, int targetStart)
        {
            double alpha = (double)sourceData[sourceStart + 3] / 255f;
            targetData[targetStart + 0] = (byte)Math.Round(sourceData[sourceStart + 0] * alpha);
            targetData[targetStart + 1] = (byte)Math.Round(sourceData[sourceStart + 1] * alpha);
            targetData[targetStart + 2] = (byte)Math.Round(sourceData[sourceStart + 2] * alpha);
        }

        public static KeyBitmap ToRawBgr24(this Bitmap bitmap)
        {
            var w = bitmap.Width;
            var h = bitmap.Height;

            System.Drawing.Imaging.BitmapData data = null;
            try
            {
                data = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, bitmap.PixelFormat);

                var targetFormat = PixelFormats.Bgr24;
                var targetStride = targetFormat.BytesPerPixel * w;

                var targetRawBitmap = new KeyBitmap(w, h, targetStride, targetFormat);

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

        public static void BitmapTransformation(
            int width, int height,
            byte[] srcArray, int srcStride, int srcBytePerPixel,
            byte[] tarArray, int tarStride, int tarBytePerPixel,
            ITransformationAction tranformationAction
        )
        {
            var tranform = tranformationAction as TransformationAction;
            if (tranform == null)
                throw new ArgumentException($"TransformationAction is not of Type {nameof(TransformationAction)}");
            var action = tranform.action;

            fixed (byte* srcData = srcArray)
            fixed (byte* tarData = srcArray)
            {
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        var startSource = srcStride * y + x * srcBytePerPixel;
                        var startTarget = tarStride * y + x * tarBytePerPixel;
                        action(srcData, startSource, tarData, startTarget);
                    }
            }
        }

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
