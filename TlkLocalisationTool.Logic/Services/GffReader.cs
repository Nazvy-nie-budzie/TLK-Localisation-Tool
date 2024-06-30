using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TlkLocalisationTool.Logic.Constants;
using TlkLocalisationTool.Logic.Entities;
using TlkLocalisationTool.Logic.Extensions;
using TlkLocalisationTool.Logic.Services.Interfaces;
using TlkLocalisationTool.Shared.Constants;
using TlkLocalisationTool.Shared.Entities;
using TlkLocalisationTool.Shared.Enums;
using TlkLocalisationTool.Shared.Settings;
using Void = TlkLocalisationTool.Shared.Entities.Void;

namespace TlkLocalisationTool.Logic.Services;

internal class GffReader : IGffReader
{
    private readonly AppSettings _appSettings;

    public GffReader(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public async Task<GffData> ReadData(string filePath) => await Task.Run(() => ReadDataInternal(filePath));

    public async Task<int[]> ReadStrRefs(string filePath) => await Task.Run(() => ReadStrRefsInternal(filePath));

    private GffData ReadDataInternal(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));
        var header = ReadHeader(reader, filePath);
        var structs = new GffStruct[header.StructCount];
        for (var i = 0; i < header.StructCount; i++)
        {
            structs[i] = ReadStruct(reader, header.FieldIndicesBlockOffset);
        }

        var fields = new GffField[header.FieldCount];
        for (var i = 0; i < header.FieldCount; i++)
        {
            fields[i] = ReadField(reader, header, structs);
        }

        foreach (var structure in structs)
        {
            FillStructFields(structure, fields);
        }

        var data = new GffData { TopLevelStruct = structs[0] };
        return data;
    }

    private int[] ReadStrRefsInternal(string filePath)
    {
        using var reader = new BinaryReader(File.OpenRead(filePath));
        var header = ReadHeader(reader, filePath);

        reader.BaseStream.Position = header.FieldBlockOffset;

        var strRefs = new List<int>();
        for (var i = 0; i < header.FieldCount; i++)
        {
            var strRef = TryReadStrRef(reader, header);
            if (strRef != null && strRef != SharedFileConstants.InvalidStrRef && !strRefs.Contains(strRef.Value))
            {
                strRefs.Add(strRef.Value);
            }
        }

        return strRefs.ToArray();
    }

    private GffHeader ReadHeader(BinaryReader reader, string filePath)
    {
        var fileType = reader.ReadString(FileConstants.FileTypeSize, _appSettings.EncodingName);
        if (!GffFileConstants.FileTypes.Contains(fileType))
        {
            throw new ArgumentException($"File {filePath} is not a GFF file");
        }

        var header = new GffHeader
        {
            FileType = fileType,
            FileVersion = reader.ReadString(FileConstants.FileVersionSize, _appSettings.EncodingName),
            StructBlockOffset = reader.ReadInt32(),
            StructCount = reader.ReadInt32(),
            FieldBlockOffset = reader.ReadInt32(),
            FieldCount = reader.ReadInt32(),
            LabelBlockOffset = reader.ReadInt32(),
            LabelCount = reader.ReadInt32(),
            FieldDataBlockOffset = reader.ReadInt32(),
            FieldDataCount = reader.ReadInt32(),
            FieldIndicesBlockOffset = reader.ReadInt32(),
            FieldIndicesCount = reader.ReadInt32(),
            ListIndicesBlockOffset = reader.ReadInt32(),
            ListIndicesCount = reader.ReadInt32(),
        };

        return header;
    }

    private static GffStruct ReadStruct(BinaryReader reader, int fieldIndiciesBlockOffset)
    {
        var structure = new GffStruct { Type = reader.ReadUInt32() };
        var dataOffset = reader.ReadInt32();
        var fieldCount = reader.ReadInt32();
        structure.Fields = new GffField[fieldCount];
        if (fieldCount > 1)
        {
            var initialStreamPosition = reader.BaseStream.Position;
            reader.BaseStream.Position = fieldIndiciesBlockOffset + dataOffset;
            structure.FieldIndicies = ReadFieldIndicies(reader, fieldCount);
            reader.BaseStream.Position = initialStreamPosition;
        }
        else
        {
            structure.FieldIndicies = [dataOffset];
        }

        return structure;
    }

    private static int[] ReadFieldIndicies(BinaryReader reader, int count)
    {
        var fieldIndicies = new int[count];
        for (var i = 0; i < count; i++)
        {
            fieldIndicies[i] = reader.ReadInt32();
        }

        return fieldIndicies;
    }

    private GffField ReadField(BinaryReader reader, GffHeader header, GffStruct[] structs)
    {
        var field = ReadFieldWithoutData(reader, header.LabelBlockOffset);
        field.Data = ReadFieldData(reader, field.Type, header, structs);
        return field;
    }

    private int? TryReadStrRef(BinaryReader reader, GffHeader header)
    {
        var field = ReadFieldWithoutData(reader, header.LabelBlockOffset);
        if (!IsFieldWithStrRef(field))
        {
            reader.BaseStream.Position += GffFileConstants.FieldDataSize;
            return null;
        }

        var strRef = ReadStrRef(reader, field.Type, header.FieldDataBlockOffset);
        return strRef;
    }

    private GffField ReadFieldWithoutData(BinaryReader reader, int labelBlockOffset)
    {
        var field = new GffField { Type = (GffFieldType)reader.ReadInt32() };

        var labelIndex = reader.ReadInt32();
        var initialStreamPosition = reader.BaseStream.Position;
        reader.BaseStream.Position = labelBlockOffset + labelIndex * GffFileConstants.LabelSize;
        field.Label = reader.ReadString(GffFileConstants.LabelSize, _appSettings.EncodingName);
        reader.BaseStream.Position = initialStreamPosition;

        return field;
    }

    private object ReadFieldData(BinaryReader reader, GffFieldType fieldType, GffHeader header, GffStruct[] structs)
    {
        var dataOrDataOffset = reader.ReadUInt32();

        var initialStreamPosition = reader.BaseStream.Position;
        reader.BaseStream.Position = header.FieldDataBlockOffset + dataOrDataOffset;

        object fieldData = fieldType switch
        {
            GffFieldType.Byte => (byte)dataOrDataOffset,
            GffFieldType.Char => (char)dataOrDataOffset,
            GffFieldType.Word => (ushort)dataOrDataOffset,
            GffFieldType.Short => (short)dataOrDataOffset,
            GffFieldType.Dword => dataOrDataOffset,
            GffFieldType.Int => (int)dataOrDataOffset,
            GffFieldType.Dword64 => reader.ReadUInt64(),
            GffFieldType.Int64 => reader.ReadInt64(),
            GffFieldType.Float => BitConverter.ToSingle(BitConverter.GetBytes(dataOrDataOffset)),
            GffFieldType.Double => reader.ReadDouble(),
            GffFieldType.ExoString => ReadExoString(reader),
            GffFieldType.ResRef => ReadResRef(reader),
            GffFieldType.ExoLocString => ReadExoLocString(reader),
            GffFieldType.Void => ReadVoid(reader),
            GffFieldType.Struct => structs[(int)dataOrDataOffset],
            GffFieldType.List => ReadList(reader, structs, header.ListIndicesBlockOffset + (int)dataOrDataOffset),
            GffFieldType.Rotation => ReadRotation(reader),
            GffFieldType.Vector => ReadVector(reader),
            _ => throw new ArgumentException($"Field had unknown type {fieldType}"),
        };

        reader.BaseStream.Position = initialStreamPosition;

        return fieldData;
    }

    private string ReadExoString(BinaryReader reader)
    {
        var size = reader.ReadInt32();
        return reader.ReadString(size, _appSettings.EncodingName);
    }

    private string ReadResRef(BinaryReader reader)
    {
        var size = reader.ReadByte();
        return reader.ReadString(size, _appSettings.EncodingName);
    }

    private ExoLocString ReadExoLocString(BinaryReader reader)
    {
        reader.BaseStream.Position += GffFileConstants.ExoLocStringTotalSizeSize;

        var exoLocString = new ExoLocString { StrRef = reader.ReadInt32() };
        var subStringCount = reader.ReadInt32();
        exoLocString.SubStrings = new ExoLocSubString[subStringCount];
        for (var i = 0; i < subStringCount; i++)
        {
            exoLocString.SubStrings[i] = ReadExoLocSubString(reader);
        }

        return exoLocString;
    }

    private ExoLocSubString ReadExoLocSubString(BinaryReader reader)
    {
        var exoLocSubString = new ExoLocSubString { Id = reader.ReadInt32() };
        var size = reader.ReadInt32();
        exoLocSubString.Value = reader.ReadString(size, _appSettings.EncodingName);
        return exoLocSubString;
    }

    private static Void ReadVoid(BinaryReader reader)
    {
        var bytesCount = reader.ReadInt32();
        var voidEntity = new Void { Data = reader.ReadBytes(bytesCount), };
        return voidEntity;
    }

    private static GffStruct[] ReadList(BinaryReader reader, GffStruct[] structs, int listIndiciesOffset)
    {
        reader.BaseStream.Position = listIndiciesOffset;

        var fieldStructCount = reader.ReadInt32();
        var fieldStructs = new GffStruct[fieldStructCount];
        for (var i = 0; i < fieldStructCount; i++)
        {
            var structIndex = reader.ReadInt32();
            fieldStructs[i] = structs[structIndex];
        }

        return fieldStructs;
    }

    public static Rotation ReadRotation(BinaryReader reader)
    {
        var rotation = new Rotation
        {
            A = reader.ReadSingle(),
            B = reader.ReadSingle(),
            C = reader.ReadSingle(),
            D = reader.ReadSingle(),
        };

        return rotation;
    }

    public static Vector ReadVector(BinaryReader reader)
    {
        var vector = new Vector
        {
            X = reader.ReadSingle(),
            Y = reader.ReadSingle(),
            Z = reader.ReadSingle(),
        };

        return vector;
    }

    private static int ReadStrRef(BinaryReader reader, GffFieldType fieldType, int fieldDataBlockOffset)
    {
        if (fieldType == GffFieldType.ExoLocString)
        {
            var strRefOffset = reader.ReadInt32() + GffFileConstants.ExoLocStringTotalSizeSize;
            var initialStreamPosition = reader.BaseStream.Position;
            reader.BaseStream.Position = fieldDataBlockOffset + strRefOffset;
            var strRef = reader.ReadInt32();
            reader.BaseStream.Position = initialStreamPosition;

            return strRef;
        }
        else
        {
            return reader.ReadInt32();
        }
    }

    private static void FillStructFields(GffStruct structure, GffField[] fields)
    {
        var fieldCount = structure.Fields.Length;
        for (var i = 0; i < fieldCount; i++)
        {
            structure.Fields[i] = fields[structure.FieldIndicies[i]];
        }
    }

    private static bool IsFieldWithStrRef(GffField field) => field.Type == GffFieldType.ExoLocString || IsFieldWithStrRefLabel(field);

    private static bool IsFieldWithStrRefLabel(GffField field) => SharedFileConstants.GffStrRefFieldLabelParts.Any(x => field.Label.Contains(x, StringComparison.OrdinalIgnoreCase));
}
