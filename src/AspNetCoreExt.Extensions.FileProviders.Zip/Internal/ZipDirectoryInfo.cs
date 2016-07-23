using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace AspNetCoreExt.Extensions.FileProviders.Zip.Internal
{
    internal class ZipDirectoryInfo : IFileInfo
    {
        internal ZipDirectoryInfo(string name, DateTimeOffset lastModified)
        {
            Name = name;
            LastModified = lastModified;
        }

        public bool Exists => true;

        public long Length => -1;

        public string PhysicalPath => null;

        public string Name { get; }

        public DateTimeOffset LastModified { get; }

        public bool IsDirectory => true;

        public Stream CreateReadStream()
        {
            throw new InvalidOperationException("Cannot create a stream for a directory.");
        }
    }
}
