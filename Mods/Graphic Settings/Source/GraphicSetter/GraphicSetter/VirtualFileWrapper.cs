using System.IO;
using RimWorld.IO;

namespace GraphicSetter
{
    public class VirtualFileWrapper : VirtualFile
    {
        private FileInfo fileInfo;

        public VirtualFileWrapper(FileInfo info)
        {
            this.fileInfo = info;
        }

        public override Stream CreateReadStream()
        {
            return this.fileInfo.OpenRead();
        }

        public override string ReadAllText()
        {
            return File.ReadAllText(this.fileInfo.FullName);
        }

        public override string[] ReadAllLines()
        {
            return File.ReadAllLines(this.fileInfo.FullName);
        }

        public override byte[] ReadAllBytes()
        {
            return File.ReadAllBytes(this.fileInfo.FullName);
        }

        public override string ToString()
        {
            return string.Format("FilesystemFile [{0}], Length {1}", this.FullPath, this.fileInfo.Length.ToString());
        }


        public override string Name => this.fileInfo.Name;

        public override string FullPath => this.fileInfo.FullName;

        public override bool Exists => this.fileInfo.Exists;

        public override long Length => this.fileInfo.Length;
    }
}
