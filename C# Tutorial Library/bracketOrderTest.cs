using System;
using IBApi;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class bracketOrderTest : DefaultEWrapper
    {

        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;


        public static void bracketOrderMain()
        {
            var testImpl = new bracketOrderTest();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7496, 2000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();

            Contract contract = new Contract();
            contract.Symbol = "AAPL";
            contract.SecType = "STK";
            contract.Exchange = "SMART";
            contract.Currency = "USD";

            Order parent = new Order();
            parent.OrderId = testImpl.NextOrderId;
            parent.OrderType = "LMT";
            parent.LmtPrice = 140;
            parent.Action = "BUY";
            parent.TotalQuantity = 10;
            parent.Transmit = false;

            Order profit_taker = new Order();
            profit_taker.OrderId = parent.OrderId + 1;
            profit_taker.ParentId = parent.OrderId;
            profit_taker.Action = "SELL";
            profit_taker.OrderType = "LMT";
            profit_taker.LmtPrice = 137;
            profit_taker.TotalQuantity = 10;
            profit_taker.Transmit = false;

            Order stop_loss = new Order();
            stop_loss.OrderId = parent.OrderId + 2;
            stop_loss.ParentId = parent.OrderId;
            stop_loss.OrderType = "STP";
            stop_loss.AuxPrice = 155;
            stop_loss.Action = "SELL";
            stop_loss.TotalQuantity = 10;
            stop_loss.Transmit = true;

            Console.WriteLine("Placed an order for " + contract.Symbol);
            clientSocket.placeOrder(parent.OrderId, contract, parent);
            clientSocket.placeOrder(profit_taker.OrderId, contract, profit_taker);
            clientSocket.placeOrder(stop_loss.OrderId, contract, stop_loss);

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
            Console.WriteLine("execDetailsEnd. " + reqId);
        }

        //! [socket_init]
        public bracketOrderTest()
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
