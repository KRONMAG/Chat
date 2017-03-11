// Decompiled with JetBrains decompiler
// Type: Chat_Client.Properties.Settings
// Assembly: Chat_Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8604D71B-FF22-47D0-937D-39A741196C39
// Assembly location: C:\Users\vovan\OneDrive\Documents\GitHub\Chat\Chat_Client\Chat_Client\bin\Debug\Chat_Client.exe

using System.CodeDom.Compiler;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace Chat_Client.Properties
{
  [CompilerGenerated]
  [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
  internal sealed class Settings : ApplicationSettingsBase
  {
    private static Settings defaultInstance = (Settings) SettingsBase.Synchronized((SettingsBase) new Settings());

    public static Settings Default
    {
      get
      {
        Settings defaultInstance = Settings.defaultInstance;
        return defaultInstance;
      }
    }
  }
}
