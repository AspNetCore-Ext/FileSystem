using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.IO.Compression;

namespace AspNetCoreExt.Extensions.FileProviders.Zip.Internal
{
    internal class ZipFileInfo : IFileInfo
    {
        private readonly ZipArchiveEntry _zipEntry;

        internal ZipFileInfo(ZipArchiveEntry zipEntry)
        {
            _zipEntry = zipEntry;
        }

        public bool Exists => true;

        public long Length => _zipEntry.Length;

        public string PhysicalPath => null;

        public string Name => _zipEntry.Name;

        public DateTimeOffset LastModified => _zipEntry.LastWriteTime;

        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            return _zipEntry.Open();
        }
    }
}
