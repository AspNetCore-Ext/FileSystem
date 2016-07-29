using AspNetCoreExt.Extensions.FileProviders.Zip.Internal;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace AspNetCoreExt.Extensions.FileProviders.Zip
{
    /// <summary>
    /// Looks up files in the specified ZIP file.
    /// This file provider is case sensitive.
    /// </summary>
    public class ZipFileProvider : IFileProvider
    {
        private readonly ZipArchive _archive;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipFileProvider" /> class using the specified
        /// ZIP file path.
        /// </summary>
        /// <param name="zipFilePath">The file path</param>
        public ZipFileProvider(string zipFilePath)
            : this(ZipFile.OpenRead(zipFilePath))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipFileProvider" /> class using the specified
        /// ZIP archive.
        /// </summary>
        /// <param name="zipArchive">The archive</param>
        public ZipFileProvider(ZipArchive zipArchive)
        {
            if (zipArchive == null)
            {
                throw new ArgumentNullException(nameof(zipArchive));
            }

            _archive = zipArchive;
        }

        /// <inheritdoc />
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return new NotFoundDirectoryContents();
            }
            subpath = PrepareSubpath(subpath);

            if (!string.IsNullOrEmpty(subpath))
            {
                var subpath2 = subpath;
                if (subpath2[subpath2.Length - 1] != '/')
                {
                    subpath2 += '/';
                }

                var subpathEntry = _archive.GetEntry(subpath2);
                if (subpathEntry == null)
                {
                    return new NotFoundDirectoryContents();
                }
            }

            var infos = new List<IFileInfo>();

            foreach (var entry in _archive.Entries)
            {
                var fileInfo = PrepareFileInfo(entry, subpath);
                if (fileInfo != null)
                {
                    infos.Add(fileInfo);
                }
            }

            return new EnumerableDirectoryContents(infos);
        }

        /// <inheritdoc />
        public IFileInfo GetFileInfo(string subpath)
        {
            if (string.IsNullOrEmpty(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }
            subpath = PrepareSubpath(subpath);

            var entry = _archive.GetEntry(subpath);
            if (entry == null)      // File not found, search a directory
            {
                entry = _archive.GetEntry(subpath + '/');
                if (entry == null)
                {
                    return new NotFoundFileInfo(subpath);
                }
            }

            return PrepareFileInfo(entry, null);
        }

        /// <inheritdoc />
        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }

        private static string PrepareSubpath(string subpath)
        {
            var builder = new StringBuilder(subpath.Length);

            // Relative paths starting with a leading slash okay
            var start = 0;
            var count = subpath.Length;

            if (subpath.StartsWith("/", StringComparison.Ordinal))
            {
                start++;
                count--;
            }

            if (subpath.EndsWith("/", StringComparison.Ordinal))
            {
                count--;
            }

            if (count > 0)
            {
                builder.Append(subpath, start, count);

                for (var i = 0; i < builder.Length; i++)
                {
                    if (builder[i] == '\\')
                    {
                        builder[i] = '/';
                    }
                }
            }

            return builder.ToString();
        }

        private static IFileInfo PrepareFileInfo(ZipArchiveEntry entry, string subpathFilter)
        {
            if (!string.IsNullOrEmpty(entry.Name))
            {
                var parent = GetParentPath(entry.FullName);
                if (subpathFilter == null || parent == subpathFilter)
                {
                    return new ZipFileInfo(entry);
                }
            }
            else
            {
                var cleanPath = entry.FullName.Substring(0, entry.FullName.Length - 1);
                var parent = GetParentPath(cleanPath);
                if (subpathFilter == null || parent == subpathFilter)
                {
                    return new ZipDirectoryInfo(Path.GetFileName(cleanPath), entry.LastWriteTime);
                }
            }

            return null;
        }

        private static string GetParentPath(string path)
        {
            var i = path.LastIndexOf('/');
            if (i >= 0)
            {
                return path.Substring(0, i);
            }
            return string.Empty;
        }
    }
}
