namespace Vostok.Metrics.Hercules
{
    internal struct ByteArrayKey
    {
        private readonly byte[] bytes;
        private readonly long offset;
        private readonly long length;
        private readonly int hashCode;

        public ByteArrayKey(byte[] bytes, long offset, long length)
        {
            this.bytes = bytes;
            this.offset = offset;
            this.length = length;
            
            unchecked
            {
                hashCode = 17;
                for (var i = 0; i < length; i++)
                {
                    hashCode = hashCode * 23 + bytes[offset + i];
                }
            }
        }

        public override bool Equals(object obj)
        {
            var other = (ByteArrayKey)obj;
            return Compare(this, other);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        private static unsafe bool Compare(ByteArrayKey first, ByteArrayKey second)
        {
            if (first.bytes == null || second.bytes == null || first.length != second.length)
                return false;

            fixed (byte* p1 = first.bytes, p2 = first.bytes)
            {
                byte* x1 = p1, x2 = p2;
                x1 += first.offset;
                x2 += second.offset;

                var l = first.length;
                for (var i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*(long*)x1 != *(long*)x2) return false;
                if ((l & 4) != 0)
                {
                    if (*(int*)x1 != *(int*)x2) return false;
                    x1 += 4;
                    x2 += 4;
                }
                if ((l & 2) != 0)
                {
                    if (*(short*)x1 != *(short*)x2) return false;
                    x1 += 2;
                    x2 += 2;
                }
                if ((l & 1) != 0) if (*x1 != *x2) return false;
                return true;
            }
        }
    }
}