using System;
using IBApi;
using System.Threading;
using System.Collections.Generic;

namespace C__Tutorial_Library
{
    internal class CurrentTimeTest : DefaultEWrapper
    {
            //! [ewrapperimpl]
            private int nextOrderId;
            EClientSocket clientSocket;
            public readonly EReaderSignal Signal;


            public static void TimeMain()
        {
            var testImpl = new CurrentTimeTest();

            EClientSocket clientSocket = testImpl.ClientSocket;
            EReaderSignal readerSignal = testImpl.Signal;

            clientSocket.eConnect("127.0.0.1", 7496, 1000);

            var reader = new EReader(clientSocket, readerSignal);
            reader.Start();
            //Once the messages are in the queue, an additional thread can be created to fetch them
            new Thread(() => { while (clientSocket.IsConnected()) { readerSignal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

            Thread.Sleep(5);

            clientSocket.reqCurrentTime();

            Thread.Sleep(1000);
            Console.WriteLine("Disconnecting...");
            clientSocket.eDisconnect();
        }

        public override void currentTime(long time)
        {
            Console.WriteLine("Your current time is: " + time);
        }

        //! [socket_init]
        public CurrentTimeTest()
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
