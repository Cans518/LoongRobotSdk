// using System;
using System.Text;
// using System.Runtime.InteropServices;

namespace LoongRobotSdk
{
    /// <summary>
    /// Sensor data class for Loong joint SDK
    /// This class just copy form Python SDK
    /// </summary>
    public class JntSdkSensDataClass
    {
        public int[] size = new int[1];
        public double[] timestamp = new double[1];
        public short[] key = new short[2];
        public string planName = "none";
        public short[] state = new short[2];
        public float[] joy = new float[4];
        public float[] rpy = new float[3];
        public float[] gyr = new float[3];
        public float[] acc = new float[3];
        public float[] actJ;
        public float[] actW;
        public float[] actT;
        public short[] drvTemp;
        public short[] drvState;
        public short[] drvErr;
        public float[] tgtJ;
        public float[] tgtW;
        public float[] tgtT;
        public float[] actFingerLeft;
        public float[] actFingerRight;
        public float[] tgtFingerLeft;
        public float[] tgtFingerRight;

        private int jntNum;
        private int fingerDofLeft;
        private int fingerDofRight;

        public JntSdkSensDataClass(int jntNum, int fingerDofLeft, int fingerDofRight)
        {
            this.jntNum = jntNum;
            this.fingerDofLeft = fingerDofLeft;
            this.fingerDofRight = fingerDofRight;

            // Initialize arrays
            actJ = new float[jntNum];
            actW = new float[jntNum];
            actT = new float[jntNum];
            drvTemp = new short[jntNum];
            drvState = new short[jntNum];
            drvErr = new short[jntNum];
            tgtJ = new float[jntNum];
            tgtW = new float[jntNum];
            tgtT = new float[jntNum];
            actFingerLeft = new float[fingerDofLeft];
            actFingerRight = new float[fingerDofRight];
            tgtFingerLeft = new float[fingerDofLeft];
            tgtFingerRight = new float[fingerDofRight];
        }

        public void Print()
        {
            Console.WriteLine("============");
            Console.WriteLine($"size = {size[0]}");
            Console.WriteLine($"timestamp = {timestamp[0]}");
            Console.WriteLine($"key = [{key[0]}, {key[1]}]");
            Console.WriteLine($"planName = {planName}");
            Console.WriteLine($"state = [{state[0]}, {state[1]}]");
            Console.WriteLine($"joy = [{string.Join(", ", joy)}]");
            Console.WriteLine($"rpy = [{string.Join(", ", rpy)}]");
            Console.WriteLine($"gyr = [{string.Join(", ", gyr)}]");
            Console.WriteLine($"acc = [{string.Join(", ", acc)}]");
            Console.WriteLine($"actJ = [{string.Join(", ", actJ)}]");
            Console.WriteLine($"actW = [{string.Join(", ", actW)}]");
            Console.WriteLine($"actT = [{string.Join(", ", actT)}]");
            Console.WriteLine($"drvTemp = [{string.Join(", ", drvTemp)}]");
            Console.WriteLine($"drvState = [{string.Join(", ", drvState)}]");
            Console.WriteLine($"drvErr = [{string.Join(", ", drvErr)}]");
            Console.WriteLine($"tgtJ = [{string.Join(", ", tgtJ)}]");
            Console.WriteLine($"tgtW = [{string.Join(", ", tgtW)}]");
            Console.WriteLine($"tgtT = [{string.Join(", ", tgtT)}]");
            Console.WriteLine($"actFingerLeft = [{string.Join(", ", actFingerLeft)}]");
            Console.WriteLine($"actFingerRight = [{string.Join(", ", actFingerRight)}]");
            Console.WriteLine($"tgtFingerLeft = [{string.Join(", ", tgtFingerLeft)}]");
            Console.WriteLine($"tgtFingerRight = [{string.Join(", ", tgtFingerRight)}]");
        }

        public void UnpackData(byte[] buffer)
        {
            int offset = 0;

            // Unpack size (int)
            size[0] = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);

            // Unpack timestamp (double)
            timestamp[0] = BitConverter.ToDouble(buffer, offset);
            offset += sizeof(double);

            // Unpack key (2 shorts)
            for (int i = 0; i < 2; i++)
            {
                key[i] = BitConverter.ToInt16(buffer, offset);
                offset += sizeof(short);
            }

            // Unpack planName (16 bytes string)
            planName = Encoding.UTF8.GetString(buffer, offset, 16).TrimEnd('\0');
            offset += 16;

            // Unpack state (2 shorts)
            for (int i = 0; i < 2; i++)
            {
                state[i] = BitConverter.ToInt16(buffer, offset);
                offset += sizeof(short);
            }

            // Unpack joy (4 floats)
            for (int i = 0; i < 4; i++)
            {
                joy[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }

            // Unpack rpy, gyr, acc (3 floats each)
            for (int i = 0; i < 3; i++)
            {
                rpy[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }
            for (int i = 0; i < 3; i++)
            {
                gyr[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }
            for (int i = 0; i < 3; i++)
            {
                acc[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }

            // Unpack actJ, actW, actT (jntNum floats each)
            for (int i = 0; i < jntNum; i++)
            {
                actJ[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }
            for (int i = 0; i < jntNum; i++)
            {
                actW[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }
            for (int i = 0; i < jntNum; i++)
            {
                actT[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }

            // Unpack drvTemp, drvState, drvErr (jntNum shorts each)
            for (int i = 0; i < jntNum; i++)
            {
                drvTemp[i] = BitConverter.ToInt16(buffer, offset);
                offset += sizeof(short);
            }
            for (int i = 0; i < jntNum; i++)
            {
                drvState[i] = BitConverter.ToInt16(buffer, offset);
                offset += sizeof(short);
            }
            for (int i = 0; i < jntNum; i++)
            {
                drvErr[i] = BitConverter.ToInt16(buffer, offset);
                offset += sizeof(short);
            }

            // Unpack tgtJ, tgtW, tgtT (jntNum floats each)
            for (int i = 0; i < jntNum; i++)
            {
                tgtJ[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }
            for (int i = 0; i < jntNum; i++)
            {
                tgtW[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }
            for (int i = 0; i < jntNum; i++)
            {
                tgtT[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }

            // Unpack finger data
            for (int i = 0; i < fingerDofLeft; i++)
            {
                actFingerLeft[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }
            for (int i = 0; i < fingerDofRight; i++)
            {
                actFingerRight[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }
            for (int i = 0; i < fingerDofLeft; i++)
            {
                tgtFingerLeft[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }
            for (int i = 0; i < fingerDofRight; i++)
            {
                tgtFingerRight[i] = BitConverter.ToSingle(buffer, offset);
                offset += sizeof(float);
            }
        }
    }

    /// <summary>
    /// Control data class for joint SDK
    /// </summary>
    public class JntSdkCtrlDataClass
    {
        public short checker = 3480;
        public short size;
        public int state = 0;
        public float torLimitRate = 0.2f;
        public float filtRate = 0.05f;
        public float[] j;
        public float[] w;
        public float[] t;
        public float[] kp;
        public float[] kd;
        public float[] fingerLeft;
        public float[] fingerRight;

        private int jntNum;
        private int fingerDofLeft;
        private int fingerDofRight;

        public JntSdkCtrlDataClass(int jntNum, int fingerDofLeft, int fingerDofRight)
        {
            this.jntNum = jntNum;
            this.fingerDofLeft = fingerDofLeft;
            this.fingerDofRight = fingerDofRight;

            // Calculate size
            size = (short)(16 + jntNum * 4 * 5 + (fingerDofLeft + fingerDofRight) * 4);

            // Initialize arrays
            j = GetStdJnt();
            w = new float[jntNum];
            t = new float[jntNum];
            kp = new float[jntNum];
            kd = new float[jntNum];
            fingerLeft = new float[fingerDofLeft];
            fingerRight = new float[fingerDofRight];

            Reset();
        }

        public void Reset()
        {
            state = 0;
            torLimitRate = 0.2f;
            filtRate = 0.05f;
            j = GetStdJnt();
            
            // Clear arrays
            Array.Clear(w, 0, w.Length);
            Array.Clear(t, 0, t.Length);
            Array.Clear(kp, 0, kp.Length);
            Array.Clear(kd, 0, kd.Length);
            Array.Clear(fingerLeft, 0, fingerLeft.Length);
            Array.Clear(fingerRight, 0, fingerRight.Length);
        }

        public float[] GetStdJnt()
        {
            return new float[] {
                0.3f, -1.3f, 1.8f, 0.5f, 0f, 0f, 0f,
                -0.3f, -1.3f, -1.8f, 0.5f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f, 0f,
                0.0533331f, 0f, 0.325429f, -0.712646f, 0.387217f, -0.0533331f,
                -0.0533331f, 0f, 0.325429f, -0.712646f, 0.387217f, 0.0533331f
            };
        }

        public byte[] PackData()
        {
            // Calculate total buffer size
            int bufferSize = 2 * sizeof(short) + sizeof(int) + 2 * sizeof(float) +
                             jntNum * 5 * sizeof(float) +
                             (fingerDofLeft + fingerDofRight) * sizeof(float);
            
            byte[] buffer = new byte[bufferSize];
            int offset = 0;

            // Pack checker and size (shorts)
            Buffer.BlockCopy(BitConverter.GetBytes(checker), 0, buffer, offset, sizeof(short));
            offset += sizeof(short);
            Buffer.BlockCopy(BitConverter.GetBytes(size), 0, buffer, offset, sizeof(short));
            offset += sizeof(short);

            // Pack state (int)
            Buffer.BlockCopy(BitConverter.GetBytes(state), 0, buffer, offset, sizeof(int));
            offset += sizeof(int);

            // Pack torLimitRate and filtRate (floats)
            Buffer.BlockCopy(BitConverter.GetBytes(torLimitRate), 0, buffer, offset, sizeof(float));
            offset += sizeof(float);
            Buffer.BlockCopy(BitConverter.GetBytes(filtRate), 0, buffer, offset, sizeof(float));
            offset += sizeof(float);

            // Pack j, w, t, kp, kd arrays (floats)
            Buffer.BlockCopy(j, 0, buffer, offset, jntNum * sizeof(float));
            offset += jntNum * sizeof(float);
            Buffer.BlockCopy(w, 0, buffer, offset, jntNum * sizeof(float));
            offset += jntNum * sizeof(float);
            Buffer.BlockCopy(t, 0, buffer, offset, jntNum * sizeof(float));
            offset += jntNum * sizeof(float);
            Buffer.BlockCopy(kp, 0, buffer, offset, jntNum * sizeof(float));
            offset += jntNum * sizeof(float);
            Buffer.BlockCopy(kd, 0, buffer, offset, jntNum * sizeof(float));
            offset += jntNum * sizeof(float);

            // Pack fingerLeft and fingerRight arrays (floats)
            Buffer.BlockCopy(fingerLeft, 0, buffer, offset, fingerDofLeft * sizeof(float));
            offset += fingerDofLeft * sizeof(float);
            Buffer.BlockCopy(fingerRight, 0, buffer, offset, fingerDofRight * sizeof(float));

            // Verify size
            if (buffer.Length != size)
            {
                Console.WriteLine($"JntSdk ctrl data packing size mismatch! {buffer.Length} != {size}");
                Environment.Exit(1);
            }

            return buffer;
        }
    }
}
