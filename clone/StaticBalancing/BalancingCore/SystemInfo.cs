using System.Collections.Generic;

namespace StaticBalancing
{
    public class SystemInfo
    {

        #region Member Variables
        public string m_model { get; set; }
        public string m_serialNumber { get; set; }

        public double m_homeTickOffset { get; set; }
        public double m_maxImbalance { get; set; }
        public double m_maxSpeed { get; set; }

        public List<BalancePosition> m_balancePos { get; set; }
        public Dictionary<string, Counter> m_counters { get; set; } // a record of support counter type

        #endregion

        public SystemInfo()
        {
            m_balancePos = new List<BalancePosition>();
            m_counters = new Dictionary<string, Counter>();
        }

    }
}
