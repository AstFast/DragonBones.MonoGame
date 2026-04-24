using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Text.Json.Nodes;
using System;

namespace DragonBones.MonoGame
{
    public static class DragonBonesLoader
    {
        public static DragonBonesData LoadDragonBonesData(ContentManager content, string filePath, MonoGameFactory factory, string name = null, float scale = 1.0f)
        {
            string fillpath = Path.Combine(content.RootDirectory, filePath);
            if (!File.Exists(fillpath))
            {
                throw new FileNotFoundException($"DragonBones data file not found: {filePath}");
            }
            string extension = Path.GetExtension(filePath).ToLower();
            if (extension == ".json")
            {
                return LoadDragonBonesDataJson(fillpath, factory, name, scale);
            }
            else if (extension == ".dbbin")
            {
                return LoadDragonBonesDataBinary(fillpath, factory, name, scale);
            }
            else
            {
                throw new NotSupportedException($"Unsupported file extension for DragonBones data: {extension}. Supported extensions are .json and .dbbin");
            }
        }
        public static DragonBonesData LoadDragonBonesData(ContentManager content, string filePath, bool IsJsonFile, MonoGameFactory factory, string name = null, float scale = 1.0f)
        {
            string fillpath = Path.Combine(content.RootDirectory, filePath);
            if (!File.Exists(fillpath))
            {
                throw new FileNotFoundException($"DragonBones data file not found: {filePath}");
            }
            string extension = Path.GetExtension(filePath).ToLower();
            if (IsJsonFile)
            {
                return LoadDragonBonesDataJson(fillpath, factory, name, scale);
            }
            else
            {
                return LoadDragonBonesDataBinary(fillpath, factory, name, scale);
            }
        }
        private static DragonBonesData LoadDragonBonesDataJson(string fullPath, MonoGameFactory factory, string name, float scale)
        {
            string jsonContent = File.ReadAllText(fullPath);
            var jsonNode = JsonNode.Parse(jsonContent);
            var rawData = ConvertJsonNode(jsonNode);
            var result = factory.ParseDragonBonesData(rawData, name, scale);
            return result;
        }

        private static DragonBonesData LoadDragonBonesDataBinary(string fullPath, MonoGameFactory factory, string name, float scale)
        {
            byte[] binaryData = File.ReadAllBytes(fullPath);
            var result = factory.ParseDragonBonesData(binaryData, name, scale);
            return result;
        }

        public static TextureAtlasData LoadTextureAtlasData(ContentManager content, string jsonpath, Texture2D texture, MonoGameFactory factory, string name = null, float scale = 1.0f)
        {
            string jsonPath = Path.Combine(content.RootDirectory, jsonpath);
            if (File.Exists(jsonPath))
            {
                string jsonContent = File.ReadAllText(jsonPath);
                var jsonNode = JsonNode.Parse(jsonContent);
                var rawData = ConvertJsonNode(jsonNode) as Dictionary<string, object>;
                var result = factory.ParseTextureAtlasData(rawData, texture, name, scale);
                return result;
            }
            else
            {
                throw new FileNotFoundException($"Texture atlas data file not found. Tried paths: {jsonPath}");
            }
        }

        private static object ConvertJsonNode(JsonNode jsonNode)
        {
            if (jsonNode == null || jsonNode is JsonValue value && value.GetValue<object>() == null)
            {
                return null;
            }
            
            if (jsonNode is JsonObject obj)
            {
                var dictResult = new Dictionary<string, object>();
                foreach (var property in obj)
                {
                    dictResult[property.Key] = ConvertJsonNode(property.Value);
                }
                return dictResult;
            }
            else if (jsonNode is JsonArray array)
            {
                var listResult = new List<object>();
                foreach (var item in array)
                {
                    listResult.Add(ConvertJsonNode(item));
                }
                return listResult;
            }
            else if (jsonNode is JsonValue jsonValue)
            {
                if (jsonValue.TryGetValue<string>(out var stringValue))
                {
                    return stringValue;
                }
                else if (jsonValue.TryGetValue<int>(out var intValue))
                {
                    return intValue;
                }
                else if (jsonValue.TryGetValue<uint>(out var uintValue))
                {
                    return uintValue;
                }
                else if (jsonValue.TryGetValue<long>(out var longValue))
                {
                    if (longValue >= int.MinValue && longValue <= int.MaxValue)
                    {
                        return (int)longValue;
                    }
                    else if (longValue >= uint.MinValue && longValue <= uint.MaxValue)
                    {
                        return (uint)longValue;
                    }
                    return longValue;
                }
                else if (jsonValue.TryGetValue<double>(out var doubleValue))
                {
                    return doubleValue;
                }
                else if (jsonValue.TryGetValue<bool>(out var boolValue))
                {
                    return boolValue;
                }
                return null;
            }
            
            return null;
        }
    }
}