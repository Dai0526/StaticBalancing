using System;

namespace StaticBalancing
{
    public class FMIBalanceException : Exception
    {
        private string Details = string.Empty;
        private Exception InnerEx = null;

        public FMIBalanceException(string err, Exception innerException) : base(err, innerException)
        {
            InnerEx = innerException;
            Details = err;
        }

        public FMIBalanceException(string err):base(String.Format("Error: {0}", err))
        {
            Details = err;
        }

        public string GetInnerException()
        {
            if(InnerEx == null)
            {
                return string.Empty;
            }

            return InnerEx.ToString();
        }

        public string GetDetail()
        {
            return Details;
        }
    }
}
