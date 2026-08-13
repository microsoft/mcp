// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using McpToolEvaluator.Core.Models;

namespace McpToolEvaluator.Core;

/// <summary>
/// Contains information about the MCP server, including its name and the tool areas it supports.
/// </summary>
public class McpServerInformation
{
    public McpServerInformation(string artifactDirectory)
    {
        ProbeSetupNames(artifactDirectory);
    }

    public Dictionary<string, ToolArea> AssemblyNameToToolsMap { get; } = new Dictionary<string, ToolArea>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Scans every managed assembly beside the server's binary folder for "*Setup" classes
    /// and reads the string literal from their "Name" getter — without loading or
    /// instantiating anything.
    ///
    /// Reads raw metadata on purpose: loading would resolve the whole dependency graph and
    /// throw if something like Microsoft.Extensions.DependencyInjection isn't present.
    /// Parsing bytes off disk only ever touches the one file we open. Because nothing is
    /// instantiated, the only Name we can read is a compile-time literal, i.e. an
    /// expression-bodied `public string Name => "grafana";`.
    /// </summary>
    internal void ProbeSetupNames(string targetPath)
    {
        targetPath = Path.GetFullPath(targetPath);

        Console.WriteLine($"Probing {targetPath} for classes...");

        // Scan the folder directly rather than reading the target's AssemblyReferences.
        // A folder scan can't miss a Setup assembly that the target references but never
        // uses a type from (the compiler can trim such references out of the metadata).
        foreach (string dll in Directory.EnumerateFiles(targetPath, "*.dll"))
        {
            try
            {
                foreach (var toolArea in ProbeAssembly(dll))
                {
                    AssemblyNameToToolsMap[toolArea.AssemblyName] = toolArea;
                }
            }
            catch (BadImageFormatException)
            {
                // Native DLL (runtime shim, *.native.dll, etc.) sitting alongside the
                // managed assemblies. Not a .NET assembly, so skip it.
            }
        }
    }

    /// <summary>
    /// Opens a single assembly's metadata and appends a ToolArea for every "*Setup" type
    /// that has a Name property returning a string literal.
    /// </summary>
    internal static List<ToolArea> ProbeAssembly(string path)
    {
        var results = new List<ToolArea>();
        using var fs = File.OpenRead(path);
        using var pe = new PEReader(fs);

        // A PE file without a metadata stream is a native image — nothing to read.
        if (!pe.HasMetadata)
        {
            return results;
        }

        var reader = pe.GetMetadataReader();
        var assemblyName = reader.GetString(reader.GetAssemblyDefinition().Name);

        foreach (var typeDefinitionHandle in reader.TypeDefinitions)
        {
            var typeDefinition = reader.GetTypeDefinition(typeDefinitionHandle);

            if (!reader.GetString(typeDefinition.Name).EndsWith("Setup", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var propertyDefinitionHandle in typeDefinition.GetProperties())
            {
                var propertyDefinition = reader.GetPropertyDefinition(propertyDefinitionHandle);
                if (reader.GetString(propertyDefinition.Name) != "Name")
                {
                    continue;
                }

                MethodDefinitionHandle getter = propertyDefinition.GetAccessors().Getter;
                if (getter.IsNil)
                {
                    continue; // write-only / no getter
                }

                var value = ReadLdstrLiteral(pe, reader, reader.GetMethodDefinition(getter));
                if (value is not null)
                {
                    results.Add(new ToolArea { AssemblyName = assemblyName, ToolNamespace = value });
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Reads the string operand of the first <c>ldstr</c> instruction in a method body.
    ///
    /// This is a deliberate SHORTCUT, not a general IL decoder. It is correct only for a
    /// getter shaped like <c>=> "literal"</c>, whose entire body is two instructions:
    ///
    ///     72 xx xx xx xx    ldstr  &lt;token for the string&gt;
    ///     2A                ret
    ///
    /// Everything here is defined by ECMA-335 (the CLI standard):
    ///   - Partition III  — opcode values and their operand encodings
    ///   - Partition II §24 — the physical metadata format, incl. the user-string (#US) heap
    ///
    /// WHY A BYTE SCAN IS SAFE HERE, BUT NOT IN GENERAL:
    ///   IL is a flat byte stream with no separators between instructions — you only know
    ///   where the next instruction begins by knowing how long the current one is. A proper
    ///   decoder walks instruction-by-instruction from the start, using each opcode's known
    ///   operand length to step over operand bytes, so it never confuses operand data with
    ///   an opcode. We skip all that because our target body starts WITH the ldstr opcode —
    ///   there is no earlier instruction whose operand bytes could coincidentally contain
    ///   0x72 and fool the scan. For arbitrary getters this assumption breaks; you'd need a
    ///   real opcode walker (or a library like Cecil that ships an operand-length table).
    /// </summary>
    private static string? ReadLdstrLiteral(PEReader pe, MetadataReader mr, MethodDefinition md)
    {
        // RVA 0 means the method has no IL body (abstract, extern, etc.).
        if (md.RelativeVirtualAddress == 0)
            return null;

        MethodBodyBlock body = pe.GetMethodBody(md.RelativeVirtualAddress);
        byte[] il = body.GetILBytes()!;

        // ldstr — "load string literal" — is opcode 0x72, followed by a 4-byte operand.
        const byte Ldstr = 0x72;

        // The `i + 4 < il.Length` bound guarantees the 4 operand bytes we're about to read
        // are actually inside the array (protects ToInt32 if a stray 0x72 sits near the end).
        for (int i = 0; i + 4 < il.Length; i++)
        {
            if (il[i] != Ldstr)
                continue;

            // The 4 operand bytes are a metadata token, stored little-endian (per the spec,
            // matching x86/x64). ToInt32 reads them in that order.
            int token = BitConverter.ToInt32(il, i + 1);

            // A metadata token packs a table id in the top byte and a row/offset in the low
            // 3 bytes. An ldstr token always points into the user-string (#US) heap, whose
            // table byte is 0x70 — so a real token looks like 0x70000001, 0x7000000A, etc.
            // Mask off the 0x70 tag to get the raw offset into the heap...
            var handle = MetadataTokens.UserStringHandle(token & 0x00FFFFFF);

            // ...then dereference that offset to pull the actual UTF-16 string the compiler
            // stored there. The token was the address; this is the lookup.
            return mr.GetUserString(handle);
        }

        return null;
    }
}
