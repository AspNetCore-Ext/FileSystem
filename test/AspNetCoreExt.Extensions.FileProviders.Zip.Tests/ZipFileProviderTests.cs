using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace AspNetCoreExt.Extensions.FileProviders.Zip.Tests
{
    public class ZipFileProviderTests
    {
        [Fact]
        public void Constructor_IncorrectFilePath()
        {
            Assert.Throws<FileNotFoundException>(() => new ZipFileProvider("dsldksdlk"));
        }

        [Fact]
        public void Constructor_NullZipArchive()
        {
            Assert.Throws<ArgumentNullException>(() => new ZipFileProvider((ZipArchive)null));
        }

        [Fact]
        public void Watch_NullChangeToken()
        {
            var provider = new ZipFileProvider("test1.zip");

            var token = provider.Watch("/file1.txt");

            Assert.NotNull(token);
            Assert.False(token.ActiveChangeCallbacks);
            Assert.False(token.HasChanged);
        }

        [Theory]
        [InlineData("file1.txt", "file1.txt", 9)]
        [InlineData("/file1.txt", "file1.txt", 9)]
        [InlineData("/Dir2", "Dir2", -1)]
        [InlineData("/Dir2/", "Dir2", -1)]
        [InlineData("/Dir1/Dir1.1/File_empty.txt", "File_empty.txt", 0)]
        [InlineData("/Dir1/Dir1.1", "Dir1.1", -1)]
        public void GetFileInfo_Success(string subpath, string expectedName, int expectedLength)
        {
            var provider = new ZipFileProvider("test1.zip");

            var fileInfo = provider.GetFileInfo(subpath);

            Assert.NotNull(fileInfo);
            Assert.True(fileInfo.Exists);
            Assert.NotEqual(default(DateTimeOffset), fileInfo.LastModified);
            Assert.Equal(expectedLength, fileInfo.Length);
            Assert.Equal(expectedLength == -1, fileInfo.IsDirectory);
            Assert.Null(fileInfo.PhysicalPath);
            Assert.Equal(expectedName, fileInfo.Name);
            if (!fileInfo.IsDirectory)
            {
                using (var stream = fileInfo.CreateReadStream())
                {
                    Assert.NotNull(stream);
                }
            }
        }

        [Theory]
        [InlineData("/unknwon")]
        [InlineData("/")]
        [InlineData("")]
        [InlineData(null)]
        public void GetFileInfo_NotFound(string subpath)
        {
            var provider = new ZipFileProvider("test1.zip");

            var fileInfo = provider.GetFileInfo(subpath);

            Assert.NotNull(fileInfo);
            Assert.False(fileInfo.Exists);
        }

        [Fact]
        public void GetDirectoryContents_RootPath()
        {
            var provider = new ZipFileProvider("test1.zip");

            var entries = provider.GetDirectoryContents("/");

            var sorted = entries.OrderBy(f => f.Name, StringComparer.Ordinal);
            Assert.Equal(3, sorted.Count());
            Assert.Equal(new string[] { "Dir1", "Dir2", "file1.txt" }, sorted.Select(f => f.Name));
            Assert.Equal(new bool[] { true, true, false }, sorted.Select(f => f.IsDirectory));
        }

        [Fact]
        public void GetDirectoryContents_EmptyDirectory()
        {
            var provider = new ZipFileProvider("test1.zip");

            var entries = provider.GetDirectoryContents("/Dir2");

            Assert.Equal(0, entries.Count());
        }

        [Fact]
        public void GetDirectoryContents_SubDirectory()
        {
            var provider = new ZipFileProvider("test1.zip");

            var entries = provider.GetDirectoryContents("/Dir1/Dir1.1");

            Assert.Equal(1, entries.Count());
            Assert.Equal(new string[] { "File_empty.txt" }, entries.Select(f => f.Name));
            Assert.Equal(new bool[] { false }, entries.Select(f => f.IsDirectory));

            Assert.Equal(1, provider.GetDirectoryContents("/Dir1/Dir1.1/").Count());     // The a tailing slash
        }

        [Fact]
        public void GetDirectoryContents_UnknownDirectory()
        {
            var provider = new ZipFileProvider("test1.zip");

            var entries = provider.GetDirectoryContents("/Dir1/Unknown");

            Assert.False(entries.Exists);
        }

        [Fact]
        public void GetDirectoryContents_SubpathIsFile()
        {
            var provider = new ZipFileProvider("test1.zip");

            var entries = provider.GetDirectoryContents("/file1.txt");

            Assert.False(entries.Exists);
        }
    }
}
