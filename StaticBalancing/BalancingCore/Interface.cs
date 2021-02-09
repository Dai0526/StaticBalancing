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
        public List<double> Speed;    // rpm - velocity feedback
        public List<double> Position; // degree - pl.cmd
        public int Count;

        public InputRaw(int n = 0)
        {
            Count = n;
            Speed = new List<double>();
            Position = new List<double>();
        }

        public override string ToString()
        {
            string format = "Velocity(rpm) = {0}, Angle = {1}\r\n";
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Speed.Count; ++i)
            {
                sb.AppendFormat(format, Speed[i], Position[i]);
            }

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

    #region Arithmetic
    public struct SineRegCoef
    {
        public double A;
        public double B;
        public double C;
    }

    #endregion

}
