using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using HamlibGUI.ViewModels;

namespace HamlibGUI;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data == null) return new TextBlock { Text = "Build(data) data is null: "}; ;
        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        if (data == null) return false; 
        return data is ViewModelBase;
    }
}