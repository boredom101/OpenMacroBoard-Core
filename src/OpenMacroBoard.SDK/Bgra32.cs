namespace OpenMacroBoard.SDK
{
    public sealed class Bgra32 : IPixelFormat
    {
        public int BytesPerPixel { get; } = 4;

        internal Bgra32() { }

        public override bool Equals(object obj)
            => PixelFormats.Equals(this, obj as IPixelFormat);

        public override int GetHashCode()
            => this.GetType().GetHashCode();
    }
}
