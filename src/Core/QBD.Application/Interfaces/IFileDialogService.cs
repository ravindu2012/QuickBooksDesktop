namespace QBD.Application.Interfaces
{
    public interface IFileDialogService
    {
        string? ShowSaveFileDialog(string fileName, string defaultExt, string filter);
    }
}
