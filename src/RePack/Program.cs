using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;


namespace RePack
{
    class Program
    {
        static void Main(string[] args)
        {
            var repoUrl = "https://github.com/{ your account }/{ your repo }";


            var packages = Directory.GetFiles("C:\\package-migration", "*.nupkg", SearchOption.AllDirectories).ToList();

            foreach (var package in packages)
            {

                using (var zipToOpen = new FileStream(package, FileMode.Open))
                using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    var sourceEntry = archive.Entries.FirstOrDefault(x => x.Name.EndsWith(".nuspec"));

                    if (sourceEntry == null)
                    {
                        return;
                    }

                    using (var sourceStream = sourceEntry.Open())
                    {
                        var xmlDocument = XDocument.Load(sourceStream);

                        var ns = xmlDocument.Root!.GetDefaultNamespace();
                        var metadataElement = xmlDocument.Descendants().First(x => x.Name.LocalName == "metadata");

                        var repositoryElement = new XElement(ns + "repository",
                            new XAttribute("type", "git"),
                            new XAttribute("url", repoUrl));

                        metadataElement.Add(repositoryElement);


                        var updatedEntry = sourceEntry.Archive!.CreateEntry(sourceEntry.Name);
                        using var destination = updatedEntry.Open();
                        using var writer = new XmlTextWriter(destination, Encoding.UTF8)
                        {
                            Formatting = Formatting.Indented
                        };
                        xmlDocument.WriteTo(writer);
                    }

                    sourceEntry.Delete();
                }

                // todo:  push package
                Console.Write("use power shell to push.");
            }
        }
    }
}
