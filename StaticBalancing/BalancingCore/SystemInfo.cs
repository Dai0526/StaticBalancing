using System.Collections.Generic;

namespace StaticBalancing
{
    public class SystemInfo
    {

        #region Member Variables
        public string m_model { get; set; }
        public string m_serialNumber { get; set; }

        public float m_homeTickOffset { get; set; }
        public float m_maxImbalance { get; set; }
        public float m_maxSpeed { get; set; }

        public List<BalancePosition> m_balancePos { get; set; }
        //public List<Counter> m_counters { get; set; }
        public Dictionary<string, Counter> m_counters { get; set; } // a record of support counter type

        #endregion

        public SystemInfo()
        {
            m_balancePos = new List<BalancePosition>();
            m_counters = new Dictionary<string, Counter>();
        }

    }
}
