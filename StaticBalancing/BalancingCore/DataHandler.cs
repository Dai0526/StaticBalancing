using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticBalancing
{
    public class DataHandler
    {

        #region Load Raw data from file

        public List<InputRaw> LoadData(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("Error: File not existed. " + path);
            }

            // choose a data reader by file extension
            string ext = Path.GetExtension(path);

            switch (ext)
            {
                case ".csv":
                    return LoadDataFromCSV(path);
                case ".txt":
                    return LoadDataFromTXT(path);
                default:
                    return new List<InputRaw>();
            }

        }

        public List<InputRaw> LoadDataFromCSV(string file)
        {
            int nLine = 0;
            string line;

            List<InputRaw> data = new List<InputRaw>();
            StreamReader csvReader = new StreamReader(file);

            while ((line = csvReader.ReadLine()) != null)
            {
                if (nLine == 0 && line.Contains("Time[ms]"))
                {
                    continue; //skip header
                }

                string[] items = line.Split(',');

                if (items.Length < 4)
                {
                    throw new Exception("Input Data Corrupted. Check File " + file + " at Line " + nLine);
                }

                InputRaw record = new InputRaw();
                record.Timestamp = items[0];              //Time[ms]
                record.PosDeg = float.Parse(items[1]);    //PL.CMD([Axis1] Position command)[deg]
                record.SpdRpM = float.Parse(items[3]);    // VL.FBFILTER([Axis1] Velocity feedback filter)[rpm]

                data.Add(record);
                ++nLine;
            }

            return data;
        }

        public List<InputRaw> LoadDataFromTXT(string file)
        {
            int nLine = 0;
            string line;

            List<InputRaw> data = new List<InputRaw>();
            StreamReader csvReader = new StreamReader(file);
            char[] delimeter = new char[2];
            delimeter[0] = ',';
            delimeter[1] = ' ';

            while ((line = csvReader.ReadLine()) != null)
            {
                string[] items = line.Split(delimeter);

                if(items.Length != 14 && string.Compare(items[4], "SP=") != 0 && string.Compare(items[9], "POS=") != 0)
                {
                    throw new Exception("Input Data Corrupted. Check File " + file + " at Line " + nLine);
                }

                InputRaw record = new InputRaw();
                record.Timestamp = items[1].TrimStart('0');              //Time[ms]
                record.SpdRpM = float.Parse(items[5]);    //SP= 
                record.PosDeg = float.Parse(items[10]);    // POS=

                data.Add(record);
                ++nLine;
            }

            return data;
        }

        #endregion


        #region Arithmetic
        /*
            1. sin(Angle), cos(Angle)
            2. Regression Form
                Speed=A*cos(Angle)+B*sin(Angle)+C
                Get 
                No Wieght A0, B0, C0
                Center Weight A1 B1 C1
                Left Weight A2 B2 C2
        
            3. Diff Result from CounterWeight
                Cnt1  Acnt1 = A1-A0, Bcnt1 = B1-B0
                Cnt2  Acnt2 = A2-A0, Bcnt2 = B2-B0

            4. Get CounterWeightVector -> Draw Diagram
                Magnitude: sqrt(Acnt1^2 + Bcnt1^2)
                Phase: degree(Arctan(Acnt1, Bcnt1))

            5. x=	[(# of center weights)*(mass of center weight)]		
	                [(# of left weights)*(mass of left weight)]		
			
	                (will be solved for each instance)		
			
                M=	[A_Center Cal; A_Left Cal]		
	                [B_Center Cal; B_Left Cal]		
                
                Thus, [Acnt1, Acnt2]
                      [Bcnt1, Bcnt2]
                        
                b=	[A_System]
	                [B_System]
	
	                (will be calculated from log data)
	
	
	                M(-x)=b
	                x=-(M^-1)b
    
                    Calibration Matrix -> 
                    -(M^-1) = [A, B]
                              [C, D]
            6. Get Balance Data
               a. Balance Calculation
               Do the same thign in Step to Get A, B, C

                b = [a]
                    [b]
                
                Solve for x = -(M^-1)*b
         */
        #endregion

    }

}
