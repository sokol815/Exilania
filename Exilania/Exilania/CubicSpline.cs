using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exilania
{
    public class CubicSpline
    {
        double[] h;
        double[] b;
        double[] u;
        double[] v;
        double[] z;
        double[] xs;
        double[] ys;


        public CubicSpline()
        {

        }



        public CubicSpline(double[] x, double[] y)
        {
            int n = x.Length;

            xs = new double[n];
            x.CopyTo(xs,0);
            ys = new double[n];
            y.CopyTo(ys, 0);
            h = new double[n];
            b = new double[n];
            for (int i = 0; i < n - 1; i++)
            {
                h[i] = x[i + 1] - x[i];
                b[i] = (y[i + 1] - y[i]) / h[i];
            }

            u = new double[n];
            v = new double[n];
            u[1] = 2 * (h[0] + h[1]);
            v[1] = 6 * (b[1] - b[0]);
            for (int i = 2; i < n - 1; i++)
            {
                u[i] = 2 * (h[i - 1] + h[i]) - (h[i - 1] * h[i - 1]) / u[i - 1];
                v[i] = 6 * (b[i] - b[i - 1]) - (h[i - 1] * v[i - 1]) / u[i - 1];
            }

            z = new double[n];
            z[n - 1] = 0;
            for (int i = n - 2; i > 0; i--)
                z[i] = (v[i] - h[i] * z[i + 1]) / u[i];
            z[0] = 0;

            
            /*double[] S = new double[questions.Length];
            int j = 0;
            for (int i = 0; i < S.Length; i++)
            {
                if (i >= x[j + 1] && j < x.Length - 2)
                    j++;
                double va = y[j];
                double vb = -(h[j] / 6) * z[j + 1] - (h[j] / 3) * z[j] + (y[j + 1] - y[j]) / h[j];
                double vc = z[j] / 2;
                double vd = (z[j + 1] - z[j]) / (6 * h[j]);
                S[i] = va + (i - x[j]) * (vb + (i - x[j]) * (vc + (i - x[j]) * vd));
            }

            return S;*/
        }

        public double get_val_at(double xloc)
        {
            int j = 0;
            while (xloc >= xs[j + 1] && j < xs.Length - 2)
            {
                j++;
            }
            double va = ys[j];
            double vb = -(h[j] / 6) * z[j + 1] - (h[j] / 3) * z[j] + (ys[j + 1] - ys[j]) / h[j];
            double vc = z[j] / 2;
            double vd = (z[j + 1] - z[j]) / (6 * h[j]);
            return  va + (xloc - xs[j]) * (vb + (xloc - xs[j]) * (vc + (xloc - xs[j]) * vd));
        }

    }
}
