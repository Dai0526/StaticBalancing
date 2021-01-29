using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticBalancing
{
    #region SystemInfo TypeDefs

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


    #region Balancing Data TypeDefs

    public struct InputRaw
    {
        public string Timestamp;
        public float SpdRpM; // velocity feedback rpm
        public float PosDeg; // pl.cmd [deg

        public override string ToString()
        {
            string format = "Timestamp = {0}, Velocity(rpm) = {1}, Angle = {2}";

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(format, Timestamp, SpdRpM, PosDeg);
            return sb.ToString();
        }
    }

    public struct BalancingData
    {
        public string Date;
        public string SerialNumber;
        public string Label;
        public List<InputRaw> Records;
    }


    #endregion

}
