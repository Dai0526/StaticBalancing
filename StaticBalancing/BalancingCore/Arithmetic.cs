using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearRegression;

namespace StaticBalancing
{

    public class Arithmetic
    {

        public Arithmetic()
        {
        }

        // step one: from inputdata with speeed and angle, do sinasoid regression and get coef.
        public SineRegCoef GetRegressionCoef(InputRaw raw)
        {
            SineRegCoef coef = new SineRegCoef();

            double[] speedArry = raw.Speed.ToArray<double>();
            int n = speedArry.Count();

            Vector<double> posArry = Vector<double>.Build.DenseOfArray(raw.Position.ToArray<double>()).Multiply(Math.PI / 180.0);
            Vector<double> cosArry = posArry.PointwiseCos();
            Vector<double> sinArry = posArry.PointwiseSin();

            Vector<double> consArray = Vector<double>.Build.DenseOfArray(Enumerable.Repeat(1.0, n).ToArray<double>());

            Matrix <double> X = Matrix<double>.Build.DenseOfColumnVectors(cosArry, sinArry, consArray);
            Vector<double> y = Vector<double>.Build.DenseOfArray(speedArry);

            Vector<double> reg = MultipleRegression.Svd(X, y);

            coef.A = reg[0];
            coef.B = reg[1];
            coef.C = reg[2];

            return coef;
        }


        /*
        * Matrix M = 
            [dA_Left  dA_Right]
            [dB_Left  dB_Right] 
        * x = 
            [number of left weight * mass of left]
            [number of right wieght * mass of right]

            M(-x)=b
            x=-(M^-1)b

        */
        public CalibrationResult GetCalibrationMatrix(SineRegCoef baseline, List<BalancePosition> bpList, SineRegCoef currStatus, Dictionary<string, Counter> counterSpec)
        {
            // set up M
            List<double> dAs = new List<double>();
            List<double> dBs = new List<double>();

            foreach(BalancePosition bp in bpList)
            {
                double diffA = bp.LastRunCoef.A - baseline.A;
                double diffB = bp.LastRunCoef.B - baseline.B;

                dAs.Add(diffA);
                dBs.Add(diffB);
            }

            List<double> curr = new List<double>();
            curr.Add(currStatus.A);
            curr.Add(currStatus.B);

            // set up M
            Matrix<double> M = Matrix<double>.Build.DenseOfRowArrays(dAs.ToArray(), dBs.ToArray());
            M = M.Inverse().Multiply(-1.0);
            //Set up b
            Matrix<double> b = Matrix<double>.Build.DenseOfColumnArrays(curr.ToArray());
            // solve for x
            Matrix<double> x = M.Multiply(b); // it is 1 column matrix

            double[] ans = x.ToColumnArrays()[0];

            Dictionary<string, double> item = new Dictionary<string, double>(); ;

            for(int i = 0; i < ans.Count(); ++i)
            {
                item[bpList[i].ID] = ans[i];
            }

            CalibrationResult result = new CalibrationResult();
            result.Speed = GetSpeedRpm(currStatus);
            result.SpeedVariation = GetSpeedVariation(currStatus);
            result.Phase = GetPhaseDeg(currStatus);

            foreach (BalancePosition bp in bpList)
            {
                result.WeightChange[bp.ID] = item[bp.ID] * bp.GetWeight(counterSpec);
                result.Imbalance[bp.ID] = item[bp.ID] * bp.GetAppliedImbalance(counterSpec);
            }

            result.ResidualImblance = 0.0;
            result.ForceAt240rpm = result.ResidualImblance  / 1000 * 25.1327411999999 * 25.1327411999999;

            return result;
        }

        public double GetPhaseDeg(SineRegCoef coef)
        {
            double phase = Math.Tanh(coef.B/ coef.A);
            return phase;
        }

        public double GetMagnitude(SineRegCoef coef)
        {
            return Math.Sqrt(coef.A * coef.A + coef.B * coef.B);
        }

        public double GetSpeedVariation(SineRegCoef coef)
        {
            double speVar = 2 * Math.Sqrt(coef.A * coef.A + coef.B * coef.B) / coef.C;
            return speVar;
        }

        public double GetSpeedRpm(SineRegCoef coef)
        {
            return coef.C;
        }

        public double GetPhaseDiff(List<double> phaseList)
        {
            return 0.0;
        }

    }


}
