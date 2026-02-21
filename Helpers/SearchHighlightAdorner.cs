using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace LucidNotes.Helpers
{
    public class SearchHighlightAdorner : Adorner
    {
        private readonly TextBox _textBox;
        private string _searchText;
        private readonly Brush _highlightBrush;
        private readonly Pen _highlightPen;

        public SearchHighlightAdorner(TextBox textBox, string searchText) : base(textBox)
        {
            _textBox = textBox;
            _searchText = searchText;
            ClipToBounds = true;

            // Semi-transparent yellow highlight with rounded corners
            _highlightBrush = new SolidColorBrush(Color.FromArgb(120, 255, 215, 0));
            _highlightPen = new Pen(_highlightBrush, 0);

            // Re-render when size, text, or scroll position changes
            _textBox.TextChanged += (s, e) => InvalidateVisual();
            _textBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChanged));
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            InvalidateVisual();
        }

        public void UpdateSearchText(string newText)
        {
            if (_searchText != newText)
            {
                _searchText = newText;
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (string.IsNullOrEmpty(_searchText) || string.IsNullOrEmpty(_textBox.Text))
            {
                return;
            }

            // Explicitly clip the adornment to the bounds of the TextBox
            // This prevents highlights from rendering over toolbars or scrollbars when scrolled out of view.
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, _textBox.RenderSize.Width, _textBox.RenderSize.Height)));

            string text = _textBox.Text;
            string search = _searchText;

            int index = 0;
            while ((index = text.IndexOf(search, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                // Measure the exact character bounds
                int endIndex = index + search.Length;

                // Ensure we do not go out of bounds (can happen during live typing)
                if (endIndex > _textBox.Text.Length) break;

                Rect startRect = _textBox.GetRectFromCharacterIndex(index);
                Rect endRect = _textBox.GetRectFromCharacterIndex(endIndex - 1, true); // Use trailing edge of last char

                if (!startRect.IsEmpty && !endRect.IsEmpty)
                {
                    // For single line, or simple word wrapping, create a bounding box
                    Rect highlightRect = new Rect(startRect.TopLeft, endRect.BottomRight);
                    highlightRect.Inflate(2, 2);

                    drawingContext.DrawRoundedRectangle(_highlightBrush, _highlightPen, highlightRect, 3, 3);
                }

                index += search.Length;
            }

            // Remove the clip
            drawingContext.Pop();

            base.OnRender(drawingContext);
        }
    }
}
