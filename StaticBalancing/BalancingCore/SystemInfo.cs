using System.Collections.Generic;


namespace StaticBalancing.BalancingCore
{
    public class SystemInfo
    {

        #region Struct And Enum
        public enum StackDirection
        {
            UNKNOWN = 0,
            RADIAL_POS = 1, // r+
            RADIAL_NEG = 2, // r-
            AXIAL = 3       // a
        }

        public struct BalancePosition
        {
            public string ID;
            public float Radius;
            public float Angle;
            public float MaxStackHeight;
            public StackDirection StackDir;
        }

        public struct Counter
        {
            public string PartNumber;
            public float Mass;
            public float Thickness;
        }

        #endregion

        #region Member Variables
        public string m_model { get; set; }
        public string m_serialNumber { get; set; }

        public float m_homeTickOffset { get; set; }
        public float m_maxImbalance { get; set; }
        public float m_maxSpeed { get; set; }

        public List<BalancePosition> m_balancePos { get; set; }
        public List<Counter> m_counters { get; set; }

        #endregion

        public SystemInfo()
        {
            m_balancePos = new List<BalancePosition>();
            m_counters = new List<Counter>();
        }

    }
}
