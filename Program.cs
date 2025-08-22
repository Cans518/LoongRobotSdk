using System;
using System.IO;
using System.Threading;
using LoongRobotSdk;

namespace LoongRobotSdk
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ONNX推理器测试Demo");

            // 1. 指定ONNX模型文件路径
            string onnxModelPath = "policy_3052.onnx"; // 替换为实际的ONNX模型文件路径
            
            // 检查模型文件是否存在
            if (!File.Exists(onnxModelPath))
            {
                Console.WriteLine($"错误: ONNX模型文件不存在: {onnxModelPath}");
                return;
            }

            try
            {
                Console.WriteLine("初始化ONNX推理器...");
                
                // 2. 创建ONNX推理器实例
                OnnxRunnerClass onnxRunner = new OnnxRunnerClass(onnxModelPath);
                
                // 3. 创建传感器数据对象 (假设机器人有32个关节, 左右手各有5个自由度)
                JntSdkSensDataClass sensorData = new JntSdkSensDataClass(31, 6, 6);
                
                // 4. 初始化传感器数据
                InitializeSensorData(sensorData);
                
                // 5. 重置ONNX推理器
                Console.WriteLine("重置ONNX推理器...");
                onnxRunner.Reset(sensorData);
                
                // 6. 模拟控制循环
                Console.WriteLine("开始模拟控制循环...");
                for (int i = 0; i < 100; i++)
                {
                    // 更新传感器数据 (在实际应用中，这些数据会从机器人硬件获取)
                    UpdateSensorData(sensorData, i);
                    
                    // 执行ONNX推理步骤
                    float[] actionOutput = onnxRunner.Step(sensorData);
                    
                    // 打印输出结果 (每10步打印一次)
                    if (i % 10 == 0)
                    {
                        Console.WriteLine($"步骤 {i} 输出:");
                        PrintArray("动作输出", actionOutput);
                    }
                    
                    // 在实际应用中，这里会将输出发送到机器人执行
                    
                    // 模拟控制周期
                    Thread.Sleep(10);  // 假设控制频率为100Hz
                }
                
                Console.WriteLine("测试完成!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
        
        // 初始化传感器数据
        static void InitializeSensorData(JntSdkSensDataClass sensorData)
        {
            // 设置基本数据
            sensorData.timestamp[0] = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000.0;
            sensorData.key[0] = 0;
            sensorData.key[1] = 0;
            sensorData.planName = "test";
            sensorData.state[0] = 1;  // 假设1表示正常状态
            sensorData.state[1] = 0;
            
            // 初始化摇杆数据
            sensorData.joy[0] = 0.0f;  // 前进/后退
            sensorData.joy[1] = 0.0f;  // 左右移动
            sensorData.joy[2] = 0.0f;  // 旋转
            sensorData.joy[3] = 0.0f;  // 高度
            
            // 初始化姿态数据
            sensorData.rpy[0] = 0.0f;  // roll (横滚角)
            sensorData.rpy[1] = 0.0f;  // pitch (俯仰角)
            sensorData.rpy[2] = 0.0f;  // yaw (偏航角)
            
            // 初始化陀螺仪数据
            sensorData.gyr[0] = 0.0f;  // x轴角速度
            sensorData.gyr[1] = 0.0f;  // y轴角速度
            sensorData.gyr[2] = 0.0f;  // z轴角速度
            
            // 初始化加速度计数据
            sensorData.acc[0] = 0.0f;  // x轴加速度
            sensorData.acc[1] = 0.0f;  // y轴加速度
            sensorData.acc[2] = 9.8f;  // z轴加速度 (重力)
            
            // 初始化关节角度、速度和扭矩
            // 这里我们设置一些合理的初始值，实际应用中应使用真实数据
            for (int i = 0; i < sensorData.actJ.Length; i++)
            {
                // 对于最后12个关节（腿部关节），设置标准姿势
                if (i >= sensorData.actJ.Length - 12)
                {
                    int legJointIndex = i - (sensorData.actJ.Length - 12);
                    if (legJointIndex % 6 == 2)
                        sensorData.actJ[i] = 0.305913f;  // 髋关节
                    else if (legJointIndex % 6 == 3)
                        sensorData.actJ[i] = -0.670418f; // 膝关节
                    else if (legJointIndex % 6 == 4)
                        sensorData.actJ[i] = 0.371265f;  // 踝关节
                    else
                        sensorData.actJ[i] = 0.0f;       // 其他关节
                }
                else
                {
                    sensorData.actJ[i] = 0.0f;
                }
                
                sensorData.actW[i] = 0.0f;
                sensorData.actT[i] = 0.0f;
                sensorData.tgtJ[i] = sensorData.actJ[i];
                sensorData.tgtW[i] = 0.0f;
                sensorData.tgtT[i] = 0.0f;
            }
            
            // 初始化驱动器状态
            for (int i = 0; i < sensorData.drvState.Length; i++)
            {
                sensorData.drvTemp[i] = 25;  // 25°C
                sensorData.drvState[i] = 1;  // 正常状态
                sensorData.drvErr[i] = 0;    // 无错误
            }
            
            // 初始化手指数据
            for (int i = 0; i < sensorData.actFingerLeft.Length; i++)
            {
                sensorData.actFingerLeft[i] = 0.0f;
                sensorData.tgtFingerLeft[i] = 0.0f;
            }
            
            for (int i = 0; i < sensorData.actFingerRight.Length; i++)
            {
                sensorData.actFingerRight[i] = 0.0f;
                sensorData.tgtFingerRight[i] = 0.0f;
            }
        }
        
        // 更新传感器数据，模拟机器人运动
        static void UpdateSensorData(JntSdkSensDataClass sensorData, int step)
        {
            // 更新时间戳
            sensorData.timestamp[0] = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond / 1000.0;
            
            // 根据步骤模拟不同的控制命令
            if (step == 20)
            {
                // 模拟开始踏步命令
                sensorData.key[0] = 6;
                Console.WriteLine("发送开始踏步命令");
            }
            else if (step == 60)
            {
                // 模拟停止踏步命令
                sensorData.key[0] = 7;
                Console.WriteLine("发送停止踏步命令");
            }
            else
            {
                sensorData.key[0] = 0;
            }
            
            // 模拟摇杆输入
            if (step > 30 && step < 50)
            {
                // 模拟向前移动
                sensorData.joy[0] = 0.3f;
                Console.WriteLine("模拟向前移动");
            }
            else if (step > 70 && step < 90)
            {
                // 模拟旋转
                sensorData.joy[2] = 0.2f;
                Console.WriteLine("模拟旋转");
            }
            else
            {
                sensorData.joy[0] = 0.0f;
                sensorData.joy[1] = 0.0f;
                sensorData.joy[2] = 0.0f;
            }
            
            // 模拟机器人姿态变化
            // 在实际应用中，这些数据应该来自IMU传感器
            sensorData.rpy[0] = (float)Math.Sin(step * 0.02) * 0.05f;  // 轻微的横滚角变化
            sensorData.rpy[1] = (float)Math.Cos(step * 0.03) * 0.03f;  // 轻微的俯仰角变化
            
            // 更新陀螺仪数据
            sensorData.gyr[0] = (sensorData.rpy[0] - (step > 0 ? (float)Math.Sin((step-1) * 0.02) * 0.05f : 0)) * 100;
            sensorData.gyr[1] = (sensorData.rpy[1] - (step > 0 ? (float)Math.Cos((step-1) * 0.03) * 0.03f : 0)) * 100;
            
            // 在实际应用中，关节角度、速度和扭矩会根据机器人的实际状态更新
            // 这里我们只是简单地保持它们不变，或者可以添加一些小的随机变化
        }
        
        // 打印数组内容的辅助方法
        static void PrintArray(string name, float[] array)
        {
            Console.Write($"{name}: [");
            for (int i = 0; i < array.Length; i++)
            {
                Console.Write($"{array[i]:F4}");
                if (i < array.Length - 1)
                    Console.Write(", ");
            }
            Console.WriteLine("]");
        }
    }
}
