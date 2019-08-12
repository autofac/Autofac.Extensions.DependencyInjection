// This software is part of the Autofac IoC container
// Copyright © 2019 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using Microsoft.Extensions.Hosting;

namespace Autofac.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension method on <see cref="IServiceProvider" /> to get the instance of <see cref="AutofacServiceProvider"/> when it has been registered with <see cref="IHost"/>.
    /// <seealso cref="HostBuilderExtensions" />
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Tries to cast the instance of <see cref="ILifetimeScope"/> from <see cref="AutofacServiceProvider"/> when possible.
        /// </summary>
        /// <param name="serviceProvider">The instance of <see cref="IServiceProvider"/>.</param>
        /// <exception cref="InvalidOperationException">Throws an <see cref="InvalidOperationException"/> when instance of <see cref="IServiceProvider"/> can't be assigned to <see cref="AutofacServiceProvider"/>.</exception>
        /// <returns>Returns the instance of <see cref="ILifetimeScope"/> exposed by <see cref="AutofacServiceProvider"/>.</returns>
        public static ILifetimeScope GetAutofacRoot(this IServiceProvider serviceProvider)
        {
            if (!(serviceProvider is AutofacServiceProvider autofacServiceProvider))
                throw new InvalidOperationException($"Instance of {nameof(serviceProvider)} is not of type {nameof(AutofacServiceProvider)}");

            return autofacServiceProvider.LifetimeScope;
        }
    }
}