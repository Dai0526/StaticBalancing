using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StaticBalancing.BalancingCore;
namespace UTStaticBalancing
{
    class Program
    {
        static void Main(string[] args)
        {
            BalancingCore sb = new BalancingCore();
            sb.ReadSystemInfoFiles(@"E:\Development\FMITools\StaticBalancing\StaticBalancing\Resource\Systems.xml");
        }
    }
}
