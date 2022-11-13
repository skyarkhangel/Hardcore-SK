using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Gagarin.Core;
using HarmonyLib;
using RimWorld;
using RimWorld.IO;
using UnityEngine;
using Verse;

namespace Gagarin
{
    public class ModContentLoader_Texture2D_Patch
    {
        [GagarinPatch(typeof(ModContentLoader<Texture2D>), "LoadTexture", parameters: new Type[] { typeof(VirtualFile) })]
        public static class LoadTexture_Patch
        {
            [HarmonyPriority(int.MaxValue)]
            public static bool Prefix(VirtualFile file, ref Texture2D __result)
            {
                return Load(file, ref __result);
            }

            private static bool Load(VirtualFile file, ref Texture2D result)
            {
                if (!GagarinPrefs.TextureCachingEnabled)
                {
                    return true;
                }
                if (!file.Exists)
                {
                    return true;
                }
                if (TryLoadDDS(file, out result))
                {
                    return false;
                }
                if (!Directory.Exists(GagarinEnvironmentInfo.CacheFolderPath))
                {
                    Directory.CreateDirectory(GagarinEnvironmentInfo.CacheFolderPath);
                }
                if (!Directory.Exists(GagarinEnvironmentInfo.TexturesFolderPath))
                {
                    Directory.CreateDirectory(GagarinEnvironmentInfo.TexturesFolderPath);
                }
                byte[] originalHash = GetHash(file);
                string binPath = GetBinTexturePath(file);
                if (File.Exists(binPath))
                {
                    try
                    {
                        byte[] buffer = File.ReadAllBytes(binPath);
                        byte[] data = new byte[buffer.Length - (4 + 16 + 4 + 4 + 1)];
                        byte[] cachedHashed = new byte[16];
                        Array.Copy(buffer, 13 + 16, data, 0, data.Length);
                        Array.Copy(buffer, 0, cachedHashed, 0, 16);
                        if (HashChanged(originalHash, cachedHashed))
                        {
                            Log.Message($"GAGARIN: Hash changed for file {file.FullPath}");
                            try
                            {
                                if (File.Exists(binPath))
                                {
                                    File.Delete(binPath);
                                }
                                LoadCache(file, ref result, originalHash, binPath);
                                return false;
                            }
                            catch (Exception er)
                            {
                                RocketMan.Logger.Debug($"GAGARIN: Error creating texture cache! skipping this one!", exception: er);
                                return true;
                            }
                        }
                        // This is the header
                        // tex.width, tex.height, tex.format, tex.mipmapCount > 1);       
                        result = new Texture2D(
                            width: BitConverter.ToInt32(buffer, 0 + 16),
                            height: BitConverter.ToInt32(buffer, 4 + 16),
                            textureFormat: (TextureFormat)BitConverter.ToInt32(buffer, 8 + 16),
                            mipChain: BitConverter.ToBoolean(buffer, 12 + 16)
                        );
                        result.LoadRawTextureData(data);
                        result.name = Path.GetFileNameWithoutExtension(file.Name);
                        result.filterMode = (FilterMode)GagarinPrefs.FilterMode;
                        result.Apply();
                    }
                    catch (Exception er)
                    {
                        if (File.Exists(binPath))
                        {
                            File.Delete(binPath);
                        }
                        RocketMan.Logger.Debug($"GAGARIN: Error loading texture! using fallback mode!", exception: er);
                        return true;
                    }
                }
                else
                {
                    try
                    {
                        LoadCache(file, ref result, originalHash, binPath);
                    }
                    catch (Exception er)
                    {
                        if (File.Exists(binPath))
                        {
                            File.Delete(binPath);
                        }
                        RocketMan.Logger.Debug($"GAGARIN: Error creating texture cache! skipping this one!", exception: er);
                        return true;
                    }
                }
                return false;
            }

            private static void LoadCache(VirtualFile file, ref Texture2D result, byte[] hash, string cachePath)
            {
                result = LoadTexture(file);

                // This is the header
                // tex.width, tex.height, tex.format, tex.mipmapCount > 1);                    
                byte[] data = result.GetRawTextureData();
                byte[] buffer = new byte[data.Length + 4 + 4 + 4 + 1 + 16];
                BitConverter.GetBytes(result.width).CopyTo(buffer, 0 + 16);
                BitConverter.GetBytes(result.height).CopyTo(buffer, 4 + 16);
                BitConverter.GetBytes((int)result.format).CopyTo(buffer, 8 + 16);
                BitConverter.GetBytes(result.mipmapCount > 1).CopyTo(buffer, 12 + 16);
                Array.Copy(hash, 0, buffer, 0, 16);
                Array.Copy(data, 0, buffer, 13 + 16, data.Length);
                File.WriteAllBytes(cachePath, buffer);
            }

            private static byte[] GetHash(VirtualFile file)
            {
                byte[] hash;
                using (var stream = new BufferedStream(File.OpenRead(file.FullPath), 1200000))
                {
                    // The rest remains the same
                    MD5 md5 = MD5.Create();
                    hash = md5.ComputeHash(stream);
                }
                return hash;
            }

            private static bool HashChanged(byte[] hash1, byte[] hash2)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (hash1[i] != hash2[i])
                    {
                        return true;
                    }
                }
                return false;
            }

            private static Texture2D LoadTexture(VirtualFile file)
            {
                byte[] data;
                data = file.ReadAllBytes();
                Texture2D texture = new Texture2D(2, 2, TextureFormat.Alpha8, mipChain: true);

                texture.LoadImage(data);
                texture.Compress(highQuality: true);
                texture.name = Path.GetFileNameWithoutExtension(file.Name);
                texture.filterMode = (FilterMode)GagarinPrefs.FilterMode;
                texture.anisoLevel = 0;
                texture.mipMapBias = GagarinPrefs.MipMapBias < float.MinValue / 2f ? (-0.7f) : GagarinPrefs.MipMapBias;
                texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
                return texture;
            }

            private static bool TryLoadDDS(VirtualFile file, out Texture2D texture)
            {
                texture = null;
                string ddsPath = Path.ChangeExtension(file.FullPath, ".dds");
                if (!File.Exists(ddsPath))
                {
                    return false;
                }
                try
                {
                    texture = DDSLoader.Load(ddsPath);
                    texture.name = Path.GetFileNameWithoutExtension(file.FullPath);
                    texture.filterMode = (FilterMode)GagarinPrefs.FilterMode;
                    texture.anisoLevel = 0;
                    texture.mipMapBias = GagarinPrefs.MipMapBias < float.MinValue / 2f ? (-0.7f) : GagarinPrefs.MipMapBias;
                    texture.Apply(true, true);
                }
                catch (Exception er)
                {
                    RocketMan.Logger.Debug($"GAGARIN: Error loading dds! trying to load png now!", exception: er);
                    return false;
                }
                return true;
            }

            private const string textures = "Textures";

            private static readonly char[] invalids = Path.GetInvalidFileNameChars();

            private static string GetBinTexturePath(VirtualFile file)
            {
                string original = GenFile.SanitizedFileName("Texture_" + file.FullPath.Trim().Replace('-', '_')) + ".v2.bin";
                string bin = "";
                for (int i = 0; i < original.Length; i++)
                {
                    if (!invalids.Contains(original[i]))
                    {
                        bin += original[i];
                    }
                }
                return Path.Combine(GagarinEnvironmentInfo.TexturesFolderPath, bin);
            }
        }
    }
}
