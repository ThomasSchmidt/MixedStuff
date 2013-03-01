using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace MixedStuff.CodeContracts
{
    public class CodeContractor
    {
	    public float Divide(int num, int divide)
	    {
		    Contract.Requires(divide > 1, "divide by zero?");
		    return num/divide;
	    }

		public void Test()
		{
			Divide(2, 1);
			Divide(2, 0);
		}
}
