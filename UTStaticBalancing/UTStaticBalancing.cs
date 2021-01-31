using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StaticBalancing;
namespace UTStaticBalancing
{

    class UTStaticBalancing
    {

        static void CaseCore()
        {
            BalancingCore sb = new BalancingCore();
            sb.ReadSystemInfoFiles(@"E:\Development\FMITools\StaticBalancing\StaticBalancing\Resource\Systems.xml");
        }

        static void CaseDataHandler()
        {
            DataHandler dh = new DataHandler();

            List<InputRaw> csvData = dh.LoadData(@"E:\Development\FMITools\StaticBalancing\reference\datasample\csv\00-0025,RDCT256-3,06MAR2020,Cal0.csv");
            Console.WriteLine("Total " + csvData.Count + " Recours were found.");


            List<InputRaw> txtData = dh.LoadData(@"E:\Development\FMITools\StaticBalancing\reference\datasample\txt\00-0002,RDCT1,06JUL2016,Cal0.txt");
            Console.WriteLine("Total " + txtData.Count + " Recours were found.");
        }
        static void CaseLoadCsv()
        {
            DataHandler dh = new DataHandler();
            List<InputRaw> list = dh.LoadDataFromCSV(@"E:\Development\FMITools\StaticBalancing\reference\datasample\csv\00-0025,RDCT256-3,06MAR2020,Cal0.csv");

            foreach (InputRaw raw in list)
            {
                Console.WriteLine(raw.ToString());
            }

            Console.WriteLine("Total " + list.Count + " Recours were found.");

        }
        static void CaseLoadText()
        {
            DataHandler dh = new DataHandler();
            List<InputRaw> list = dh.LoadDataFromTXT(@"E:\Development\FMITools\StaticBalancing\reference\datasample\txt\00-0002,RDCT1,06JUL2016,Cal0.txt");

            foreach (InputRaw raw in list)
            {
                Console.WriteLine(raw.ToString());
            }

            Console.WriteLine("Total " + list.Count + " Recours were found.");
        }

        static void CaseFitSine()
        {
            
            // Get Data
            DataHandler dh = new DataHandler();
            List<InputRaw> list = dh.LoadDataFromCSV(@"E:\Development\FMITools\StaticBalancing\reference\datasample\csv\00-0025,RDCT256-3,06MAR2020,Cal0.csv");

            List<double> angle = new List<double>();
            List<double> speed = new List<double>();
            foreach (InputRaw raw in list)
            {
                angle.Add(raw.PosDeg);
                speed.Add(raw.SpdRpM);
            }

            // init math
            double offset = 18.0;
            Arithmetic math = new Arithmetic();
            SineRegCoef coef = math.GetSineCoef(angle, speed, offset);

            Console.WriteLine("A = {0}, B = {1}", coef.A, coef.B);
        }

        static void Main(string[] args)
        {
            CaseFitSine();
        }
    }
}
