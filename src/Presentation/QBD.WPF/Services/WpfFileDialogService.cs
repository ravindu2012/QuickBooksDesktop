// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using QBD.Application.Interfaces;

namespace QBD.WPF.Services;

public class WpfFileDialogService : IFileDialogService
{
    public string? ShowSaveFileDialog(string fileName, string defaultExt, string filter)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = fileName,
            DefaultExt = defaultExt,
            Filter = filter
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
