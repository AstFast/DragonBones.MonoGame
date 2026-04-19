using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using DragonBones;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DragonBones.MonoGame
{
    public static class DragonBonesLoader
    {
        public static DragonBonesData LoadDragonBonesData(ContentManager content, string path, MonoGameFactory factory, string name = null, float scale = 1.0f)
        {
            string jsonPath = Path.ChangeExtension(path, ".json");
            string dbbinPath = Path.ChangeExtension(path, ".dbbin");
            List<string> possiblePaths = new List<string>
            {
                Path.Combine(content.RootDirectory, jsonPath),
                Path.Combine(content.RootDirectory, dbbinPath),
                Path.Combine(Directory.GetCurrentDirectory(), "Content", jsonPath),
                Path.Combine(Directory.GetCurrentDirectory(), "Content", dbbinPath),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "Content", jsonPath),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "Content", dbbinPath),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Content", jsonPath),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Content", dbbinPath)
            };
            string foundJsonPath = null;
            foreach (var possiblePath in possiblePaths)
            {
                if (possiblePath.EndsWith(".json") && File.Exists(possiblePath))
                {
                    foundJsonPath = possiblePath;
                    break;
                }
            }
            string foundBinaryPath = null;
            foreach (var possiblePath in possiblePaths)
            {
                if (possiblePath.EndsWith(".dbbin") && File.Exists(possiblePath))
                {
                    foundBinaryPath = possiblePath;
                    break;
                }
            }

            if (foundBinaryPath != null)
            {
                return LoadDragonBonesDataBinary(foundBinaryPath, factory, name, scale);
            }
            else if (foundJsonPath != null)
            {
                return LoadDragonBonesDataJson(foundJsonPath, factory, name, scale);
            }
            else
            {
                throw new FileNotFoundException($"DragonBones data file not found. Tried paths: {string.Join(", ", possiblePaths)}");
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
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"DragonBones binary data file not found: {fullPath}");
            }
            
            byte[] binaryData = File.ReadAllBytes(fullPath);
            
            var result = factory.ParseDragonBonesData(binaryData, name, scale);
            
            return result;
        }

        public static TextureAtlasData LoadTextureAtlasData(ContentManager content, string path, Texture2D texture, MonoGameFactory factory, string name = null, float scale = 1.0f)
        {
            string jsonPath = Path.ChangeExtension(path, ".json");
            List<string> possiblePaths = new List<string>
            {
                Path.Combine(content.RootDirectory, jsonPath),
                Path.Combine(Directory.GetCurrentDirectory(), "Content", jsonPath),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "Content", jsonPath),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Content", jsonPath)
            };

            // 检查是否存在JSON文件
            string foundJsonPath = null;
            foreach (var possiblePath in possiblePaths)
            {
                if (File.Exists(possiblePath))
                {
                    foundJsonPath = possiblePath;
                    break;
                }
            }

            if (foundJsonPath != null)
            {
                string jsonContent = File.ReadAllText(foundJsonPath);
                
                var jsonNode = JsonNode.Parse(jsonContent);
                var rawData = ConvertJsonNode(jsonNode) as Dictionary<string, object>;
                
                var result = factory.ParseTextureAtlasData(rawData, texture, name, scale);
                
                return result;
            }
            else
            {
                throw new FileNotFoundException($"Texture atlas data file not found. Tried paths: {string.Join(", ", possiblePaths)}");
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