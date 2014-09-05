using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;

namespace CineWorld
{
    public class PersistHelper
    {
        public static bool FileExisits(string fileName, bool bCinemaPerformaceData)
        {
            try
            {
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

                bool bRetVal = true;
                bRetVal = isf.FileExists(fileName);

                if (bRetVal)
                {
                    DateTime dtLastWriteTime = isf.GetLastWriteTime(fileName).UtcDateTime;
                    
                    if(bCinemaPerformaceData)
                        bRetVal = (DateTime.UtcNow.Subtract(dtLastWriteTime).TotalHours < 2);
                    else
                        bRetVal = (DateTime.UtcNow.Date == dtLastWriteTime.Date);

                    if (bRetVal)
                    {
                        using (Stream s = isf.OpenFile(fileName, FileMode.Open))
                        {
                            bRetVal = s.Length > 0;
                        }
                    }
                }

                return bRetVal;
            }
            catch
            {
                return false;
            }
        }

        public static T LoadObjectFromStorage<T>()
        {
            T ObjToLoad = default(T);

            try
            {

                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isf.FileExists(GetFileName<T>()))
                    {
                        using (IsolatedStorageFileStream fs = isf.OpenFile(GetFileName<T>(), System.IO.FileMode.Open))
                        {
                            XmlSerializer ser = new XmlSerializer(typeof(T));
                            ObjToLoad = (T)ser.Deserialize(fs);
                        }
                    }

                }
            }
            catch (Exception error)
            {
                throw new NotImplementedException(error.Message);
            }

            return ObjToLoad;
        }

        public static void SaveObjectToStorage<T>(T ObjectToSave)
        {
            TextWriter writer;

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream fs = isf.OpenFile(GetFileName<T>(), System.IO.FileMode.Create))
                {
                    writer = new StreamWriter(fs);
                    XmlSerializer ser = new XmlSerializer(typeof(T));
                    ser.Serialize(writer, ObjectToSave);
                    writer.Close();
                }

            }
        }

        public static string GetFileName<T>()
        {
            return typeof(T).FullName + ".xml";
        }

        public static bool IsObjectPersisted<T1>()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isf.FileExists(GetFileName<T1>());
            }
        }

        public static T LoadSetttingFromStorage<T>(string Key)
        {
            T ObjToLoad = default(T);

            if (IsolatedStorageSettings.ApplicationSettings.Contains(Key))
            {
                ObjToLoad = (T)IsolatedStorageSettings.ApplicationSettings[Key];
            }

            return ObjToLoad;
        }

        public static void SaveSettingToStorage(string Key, object Setting)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(Key))
            {
                IsolatedStorageSettings.ApplicationSettings.Add(Key, Setting);
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings[Key] = Setting;
            }

        }

        public static bool IsSettingPersisted(string Key)
        {
            return IsolatedStorageSettings.ApplicationSettings.Contains(Key);
        }
    }
}
