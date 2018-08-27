namespace OpenMacroBoard.SDK
{
    public static class PixelFormats
    {
        public static Bgr24 Bgr24 { get; } = new Bgr24();
        public static Bgra32 Bgra32 { get; } = new Bgra32();

        public static bool Equals(IPixelFormat a, IPixelFormat b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a is null)
                return false;

            if (b is null)
                return false;

            if (a.BytesPerPixel != b.BytesPerPixel)
                return false;

            if (!a.GetType().Equals(b.GetType()))
                return false;

            return true;
        }
    }
}
