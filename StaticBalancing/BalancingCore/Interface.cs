﻿using System;
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
        public Dictionary<string, int> Counters; // partname, # of counters
        public SineRegCoef LastRunCoef;

        public BalancePosition(string id)
        {
            ID = id;
            Radius = 0.0F;
            Angle = 0.0F;
            MaxStackHeight = 0.0F;
            StackDir = StackDirection.UNKNOWN;
            LastRunCoef = new SineRegCoef();
            Counters = new Dictionary<string, int>();
        }

        public double GetWeight(Dictionary<string, Counter> counterSpec)
        {
            double weight = 0.0;

            foreach(KeyValuePair<string, int> cntr in Counters)
            {
                weight += cntr.Value * counterSpec[cntr.Key].Mass;
            }

            return weight;
        }

        public double GetAppliedImbalance(Dictionary<string, Counter> counterSpec)
        {
            return GetWeight(counterSpec) * Radius; // *1000/1000
        }
        
        public bool ValidateCounterSize(Dictionary<string, Counter> counterSpec)
        {
            double length = 0.0;
            foreach (KeyValuePair<string, int> cntr in Counters)
            {
                length += cntr.Value * counterSpec[cntr.Key].Thickness;
            }

            return length <= MaxStackHeight;
        }

    }

    public struct Counter
    {
        public string PartNumber;
        public float Mass;
        public float Thickness;

        public Counter(string pn, float mass, float thickness)
        {
            PartNumber = pn;
            Mass = mass;
            Thickness = thickness;
        }
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

    public struct CalibrationResult
    {
        public string Label;

        public double Speed;
        public double SpeedVariation;
        public double Phase;

        public Dictionary<string, double> WeightChange;
        public Dictionary<string, double> Imbalance;

        public double ResidualImblance;
        public double ForceAt240rpm;

        public CalibrationResult(string id = "")
        {
            Label = id;
            Speed = 0.0;
            SpeedVariation = 0.0;
            Phase = 0.0;
            WeightChange = new Dictionary<string, double>();
            Imbalance = new Dictionary<string, double>();

            ResidualImblance = 0.0;
            ForceAt240rpm = 0.0;
        }

    } 

    #endregion

}
