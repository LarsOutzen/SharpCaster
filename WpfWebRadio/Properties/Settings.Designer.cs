﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WpfWebRadio.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>http://mp3stream3.apasf.apa.at:8000/;|Ö1|Image1</string>
  <string>http://mp3stream2.apasf.apa.at:80/;|Ö Regional|Image2</string>
  <string>http://mp3stream7.apasf.apa.at:80/;|Ö 3|Image3</string>
  <string>http://mp3stream1.apasf.apa.at:80/;|FM 4|Image4</string>
  <string>http://icecast-qmusic.cdp.triple-it.nl/Qmusic_nl_live_96.mp3|Q-Music|Image5</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection MyStations {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["MyStations"]));
            }
            set {
                this["MyStations"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Büro")]
        public string DefaultCastName {
            get {
                return ((string)(this["DefaultCastName"]));
            }
            set {
                this["DefaultCastName"] = value;
            }
        }
    }
}
