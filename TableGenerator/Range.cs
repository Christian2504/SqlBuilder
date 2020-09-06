namespace TableGenerator
{
    public struct IntegralRange
    {
        public long Start;
        public long Length;

        public bool IsEmpty { get { return Length == 0; } }

        public long End { get { return Start + Length; } }

        public static IntegralRange AbsoluteRange(long start, long end)
        {
            return new IntegralRange
            {
                Start = start,
                Length = end - start
            };
        }

        public static IntegralRange RelativeRange(long start, long length)
        {
            return new IntegralRange
            {
                Start = start,
                Length = length
            };
        }

        public bool Contains(long value)
        {
            return value >= Start && value < Start + Length;
        }
    }
}
