// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;

namespace Integration.Net6.Controllers;

[ApiController]
[Route("[controller]")]
public class DateController : ControllerBase
{
    public DateController(IDateProvider dateProvider)
    {
        DateProvider = dateProvider;
    }

    public IDateProvider DateProvider { get; }

    [HttpGet]
    public ActionResult<DateTimeOffset> Get() => DateProvider.GetDate();
}
