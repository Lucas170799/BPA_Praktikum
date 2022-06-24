using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bpa_Praktikum
{
    internal static class Sensors
    {
        public const string Bg30 = "ns=3;s=\"xG1_BG30\"";
        public const string Bg31 = "ns=3;s=\"xG1_BG31\"";
        public const string RfidCntrBusy = "ns=3;s=\"dbRfidCntr\".\"ID2\".\"xBusy\"";
        public const string DbRobBusy = "ns=3;s=\"dbRob\".\"xBusy\"";
        public const string CarrierID = "ns=3;s=\"dbRfidData\".\"ID2\".\"iCarrierID\"";
        public const string OpNo = "ns=3;s=\"dbRfidData\".\"ID2\".\"Mes\".\"iOpNo\"";
        public const string OPos = "ns=3;s=\"dbRfidData\".\"ID2\".\"Mes\".\"iOPos\"";
        public const string ResourceID = "ns=3;s=\"dbRfidData\".\"ID2\".\"Mes\".\"iResourceId\"";
        public const string ProgNo = "ns=3;s=\"dbRob\".\"iProgNo\"";
        public const string BoxInside = "ns=3;s=\"dbVar\".\"BoxChange\".\"xBoxInside\"";
        public const string ActStep = "ns=3;s=\"dbRob\".\"iActStep\"";
        public const string ErrxSF1 = "ns=3;s=\"dbVar\".\"Err\".\"xSF1\"";
        public const string ErrxSF2 = "ns=3;s=\"dbVar\".\"Err\".\"xSF2\"";
        public const string HmixSF2 = "ns=3;s=\"dbVar\".\"Hmi\".\"Flags\".\"xSF2\"";
        public const string StateCode = "ns=3;s=\"dbVar\".\"Hmi\".\"ISTB\".\"iStateCode\"";
    }
}
