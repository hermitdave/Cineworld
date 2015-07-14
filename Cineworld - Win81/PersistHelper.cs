using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Cineworld
{
    public class PersistHelper
    {
        //private static StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
        //private static ApplicationDataContainer storageSettings = ApplicationData.Current.LocalSettings;

        public async static Task<StorageFile> GetFileIfExistsAsync(StorageFolder folder, string fileName)
        {
            try
            {
                return await folder.GetFileAsync(fileName);

            }
            catch
            {
                return null;
            }
        }

        public async static Task<bool> FileExisits(StorageFolder folder, string fileName)
        {
            try
            {
                StorageFile file = await folder.GetFileAsync(fileName);
                bool bRetVal = true;
                bRetVal = (file != null && file.DateCreated.Date == DateTime.Today) ? true : false;

                if (bRetVal)
                {
                    using (Stream s = await file.OpenStreamForReadAsync())
                    {
                        bRetVal = s.Length > 0;
                    }
                }

                return bRetVal;
            }
            catch
            {
                return false;
            }
        }

        public async static Task<StorageFolder> FolderExisits(StorageFolder folder, string folderName)
        {
            try
            {
                StorageFolder tempfolder = await folder.GetFolderAsync(folderName);
                return tempfolder;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<T> LoadObjectFromStorage<T>(Windows.Storage.StorageFolder storageFolder)
        {
            T ObjToLoad = default(T);

            try
            {
                StorageFile storageFile = await storageFolder.CreateFileAsync(GetFileName<T>(), 
                    CreationCollisionOption.OpenIfExists);

                using (Stream inStream = await storageFile.OpenStreamForReadAsync())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    ObjToLoad = (T)serializer.Deserialize(inStream);
                }
            }
            catch (Exception error)
            {
                throw new NotImplementedException(error.Message);
            }

            return ObjToLoad;
        }

        public static async void SaveObjectToStorage<T>(T ObjectToSave, Windows.Storage.StorageFolder storageFolder)
        {
            string filename = GetFileName<T>();
                
            using (Stream fs = await storageFolder.OpenStreamForWriteAsync(filename, CreationCollisionOption.ReplaceExisting))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(T));
                    ser.Serialize(sw, ObjectToSave);
                }
            }
        }

        public static string GetFileName<T>()
        {
            return typeof(T).FullName + ".xml";
        }

        public async static Task<bool> IsObjectPersisted<T1>(Windows.Storage.StorageFolder storageFolder)
        {
            string file = GetFileName<T1>();

            StorageFile storageFile = await GetFileIfExistsAsync(storageFolder, file);

            return (storageFile != null);
        }

        public static T LoadSetttingFromStorage<T>(string Key, Windows.Storage.ApplicationDataContainer storageSettings)
        {
            T ObjToLoad = default(T);

            if (storageSettings.Values.ContainsKey(Key))
            {
                using (StringReader sr = new StringReader((string)storageSettings.Values[Key]))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    ObjToLoad = (T)serializer.Deserialize(sr);
                }
            }

            return ObjToLoad;
        }

        public static void SaveSettingToStorage(string Key, object Setting, Windows.Storage.ApplicationDataContainer storageSettings)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                XmlSerializer ser = new XmlSerializer(Setting.GetType());
                ser.Serialize(sw, Setting);
            }

            if (!storageSettings.Values.ContainsKey(Key))
            {
                storageSettings.Values.Add(Key, sb.ToString());
            }
            else
            {
                storageSettings.Values[Key] = sb.ToString();
            }

        }

        public static bool IsSettingPersisted(string Key, Windows.Storage.ApplicationDataContainer storageSettings)
        {
            return storageSettings.Values.ContainsKey(Key);
        }
    }
}
