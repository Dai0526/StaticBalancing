using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StaticBalancing
{
    public class BalancingCore
    {

        #region Member
        public Dictionary<string, SystemInfo> m_systemArchives;
        public SystemInfo m_systemSelected = null;
        #endregion


        public BalancingCore()
        {
            
        }

        // Load System Info
        public void LoadSystemInfos()
        {
            m_systemArchives = new Dictionary<string, SystemInfo>();
        }

        public void ReadSystemInfoFiles(string path)
        {
            try
            {
                m_systemArchives = new Dictionary<string, SystemInfo>();

                XmlDocument xdc = new XmlDocument();
                xdc.Load(path);

                XmlNodeList nodes = xdc.SelectNodes("SystemInfo/System");

                foreach (XmlNode node in nodes)
                {
                    SystemInfo si = new SystemInfo();

                    si.m_model = Convert.ToString(node["Model"].InnerText);
                    si.m_homeTickOffset = double.Parse(node["HomeTickOffset"].InnerText);
                    si.m_maxImbalance = double.Parse(node["MaxImbalance"].InnerText);
                    si.m_maxSpeed = double.Parse(node["MaxSpeed"].InnerText);


                    XmlNodeList bpNodes = node.SelectNodes("BalancePositionList/position");
                    foreach (XmlNode bps in bpNodes)
                    {
                        BalancePosition pos = new BalancePosition();
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
                        Counter ctr = new Counter(Convert.ToString(cts.Attributes["pn"].Value),
                                                    float.Parse(cts.Attributes["mass"].Value),
                                                    float.Parse(cts.Attributes["thickness"].Value));
                        si.m_counters[ctr.PartNumber] = ctr;
                    }

                    m_systemArchives[si.m_model] = si;

                }
            }catch(Exception ex)
            {
                throw new Exception("ReadSystemInfoFiles occurs at BalancingCore: " + ex.Data);
            }
        }

        private StackDirection GetStackDirection(string s)
        {
            switch (s)
            {
                case "Axial":
                    return StackDirection.AXIAL;
                case "Radial-":
                    return StackDirection.RADIAL_NEG;
                case "Radial+":
                    return StackDirection.RADIAL_POS;
                default:
                    return StackDirection.UNKNOWN;
            }
        }

        public void SetCurrentSystem(string model, string serial = "")
        {
            if (!m_systemArchives.ContainsKey(model))
            {
                throw new Exception("Error: Target model " + model + " not found.");
            }

            m_systemSelected = m_systemArchives[model];
            m_systemSelected.m_serialNumber = serial;
        }

        public SystemInfo GetCurrentSystem()
        {
            return m_systemSelected;
        }

        // for rnd use
        public void CreateNewSystem()
        {

        }

        // Load Data
        public void LoadBalancingData()
        {

        }

        // Process Data


        // Save Data

    }
}
