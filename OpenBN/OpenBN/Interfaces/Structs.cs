namespace OpenBN
{
    public struct Size
    {
        public int W;
        public int H;

        public Size(int W, int H) : this()
        {
            this.W = W;
            this.H = H;
        }
    }
    
    public struct Point
    {
        public int X;
        public int Y;

        public Point(int X, int Y) : this()
        {
            this.X = X;
            this.Y = Y;
        }
    }

    public enum StagePnlColor
    {
        Red, Blue, None
    }

    public enum StagePnlType
    {
        NORMAL,
        CRACKED,
        BROKEN,
        POISON,
        ICE,
        GRASS,
        HOLE,
        HOLY,
        CONV_D, CONV_U,
        CONV_L, CONV_R,
        VOLCANO,
        NONE
    }

    public enum CustomBarState
    {
        Full, Loading, Paused
    }

    public enum CustomBarModifiers
    {
        Normal, Slow, Fast
    }


}
