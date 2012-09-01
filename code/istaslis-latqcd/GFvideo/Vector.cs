using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCLNet;

namespace GFVideo
{
    using floattype = System.Single;

    public class Vector
    {
        public Mem buf;
        public floattype[] array = new floattype[12 * Core.N*2];
        IntPtr pointer;
        Random r = new Random();
        public Vector()
        {
            buf = Core.openCLContext.CreateBuffer((MemFlags)((long)MemFlags.READ_WRITE), 12*Core.N*Core.floatsize*2, IntPtr.Zero);
            pointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(array.Length * sizeof(floattype));
            Updatebuffer();
        }

        public void FillWith(Complex what)
        {
            Core.FillWithKernel.SetArg(0, (floattype)what.x);
            Core.FillWithKernel.SetArg(1, (floattype)what.y);
            Core.FillWithKernel.SetArg(2, buf);
            Core.openCLCQ.EnqueueNDRangeKernel(Core.FillWithKernel, 1, null, new int[1] { 4 * Core.N }, null);

            Core.openCLCQ.Finish();
        }


        public void FillWithRandom(bool normaldistribution)
        {
            Core.FillWithRandomKernel.SetArg(0, buf);
            Core.FillWithRandomKernel.SetArg(1, normaldistribution?(byte)1:(byte)0);
            Core.FillWithRandomKernel.SetArg(2, Core.SeedVectorMem);
            Core.openCLCQ.EnqueueNDRangeKernel(Core.FillWithRandomKernel, 1, null, new int[1] { 4 * Core.N }, null);

            Core.openCLCQ.Finish();
            Updatearray();
        }

        //public void FillWithRandom()
        //{
        //    for (int i = 0; i < array.Length; i++) array[i] = r.NextDouble();
        //    Updatebuffer();
        //}
        public void SetArrayToZero()
        {
            for (int i = 0; i < array.Length; i++) array[i] = 0;
            array[0] = 1;
        }


        public void Updatearray()
        {
            Core.openCLCQ.EnqueueReadBuffer(buf, true, 0, array.Length * Core.floatsize, pointer);
            System.Runtime.InteropServices.Marshal.Copy(pointer, array, 0, array.Length);
        }

        public void Updatebuffer()
        {
            System.Runtime.InteropServices.Marshal.Copy(array, 0, pointer, array.Length);
            Core.openCLCQ.EnqueueWriteBuffer(buf, true, 0, array.Length * Core.floatsize, pointer);

        }
    }
}
