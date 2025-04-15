using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VISAInstrument
{
    internal class wavefunction
    {
        //三角波一个周期波形函数，输入参数为上图对应参数
        public static List<List<double>> triangular(double Amplitude, double Offset, double Frequency, double Phase)
        {
            //设置x,y的列表来储存x,y的值。分为几段函数就设置几个列表
            List<double> x1 = new List<double>();
            List<double> y1 = new List<double>();
            List<double> x2 = new List<double>();
            List<double> y2 = new List<double>();
            List<double> x3 = new List<double>();
            List<double> y3 = new List<double>();
            double T = (double)(1 / Frequency);//周期T
            int num = 200;//一个周期取值200个点画线
                          //用for循环遍历将x,y的值存入列表
            for (int i = 1; i < (num / 4) + 1; i++)
            {
                x1.Add((i * (T / num)) + Phase);
                x3.Add((i * (T / num) + 3 * (T / 4)) + Phase);

                y1.Add((double)((Amplitude / (T / 4)) * ((i * (T / 200))) + Offset));
                y3.Add((double)(((4 * ((i * (T / 200) + 3 * (T / 4))) - 3 * T) / T * Amplitude) - Amplitude) + Offset);

            }
            for (int i = 1; i < (num / 2) + 1; i++)
            {
                x2.Add((i * (T / num) + T / 4) + Phase);

                y2.Add((double)(((T / 2 - ((i * (T / 200) + T / 4)))) * (Amplitude / (T / 4)) + Offset));

            }
            //用中间变量将分段的函数列表中所有元素依次存入一个列表resX（Y）
            List<double> X = x1.Concat(x2).ToList<double>();
            List<double> resX = X.Concat(x3).ToList<double>();
            List<double> Y = y1.Concat(y2).ToList<double>();
            List<double> resY = Y.Concat(y3).ToList<double>();
            //创建一个列表N,T存储预设值num（一个周期的取点数量），周期
            List<double> resN = new List<double>();
            List<double> resT = new List<double>();
            resN.Add(num);
            resT.Add(T);
            //创建一个list的list列表，将四个列表添加到此列表中
            List<List<double>> RET = new List<List<double>>();
            RET.Add(resX);
            RET.Add(resY);
            RET.Add(resN);
            RET.Add(resT);
            //返回一个元素为list的list
            return RET;

        }

        //直线函数，注释参见三角波函数
        public static List<List<double>> line(double Offset)
        {
            List<double> y1 = new List<double>();
            List<double> x1 = new List<double>();
            int num = 201;
            double T = 10;
            for (int i = 0; i < num; i++)
            {
                x1.Add(i * (T / 200));
                y1.Add(Offset);
            }
            List<double> resN = new List<double>();
            List<double> resT = new List<double>();
            resN.Add(num);
            resT.Add(T);
            List<List<double>> RET = new List<List<double>>();
            RET.Add(x1);
            RET.Add(y1);
            RET.Add(resN);
            RET.Add(resT);
            return RET;
        }
        //方波函数，注释参见三角波函数
        public static List<List<double>> rectangle(double Amplitude, double Offset, double Frequency, double Phase, double Dutycyle)
        {
            List<double> x1 = new List<double>();
            List<double> x2 = new List<double>();
            List<double> y1 = new List<double>();
            List<double> y2 = new List<double>();
            double T = 1 / Frequency;
            int num = 200;
            int num1 = (int)(num * Dutycyle);
            for (int i = 0; i < num1; i++)
            {
                x1.Add((i * (T / 200)) + Phase);

                y1.Add(Amplitude + Offset);

            }
            for (int i = 0; i < num - num1; i++)
            {
                x2.Add((i * (T / 200) + T / 2) + Phase);
                y2.Add(-Amplitude + Offset);
            }

            List<double> resX = x1.Concat(x2).ToList<double>();
            List<double> resY = y1.Concat(y2).ToList<double>();
            List<double> resN = new List<double>();
            List<double> resT = new List<double>();
            resN.Add(num);
            resT.Add(T);
            List<List<double>> RET = new List<List<double>>();
            RET.Add(resX);
            RET.Add(resY);
            RET.Add(resN);
            RET.Add(resT);
            return RET;

        }
        //脉冲函数，注释参见三角波函数
        public static List<List<double>> rectangle1(double Amplitude, double Offset, double Frequency, double Phase, double Plusewidth, double Leading, double Trailing)
        {

            List<double> x1 = new List<double>();
            List<double> x2 = new List<double>();
            List<double> y1 = new List<double>();
            List<double> y2 = new List<double>();
            List<double> x3 = new List<double>();
            List<double> x4 = new List<double>();
            List<double> y3 = new List<double>();
            List<double> y4 = new List<double>();
            double T = 1 / Frequency;
            int num = 200;
            int num1 = (int)((Leading / T) * num);
            int num2 = (int)((Plusewidth / T) * num);
            int num3 = num - 2 * num1 - num2;
            for (int i = 0; i < num1; i++)
            {
                x1.Add((i * (T / 200)) + Phase);
                x3.Add((i * (T / 200) + Leading + Plusewidth) + Phase);
                y1.Add(((Amplitude / Leading) * (i * (T / 200))) - Amplitude / 2);
                y3.Add(((Amplitude / Trailing) * (Leading + Plusewidth - (i * (T / 200) + Leading + Plusewidth))) + Offset - Amplitude / 2);
            }
            for (int i = 0; i < num2; i++)
            {
                x2.Add((i * (T / 200) + Leading) + Phase);
                y2.Add(Amplitude / 2);
            }
            for (int i = 0; i < num3; i++)
            {
                x4.Add((i * (T / 200) + (Leading + Plusewidth + Trailing)) + Phase);
                y4.Add(-Amplitude / 2);
            }
            List<double> X = x1.Concat(x2).ToList<double>();
            List<double> X1 = X.Concat(x3).ToList<double>();
            List<double> resX = X1.Concat(x4).ToList<double>();
            List<double> Y = y1.Concat(y2).ToList<double>();
            List<double> Y1 = Y.Concat(y3).ToList<double>();
            List<double> resY = Y1.Concat(y4).ToList<double>();
            List<double> resN = new List<double>();
            List<double> resT = new List<double>();
            resN.Add(num);
            resT.Add(T);
            List<List<double>> RET = new List<List<double>>();
            RET.Add(resX);
            RET.Add(resY);
            RET.Add(resN);
            RET.Add(resT);
            return RET;
        }


        static int GetRandomSeed() //产生随机种子
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
//高斯噪声函数
        public static List<List<double>> GaussNiose1(int num,int A, int Offset, double Bandwidth)//用box muller的方法产生均值为0，方差为1的正太分布随机数
        {
            // Random ro = new Random(10);
            // long tick = DateTime.Now.Ticks;
            List<double> x1 = new List<double>();
            List<double> y1 = new List<double>();
            double T = Bandwidth;
            for (int i = 0; i < num; i++)
            {
                Random ran = new Random(GetRandomSeed());
                // Random rand = new Random();
                double r1 = ran.NextDouble();
                double r2 = ran.NextDouble();
                double result = Math.Sqrt((-2) * Math.Log(r2)) * Math.Sin(2 * Math.PI * r1);
                x1.Add(i * (T / num));
                y1.Add(A / 2 * result + Offset);
            }
            List<double> resN = new List<double>();
            List<double> resT = new List<double>();
            resN.Add(num);
            resT.Add(T);
            List<List<double>> RET = new List<List<double>>();
            RET.Add(x1);
            RET.Add(y1);
            RET.Add(resN);
            RET.Add(resT);
            return RET;

        }

        //锯齿波函数，注释参见三角波函数
        public static List<List<double>> sawtooth(double Amplitude, double Offset, double Frequency, double Phase, double Symmetry)
        {
            List<double> x1 = new List<double>();
            List<double> x2 = new List<double>();
            List<double> y1 = new List<double>();
            List<double> y2 = new List<double>();
            double T = 1 / Frequency;
            double num = 200;
            for (int i = 0; i < (num / 2); i++)
            {
                x1.Add((i * (T / num)) + Phase);
                x2.Add((i * (T / num) + T / 2) + Phase);
                y1.Add((double)(Amplitude / (T / 2)) * ((i * (T / 200)) + Offset));
                y2.Add(((double)((Amplitude / (T / 2)) * (i * (T / 200) + T / 2)) - 2 * Amplitude) + Offset);
            }
            List<double> resX = x1.Concat(x2).ToList<double>();
            List<double> resY = y1.Concat(y2).ToList<double>();
            List<double> resN = new List<double>();
            List<double> resT = new List<double>();
            resN.Add(num);
            resT.Add(T);
            List<List<double>> RET = new List<List<double>>();
            RET.Add(resX);
            RET.Add(resY);
            RET.Add(resN);
            RET.Add(resT);
            return RET;
        }

        //起止值函数
        public static List<List<double>> range(double x1, double x2, List<List<double>> RET)
        {
            //取出上面函数的T
            double T = RET[3][0];
            //取出上面函数的num
            int num = (int)(RET[2][0]);
            //定义num1储存x2到x1的距离
            double num1 = x2 - x1;
            int num2 = (int)(num1 / T);//x1到x2的完整周期个数
            int num3 = (int)(x1 / T);
            int num4 = (int)(x2 / T);
            double x3 = x1 - num3 * T;//x1在第一个周期内的对应值
            double x4 = x2 - num4 * T;//x2在第一个周期内的对应值
            int num5 = (int)((x3 / T) * num);
            int num6 = num - num5;
            int num7;
// 判断x2是否为完整周期段的结束点，若是则第三段增加一个num的点数
            if (x4 == 0)
            {
                num7 = (int)(((x4 / T) * num) + num);
            }
            else
            {
                num7 = (int)(((x4 / T) * num) + 0.5);
            }
            List<double> xa = new List<double>();
            List<double> ya = new List<double>();
            List<double> yb = new List<double>();
            List<double> yc = new List<double>();
            List<double> resY = RET[1];
            ya.AddRange(resY.Skip(num5).Take(num6));
            if (num2 > 0)
            {
                for (int i = 1; i < num2; i++)
                {
                    yb.AddRange(resY);
                }
                yc.AddRange(resY.Take(num7));
            }
            else
            {
                yb.Clear();
                yc.Clear();
            }

            List<double> Ya = ya.Concat(yb).ToList<double>();
            List<double> Y = Ya.Concat(yc).ToList<double>();
            int num8 = (int)((num1 / T) * num + 0.5);
            List<double> X = new List<double>();
            for (int i = 0; i < num8; i++)
            {

                X.Add(x1 + (i * (T / num)));
            }

            //List<double> Y = y1.Concat(y2).ToList<double>();
            //List<double> resY = Y.Concat(y3).ToList<double>();
            List<List<double>> RETa = new List<List<double>>();
            RETa.Add(X);
            RETa.Add(Y);
            return RETa;
        }

    }
}
