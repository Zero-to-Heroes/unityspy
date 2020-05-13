﻿// ReSharper disable IdentifierTypo
namespace HackF5.UnitySpy.Detail
{
    // Remember that here pointers are 4 bytes
    internal static class MonoLibraryOffsets
    {
        // MonoAssembly defined at https://github.com/Unity-Technologies/mono/blob/unity-2018.4-mbe/mono/metadata/metadata-internals.h
        public const uint AssemblyImage = 0x44;

        // MonoImage at https://github.com/Unity-Technologies/mono/blob/unity-2018.4-mbe/mono/metadata/metadata-internals.h#L184
        public const uint ImageClassCache = 0x354; // 672

        public const uint HashTableSize = 0xc;

        public const uint HashTableTable = 0x14;

        public const uint TypeDefinitionBitFields = 0x14;

        public const uint TypeDefinitionByValArg = 0x74;

        public const uint TypeDefinitionFieldCount = 0xa4;

        public const uint TypeDefinitionFields = 0x60;

        public const uint TypeDefinitionName = 0x2c;

        public const uint TypeDefinitionNamespace = 0x30;

        public const uint TypeDefinitionNestedIn = 0x24;

        public const uint TypeDefinitionNextClassCache = 0xa8;

        public const uint TypeDefinitionParent = 0x20;

        public const uint TypeDefinitionRuntimeInfo = 0x84;

        public const uint TypeDefinitionSize = 0x5c;

        public const uint TypeDefinitionRuntimeInfoDomainVtables = 0x4;

        public const uint ReferencedAssemblies = 0x6c; // 0x70;
    }
}