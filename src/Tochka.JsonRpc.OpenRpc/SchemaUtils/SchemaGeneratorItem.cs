using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NJsonSchema;
using NJsonSchema.Generation;
using Tochka.JsonRpc.Common.Serializers;

namespace Tochka.JsonRpc.OpenRpc.SchemaUtils
{
    public class SchemaGeneratorItem
    {
        public JsonSchemaGenerator Generator;
        public JsonSchemaResolver Resolver;

        public SchemaGeneratorItem(IJsonRpcSerializer jsonRpcSerializer, JsonSchema root)
        {
            var settings = new JsonSchemaGeneratorSettings()
            {
                DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull,
                SerializerSettings = jsonRpcSerializer.Settings,
                SchemaNameGenerator = new NameGenerator(jsonRpcSerializer),
                TypeNameGenerator = new CamelCaseTypeNameGenerator(),
                GenerateExamples = true
            };
            Resolver = new JsonSchemaResolver(root, settings);
            Generator = new JsonSchemaGenerator(settings);
        }

        public JsonSchema Generate(Type type)
        {
            return Generator.Generate(type, Resolver);
        }

    }
}