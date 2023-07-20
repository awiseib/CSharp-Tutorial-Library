using System;
using IBApi;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class placeOrderTest : DefaultEWrapper
    {

        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;


        public static void placeOrderMain()
        {
            var testImpl = new placeOrderTest();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7497, 2000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();

            //while (testImpl.NextOrderId <= 0) { }

            Contract contract = new Contract();
            contract.Symbol = "AAPL";
            contract.SecType = "STK";
            contract.Exchange = "SMART";
            contract.Currency = "USD";

            Order order = new Order();
            order.Action = "BUY";
            order.OrderType = "LMT";
            order.LmtPrice = 147;
            order.TotalQuantity = 10;
            order.Tif = "GTC";

            Console.WriteLine("Placed an order for " + contract.Symbol);
            clientSocket.placeOrder(testImpl.NextOrderId, contract, order);

            Thread.Sleep(5000);
            Console.WriteLine("Disconnecting...");
            clientSocket.eDisconnect();
        }

        public override void openOrder(int orderId, Contract contract, Order order, OrderState orderState)
        {
            Console.WriteLine("openOrder. " + ", " + orderId + ", " + contract + ", " + order + ", " + orderState);
        }

        public override void orderStatus(int orderId, string status, decimal filled, decimal remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
        {
            Console.WriteLine("orderStatus. " + orderId + ", " + status + ", " + filled + ", " + remaining + ", " + avgFillPrice + ", " + permId + ", " + parentId + ", " + lastFillPrice + ", " + clientId + ", " + whyHeld + ", " + mktCapPrice);
        }

        public override void execDetails(int reqId, Contract contract, Execution execution)
        {
            Console.WriteLine("execDetails. " + reqId + contract + ", " + execution);
        }

        public override void execDetailsEnd(int reqId)
        {
            Console.WriteLine("execDetailsEnd. "+ reqId);
        }

        //! [socket_init]
        public placeOrderTest()
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
