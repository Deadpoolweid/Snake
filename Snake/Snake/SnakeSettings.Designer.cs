﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Snake {
    
    
    [CompilerGenerated()]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class SnakeSettings : ApplicationSettingsBase {
        
        private static SnakeSettings defaultInstance = ((SnakeSettings)(Synchronized(new SnakeSettings())));
        
        public static SnakeSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [ApplicationScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("0.0.2.3")]
        public string Version {
            get {
                return ((string)(this["Version"]));
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("0")]
        public int open_sum {
            get {
                return ((int)(this["open_sum"]));
            }
            set {
                this["open_sum"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("")]
        public string Save_text {
            get {
                return ((string)(this["Save_text"]));
            }
            set {
                this["Save_text"] = value;
            }
        }
    }
}
