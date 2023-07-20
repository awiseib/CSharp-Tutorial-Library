using System;
using IBApi;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class comboOrderTest : DefaultEWrapper
    {

        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;


        public static void comboOrderMain()
        {
            var testImpl = new comboOrderTest();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7497, 2000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();

            //while (testImpl.NextOrderId <= 0) { }

            Contract contract = new Contract();
            contract.Symbol = "AAPL,TSLA";
            contract.SecType = "BAG";
            contract.Exchange = "SMART";
            contract.Currency = "USD";


            ComboLeg leg1 = new ComboLeg();
            leg1.ConId = 76792991;
            leg1.Ratio = 1;
            leg1.Action = "BUY";
            leg1.Exchange = "SMART";

            ComboLeg leg2 = new ComboLeg();
            leg2.ConId = 265598;
            leg2.Ratio = 1;
            leg2.Action = "SELL";
            leg2.Exchange = "SMART";

            contract.ComboLegs = new List<ComboLeg>(); ;
            contract.ComboLegs.Add(leg1);
            contract.ComboLegs.Add(leg2);

            Order order = new Order();
            order.Action = "BUY";
            order.OrderType = "LMT";
            order.LmtPrice = 8.3;
            order.TotalQuantity = 10;
            order.Tif = "GTC";

            order.SmartComboRoutingParams = new List<IBApi.TagValue>();
            order.SmartComboRoutingParams.Add(new TagValue("NonGuaranteed", "1"));

            Console.WriteLine("Placed an order for " + contract.Symbol);
            clientSocket.placeOrder(654654564, contract, order);

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
        public comboOrderTest()
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
