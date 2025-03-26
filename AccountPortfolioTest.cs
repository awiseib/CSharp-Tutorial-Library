using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class AccountPortfolioTest : DefaultEWrapper
    {
        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;


        public static void AccountMain()
        {
            var testImpl = new AccountPortfolioTest();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7496, 0);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            Console.WriteLine("Requesting account updates.");
            Thread.Sleep(5000);
            clientSocket.reqAutoOpenOrders(true);
            Thread.Sleep(5000);
            clientSocket.reqAccountUpdates(true, "DU5240685");

            // Disconnect if it takes more than 5 seconds
            Thread.Sleep(500000);
            //Console.WriteLine("Disconnecting...");
            //clientSocket.eDisconnect();
        }

        public override void updatePortfolio(Contract contract, decimal position, double marketPrice, double marketValue, double averageCost, double unrealizedPNL, double realizedPNL, string accountName)
        {
            Console.WriteLine("updatePortfolio. Contract: " + contract + ", Position: " + position + ", marketPrice: " + marketPrice + ", marketValue: " + marketValue + ", averageCost: " + averageCost + ", unrealizedPNL: " + unrealizedPNL + ", realizedPNL: " + realizedPNL + ", accountName: " + accountName);
        }

        public override void updateAccountValue(string key, string value, string currency, string accountName)
        {
            Console.WriteLine("updateAccountValue. key: " + key + ", value: " + value + ", currency: " + currency + ", accountName: " + accountName);
        }

        public override void accountDownloadEnd(string account)
        {
            Console.WriteLine("accountDownloadEnd. " + account);
            //clientSocket.eDisconnect();
        }

        //! [socket_init]
        public AccountPortfolioTest()
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
