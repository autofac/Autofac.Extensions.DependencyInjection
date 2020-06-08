using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenchProject.AutofacApiServer
{
    public class A
    {
        public A(B1 b1, B2 b2) { }
    }

    public class B1
    {
        public B1(B2 b2, C1 c1, C2 c2) { }
    }

    public class B2
    {
        public B2(C1 c1, C2 c2) { }
    }

    public class C1
    {
        public C1(C2 c2, D1 d1, D2 d2) { }
    }

    public class C2
    {
        public C2(D1 d1, D2 d2) { }
    }

    public class D1 { }

    public class D2 { }
}
