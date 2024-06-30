using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TlkLocalisationTool.UI.Models;

public abstract class ModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
