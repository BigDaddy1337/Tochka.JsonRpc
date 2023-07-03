﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tochka.JsonRpc.ApiExplorer;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;
using Utils = Tochka.JsonRpc.Server.Utils;

namespace Tochka.JsonRpc.Swagger;

public class JsonRpcSchemaGenerator : ISchemaGenerator
{
    private readonly SchemaGeneratorOptions generatorOptions;
    private readonly IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders;
    private readonly JsonRpcServerOptions options;

    public JsonRpcSchemaGenerator(SchemaGeneratorOptions generatorOptions, IEnumerable<IJsonSerializerOptionsProvider> serializerOptionsProviders, IOptions<JsonRpcServerOptions> options)
    {
        this.generatorOptions = generatorOptions;
        this.serializerOptionsProviders = serializerOptionsProviders;
        this.options = options.Value;
    }

    public OpenApiSchema GenerateSchema(Type modelType, SchemaRepository schemaRepository, MemberInfo? memberInfo = null, ParameterInfo? parameterInfo = null, ApiParameterRouteInfo? routeInfo = null)
    {
        var typeMetadata = modelType.GetCustomAttribute<JsonRpcTypeMetadataAttribute>();
        var serializerOptions = typeMetadata?.SerializerOptionsProviderType == null
            ? options.DefaultDataJsonSerializerOptions
            : Utils.GetJsonSerializerOptions(serializerOptionsProviders, typeMetadata.SerializerOptionsProviderType);
        return UseDefaultGenerator(modelType, schemaRepository, memberInfo, parameterInfo, routeInfo, serializerOptions);
    }

    // internal virtual for mocking in tests
    [ExcludeFromCodeCoverage]
    internal virtual OpenApiSchema UseDefaultGenerator(Type modelType, SchemaRepository schemaRepository, MemberInfo? memberInfo, ParameterInfo? parameterInfo, ApiParameterRouteInfo? routeInfo, JsonSerializerOptions serializerOptions)
    {
        var schemaGenerator = new SchemaGenerator(generatorOptions, new JsonSerializerDataContractResolver(serializerOptions));
        return schemaGenerator.GenerateSchema(modelType, schemaRepository, memberInfo, parameterInfo, routeInfo);
    }
}
