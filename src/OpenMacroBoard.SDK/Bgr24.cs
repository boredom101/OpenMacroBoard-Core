namespace OpenMacroBoard.SDK
{
    public sealed class Bgr24 : IPixelFormat
    {
        public int BytesPerPixel { get; } = 3;

        internal Bgr24() { }

        public override bool Equals(object obj)
            => PixelFormats.Equals(this, obj as IPixelFormat);

        public override int GetHashCode()
            => this.GetType().GetHashCode();
    }
}
