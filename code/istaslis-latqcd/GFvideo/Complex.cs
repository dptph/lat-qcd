using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GFVideo
{
    using floattype = System.Single;

    public struct Complex
    {
        public static Complex One = new Complex(1, 0);
        public static Complex MinOne = new Complex(-1, 0);
        public static Complex Zero = new Complex(0, 0);
        public static Complex I = new Complex(0, 1);
        public static Complex MinI = new Complex(0, -1);

        public floattype x, y;
        public Complex(floattype X, floattype Y)
        {
            x = X; y = Y;
        }
        static public Complex operator *(Complex A, Complex B)
        {
            return new Complex(A.x * B.x - A.y * B.y, A.x * B.y + A.y * B.x);
        }
        public static Complex operator /(Complex A, Complex B)//THIS METHOD WAS ADDED WITH DIRAC!!!!
        {
            return new Complex((A.x * B.x + A.y * B.y) / (B.x * B.x + B.y * B.y), (A.y * B.x - A.x * B.y) / (B.x * B.x + B.y * B.y));
        }
        static public Complex operator -(Complex A, Complex B)
        {
            return new Complex(A.x - B.x, A.y - B.y);
        }
        static public Complex operator +(Complex A, Complex B)
        {
            return new Complex(A.x + B.x, A.y + B.y);
        }
        public static Complex operator -(Complex A)//THIS METHOD WAS ADDED WITH DIRAC!!!!
        {
            return new Complex(-A.x, -A.y);
        }

        public override string ToString()
        {
            return x.ToString() + "+" + y.ToString() + "i";
        }

        public Complex Conj()
        {
            return new Complex(x, -y);
        }
        public floattype Module()
        {
            return (floattype)Math.Sqrt(x * x + y * y);
        }
    }
}
