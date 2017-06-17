using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QOLTweaksPack.hugsLibSettings
{
    internal class StringHashSetHandler : SettingHandleConvertible
    {
        protected HashSet<string> strings = new HashSet<string>();
        internal HashSet<string> InnerList { get { return strings; } set { strings = value; } }

        private const string EmptySet = "EmptySet";

        public StringHashSetHandler()
        {
            SetToDefault();
        }

        protected virtual void SetToDefault()
        {

        }

        public override void FromString(string settingValue)
        {
            strings = new HashSet<string>();
            
            if (settingValue.Equals(string.Empty))
            {
                SetToDefault();
            }
            else
            {
                if (!settingValue.Equals(EmptySet))
                {
                    foreach (string str in settingValue.Split('|'))
                    {
                        strings.Add(str);
                    }
                }
            }         
        }

        public override string ToString()
        {
            return strings.Count != 0 ? String.Join("|", strings.ToArray()) : "EmptySet";
        }
    }
}
