using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class Launchpad
    {
        public static void Main()
        {
            CurrentTimeTest.TimeMain();

            ContractDetailsTest.ContractMain();

            LiveData.LiveDataMain();

            historicalDataTest.historicalMain();

            PlaceOrderTest.PlaceOrderMain();

            ComboOrderTest.ComboOrderMain();

            BracketOrderTest.BracketOrderMain();

            AccountPortfolioTest.AccountMain();

            ExecutionsTest.ExecutionsMain();

            ScannerParamsTest.ScannerParamsTestMain();

            ScannerTest.ScannerMain();

            TickByTickTest.TickByTickTestMain();

            PositionsTest.PositionMain();

            SixLegComboOrder.SixLegMain();

        }
    }
}
