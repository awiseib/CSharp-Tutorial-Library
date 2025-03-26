using System;
using IBApi;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class BracketOrderTest : DefaultEWrapper
    {

        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;


        public static void BracketOrderMain()
        {
            var testImpl = new BracketOrderTest();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7496, 2000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            Thread.Sleep(50);
            Contract contract = new()
            {
                Symbol = "AAPL",
                SecType = "STK",
                Exchange = "SMART",
                Currency = "USD",
            };

            Order parent = new()
            {
                OrderId = testImpl.NextOrderId,
                OrderType = "MKT",
                Action = "BUY",
                TotalQuantity = 10,
                Transmit = false
            };

            Order profit_taker = new()
            {
                OrderId = parent.OrderId + 1,
                ParentId = parent.OrderId,
                Action = "SELL",
                OrderType = "LMT",
                LmtPrice = 230,
                TotalQuantity = 10,
                Transmit = false
            };

            Order stop_loss = new()
            {
                OrderId = parent.OrderId + 2,
                ParentId = parent.OrderId,
                OrderType = "STP",
                AuxPrice = 225,
                Action = "SELL",
                TotalQuantity = 10,
                Transmit = true
            };

            Thread.Sleep(50);
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

        public override void orderStatus(int orderId, string status, decimal filled, decimal remaining, double avgFillPrice, long permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
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
        public BracketOrderTest()
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
