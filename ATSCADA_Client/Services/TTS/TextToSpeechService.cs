namespace ATSCADA_Client.Services.TTS
{
    public static class TextToSpeechService
    {
        // Bảng phát âm chữ cái tiếng Việt
        private static readonly Dictionary<char, string> VietnameseAlphabetPronunciation = new Dictionary<char, string>
    {
        { 'A', "a" },
        { 'B', "bê" },
        { 'C', "xê" },
        { 'D', "đê" },
        { 'E', "e" },
        { 'G', "gờ" },
        { 'H', "hắt" },
        { 'I', "i" },
        { 'K', "ca" },
        { 'L', "lờ" },
        { 'M', "mờ" },
        { 'N', "nờ" },
        { 'O', "o" },
        { 'P', "pê" },
        { 'Q', "quy" },
        { 'R', "rờ" },
        { 'S', "ét" },
        { 'T', "tê" },
        { 'U', "u" },
        { 'V', "vê" },
        { 'X', "ích" },
        { 'Y', "i" },
        // Bổ sung thêm nếu bạn cần (F, J, W, Z...)
    };

        /// <summary>
        /// Chèn khoảng trắng giữa các ký tự và chuyển chữ cái thành phát âm tiếng Việt.
        /// </summary>
        public static string InsertSpacesBetweenCharacters(string input)
        {
            var result = new List<string>();

            foreach (var c in input.ToUpperInvariant()) // xử lý hoa/thường thống nhất
            {
                if (VietnameseAlphabetPronunciation.TryGetValue(c, out string pronunciation))
                {
                    result.Add(pronunciation);
                }
                else
                {
                    result.Add(c.ToString());
                }
            }

            return string.Join(" ", result);
        }
    }
}
