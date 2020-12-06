using OpenBN.Interfaces;

namespace OpenBN.BattleElements
{
    public class Panel
    {
        public StagePnlColor StgPnlClr { get; set; }
        public StagePnlType StgPnlTyp { get; set; }
        public Point StgPnlPos { get; set; }
        public Point StgRowCol { get; set; }
    }
}