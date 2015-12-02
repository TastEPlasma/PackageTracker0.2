//////////////////////////////////////////////////////////////////////////
///This software is provided to you as-is and with not warranties!!!
///Use this software at your own risk.
///This software is Copyright by Scott Smith 2006
///You are free to use this software as you see fit.
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace MAX.USPS
{
    public class AddressValidateResponse
    {
        private AddressValidateResponse()
        {

        }

        private List<Address> _Addresses;
        /// <summary>
        /// A collection of Addresses return from the Address Validation routine.
        /// </summary>
        public List<Address> Addresses
        {
            get { return _Addresses; }
            set { _Addresses = value; }
        }



    }
}
