﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using Tochka.JsonRpc.Common.Models.Request;
using Tochka.JsonRpc.Common.Models.Response.Errors;
using Tochka.JsonRpc.Server.Features;
using Tochka.JsonRpc.Server.Filters;
using Tochka.JsonRpc.Server.Services;

namespace Tochka.JsonRpc.Server.Tests.Filters;

[TestFixture]
internal class JsonRpcExceptionFilterTests
{
    private Mock<IJsonRpcErrorFactory> errorFactoryMock;
    private JsonRpcExceptionFilter exceptionFilter;

    [SetUp]
    public void Setup()
    {
        errorFactoryMock = new Mock<IJsonRpcErrorFactory>();

        exceptionFilter = new JsonRpcExceptionFilter(errorFactoryMock.Object);
    }

    [Test]
    public void OnException_NotJsonRpc_DoNothing()
    {
        var exception = new ArgumentException();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };

        exceptionFilter.OnException(context);

        context.Result.Should().BeNull();
    }

    [Test]
    public void OnException_JsonRpcCall_SetErrorResult()
    {
        var exception = new ArgumentException();
        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IJsonRpcFeature>(new JsonRpcFeature { Call = Mock.Of<ICall>() });
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
        var error = Mock.Of<IError>();
        errorFactoryMock.Setup(f => f.Exception(exception))
            .Returns(error)
            .Verifiable();

        exceptionFilter.OnException(context);

        var expected = new ObjectResult(error);
        context.Result.Should().BeEquivalentTo(expected);
        errorFactoryMock.Verify();
    }
}
