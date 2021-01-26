using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace StaticBalancing.BalancingCore
{
    public class BalancingCore
    {

        #region Member
        public Dictionary<string, SystemInfo> m_systemCollection;
        #endregion


        public BalancingCore()
        {
            
        }

        // Load System Info
        public void LoadSystemInfos()
        {
            m_systemCollection = new Dictionary<string, SystemInfo>();



        }

        public bool ReadSystemInfoFiles(string path)
        {
            m_systemCollection = new Dictionary<string, SystemInfo>();

            XmlDocument xdc = new XmlDocument();
            xdc.Load(path);

            XmlNodeList nodes = xdc.SelectNodes("SystemInfo/System");

            foreach(XmlNode node in nodes)
            {
                SystemInfo si = new SystemInfo();

                si.m_model = Convert.ToString(node["Model"].InnerText);
                si.m_homeTickOffset = float.Parse(node["HomeTickOffset"].InnerText);
                si.m_maxImbalance = float.Parse(node["MaxImbalance"].InnerText);
                si.m_maxSpeed = Convert.ToInt32(node["MaxSpeed"].InnerText);


                XmlNodeList bpNodes = node.SelectNodes("BalancePositionList/position");
                foreach(XmlNode bps in bpNodes)
                {
                    SystemInfo.BalancePosition pos = new SystemInfo.BalancePosition();
                    pos.ID = Convert.ToString(bps.Attributes["id"].Value);
                    pos.Radius = float.Parse(bps.Attributes["radius"].Value);
                    pos.Angle = float.Parse(bps.Attributes["angle"].Value);
                    pos.MaxStackHeight = float.Parse(bps.Attributes["maxStackHeight"].Value);
                    pos.StackDir = GetStackDirection(Convert.ToString(bps.Attributes["direction"].Value));

                    si.m_balancePos.Add(pos);
                }

                XmlNodeList ctNodes = node.SelectNodes("CounterTypeList/counter");
                foreach (XmlNode cts in ctNodes)
                {
                    SystemInfo.Counter ctr = new SystemInfo.Counter();
                    ctr.PartNumber = Convert.ToString(cts.Attributes["pn"].Value);
                    ctr.Mass = float.Parse(cts.Attributes["mass"].Value);
                    ctr.Thickness = float.Parse(cts.Attributes["thickness"].Value);

                    si.m_counters.Add(ctr);
                }

                m_systemCollection[si.m_model] = si;

            }

            return true;
        }

        private SystemInfo.StackDirection GetStackDirection(string s)
        {
            switch (s)
            {
                case "Axial":
                    return SystemInfo.StackDirection.AXIAL;
                case "Radial-":
                    return SystemInfo.StackDirection.RADIAL_NEG;
                case "Radial+":
                    return SystemInfo.StackDirection.RADIAL_POS;
                default:
                    return SystemInfo.StackDirection.UNKNOWN;
            }
        }

        // for rnd use
        public void CreateNewSystem()
        {

        }

        // Load Data


        // Process Data


        // Save Data

    }
}
