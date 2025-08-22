// using System;
// using System.Threading;

namespace LoongRobotSdk
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define robot parameters
            int jntNum = 31;        // Total number of joints
            int fingerDofLeft = 6;  // Left hand finger DOF
            int fingerDofRight = 6; // Right hand finger DOF
            
            // Create SDK instance
            using (JntSdkClass sdk = new JntSdkClass("172.18.175.239", 8006, jntNum, fingerDofLeft, fingerDofRight))
            {
                Console.WriteLine("Connecting to robot...");
                sdk.WaitSens();
                Console.WriteLine("Connected successfully!");
                
                // Create control data
                JntSdkCtrlDataClass ctrl = new JntSdkCtrlDataClass(31, 1, 1);
                
                // Main control loop
                for (int i = 0; i < 100; i++)
                {
                    // Get sensor data
                    JntSdkSensDataClass sens = sdk.Recv();
                    
                    sens.Print();
                    
                    // Wait a bit
                    Thread.Sleep(100);
                }
                
                // Reset control before exiting
                ctrl.Reset();
                sdk.Send(ctrl);
            }
            
            Console.WriteLine("Program completed.");
        }
    }
}
