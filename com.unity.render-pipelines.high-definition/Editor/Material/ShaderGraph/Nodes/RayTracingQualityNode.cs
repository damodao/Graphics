using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Drawing;
using UnityEditor.ShaderGraph.Internal;

namespace UnityEditor.Rendering.HighDefinition
{
    [FormerName("UnityEditor.Rendering.HighDefinition.RayTracingNode")]
    class RayTracingQualityNode
    {
        private const string k_KeywordDefault = "RAYTRACING_SHADER_GRAPH_DEFAULT";
        private const string k_KeywordOptimized = "RAYTRACING_SHADER_GRAPH_OPTIMIZED";

        public enum RayTracingQualityVariant
        {
            Default,
            Optimized
        }

        public static string RaytracingVariantKeyword(RayTracingQualityVariant variant)
        {
            switch (variant)
            {
                case RayTracingQualityVariant.Default: return k_KeywordDefault;
                case RayTracingQualityVariant.Optimized: return k_KeywordOptimized;
                default: throw new ArgumentOutOfRangeException(nameof(variant));
            }
        }

        [BuiltinKeyword]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        public static KeywordDescriptor GetRayTracingQualityKeyword()
        {
            return new KeywordDescriptor()
            {
                displayName = "Raytracing Quality",
                referenceName = "RAYTRACING_SHADER_GRAPH",
                type = KeywordType.Enum,
                definition = KeywordDefinition.Predefined,
                scope = KeywordScope.Global,
                value = 0,
                entries = new KeywordEntry[]
                {
                    new KeywordEntry("Default", "DEFAULT"),
                    new KeywordEntry("Optimized", "OPTIMIZED"),
                },
            };
        }
    }
}
