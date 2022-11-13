using HarmonyLib;
using RimWorld.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Gagarin.Core
{
    public class DDSLoader
    {
        private const uint DDSD_MIPMAPCOUNT_BIT = 0x00020000;
        private const uint DDPF_ALPHAPIXELS = 0x00000001;
        private const uint DDPF_ALPHA = 0x00000002;
        private const uint DDPF_FOURCC = 0x00000004;
        private const uint DDPF_RGB = 0x00000040;
        private const uint DDPF_YUV = 0x00000200;
        private const uint DDPF_LUMINANCE = 0x00020000;
        private const uint DDPF_NORMAL = 0x80000000;

        public static string error;

        // DDS Texture loader inspired by
        // http://answers.unity3d.com/questions/555984/can-you-load-dds-textures-during-runtime.html#answer-707772
        // http://msdn.microsoft.com/en-us/library/bb943992.aspx
        // http://msdn.microsoft.com/en-us/library/windows/desktop/bb205578(v=vs.85).aspx
        // mipmapBias limits the number of mipmap when > 0
        public static Texture2D Load(string path, int mipmapBias = -1)
        {
            if (!File.Exists(path))
            {
                error = "File does not exist";
                return null;
            }
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read)))
            {
                byte[] dwMagic = reader.ReadBytes(4);

                if (!fourCCEquals(dwMagic, "DDS "))
                {
                    error = "Invalid DDS file";
                    return null;
                }

                int dwSize = (int)reader.ReadUInt32();

                //this header byte should be 124 for DDS image files
                if (dwSize != 124)
                {
                    error = "Invalid header size";
                    return null;
                }

                int dwFlags = (int)reader.ReadUInt32();
                int dwHeight = (int)reader.ReadUInt32();
                int dwWidth = (int)reader.ReadUInt32();

                int dwPitchOrLinearSize = (int)reader.ReadUInt32();
                int dwDepth = (int)reader.ReadUInt32();
                int dwMipMapCount = (int)reader.ReadUInt32();

                if ((dwFlags & DDSD_MIPMAPCOUNT_BIT) == 0)
                {
                    dwMipMapCount = 1;
                }

                // dwReserved1
                for (int i = 0; i < 11; i++)
                {
                    reader.ReadUInt32();
                }

                // DDS_PIXELFORMAT
                uint dds_pxlf_dwSize = reader.ReadUInt32();
                uint dds_pxlf_dwFlags = reader.ReadUInt32();
                byte[] dds_pxlf_dwFourCC = reader.ReadBytes(4);
                string fourCC = Encoding.ASCII.GetString(dds_pxlf_dwFourCC);
                uint dds_pxlf_dwRGBBitCount = reader.ReadUInt32();
                uint pixelSize = dds_pxlf_dwRGBBitCount / 8;
                uint dds_pxlf_dwRBitMask = reader.ReadUInt32();
                uint dds_pxlf_dwGBitMask = reader.ReadUInt32();
                uint dds_pxlf_dwBBitMask = reader.ReadUInt32();
                uint dds_pxlf_dwABitMask = reader.ReadUInt32();

                int dwCaps = (int)reader.ReadUInt32();
                int dwCaps2 = (int)reader.ReadUInt32();
                int dwCaps3 = (int)reader.ReadUInt32();
                int dwCaps4 = (int)reader.ReadUInt32();
                int dwReserved2 = (int)reader.ReadUInt32();

                TextureFormat textureFormat = TextureFormat.ARGB32;
                bool isCompressed = false;
                bool isNormalMap = (dds_pxlf_dwFlags & DDPF_NORMAL) != 0;

                bool alpha = (dds_pxlf_dwFlags & DDPF_ALPHA) != 0;
                bool fourcc = (dds_pxlf_dwFlags & DDPF_FOURCC) != 0;
                bool rgb = (dds_pxlf_dwFlags & DDPF_RGB) != 0;
                bool alphapixel = (dds_pxlf_dwFlags & DDPF_ALPHAPIXELS) != 0;
                bool luminance = (dds_pxlf_dwFlags & DDPF_LUMINANCE) != 0;
                bool rgb888 = dds_pxlf_dwRBitMask == 0x000000ff && dds_pxlf_dwGBitMask == 0x0000ff00 && dds_pxlf_dwBBitMask == 0x00ff0000;
                bool bgr888 = dds_pxlf_dwRBitMask == 0x00ff0000 && dds_pxlf_dwGBitMask == 0x0000ff00 && dds_pxlf_dwBBitMask == 0x000000ff;
                bool rgb565 = dds_pxlf_dwRBitMask == 0x0000F800 && dds_pxlf_dwGBitMask == 0x000007E0 && dds_pxlf_dwBBitMask == 0x0000001F;
                bool argb4444 = dds_pxlf_dwABitMask == 0x0000f000 && dds_pxlf_dwRBitMask == 0x00000f00 && dds_pxlf_dwGBitMask == 0x000000f0 && dds_pxlf_dwBBitMask == 0x0000000f;
                bool rbga4444 = dds_pxlf_dwABitMask == 0x0000000f && dds_pxlf_dwRBitMask == 0x0000f000 && dds_pxlf_dwGBitMask == 0x000000f0 && dds_pxlf_dwBBitMask == 0x00000f00;
                if (fourcc)
                {
                    // Texture dos not contain RGB data, check FourCC for format
                    isCompressed = true;

                    if (fourCCEquals(dds_pxlf_dwFourCC, "DXT1"))
                    {
                        textureFormat = TextureFormat.DXT1;
                    }
                    else if (fourCCEquals(dds_pxlf_dwFourCC, "DXT5"))
                    {
                        textureFormat = TextureFormat.DXT5;
                    }
                    else if (fourCCEquals(dds_pxlf_dwFourCC, "DX10"))
                    {
                        textureFormat = TextureFormat.BC7;
                    }
                }
                else if (rgb && (rgb888 || bgr888))
                {
                    // RGB or RGBA format
                    textureFormat = alphapixel
                        ? TextureFormat.RGBA32
                        : TextureFormat.RGB24;
                }
                else if (rgb && rgb565)
                {
                    // Nvidia texconv B5G6R5_UNORM
                    textureFormat = TextureFormat.RGB565;
                }
                else if (rgb && alphapixel && argb4444)
                {
                    // Nvidia texconv B4G4R4A4_UNORM
                    textureFormat = TextureFormat.ARGB4444;
                }
                else if (rgb && alphapixel && rbga4444)
                {
                    textureFormat = TextureFormat.RGBA4444;
                }
                else if (!rgb && alpha != luminance)
                {
                    // A8 format or Luminance 8
                    textureFormat = TextureFormat.Alpha8;
                }
                else
                {
                    error = "Only BC7, DXT1, DXT5, A8, RGB24, BGR24, RGBA32, BGBR32, RGB565, ARGB4444 and RGBA4444 are supported";
                    return null;
                }

                long dataBias;
                if (textureFormat != TextureFormat.BC7)
                {
                    dataBias = 128;
                }
                else
                {
                    dataBias = 148;
                }

                /*
                if (Settings.MipmapBias != 0 || Settings.NormalMipmapBias != 0)
                {
                    int bias = isNormalMap ? Settings.NormalMipmapBias : Settings.MipmapBias;
                    if (mipmapBias > 0)
                        bias = mipmapBias;
                    int blockSize = textureFormat == TextureFormat.DXT1 ? 8 : 16;
                    int levels = Math.Min(bias, dwMipMapCount - 1);
                    for (int i = 0; i < levels; ++i)
                    {
                        dataBias += isCompressed
                            ? ((dwWidth + 3) / 4) * ((dwHeight + 3) / 4) * blockSize
                            : dwWidth * dwHeight * pixelSize;
                        dwWidth = Math.Max(1, dwWidth / 2);
                        dwHeight = Math.Max(1, dwHeight / 2);
                    }
                }*/

                long dxtBytesLength = reader.BaseStream.Length - dataBias;
                reader.BaseStream.Seek(dataBias, SeekOrigin.Begin);
                byte[] dxtBytes = reader.ReadBytes((int)dxtBytesLength);

                // Swap red and blue.
                if (!isCompressed && bgr888)
                {
                    for (uint i = 0; i < dxtBytes.Length; i += pixelSize)
                    {
                        byte b = dxtBytes[i + 0];
                        byte r = dxtBytes[i + 2];

                        dxtBytes[i + 0] = r;
                        dxtBytes[i + 2] = b;
                    }
                }

                //QualitySettings.masterTextureLimit = 0;
                // Work around for an >Unity< Bug.
                // if QualitySettings.masterTextureLimit != 0 (half or quarter texture rez)
                // and dwWidth and dwHeight divided by 2 (or 4 for quarter rez) are not a multiple of 4 
                // and we are creating a DXT5 or DXT1 texture
                // Then you get an Unity error on the "new Texture"

                int quality = QualitySettings.masterTextureLimit;

                // If the bug conditions are present then switch to full quality
                if (isCompressed && quality > 0 && (dwWidth >> quality) % 4 != 0 && (dwHeight >> quality) % 4 != 0)
                    QualitySettings.masterTextureLimit = 0;

                Texture2D texture = new Texture2D(dwWidth, dwHeight, textureFormat, dwMipMapCount > 1);
                texture.LoadRawTextureData(dxtBytes);
                return texture;
            }
        }

        private static bool fourCCEquals(IList<byte> bytes, string s)
        {
            return bytes[0] == s[0] && bytes[1] == s[1] && bytes[2] == s[2] && bytes[3] == s[3];
        }
    }
}
