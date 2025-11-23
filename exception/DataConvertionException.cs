namespace HandHistoryParser.exception
{
    [Serializable]
    internal class DataConvertionException : Exception
    {
        public DataConvertionException()
        {
        }

        public DataConvertionException(string? message) : base(message)
        {
        }

        public DataConvertionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}