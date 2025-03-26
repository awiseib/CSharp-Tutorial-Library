using System;
using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class historicalDataTest : DefaultEWrapper
    {

        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;


        public static void historicalMain()
        {
            var testImpl = new historicalDataTest();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7496, 1000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            //Once the messages are in the queue, an additional thread can be created to fetch them
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            Thread.Sleep(5);

            Contract contract = new()
            {
                Symbol = "AAPL",
                SecType = "STK",
                Exchange = "SMART",
                Currency = "USD"
            };

            Console.WriteLine("Requesting historical data for " + contract.Symbol);
            clientSocket.reqHistoricalData(testImpl.NextOrderId, contract, "", "1 D", "1 hour", "Trades", 1, 1, false, null);

            Thread.Sleep(1000);
            Console.WriteLine("Disconnecting...");
            clientSocket.eDisconnect();
        }

        //! [historicaldata]
        public override void historicalData(int reqId, Bar bar)
        {
            Console.WriteLine("" + reqId + " - Time: " + bar.Time + ", Open: " + Util.DoubleMaxString(bar.Open) + ", High: " + Util.DoubleMaxString(bar.High) +
                ", Low: " + Util.DoubleMaxString(bar.Low) + ", Close: " + Util.DoubleMaxString(bar.Close) + ", Volume: " + Util.DecimalMaxString(bar.Volume) +
                ", Count: " + Util.IntMaxString(bar.Count) + ", WAP: " + Util.DecimalMaxString(bar.WAP));
        }
        //! [historicaldata]

        //! [socket_init]
        public historicalDataTest()
        {
            Signal = new EReaderMonitorSignal();
            clientSocket = new EClientSocket(this, Signal);
        }
        //! [socket_init]

        public EClientSocket ClientSocket
        {
            get { return clientSocket; }
            set { clientSocket = value; }
        }

        public int NextOrderId
        {
            get { return nextOrderId; }
            set { nextOrderId = value; }
        }

        public override void nextValidId(int orderId)
        {
            Console.WriteLine("Next Valid Id: " + orderId);
            NextOrderId = orderId;
        }

        public override void error(int id, long errorTime, int errorCode, string errorMsg, string advancedOrderRejectJson)
        {
            if (!Util.StringIsEmpty(advancedOrderRejectJson))
            {
                Console.WriteLine("Error. Id: " + id + ", Code: " + errorCode + ", Msg: " + errorMsg + ", AdvancedOrderRejectJson: " + advancedOrderRejectJson + "\n");
            }
            else
            {
                Console.WriteLine("Error. Id: " + id + ", Code: " + errorCode + ", Msg: " + errorMsg + "\n");
            }
        }
    }
}
