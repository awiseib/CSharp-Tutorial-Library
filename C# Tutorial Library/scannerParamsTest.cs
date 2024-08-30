using System;
using IBApi;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C__Tutorial_Library
{
    internal class ScannerParamsTest : DefaultEWrapper
    {
        //! [ewrapperimpl]
        private int nextOrderId;
        EClientSocket clientSocket;
        public readonly EReaderSignal Signal;


        public static void ScannerParamsTestMain()
        {
            var testImpl = new ScannerParamsTest();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7496, 1000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            //Once the messages are in the queue, an additional thread can be created to fetch them
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            clientSocket.reqScannerParameters();

            Thread.Sleep(1000);
            Console.WriteLine("Disconnecting...");
            clientSocket.eDisconnect();
        }

        public override void scannerParameters(string xml)
        {
            Console.WriteLine("Writing...");

            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            using (StreamWriter writer = new StreamWriter(Path.Combine(docPath, "ibScannerParameters.xml")))
            {
                writer.WriteLine(xml);
            }

            Console.WriteLine("Done!");
        }

        //! [socket_init]
        public ScannerParamsTest()
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
