using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLApp;
namespace StaticBalancing
{

    public class Arithmetic
    {



        private const string FUNCTION_FILE_PATH2 = @"pack://application:,,,/StaticBalancing;component/Resource/MatlbFunc";
        public MLApp.MLApp m_matlab; // = new MLApp.MLApp();

        public Arithmetic()
        {
            string cmd = string.Format(@"cd {0}", FUNCTION_FILE_PATH2);

            m_matlab = new MLApp.MLApp();
            m_matlab.Execute(cmd);
        }

        public SineRegCoef GetSineCoef(List<double> angle, List<double> speed, double offset)
        {
            SineRegCoef coef = new SineRegCoef();

            System.Array angleIn = angle.ToArray<double>();
            System.Array speedIn = speed.ToArray<double>();

            object result = null;
            m_matlab.Feval("sinefit", 3, out result, angleIn, speedIn, offset);
            object[] res = result as object[];

            coef.A = (double)res[0];
            coef.B = (double)res[1];
            return coef;
        }

    }


}
