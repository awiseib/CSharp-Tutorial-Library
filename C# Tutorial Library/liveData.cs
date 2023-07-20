using System;
using IBApi;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class liveData : DefaultEWrapper
    {

        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;


        public static void liveDataMain()
        {
            var testImpl = new liveData();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7496, 1000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            //Once the messages are in the queue, an additional thread can be created to fetch them
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            while (testImpl.NextOrderId <= 0) { }

            Contract contract = new Contract();
            contract.Symbol = "AAPL";
            contract.SecType = "STK";
            contract.Exchange = "SMART";
            contract.Currency = "USD";

            Console.WriteLine("Requesting market data for " + contract.Symbol);
            clientSocket.reqMktData(testImpl.NextOrderId, contract, "", false, false, null);

            // We can stream data forever, but we'll kill the connection after 1000 milliseconds.
            Thread.Sleep(1000);
            Console.WriteLine("Disconnecting...");
            clientSocket.eDisconnect();
        }


        //! [tickprice]
        public override void tickPrice(int tickerId, int field, double price, TickAttrib attribs)
        {
            Console.WriteLine("Tick Price. Ticker Id:" + tickerId + ", Field: " + field + ", Price: " + Util.DoubleMaxString(price) + ", CanAutoExecute: " + attribs.CanAutoExecute +
                ", PastLimit: " + attribs.PastLimit + ", PreOpen: " + attribs.PreOpen);
        }
        //! [tickprice]

        //! [ticksize]
        public override void tickSize(int tickerId, int field, decimal size)
        {
            Console.WriteLine("Tick Size. Ticker Id:" + tickerId + ", Field: " + field + ", Size: " + Util.DecimalMaxString(size));
        }
        //! [ticksize]

        //! [tickstring]
        public override void tickString(int tickerId, int tickType, string value)
        {
            Console.WriteLine("Tick string. Ticker Id:" + tickerId + ", Type: " + tickType + ", Value: " + value);
        }
        //! [tickstring]

        //! [tickgeneric]
        public override void tickGeneric(int tickerId, int field, double value)
        {
            Console.WriteLine("Tick Generic. Ticker Id:" + tickerId + ", Field: " + field + ", Value: " + Util.DoubleMaxString(value));
        }
        //! [tickgeneric]

        public override void tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture, int holdDays, string futureLastTradeDate, double dividendImpact, double dividendsToLastTradeDate)
        {
            Console.WriteLine("TickEFP. " + tickerId + ", Type: " + tickType + ", BasisPoints: " + Util.DoubleMaxString(basisPoints) + ", FormattedBasisPoints: " + formattedBasisPoints +
                ", ImpliedFuture: " + Util.DoubleMaxString(impliedFuture) + ", HoldDays: " + Util.IntMaxString(holdDays) + ", FutureLastTradeDate: " + futureLastTradeDate +
                ", DividendImpact: " + Util.DoubleMaxString(dividendImpact) + ", DividendsToLastTradeDate: " + Util.DoubleMaxString(dividendsToLastTradeDate));
        }

        //! [ticksnapshotend]
        public override void tickSnapshotEnd(int tickerId)
        {
            Console.WriteLine("TickSnapshotEnd: " + tickerId);
        }
        //! [ticksnapshotend]


        //! [socket_init]
        public liveData()
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

        public override void error(int id, int errorCode, string errorMsg, string advancedOrderRejectJson)
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
