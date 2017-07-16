using System.IO;
using CSharpUtils.VirtualFileSystem;

namespace CSharpUtils.Templates.TemplateProvider
{
    public class TemplateProviderVirtualFileSystem : ITemplateProvider
    {
        FileSystem FileSystem;

        public TemplateProviderVirtualFileSystem(FileSystem FileSystem)
        {
            this.FileSystem = FileSystem;
        }

        public Stream GetTemplate(string Name)
        {
            return this.FileSystem.OpenFile(Name, FileMode.Open);
        }
    }
}
