namespace MAX.USPS
{
    using System;

    public class USPSManagerException : ApplicationException
    {
        public USPSManagerException(string ErrorMessage)
        {
            _Exception = new Exception(ErrorMessage);
        }

        public USPSManagerException(string ErrorMessage, Exception ex)
        {
            _Exception = new Exception(ErrorMessage, ex);
        }

        public USPSManagerException(Exception ex)
        {
            _Exception = ex;
        }

        private Exception _Exception;

        public override string Message
        {
            get
            {
                return _Exception.Message;
            }
        }

        public override string Source
        {
            get
            {
                return _Exception.Source;
            }
            set
            {
                _Exception.Source = value;
            }
        }

        public override string StackTrace
        {
            get
            {
                return _Exception.StackTrace;
            }
        }

        public override System.Collections.IDictionary Data
        {
            get
            {
                return _Exception.Data;
            }
        }

        public override string HelpLink
        {
            get
            {
                return _Exception.HelpLink;
            }
            set
            {
                _Exception.HelpLink = value;
            }
        }
    }
}