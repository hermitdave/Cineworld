using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Cineworld
{
    public class Changes : ObservableCollection<KeyValuePair<string, string>>
    {
        public async static Task LoadChanges(Changes c)
        {
            string myFile = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/ChangeList.xml");
            StorageFolder myFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(myFile));

            using (Stream s = await myFolder.OpenStreamForReadAsync(Path.GetFileName(myFile)))
            {
                XDocument doc = XDocument.Load(s);
                IEnumerable<XElement> elements = doc.Descendants("Change");

                if (elements.Count() > 0)
                {
                    foreach (var element in elements)
                    {
                        c.Add(new KeyValuePair<string, string>(element.Element("ChangeDate").Value, element.Element("Description").Value));
                    }
                }
            }
        }
    }
}
