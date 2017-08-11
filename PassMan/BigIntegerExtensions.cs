using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PassMan
{
    class BigIntegerExtensions
    {
        public static BigInteger Encrypt(BigInteger plaintext, string password, int bytewidth)
        {
            BigInteger[] edn = PasswordToEDN(password, bytewidth);
            return BigInteger.ModPow(plaintext, edn[0], edn[2]);
        }

        public static BigInteger Decrypt(BigInteger ciphertext, string password, int bytewidth)
        {
            BigInteger[] edn = PasswordToEDN(password, bytewidth);
            return BigInteger.ModPow(ciphertext, edn[1], edn[2]);
        }

        public static BigInteger[] PasswordToEDN(string password, int bytewidth)
        {
            //derives from password string 
            //numbers e, d, n 
            // for which
            //        ed ~ 1 (mod (n-1))
            // and
            //        n > 256^bytewidth
            // and
            //        n is prime

            byte[] temp = new byte[bytewidth + 1];
            temp[bytewidth] = 1;
            Random rnd = new Random();

            //derive a large prime number, n, from password hashcode
            BigInteger hashcode = new BigInteger(Math.Abs(password.GetHashCode()));
            BigInteger modMax = BigInteger.Parse("2147483647");

            for (int i = 0; i < bytewidth; i++)
            {
                temp[i] = (byte)(hashcode % 256);
                hashcode = BigInteger.ModPow(hashcode, 2, modMax);
            }
            BigInteger n = new BigInteger(temp);
            n = NextProbablePrime(n, 100, rnd);

            //find e, the jth number that is
            //relatively prime to n-1.
            //       GCD(e_j, n-1) == 1
            //where j is derived from
            //the password hashcode
            BigInteger nMinusOne = n - 1;
            BigInteger e = new BigInteger(1);
            int j = (int)(hashcode % 1000);
            for (int k = 0; k < j; k++)
            {
                e = nNextRelativePrimeToM(e, nMinusOne);
            }

            //find e inverse, d
            BigInteger d = xInverseModN(e, nMinusOne);

            return new BigInteger[] { e, d, n };
        }

        public static BigInteger NextProbablePrime(BigInteger n, int numTrials, Random random)
        {
            BigInteger nNext = n + 1;
            if (nNext.IsEven)
            {
                nNext++;
            }

            while (MillerRabinPrimalityTest(nNext, numTrials, random) == 0.0)
            {
                nNext += 2;
            }

            return nNext;
        }

        public static double MillerRabinPrimalityTest(BigInteger n, int numTrials, Random random)
        {
            //Probability that n passes test but is not prime ~ 4^(-numTrials)
            if ((n == 2) || (n == 3) || (n == 5))
            {
                return 1.0;
            }
            if (n.IsEven)
            {
                return 0.0;
            }
            else
            {
                //find d, r, for which d*(2^r) = n-1
                BigInteger d = n - 1;
                int r = 0;
                while (d.IsEven)
                {
                    r++;
                    d = d / 2;
                }
                BigInteger nMinusThree = n - 3;
                for (int i = 0; i < numTrials; i++)
                {
                    BigInteger a = RandomBigIntegerLessThan(nMinusThree, random);
                    while (a < 2 || a > nMinusThree)
                    {
                        a = RandomBigIntegerLessThan(nMinusThree, random);
                    }
                    BigInteger x = BigInteger.ModPow(a, d, n);
                    if (!(x.IsOne || (x == (n - 1))))
                    {
                        int j = 1;
                        while ((j < r) && (x != (n - 1)))
                        {
                            x = BigInteger.ModPow(x, 2, n);
                            if (x.IsOne)
                            {
                                return 0.0;
                            }
                            j++;
                        }
                        if (x != (n - 1))
                        {
                            return 0.0;
                        }
                    }
                }
                return Math.Pow(4.0, Convert.ToDouble(-numTrials));
            }
        }

        public static BigInteger RandomBigIntegerLessThan(BigInteger N, Random random)
        {
            byte[] bytes = N.ToByteArray();
            BigInteger R;

            do
            {
                random.NextBytes(bytes);
                bytes[bytes.Length - 1] &= (byte)0x7F; //force sign bit to positive
                R = new BigInteger(bytes);
            } while (R >= N);

            return R;
        }

        public static BigInteger nNextRelativePrimeToM(BigInteger n, BigInteger m)
        {
            BigInteger nextRelativePrimeToM = (n + 1) % m;
            BigInteger gcd = BigInteger.GreatestCommonDivisor(nextRelativePrimeToM, m);
            while (gcd != 1)
            {
                nextRelativePrimeToM = (nextRelativePrimeToM + 1) % m;
                gcd = BigInteger.GreatestCommonDivisor(nextRelativePrimeToM, m);
            }

            return nextRelativePrimeToM;
        }

        public static BigInteger xInverseModN(BigInteger x, BigInteger n)
        {
            if (BigInteger.GreatestCommonDivisor(x, n) == 1)
            {
                BigInteger[] gcd = ExtendedEuclideanAlg(x, n);
                BigInteger inverse = gcd[0];
                while (inverse < 0)
                {
                    inverse += n;
                }
                return inverse;
            }
            else
            {
                return 0;
            }
        }

        public static BigInteger[] ExtendedEuclideanAlg(BigInteger x, BigInteger y)
        {
            List<BigInteger> n = new List<BigInteger>();
            List<BigInteger> u = new List<BigInteger>();
            List<BigInteger> v = new List<BigInteger>();
            BigInteger q, r;

            //initialize:
            //         [_____u____________v____________n_____]
            //    R0:  [  sign(x)*x  +    0y       =  abs(x) ]
            //    R1:  [     0x      +  sign(y)*y  =  abs(y) ]
            n.Add(x * x.Sign);
            n.Add(y * y.Sign);
            u.Add(x.Sign);
            u.Add(0);
            v.Add(0);
            v.Add(y.Sign);

            // add multiples of rows together until n.last == gcd(x,y)
            while ((!(BigInteger.Equals(n[n.Count - 1], BigInteger.One))) && (!BigInteger.Equals(n[n.Count - 1], n[n.Count - 2])))
            {
                q = BigInteger.DivRem(n[n.Count - 2], n[n.Count - 1], out r);

                //if n.last divides n.secondToLast, 
                //     n.last == gcd(x,y)
                if (BigInteger.Equals(r, BigInteger.Zero))
                {
                    q = q - BigInteger.One;
                }

                n.Add(n[n.Count - 2] - q * n[n.Count - 1]);
                u.Add(u[u.Count - 2] - q * u[u.Count - 1]);
                v.Add(v[v.Count - 2] - q * v[v.Count - 1]);
            }

            // u.last*x + v.last*y = n.last = gcd(x, y)
            return new BigInteger[3] { u.Last(), v.Last(), n.Last() };
        }
    }
}
