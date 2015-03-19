﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Threading;
using System.IO;

using MatterHackers.Agg.PlatformAbstract;
using MatterHackers.Agg;

namespace MatterHackers.Localizations
{
    public class TranslationMap
    {
        const string engishTag = "English:";
        const string translatedTag = "Translated:";

        Dictionary<string, string> translationDictionary = new Dictionary<string, string>();
        Dictionary<string, string> masterDictionary = new Dictionary<string, string>();

        string pathToTranslationFile;
        string pathToMasterFile;

        string twoLetterIsoLanguageName;
        public string TwoLetterIsoLanguageName
        {
            get { return twoLetterIsoLanguageName; }
        }

        public TranslationMap(string pathToTranslationsFolder, string twoLetterIsoLanguageName = "")
        {
            if (twoLetterIsoLanguageName == "")
            {
                twoLetterIsoLanguageName = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            }

            LoadTranslation(pathToTranslationsFolder, twoLetterIsoLanguageName);
        }

        void ReadIntoDictonary(Dictionary<string, string> dictionary, string pathAndFilename)
        {
            string[] lines = StaticData.Instance.ReadAllLines(pathAndFilename);
            bool lookingForEnglish = true;
            string englishString = "";
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0)
                {
                    // we are happy to skip blank lines
                    continue;
                }

                if (lookingForEnglish)
                {
                    if (line.Length < engishTag.Length || !line.StartsWith(engishTag))
                    {
                        throw new Exception("Found unknown string at line {0}. Looking for {1}.".FormatWith(i, engishTag));
                    }
                    else
                    {
                        englishString = lines[i].Substring(engishTag.Length);
                        lookingForEnglish = false;
                    }
                }
                else
                {
                    if (line.Length < translatedTag.Length || !line.StartsWith(translatedTag))
                    {
                        throw new Exception("Found unknown string at line {0}. Looking for {1}.".FormatWith(i, translatedTag));
                    }
                    else
                    {
                        string translatedString = lines[i].Substring(translatedTag.Length);
                        // store the string
                        if (!dictionary.ContainsKey(DecodeWhileReading(englishString)))
                        {
                            dictionary.Add(DecodeWhileReading(englishString), DecodeWhileReading(translatedString));
                        }
                        // go back to looking for english
                        lookingForEnglish = true;
                    }
                }
            }
        }

        public void LoadTranslation(string pathToTranslationsFolder, string twoLetterIsoLanguageName)
        {
            this.twoLetterIsoLanguageName = twoLetterIsoLanguageName.ToLower();

            this.pathToMasterFile = Path.Combine(pathToTranslationsFolder, "Master.txt");
            ReadIntoDictonary(masterDictionary, pathToMasterFile);

            this.pathToTranslationFile = Path.Combine(pathToTranslationsFolder, TwoLetterIsoLanguageName, "Translation.txt");
            if (StaticData.Instance.FileExists(pathToTranslationFile))
            {
                ReadIntoDictonary(translationDictionary, pathToTranslationFile);
            }

            foreach (KeyValuePair<string, string> keyValue in translationDictionary)
            {
                if (!masterDictionary.ContainsKey(keyValue.Key))
                {
                    AddNewString(masterDictionary, pathToMasterFile, keyValue.Key);
                }
            }

            if (TwoLetterIsoLanguageName != "en")
            {
                foreach (KeyValuePair<string, string> keyValue in masterDictionary)
                {
                    if (!translationDictionary.ContainsKey(keyValue.Key))
                    {
                        AddNewString(translationDictionary, pathToTranslationFile, keyValue.Key);
                    }
                }
            }
        }

        string EncodeForSaving(string stringToEncode)
        {
            // replace the cariage returns with '\n's
            return stringToEncode.Replace("\n", "\\n");
        }

        string DecodeWhileReading(string stringToDecode)
        {
            return stringToDecode.Replace("\\n", "\n");
        }

        void AddNewString(Dictionary<string, string> dictionary, string pathAndFilename, string englishString)
        {
            // We only ship release and this could cause a write to the ProgramFiles directory which is not allowed.
            // So we only write translation text while in debug (another solution in the future could be implemented). LBB
#if DEBUG 
            if (OsInformation.OperatingSystem == OSType.Windows)
            {
                // TODO: make sure we don't throw an assertion when running from the ProgramFiles directory.
                // Don't do saving when we are.
                if (!dictionary.ContainsKey(englishString))
                {
                    dictionary.Add(englishString, englishString);

                    using (TimedLock.Lock(this, "TranslationMap"))
                    {
                        string staticDataPathAndFilename = Path.Combine("../../StaticData", pathAndFilename);
                        string staticDataPath = Path.Combine("../../StaticData", staticDataPathAndFilename);
                        string pathName = Path.GetDirectoryName(staticDataPath);
                        if (!Directory.Exists(pathName))
                        {
                            Directory.CreateDirectory(pathName);
                        }
                        if (!File.Exists(staticDataPathAndFilename))
                        {
                            using (StreamWriter masterFileStream = File.CreateText(staticDataPathAndFilename))
                            {
                            }
                        }

                        using (StreamWriter masterFileStream = File.AppendText(staticDataPathAndFilename))
                        {

                            masterFileStream.WriteLine("{0}{1}".FormatWith(engishTag, EncodeForSaving(englishString)));
                            masterFileStream.WriteLine("{0}{1}".FormatWith(translatedTag, EncodeForSaving(englishString)));
                            masterFileStream.WriteLine("");
                        }
                    }
                }
            }
#endif
        }

        public string Translate(string englishString)
        {
            string tranlatedString;
            if (!translationDictionary.TryGetValue(englishString, out tranlatedString))
            {
                if (TwoLetterIsoLanguageName != "en")
                {
                    AddNewString(translationDictionary, pathToTranslationFile, englishString);
                }
                AddNewString(masterDictionary, pathToMasterFile, englishString);
                return englishString;
            }

            return tranlatedString;
        }

        public static void AssertDebugNotDefined()
        {
#if DEBUG
            throw new Exception("DEBUG is defined and should not be!");
#endif
        }
    }
}
