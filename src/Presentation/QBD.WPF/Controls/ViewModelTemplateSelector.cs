using System.Windows;
using System.Windows.Controls;
using QBD.Application.ViewModels;

namespace QBD.WPF.Controls;

public class ViewModelTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item == null || container is not FrameworkElement element)
            return null;

        var type = item.GetType();

        // Check for HomePageViewModel
        if (item is HomePageViewModel)
            return element.TryFindResource("HomePageTemplate") as DataTemplate;

        // Check for CenterViewModelBase<,>
        if (IsGenericSubclassOf(type, typeof(CenterViewModelBase<,>)))
            return element.TryFindResource("CenterTemplate") as DataTemplate;

        // Check for TransactionFormViewModelBase<,>
        if (IsGenericSubclassOf(type, typeof(TransactionFormViewModelBase<,>)))
            return element.TryFindResource("TransactionFormTemplate") as DataTemplate;

        // Check for RegisterViewModelBase
        if (type.IsSubclassOf(typeof(RegisterViewModelBase)) || type == typeof(RegisterViewModelBase))
            return element.TryFindResource("RegisterTemplate") as DataTemplate;

        // Check for ReportViewModelBase
        if (type.IsSubclassOf(typeof(ReportViewModelBase)) || type == typeof(ReportViewModelBase))
            return element.TryFindResource("ReportTemplate") as DataTemplate;

        // Check for ListViewModelBase<>
        if (IsGenericSubclassOf(type, typeof(ListViewModelBase<>)))
            return element.TryFindResource("ListTemplate") as DataTemplate;

        // Default
        return element.TryFindResource("DefaultTemplate") as DataTemplate;
    }

    private static bool IsGenericSubclassOf(Type? type, Type genericBase)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericBase)
                return true;
            type = type.BaseType;
        }
        return false;
    }
}
