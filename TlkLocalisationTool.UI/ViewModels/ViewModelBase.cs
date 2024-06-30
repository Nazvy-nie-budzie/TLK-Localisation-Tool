using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TlkLocalisationTool.UI.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    private string _title;
    private bool _isLoading;

    public event PropertyChangedEventHandler PropertyChanged;

    public event Action ClosureRequested;

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public virtual Task Init() => Task.CompletedTask;

    protected void Close() => ClosureRequested();

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
