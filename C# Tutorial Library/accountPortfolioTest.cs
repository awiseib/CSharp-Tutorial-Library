using IBApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class accountPortfolioTest : DefaultEWrapper
    {
        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;


        public static void accountMain()
        {
            var testImpl = new accountPortfolioTest();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7497, 1000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            //Once the messages are in the queue, an additional thread can be created to fetch them
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            while (testImpl.NextOrderId <= 0) { }

            Console.WriteLine("Requesting account updates.");
            clientSocket.reqAccountUpdates(true, "");

            // Disconnect if it takes more than 5 seconds
            Thread.Sleep(5000);
            Console.WriteLine("Disconnecting...");
            clientSocket.eDisconnect();
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
            clientSocket.eDisconnect();
        }

        //! [socket_init]
        public accountPortfolioTest()
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
