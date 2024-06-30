using System;
using System.Collections.Generic;
using System.Linq;
using TlkLocalisationTool.Shared.Constants;
using TlkLocalisationTool.Shared.Entities;
using TlkLocalisationTool.Shared.Enums;
using TlkLocalisationTool.UI.Models;
using TlkLocalisationTool.UI.Resources;

namespace TlkLocalisationTool.UI.Utils;

internal static class GffDataParser
{
    private static readonly GffFieldType[] ComplexFieldTypes = [GffFieldType.ExoLocString, GffFieldType.Struct, GffFieldType.List, GffFieldType.Rotation, GffFieldType.Vector];

    public static GffEntityModel Parse(GffStruct topLevelStruct, Dictionary<int, string> tlkEntitiesDictionary) => ParseStruct(topLevelStruct, tlkEntitiesDictionary);

    private static GffEntityModel ParseStruct(GffStruct structure, Dictionary<int, string> tlkEntitiesDictionary)
    {
        var fieldCount = structure.Fields.Length;
        var entity = new GffEntityModel { DisplayText = GetStructDisplayText(structure), Entities = new GffEntityModel[fieldCount] };
        for (var i = 0; i < fieldCount; i++)
        {
            entity.Entities[i] = ParseField(structure.Fields[i], tlkEntitiesDictionary);
        }

        return entity;
    }

    private static GffEntityModel ParseField(GffField field, Dictionary<int, string> tlkEntitiesDictionary)
    {
        var entity = new GffEntityModel();
        if (ComplexFieldTypes.Contains(field.Type))
        {
            entity.DisplayText = GetComplexFieldDisplayText(field);
            entity.Entities = field.Type switch
            {
                GffFieldType.ExoLocString => ParseExoLocString((ExoLocString)field.Data, tlkEntitiesDictionary),
                GffFieldType.Struct => ParseStructFields((GffStruct)field.Data, tlkEntitiesDictionary),
                GffFieldType.List => ParseList((GffStruct[])field.Data, tlkEntitiesDictionary),
                GffFieldType.Rotation => ParseRotation((Rotation)field.Data),
                GffFieldType.Vector => ParseVector((Vector)field.Data),
                _ => throw new ArgumentException(string.Format(Strings.GffDataParser_FieldHadUnexpectedTypeMessage, field.Label, field.Type)),
            };
        }
        else
        {
            entity.DisplayText = IsStrRefField(field) ? GetStrRefDisplayText(field, tlkEntitiesDictionary) : GetSimpleFieldDisplayText(field);
        }

        return entity;
    }

    private static GffEntityModel[] ParseExoLocString(ExoLocString exoLocString, Dictionary<int, string> tlkEntitiesDictionary)
    {
        var subStringCount = exoLocString.SubStrings.Length;
        var entities = new GffEntityModel[subStringCount + 1];

        tlkEntitiesDictionary.TryGetValue(exoLocString.StrRef, out string tlkEntityValue);
        var strRefData = $"{exoLocString.StrRef} ({tlkEntityValue})";
        entities[0] = new GffEntityModel { DisplayText = GetSimpleFieldDisplayText(GffFieldType.Dword, "StringRef", strRefData) };
        for (var i = 0; i < subStringCount; i++)
        {
            entities[i + 1] = ParseExoLogSubString(exoLocString.SubStrings[i], i);
        }

        return entities;
    }

    private static GffEntityModel ParseExoLogSubString(ExoLocSubString exoLocSubString, int number)
    {
        var entity = new GffEntityModel
        {
            DisplayText = GetComplexFieldDisplayText(GffFieldType.ExoLocSubString, $"SubString {number}"),
            Entities =
            [
                new() { DisplayText = GetSimpleFieldDisplayText(GffFieldType.Int, "StringId", exoLocSubString.Id) },
                new() { DisplayText = GetSimpleFieldDisplayText(GffFieldType.ExoString, "Value", exoLocSubString.Value) },
            ]
        };

        return entity;
    }

    private static GffEntityModel[] ParseStructFields(GffStruct structure, Dictionary<int, string> tlkEntitiesDictionary)
    {
        var fieldCount = structure.Fields.Length;
        var entities = new GffEntityModel[fieldCount];
        for (var i = 0; i < fieldCount; i++)
        {
            entities[i] = ParseField(structure.Fields[i], tlkEntitiesDictionary);
        }

        return entities;
    }

    private static GffEntityModel[] ParseList(GffStruct[] structs, Dictionary<int, string> tlkEntitiesDictionary)
    {
        var entities = new GffEntityModel[structs.Length];
        for (var i = 0; i < structs.Length; i++)
        {
            entities[i] = ParseStruct(structs[i], tlkEntitiesDictionary);
        }

        return entities;
    }

    private static GffEntityModel[] ParseRotation(Rotation rotation)
    {
        var entities = new GffEntityModel[]
        {
            new() { DisplayText = GetSimpleFieldDisplayText(GffFieldType.Float, "A", rotation.A) },
            new() { DisplayText = GetSimpleFieldDisplayText(GffFieldType.Float, "B", rotation.B) },
            new() { DisplayText = GetSimpleFieldDisplayText(GffFieldType.Float, "C", rotation.C) },
            new() { DisplayText = GetSimpleFieldDisplayText(GffFieldType.Float, "D", rotation.D) },
        };

        return entities;
    }

    private static GffEntityModel[] ParseVector(Vector vector)
    {
        var entities = new GffEntityModel[]
        {
            new() { DisplayText = GetSimpleFieldDisplayText(GffFieldType.Float, "X", vector.X) },
            new() { DisplayText = GetSimpleFieldDisplayText(GffFieldType.Float, "Y", vector.Y) },
            new() { DisplayText = GetSimpleFieldDisplayText(GffFieldType.Float, "Z", vector.Z) },
        };

        return entities;
    }

    private static bool IsStrRefField(GffField field)
    {
        var isStrRefField = field.Type == GffFieldType.Dword && SharedFileConstants.GffStrRefFieldLabelParts.Any(x => field.Label.Contains(x, StringComparison.OrdinalIgnoreCase));
        return isStrRefField;
    }

    private static string GetStrRefDisplayText(GffField field, Dictionary<int, string> tlkEntitiesDictionary)
    {
        var strRef = (int)(uint)field.Data;
        tlkEntitiesDictionary.TryGetValue(strRef, out string tlkEntityValue);
        var strRefData = $"{strRef} ({tlkEntityValue})";
        var displayText = GetSimpleFieldDisplayText(GffFieldType.Dword, field.Label, strRefData);
        return displayText;
    }

    private static string GetStructDisplayText(GffStruct structure) => structure.Type == uint.MaxValue ? "Top level struct" : $"Struct {structure.Type}";

    private static string GetSimpleFieldDisplayText(GffField field) => GetSimpleFieldDisplayText(field.Type, field.Label, field.Data);

    private static string GetSimpleFieldDisplayText(GffFieldType type, string label, object data) => $"{type} {label}: {data}";

    private static string GetComplexFieldDisplayText(GffField field) => GetComplexFieldDisplayText(field.Type, field.Label);

    private static string GetComplexFieldDisplayText(GffFieldType type, string label) => $"{type} {label}";
}
