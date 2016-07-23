using Microsoft.Extensions.FileProviders;
using System.Collections;
using System.Collections.Generic;

namespace AspNetCoreExt.Extensions.FileProviders.Zip.Internal
{
    internal class EnumerableDirectoryContents : IDirectoryContents
    {
        private readonly IEnumerable<IFileInfo> _entries;



        public EnumerableDirectoryContents(IEnumerable<IFileInfo> entries)
        {
            _entries = entries;
        }



        public bool Exists
        {
            get { return true; }
        }



        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entries.GetEnumerator();
        }
    }
}
