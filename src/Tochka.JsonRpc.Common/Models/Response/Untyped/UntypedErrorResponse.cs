﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tochka.JsonRpc.Common.Models.Id;
using Tochka.JsonRpc.Common.Models.Response.Errors;

namespace Tochka.JsonRpc.Common.Models.Response.Untyped;

[ExcludeFromCodeCoverage]
public record UntypedErrorResponse(IRpcId Id, Error<JsonDocument> Error, string Jsonrpc = JsonRpcConstants.Version)
    : ErrorResponse<JsonDocument>(Id, Error, Jsonrpc);
