using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LucidNotes.Helpers
{
    public static class SearchHighlightBehavior
    {
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.RegisterAttached(
                "SearchText",
                typeof(string),
                typeof(SearchHighlightBehavior),
                new PropertyMetadata(string.Empty, OnSearchTextChanged));

        public static string GetSearchText(DependencyObject obj)
        {
            return (string)obj.GetValue(SearchTextProperty);
        }

        public static void SetSearchText(DependencyObject obj, string value)
        {
            obj.SetValue(SearchTextProperty, value);
        }

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                string? newSearchText = e.NewValue as string;

                if (!textBox.IsLoaded)
                {
                    // Delay attachment until element is loaded if necessary
                    textBox.Loaded += (s, args) => AttachAdorner((TextBox)s, newSearchText);
                }
                else
                {
                    AttachAdorner(textBox, newSearchText);
                }
            }
        }

        private static void AttachAdorner(TextBox textBox, string? searchText)
        {
            AdornerLayer? layer = AdornerLayer.GetAdornerLayer(textBox);
            if (layer == null) return;

            // Find existing adorner
            SearchHighlightAdorner? existingAdorner = null;
            var adorners = layer.GetAdorners(textBox);
            if (adorners != null)
            {
                foreach (var adorner in adorners)
                {
                    if (adorner is SearchHighlightAdorner searchAdorner)
                    {
                        existingAdorner = searchAdorner;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(searchText))
            {
                // Remove existing if empty
                if (existingAdorner != null)
                {
                    layer.Remove(existingAdorner);
                }
            }
            else
            {
                // Update or create
                if (existingAdorner != null && searchText != null)
                {
                    existingAdorner.UpdateSearchText(searchText);
                }
                else if (searchText != null)
                {
                    var newAdorner = new SearchHighlightAdorner(textBox, searchText);
                    layer.Add(newAdorner);
                }
            }
        }
    }
}
