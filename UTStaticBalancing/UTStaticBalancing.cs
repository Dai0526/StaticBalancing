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

            InputRaw csvData = dh.LoadData(@"E:\Development\FMITools\StaticBalancing\reference\datasample\csv\00-0025,RDCT256-3,06MAR2020,Cal0.csv");
            Console.WriteLine("Total " + csvData.Count + " Recours were found.");


            InputRaw txtData = dh.LoadData(@"E:\Development\FMITools\StaticBalancing\reference\datasample\txt\00-0002,RDCT1,06JUL2016,Cal0.txt");
            Console.WriteLine("Total " + txtData.Count + " Recours were found.");
        }
        static void CaseLoadCsv()
        {
            DataHandler dh = new DataHandler();
            InputRaw data = dh.LoadDataFromCSV(@"E:\Development\FMITools\StaticBalancing\reference\datasample\csv\00-0025,RDCT256-3,06MAR2020,Cal0.csv");

            Console.WriteLine(data);
            Console.WriteLine("Total " + data.Count + " Records were found.");

        }
        static void CaseLoadText()
        {
            DataHandler dh = new DataHandler();
            InputRaw data = dh.LoadDataFromTXT(@"E:\Development\FMITools\StaticBalancing\reference\datasample\txt\00-0002,RDCT1,06JUL2016,Cal0.txt");

            Console.WriteLine(data);
            Console.WriteLine("Total " + data.Count + " Recours were found.");
        }

        static void CaseArithmetic()
        {
            CaseFitSine();
            CaseGetMatrix();
        }

        static void CaseGetMatrix()
        {
            SystemInfo sys = new SystemInfo();
            
            // make counter list
            //<counter pn = "16-3241" mass = "1.7" thickness = "6"/>
            Counter cnt3241 = new Counter("16-3241", 1.7F, 6);
            Dictionary<string, Counter> counterSpec = new Dictionary<string, Counter>();
            counterSpec[cnt3241.PartNumber] = cnt3241;

            // Create Balance Position
            BalancePosition Left = new BalancePosition("left");
            Left.ID = "5'0Clock";
            Left.Counters[cnt3241.PartNumber] = 2;
            Left.StackDir = StackDirection.RADIAL_NEG;
            BalancePosition Right = new BalancePosition("right");
            Right.ID = "7'0Clock";
            Right.Counters[cnt3241.PartNumber] = 2;
            Right.StackDir = StackDirection.RADIAL_NEG;
            DataHandler dh = new DataHandler();
            InputRaw raw = dh.LoadDataFromCSV(@"E:\Development\FMITools\StaticBalancing\reference\datasample\csv\00-0025,RDCT256-3,06MAR2020,Cal0.csv");
            InputRaw L = dh.LoadDataFromCSV(@"E:\Development\FMITools\StaticBalancing\reference\datasample\csv\00-0025,RDCT256-3,06MAR2020,CalL.csv");
            InputRaw R = dh.LoadDataFromCSV(@"E:\Development\FMITools\StaticBalancing\reference\datasample\csv\00-0025,RDCT256-3,06MAR2020,CalR.csv");

            Arithmetic math = new Arithmetic();
            SineRegCoef coef0 = math.GetRegressionCoef(raw);
            SineRegCoef coefL = math.GetRegressionCoef(L);
            SineRegCoef coefR = math.GetRegressionCoef(R);

            Left.LastRunCoef = coefL;
            Right.LastRunCoef = coefR;

            sys.m_balancePos.Add(Left);
            sys.m_balancePos.Add(Right);

            CalibrationResult result = math.GetCalibrationMatrix(coef0, sys.m_balancePos, coef0, counterSpec, 240);

            

        }

        static void CaseFitSine()
        {
            
            // Get Data
            DataHandler dh = new DataHandler();
            InputRaw raw = dh.LoadDataFromCSV(@"E:\Development\FMITools\StaticBalancing\reference\datasample\csv\00-0025,RDCT256-3,06MAR2020,Cal0.csv");

            // init math
            Arithmetic math = new Arithmetic();
            SineRegCoef coef = math.GetRegressionCoef(raw);

            Console.WriteLine("A = {0}, B = {1}", coef.A, coef.B);
        }

        static void Main(string[] args)
        {
            CaseGetMatrix();
        }
    }
}
