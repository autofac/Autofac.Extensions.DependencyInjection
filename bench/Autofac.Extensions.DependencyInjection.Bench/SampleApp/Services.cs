// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#pragma warning disable SA1402, SA1649

namespace Autofac.Extensions.DependencyInjection.Bench.SampleApp;

public class A
{
    public A(B1 b1, B2 b2)
    {
    }
}

public class B1
{
    public B1(B2 b2, C1 c1, C2 c2)
    {
    }
}

public class B2
{
    public B2(C1 c1, C2 c2)
    {
    }
}

public class C1
{
    public C1(C2 c2, D1 d1, D2 d2)
    {
    }
}

public class C2
{
    public C2(D1 d1, D2 d2)
    {
    }
}

public class D1
{
}

public class D2
{
}

#pragma warning restore SA1402, SA1649
