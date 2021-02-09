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

        public void Foo(string basePath, string leftPath, string rightPath)
        {
            // 1. Get SineCoef - Base run   A0, B0
            // Get SineCoef - Left       A1, B1
            // Get SineCoef - Right      A2, B2

            // 2. Get Counter Weight - left   dA1 = (A1 - A0), dB1 = (B1 - B0)
            // Get Counter Weight - right  dA2 = (A2 - A0), dB2 = (B2 - B0)

            // 3. A*cos(x) + B*sin(x) = M*cos(x+P)
            // Get Mag left and Phase left  sqrt(dA1^2 + dB1^2), phase arctan(dA1/dB1)
            // Get Mag Right and Phase Right sqrt(dA2^2 + dB2^2), phase arctan(dA2/dB2)

            // 4. Get phase diff - draw graph


            // 5. Get Calibration matrix
        }


        // step one: from inputdata with speeed and angle, do sinasoid regression and get coef.
        public SineRegCoef GetRegressionCoef(InputRaw raw, double offset)
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

        // Need improve
        private double[] GetCos(List<double> vec)
        {
            int n = vec.Count;
            double[] temp = new double[n];

            for(int i = 0; i < n; ++i)
            {
                temp[i] = Math.Cos(vec[i]);
            }

            return temp;

        }

        // Need improve
        private double[] GetSine(List<double> vec)
        {
            int n = vec.Count;
            double[] temp = new double[n];

            for (int i = 0; i < n; ++i)
            {
                temp[i] = Math.Sin(vec[i]);
            }

            return temp;
        }
        // setep two: 

    }


}
