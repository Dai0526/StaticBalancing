using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
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

        public Matrix<double> GetCoefMatrix(SineRegCoef baseline, List<BalancePosition> bpList)
        {
            // set up M
            List<double> dAs = new List<double>();
            List<double> dBs = new List<double>();

            foreach (BalancePosition bp in bpList)
            {
                double diffA = bp.CalibrationRunCoef.A - baseline.A;
                double diffB = bp.CalibrationRunCoef.B - baseline.B;

                dAs.Add(diffA);
                dBs.Add(diffB);
            }

            // set up M
            Matrix<double> M = Matrix<double>.Build.DenseOfRowArrays(dAs.ToArray(), dBs.ToArray());
            M = M.Inverse().Multiply(-1.0);

            return M;
        }

        public Dictionary<string, double> SolveCalibrationMatrix(Matrix<double> M, SineRegCoef currStatus, List<BalancePosition> bpList)
        {
            //Set up b
            List<double> curr = new List<double>();
            curr.Add(currStatus.A);
            curr.Add(currStatus.B);
            Matrix<double> b = Matrix<double>.Build.DenseOfColumnArrays(curr.ToArray());

            // solve for x
            Matrix<double> x = M.Multiply(b); // it is 1 column matrix

            double[] ans = x.ToColumnArrays()[0];

            Dictionary<string, double> item = new Dictionary<string, double>();

            for (int i = 0; i < ans.Count(); ++i)
            {
                item[bpList[i].ID] = ans[i];
            }

            return item;
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
        public CalibrationResult Calibrate(SineRegCoef baseline, List<BalancePosition> bpList, SineRegCoef currStatus, Dictionary<string, Counter> counterSpec, float maxSpeed)
        {
            Matrix<double> M = GetCoefMatrix(baseline, bpList);
            Dictionary<string, double> item = SolveCalibrationMatrix(M, currStatus, bpList);

            CalibrationResult result = new CalibrationResult(GetCurrentTimestamp());
            result.CalibrationMatrix = M;
            
            result.Speed = GetSpeedRpm(currStatus);
            result.SpeedVariation = GetSpeedVariation(currStatus);
            result.Phase = GetPhaseDeg(currStatus);
            result.BaseCoef = currStatus;
            foreach (BalancePosition bp in bpList)
            {
                ForceVector fv = new ForceVector();
                fv.ID = bp.ID;
                fv.Imbalance = item[bp.ID] * bp.GetAppliedImbalance(counterSpec);
                fv.WeightChange = item[bp.ID] * bp.GetWeight(counterSpec);

                // get vector 
                double diffA = bp.CalibrationRunCoef.A - baseline.A;
                double diffB = bp.CalibrationRunCoef.B - baseline.B;

                Vector dv = new Vector();
                dv.Phase = Math.Atan2(diffB, diffA);
                dv.Magnitude = Math.Sqrt(diffA * diffA + diffB * diffB);

                fv.CoefDiffVector = dv;

                result.ForceVectors[bp.ID] = fv;

                result.WeightChange[bp.ID] = fv.WeightChange;
            }

            List<ForceVector> fvs = result.ForceVectors.Values.ToList();
            result.ResidualImblance = GetResidualImbalance(fvs);
            result.ForceAtMaxSpeed = result.ResidualImblance  / 1000 * Math.Pow(maxSpeed * Math.PI / 30.0, 2);

            return result;
        }

        public MeasurementData Measure(SineRegCoef baseline, Matrix<double> CaliMatrix, SineRegCoef measureCoef, List<BalancePosition> bpList, Dictionary<string, Counter> counterSpec, float maxSpeed)
        {
            Dictionary<string, double> item = SolveCalibrationMatrix(CaliMatrix, measureCoef, bpList);

            MeasurementData result = new MeasurementData();
            result.Timestamp = GetCurrentTimestamp();
            result.Speed = GetSpeedRpm(measureCoef);
            result.SpeedVariation = GetSpeedVariation(measureCoef);
            result.Angle = GetPhaseDeg(measureCoef);
            result.StatusCoef = measureCoef;
            result.DeWeightMap = new Dictionary<string, double>();

            List<ForceVector> fvs = new List<ForceVector>();
            foreach (BalancePosition bp in bpList)
            {
                ForceVector fv = new ForceVector();
                fv.ID = bp.ID;
                fv.Imbalance = item[bp.ID] * bp.GetAppliedImbalance(counterSpec);
                fv.WeightChange = item[bp.ID] * bp.GetWeight(counterSpec);

                // get vector 
                double diffA = bp.CalibrationRunCoef.A - baseline.A;
                double diffB = bp.CalibrationRunCoef.B - baseline.B;

                Vector dv = new Vector();
                dv.Phase = Math.Atan2(diffB, diffA);
                dv.Magnitude = Math.Sqrt(diffA * diffA + diffB * diffB);

                fv.CoefDiffVector = dv;
                fvs.Add(fv);

                
                result.DeWeightMap[bp.ID] = fv.WeightChange;
            }

            result.Imbalance = GetResidualImbalance(fvs);
            result.ForceAtMaxSpeed = result.Imbalance / 1000 * Math.Pow(maxSpeed * Math.PI / 30.0, 2);

            return result;
        }


        public double GetPhaseDeg(SineRegCoef coef)
        {
            double phase = Math.Atan2(coef.B, coef.A) / Math.PI * 180;
            return phase;
        }

        public double GetMagnitude(SineRegCoef coef)
        {
            return Math.Sqrt(coef.A * coef.A + coef.B * coef.B);
        }

        public double GetSpeedVariation(SineRegCoef coef)
        {
            double speVar = 2.0 * Math.Sqrt(coef.A * coef.A + coef.B * coef.B) / coef.C;
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

        public double GetResidualImbalance(List<ForceVector> fvs)
        {
            int n = fvs.Count();

            double mag = 0.0;
            double phase = 0.0;

            for(int i = 0; i < n; ++i)
            {
                ForceVector curr = fvs[i];

                double x1 = mag * Math.Cos(phase);
                double y1 = mag * Math.Sin(phase);

                double x2 = curr.Imbalance * Math.Cos(curr.CoefDiffVector.Phase);
                double y2 = curr.Imbalance * Math.Sin(curr.CoefDiffVector.Phase);

                double x = x1 + x2;
                double y = y1 + y2;

                mag = Math.Sqrt(x * x + y * y);
                phase = Math.Atan2(y, x);
            }

            return mag;
        }

        public static string GetCurrentTimestamp()
        {
            DateTime now = DateTime.Now;
            return now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }


}
