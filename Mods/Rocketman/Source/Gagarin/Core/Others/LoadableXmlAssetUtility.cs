using System;
using Verse;

namespace Gagarin
{
    public static class LoadableXmlAssetUtility
    {
        public static string GetLoadableId(this LoadableXmlAsset loadable) => loadable.FullFilePath;
    }
}
