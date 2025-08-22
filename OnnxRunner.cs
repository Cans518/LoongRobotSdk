// using System;
// using System.Collections.Generic;
// using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Numerics;
// using LoongRobotSdk;

/// <summary> 
/// ONNX推理，角标直白风格，便于后期向cpp迁移
/// </summary>
namespace LoongRobotSdk
{
    public class ObsClass
    {
        // 初始化字段以解决CS8618错误
        public float[] cmd = new float[3];
        public float[] z = new float[] { 0.42f };
        public float[] standFlag = new float[] { 0.5f };
        public float[] w = new float[3];
        public float[] g = new float[] { 0, 0, -1 };
        public float[] action = new float[12];
        public float[] q = new float[12];
        public float[] qd = new float[12];

        public ObsClass()
        {
            Reset();
        }

        public void Reset()
        {
            Array.Clear(cmd, 0, cmd.Length);
            z[0] = 0.42f;
            standFlag[0] = 0.5f; // stand 0.5  step -0.5 walk 0
            Array.Clear(w, 0, w.Length);
            g[0] = 0;
            g[1] = 0;
            g[2] = -1;
            Array.Clear(action, 0, action.Length);
            Array.Clear(q, 0, q.Length);
            Array.Clear(qd, 0, qd.Length);
        }

        public ObsClass DeepCopy()
        {
            ObsClass copy = new ObsClass();
            Array.Copy(cmd, copy.cmd, cmd.Length);
            Array.Copy(z, copy.z, z.Length);
            Array.Copy(standFlag, copy.standFlag, standFlag.Length);
            Array.Copy(w, copy.w, w.Length);
            Array.Copy(g, copy.g, g.Length);
            Array.Copy(action, copy.action, action.Length);
            Array.Copy(q, copy.q, q.Length);
            Array.Copy(qd, copy.qd, qd.Length);
            return copy;
        }
    }

    public class OnnxRunnerClass
    {
        private float[] lim;
        private InferenceSession ort;
        private string inName;
        private string outName;
        private float[] cmdAdd;
        private ObsClass obs;
        private Queue<ObsClass> obsHist;
        private float[] input;
        private float[] action;
        private float[] legQStd;

        public OnnxRunnerClass(string onnxFile)
        {
            lim = new float[] { 1, 0.2f, 0.8f }; // vx vy wz限幅
            ort = new InferenceSession(onnxFile);
            inName = ort.InputMetadata.Keys.First();
            outName = ort.OutputMetadata.Keys.First();
            cmdAdd = new float[3];
            obs = new ObsClass();
            obsHist = new Queue<ObsClass>(3);
            for (int i = 0; i < 3; i++)
            {
                obsHist.Enqueue(new ObsClass());
            }
            input = new float[5 + 42 + 126 + 4200];
            action = new float[12];
            legQStd = new float[] { 0, 0, 0.305913f, -0.670418f, 0.371265f, 0,
                                    0, 0, 0.305913f, -0.670418f, 0.371265f, 0 };
        }

        public void Reset(JntSdkSensDataClass sens)
        {
            obs.Reset();
            Array.Copy(sens.gyr, obs.w, 3);
            
            // 将RPY转换为旋转矩阵并转置，然后与[0,0,-1]相乘
            Matrix4x4 rotMatrix = CreateRotationMatrixFromEuler(sens.rpy);
            Matrix4x4 transposed = Matrix4x4.Transpose(rotMatrix);
            Vector3 gravity = Vector3.Transform(new Vector3(0, 0, -1), transposed);
            obs.g[0] = gravity.X;
            obs.g[1] = gravity.Y;
            obs.g[2] = gravity.Z;
            
            // 获取最后12个关节角度
            Array.Copy(sens.actJ, sens.actJ.Length - 12, obs.q, 0, 12);
            Array.Fill(obs.qd, 0);

            // 清空并重新填充队列
            obsHist.Clear();
            for (int i = 0; i < 4; i++)
            {
                obsHist.Enqueue(obs.DeepCopy());
            }

            Obs2Input(reset: 1);
            for (int i = 0; i < 99; i++)
            {
                Array.Copy(input, input.Length - 42, input, 173 + 42 * i, 42);
            }
            action = new float[12];
        }

        private void Obs2Input(int reset = 0)
        {
            // 将obs数据连接到input中
            Array.Copy(obs.cmd, 0, input, 0, 3);
            Array.Copy(obs.z, 0, input, 3, 1);
            Array.Copy(obs.standFlag, 0, input, 4, 1);
            Array.Copy(obs.w, 0, input, 5, 3);
            Array.Copy(obs.g, 0, input, 8, 3);
            Array.Copy(obs.action, 0, input, 11, 12);
            Array.Copy(obs.q, 0, input, 23, 12);
            Array.Copy(obs.qd, 0, input, 35, 12);

            // 复制历史数据
            int offset = 47;
            foreach (var h in obsHist)
            {
                Array.Copy(h.w, 0, input, offset, 3);
                offset += 3;
            }
            
            foreach (var h in obsHist)
            {
                Array.Copy(h.g, 0, input, offset, 3);
                offset += 3;
            }
            
            foreach (var h in obsHist)
            {
                Array.Copy(h.action, 0, input, offset, 12);
                offset += 12;
            }
            
            foreach (var h in obsHist)
            {
                Array.Copy(h.q, 0, input, offset, 12);
                offset += 12;
            }
            
            foreach (var h in obsHist)
            {
                Array.Copy(h.qd, 0, input, offset, 12);
                offset += 12;
            }

            // 复制input的最后部分以保留历史
            Array.Copy(input, input.Length - 4158, input, 173, 4158);
            
            // 复制最后部分
            Array.Copy(input, 11, input, input.Length - 42, 36);
            Array.Copy(input, 5, input, input.Length - 6, 6);
        }

        public float[] Step(JntSdkSensDataClass sens)
        {
            if (sens.key[0] == 6)
            {
                obs.standFlag[0] = 0;
                // Console.WriteLine("开始踏步");
            }
            else if (sens.key[0] == 7)
            {
                obs.standFlag[0] = 0.5f;
                // Console.WriteLine("停止踏步");
            }
            
            float[] cmd = new float[3];
            Array.Copy(sens.joy, cmd, 3);

            if (Math.Abs(cmd[0]) < 0.09f)
                cmd[0] = 0;
            if (Math.Abs(cmd[1]) < 0.04f)
                cmd[1] = 0;
            if (Math.Abs(cmd[2]) < 0.19f)
                cmd[2] = 0;

            for (int i = 0; i < 3; i++)
            {
                cmd[i] += cmdAdd[i];
                obs.cmd[i] = 0.95f * obs.cmd[i] + 0.05f * cmd[i];
                obs.cmd[i] = Math.Clamp(obs.cmd[i], -lim[i], lim[i]);
            }
            
            obs.z[0] = 0.95f * obs.z[0] + 0.05f * (0.42f + sens.joy[3] * 10); // 不是标准单位
            obs.z[0] = Math.Clamp(obs.z[0], -1, 1);
            
            Array.Copy(sens.gyr, obs.w, 3);
            
            // 将RPY转换为旋转矩阵并转置，然后与[0,0,-1]相乘
            Matrix4x4 rotMatrix = CreateRotationMatrixFromEuler(sens.rpy);
            Matrix4x4 transposed = Matrix4x4.Transpose(rotMatrix);
            Vector3 gravity = Vector3.Transform(new Vector3(0, 0, -1), transposed);
            obs.g[0] = gravity.X;
            obs.g[1] = gravity.Y;
            obs.g[2] = gravity.Z;
            
            Array.Copy(action, obs.action, 12);
            Array.Copy(sens.actJ, sens.actJ.Length - 12, obs.q, 0, 12);
            Array.Copy(sens.actW, sens.actW.Length - 12, obs.qd, 0, 12);

            Obs2Input();
            
            // 运行推理
            var inputTensor = new DenseTensor<float>(input, new[] { 1, 4373 });
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inName, inputTensor) };
            var outputs = ort.Run(inputs);
            var outputTensor = outputs.First().AsTensor<float>();
            
            for (int i = 0; i < 12; i++)
            {
                action[i] = outputTensor[i];
            }

            obsHist.Dequeue();
            obsHist.Enqueue(obs.DeepCopy());

            float[] result = new float[12];
            for (int i = 0; i < 12; i++)
            {
                result[i] = action[i] + legQStd[i];
            }
            
            return result;
        }

        private Matrix4x4 CreateRotationMatrixFromEuler(float[] rpy)
        {
            // 从欧拉角（弧度）创建旋转矩阵
            float cosX = (float)Math.Cos(rpy[0]);
            float sinX = (float)Math.Sin(rpy[0]);
            float cosY = (float)Math.Cos(rpy[1]);
            float sinY = (float)Math.Sin(rpy[1]);
            float cosZ = (float)Math.Cos(rpy[2]);
            float sinZ = (float)Math.Sin(rpy[2]);

            Matrix4x4 rotX = new Matrix4x4(
                1, 0, 0, 0,
                0, cosX, -sinX, 0,
                0, sinX, cosX, 0,
                0, 0, 0, 1
            );

            Matrix4x4 rotY = new Matrix4x4(
                cosY, 0, sinY, 0,
                0, 1, 0, 0,
                -sinY, 0, cosY, 0,
                0, 0, 0, 1
            );

            Matrix4x4 rotZ = new Matrix4x4(
                cosZ, -sinZ, 0, 0,
                sinZ, cosZ, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );

            // 修复Matrix4x4.Multiply方法的调用
            Matrix4x4 temp = Matrix4x4.Multiply(rotX, rotY);
            Matrix4x4 result = Matrix4x4.Multiply(temp, rotZ);
            return result;
        }
    }
}
