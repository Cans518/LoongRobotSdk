// using System;
using System.Net;
using System.Net.Sockets;
// using System.Threading;


namespace LoongRobotSdk
{
    /// <summary>
    /// UDP communication class for joint SDK, supports multi-machine debugging with millisecond-level delay
    /// Pure C# implementation, no C++ dependencies
    /// </summary>
    public class JntSdkClass : IDisposable
    {
        private UdpClient? sock;
        private IPEndPoint targetEndPoint;
        private JntSdkSensDataClass sens;
        private byte[] recvBuffer = new byte[2048];
        private object bufferLock = new object();
        private byte[]? latestData = null;
        private bool running = true;
        private Thread recvThread;

        public JntSdkClass(string ip, int port, int jntNum, int fingerDofLeft, int fingerDofRight)
        {
            // Create UDP socket
            sock = new UdpClient();
            sock.Client.ReceiveTimeout = 500; // 0.5 second timeout
            
            // Bind to any available port
            sock.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            
            // Set target address and port
            targetEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            
            // Initialize sensor data class
            sens = new JntSdkSensDataClass(jntNum, fingerDofLeft, fingerDofRight);
            
            // Create and start receiving thread
            recvThread = new Thread(ReceiveLoop);
            recvThread.IsBackground = true;
            recvThread.Start();
        }

        private void ReceiveLoop()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            
            while (running)
            {
                try
                {
                    // 如果sock为空，扔出异常
                    if (sock == null)
                    {
                        throw new InvalidOperationException("Socket is not initialized.");
                    }
                    byte[] data = sock.Receive(ref remoteEP);
                    lock (bufferLock)
                    {
                        latestData = data;
                    }
                }
                catch (SocketException)
                {
                    // Timeout or other socket error, continue
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Receive error: {e}");
                }
            }
        }

        public void Send(JntSdkCtrlDataClass ctrl)
        {
            try
            {
                // 如果sock为空，返回报错信息
                if (sock == null)
                {
                    Console.WriteLine("Socket is not initialized.");
                    return;
                }
                byte[] buf = ctrl.PackData();
                sock.Send(buf, buf.Length, targetEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Send error: {e}");
            }
        }

        public void WaitSens()
        {
            Send(new JntSdkCtrlDataClass(31, 1, 1));
            while (true)
            {
                Thread.Sleep(500);
                Console.WriteLine("SDK waiting for connection...");
                JntSdkSensDataClass sens = Recv();
                if (sens.timestamp[0] > 0)
                {
                    break;
                }
            }
        }

        public JntSdkSensDataClass Recv()
        {
            byte[]? data;
            lock (bufferLock)
            {
                data = latestData;
            }
            
            if (data != null)
            {
                sens.UnpackData(data);
            }
            return sens;
        }

        public void Dispose()
        {
            running = false;
            
            // Wait for receive thread to finish (max 1 second)
            if (recvThread != null && recvThread.IsAlive)
            {
                recvThread.Join(1000);
            }
            
            // Close socket
            if (sock != null)
            {
                sock.Close();
                sock = null;
            }
        }
    }
}
