using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Utilities
{
    public static class SocketUtility
    {
        //Метод для получения строки из бинарки
        public static string ReceiveString(Socket socket, 
            Action onReceiveDataSizeCheckFail, Action onReceiveDataCheckFail)
        {
            using (Stream dataStream = new MemoryStream())
            using (BinaryReader dataStreamReader = new BinaryReader(dataStream))
            {
                var dataSize = ReceiveDataSize(socket, dataStream, dataStreamReader, onReceiveDataSizeCheckFail);
                ReceiveDataToStream(socket, dataSize, dataStream, onReceiveDataCheckFail);
                
                dataStream.Seek(0, SeekOrigin.Begin);
                return dataStreamReader.ReadString();
            }
        }
        
        //Получаем данные из Stream потока
        private static void ReceiveDataToStream(
            Socket socket, long dataSize, 
            Stream dataStream, Action onReceiveDataCheckFail)
        {
            var maxBufferSize = 1024;
            var remainingDataSize = dataSize;

            dataStream.Seek(0, SeekOrigin.Begin);
            
            while (remainingDataSize > maxBufferSize)
            {
                ReceiveBufferToStream(socket, dataStream, maxBufferSize, onReceiveDataCheckFail);

                remainingDataSize -= maxBufferSize;
            }
            
            ReceiveBufferToStream(socket, dataStream, (int)remainingDataSize, onReceiveDataCheckFail);
        }

        //Метод для перевода буффера в Stream поток 
        private static void ReceiveBufferToStream(
            Socket socket, Stream dataStream, int bufferSize,
            Action onReceiveDataCheckFail)
        {
            WaitDataFromSocket(socket, bufferSize);

            byte[] dataBuffer = new byte[bufferSize];
            var receivedBufferSize = socket.Receive(dataBuffer);

            if (receivedBufferSize != bufferSize)
            {
                onReceiveDataCheckFail();
            }

            dataStream.Write(dataBuffer, 0, bufferSize);
        }

        //Получаем размер данных
        private static long ReceiveDataSize(Socket socket, Stream dataStream, 
            BinaryReader dataStreamReader, Action onReceiveDataCheckFail)
        {
            WaitDataFromSocket(socket, sizeof(long));

            byte[] dataBuffer = new byte[sizeof(long)];
            var receivedBufferSize = socket.Receive(dataBuffer);

            //Проверяем целостность данных
            if (receivedBufferSize != dataBuffer.Length)
            {
                onReceiveDataCheckFail();
            }

            dataStream.Seek(0, SeekOrigin.Begin);//Ставим полодение каретки в начало
            //записываем последовательность байт в поток и перемещаем его на указанное кол-во байт
            dataStream.Write(dataBuffer, 0, dataBuffer.Length);
            dataStream.Seek(0, SeekOrigin.Begin);
            return dataStreamReader.ReadInt64();
        }

        //Ожидаем данные из сокета клиента
        public static void WaitDataFromSocket(Socket clientSocket)
        {
            WaitDataFromSocket(clientSocket, 1);
        }

        //Ожжидаем пока есть доступные байты
        private static void WaitDataFromSocket(Socket clientSocket, int waitForBytesAvailable)
        {
            while (clientSocket.Available < waitForBytesAvailable)
            {
                Thread.Sleep(100);
            }
        }

        //метод для отправки данных в сокет
        public static void SendString(Socket socket, string dataToSend, Action onSendDataCheckFail)
        {
            using (Stream dataStream = new MemoryStream())
            using (BinaryWriter dataStreamWriter = new BinaryWriter(dataStream))
            {
                /*
                 * записываем пустышку вместо размера пакета данных,
                 * на данном этапе мы не знаем размер отправляемых данных
                 */
                dataStreamWriter.Write((long)0);
                
                dataStreamWriter.Write(dataToSend);
                dataStreamWriter.Flush();//очищаем буфферы потока
            
                byte[] sendDataBuffer = new byte[dataStream.Position];

                /*
                 * Перезаписываем актуальный размер пакета данных,
                 * теперь мы знаем его размер
                 */
                dataStream.Seek(0, SeekOrigin.Begin);
                //считываем данные длиной лонга
                dataStreamWriter.Write(dataStream.Length - sizeof(long));
                
                dataStream.Seek(0, SeekOrigin.Begin);
                int readBytesFromMemoryStream = dataStream.Read(sendDataBuffer, 0, sendDataBuffer.Length);

                if (readBytesFromMemoryStream != sendDataBuffer.Length)
                {
                    onSendDataCheckFail();
                }

                socket.Send(sendDataBuffer);
            }
        }

    }
}