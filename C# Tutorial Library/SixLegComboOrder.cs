using System;
using IBApi;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Principal;


namespace C__Tutorial_Library
{
    internal class Legs
    {
        public string Symbol { get; set; }
        public int Id { get; set; }
        public int Ratio { get; set; }
        public string IsBuyer { get; set; }
    }

    internal class SixLegComboOrder : DefaultEWrapper
    {
        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;

        public static void SixLegMain()
        {

            var testImpl = new SixLegComboOrder();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7496, 2000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            Order order = new()
            {
                Action = "BUY",
                OrderType = "MKT",
                TotalQuantity = 10,
                Tif = "DAY",
                Transmit = true,
                OutsideRth = false,
                SmartComboRoutingParams = new List<TagValue>() { new("NonGuaranteed", "0") }

            };

            var contract = new Contract();
            contract.Symbol = "SPY";
            contract.SecType = "BAG";
            contract.Currency = "USD";
            contract.Exchange = "SMART";
            contract.ComboLegs = new() 
            {
                new ComboLeg() { ConId = 715192497, Exchange = "SMART", Ratio = 1, Action = "BUY" },
                new ComboLeg() { ConId = 715192524, Exchange = "SMART", Ratio = 1, Action = "BUY" },
                new ComboLeg() { ConId = 715193307, Exchange = "SMART", Ratio = 1, Action = "BUY" },
                new ComboLeg() { ConId = 715193325, Exchange = "SMART", Ratio = 1, Action = "BUY" },
                
                new ComboLeg() { ConId = 723281337, Exchange = "SMART", Ratio = 1, Action = "BUY" },
                new ComboLeg() { ConId = 723282650, Exchange = "SMART", Ratio = 20, Action = "SELL" },
        };

            Thread.Sleep(200);
            Console.WriteLine("Placed an order for " + contract.Symbol);
            clientSocket.placeOrder(testImpl.NextOrderId, contract, order);

            Thread.Sleep(1000);
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
        public SixLegComboOrder()
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
