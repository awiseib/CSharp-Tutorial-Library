using IBApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class ScannerTest : DefaultEWrapper
    {
        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;


        public static void ScannerMain()
        {
            var testImpl = new ScannerTest();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7496, 1000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            //Once the messages are in the queue, an additional thread can be created to fetch them
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            Thread.Sleep(5);

            ScannerSubscription sub = new()
            {
                Instrument = "STK",
                LocationCode = "STK.US.MAJOR",
                ScanCode = "TOP_PERC_GAIN",
            };

            List<IBApi.TagValue> scanOptions = new();

            List<IBApi.TagValue> filterOptions = new() {
                new TagValue("volumeAbove", "10000"),
                new TagValue("marketCapBelow1e6", "1000"),
                new TagValue("priceAbove", "1")
            };

            clientSocket.reqScannerSubscription(1234, sub, scanOptions, filterOptions);

            Thread.Sleep(10000);
            Console.WriteLine("Disconnecting...");
            clientSocket.eDisconnect();
        }

        public override void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr)
        {
            Console.WriteLine("scannerData. reqId: " + reqId + ", rank: " + rank + ", contractDetails: " + contractDetails.Contract + ", distance: " + distance + ", benchmark: " + benchmark + ", projection: " + projection + ", legsStr: " + legsStr);
        }

        public override void scannerDataEnd(int reqId)
        {
            Console.WriteLine("End of market scanner: " + reqId);
        }

        //! [socket_init]
        public ScannerTest()
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
