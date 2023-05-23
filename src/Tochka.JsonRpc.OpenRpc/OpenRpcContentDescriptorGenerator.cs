﻿using System.Reflection;
using System.Text.Json;
using Json.Schema.Generation;
using Microsoft.Extensions.Options;
using Namotion.Reflection;
using Tochka.JsonRpc.Common;
using Tochka.JsonRpc.OpenRpc.Models;
using Tochka.JsonRpc.Server.Metadata;
using Tochka.JsonRpc.Server.Serialization;
using Tochka.JsonRpc.Server.Settings;
using Utils = Tochka.JsonRpc.Server.Utils;

namespace Tochka.JsonRpc.OpenRpc;

public class OpenRpcContentDescriptorGenerator : IOpenRpcContentDescriptorGenerator
{
    private readonly IOpenRpcSchemaGenerator schemaGenerator;

    public OpenRpcContentDescriptorGenerator(IOpenRpcSchemaGenerator schemaGenerator) => this.schemaGenerator = schemaGenerator;

    public ContentDescriptor GenerateForType(ContextualType type, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        var name = jsonSerializerOptions.ConvertName(type.TypeName);
        var isRequired = type.GetAttribute<RequiredAttribute>() != null;
        return Generate(type, name, isRequired, methodName, jsonSerializerOptions);
    }

    public ContentDescriptor GenerateForParameter(ContextualType type, string methodName, JsonRpcParameterMetadata parameterMetadata, JsonSerializerOptions jsonSerializerOptions)
    {
        var name = parameterMetadata.PropertyName;
        var isRequired = !parameterMetadata.IsOptional;
        return Generate(type, name, isRequired, methodName, jsonSerializerOptions);
    }

    public ContentDescriptor GenerateForProperty(ContextualPropertyInfo propertyInfo, string methodName, JsonSerializerOptions jsonSerializerOptions)
    {
        var name = jsonSerializerOptions.ConvertName(propertyInfo.Name);
        var isRequired = propertyInfo.PropertyType.GetAttribute<RequiredAttribute>() != null;
        return Generate(propertyInfo.PropertyType, name, isRequired, methodName, jsonSerializerOptions);
    }

    private ContentDescriptor Generate(ContextualType type, string name, bool isRequired, string methodName, JsonSerializerOptions jsonSerializerOptions) =>
        new()
        {
            Name = name,
            Schema = schemaGenerator.CreateOrRef(type, methodName, jsonSerializerOptions),
            Summary = type.GetXmlDocsSummary(),
            Description = type.GetXmlDocsRemarks(),
            Required = isRequired,
            Deprecated = type.GetAttribute<ObsoleteAttribute>() != null
        };
}
