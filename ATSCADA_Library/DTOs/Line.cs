using CallQueue.Core;

namespace ATSCADA_Library.DTOs
{
    [Serializable]
    public class Line
    {
        public string Text { get; set; }

        public HeightSize HeightSize { get; set; }

        public WidthSize WidthSize { get; set; }

        public JustifyMode Justify { get; set; }

        public PrintMode PrintMode { get; set; }
    }
}
