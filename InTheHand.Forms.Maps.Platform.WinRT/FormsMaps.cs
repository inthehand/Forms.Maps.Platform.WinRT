using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InTheHand
{
    public static class FormsMaps
    {
        internal static string _serviceToken;

        public static void Init(string serviceToken)
        {
            _serviceToken = serviceToken;
            InTheHand.Forms.Maps.Platform.WinRT.GeocoderBackend.Register();
        }
    }
}
