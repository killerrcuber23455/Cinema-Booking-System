// the booking summary drawn as a cinema ticket: a perforated line, two round notches, and a tear-off stub (owner: divyesh)
// the main part has the film and the price lines, the stub says how many people it lets in and which seats.
// it only draws the text it is given, the lines come from BookingSummary
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class TicketStub : Control
    {
        private const int CORNER_RADIUS = 14;
        private const int NOTCH_RADIUS = 12;
        private const int STUB_WIDTH = 150;
        private const int INSIDE_PADDING = 24;
        private const int TOP_PART_HEIGHT = 74;
        private const int LINE_HEIGHT = 26;
        private const int BOTTOM_PADDING = 18;

        private readonly string _title;
        private readonly string _showing_line;
        private readonly string[] _lines;
        private readonly string[] _seats;

        public TicketStub(string title, string showing_line, string[] lines, string[] seats)
        {
            _title = title;
            _showing_line = showing_line;
            _lines = lines;
            _seats = seats;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
        }

        // how tall the ticket must be for this many lines
        public static int HeightFor(int line_count)
        {
            return TOP_PART_HEIGHT + (line_count * LINE_HEIGHT) + BOTTOM_PADDING;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Color behind = Parent != null ? Parent.BackColor : UiTheme.PANEL;
            graphics.Clear(behind);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int tear_x = Width - STUB_WIDTH;
            RectangleF ticket_area = new RectangleF(0, 0, Width - 1, Height - 1);

            // the ticket shape, with a half circle cut out at the top and bottom of the tear line
            using (GraphicsPath outline = GlowPainter.RoundedPath(ticket_area, CORNER_RADIUS))
            using (Region ticket = new Region(outline))
            {
                ticket.Exclude(new RectangleF(tear_x - NOTCH_RADIUS, -NOTCH_RADIUS, NOTCH_RADIUS * 2, NOTCH_RADIUS * 2));
                ticket.Exclude(new RectangleF(tear_x - NOTCH_RADIUS, Height - NOTCH_RADIUS, NOTCH_RADIUS * 2, NOTCH_RADIUS * 2));

                using (LinearGradientBrush paper = new LinearGradientBrush(ClientRectangle,
                    ColorMath.Blend(UiTheme.CARD, UiTheme.AMBER, 0.05), UiTheme.CARD, LinearGradientMode.Horizontal))
                {
                    graphics.FillRegion(paper, ticket);
                }
            }

            // the dashed tear line between the ticket and the stub
            using (Pen tear_pen = new Pen(Color.FromArgb(110, UiTheme.TEXT_MUTED), 1.4f))
            {
                tear_pen.DashPattern = new float[] { 4f, 4f };
                graphics.DrawLine(tear_pen, tear_x, NOTCH_RADIUS + 6, tear_x, Height - NOTCH_RADIUS - 6);
            }

            DrawMainPart(graphics, tear_x);
            DrawStub(graphics, tear_x);
        }

        private void DrawMainPart(Graphics graphics, int tear_x)
        {
            int text_width = tear_x - (2 * INSIDE_PADDING);

            TextRenderer.DrawText(graphics, _title, UiTheme.CARD_TITLE_FONT,
                new Rectangle(INSIDE_PADDING, 18, text_width, 26), UiTheme.TEXT,
                TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(graphics, _showing_line, UiTheme.SMALL_FONT,
                new Rectangle(INSIDE_PADDING, 44, text_width, 20), UiTheme.TEXT_MUTED,
                TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

            for (int line_index = 0; line_index < _lines.Length; line_index++)
            {
                // the amount to pay stands out
                bool is_total = _lines[line_index].StartsWith("To pay");
                Font line_font = is_total ? UiTheme.CARD_TITLE_FONT : UiTheme.BODY_FONT;

                TextRenderer.DrawText(graphics, _lines[line_index], line_font,
                    new Rectangle(INSIDE_PADDING, TOP_PART_HEIGHT + (line_index * LINE_HEIGHT), text_width, LINE_HEIGHT),
                    UiTheme.TEXT, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        // the stub: how many people it lets in, in big warm figures, and the seats under it
        private void DrawStub(Graphics graphics, int tear_x)
        {
            int stub_left = tear_x + 18;
            int stub_width = STUB_WIDTH - 36;

            TextRenderer.DrawText(graphics, "Admits", UiTheme.SMALL_FONT,
                new Rectangle(stub_left, 20, stub_width, 18), UiTheme.TEXT_MUTED,
                TextFormatFlags.Left | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(graphics, _seats.Length.ToString(), UiTheme.TITLE_FONT,
                new Rectangle(stub_left, 38, stub_width, 40), UiTheme.AMBER,
                TextFormatFlags.Left | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(graphics, string.Join(" ", _seats), UiTheme.SMALL_FONT,
                new Rectangle(stub_left, 84, stub_width, Height - 100), UiTheme.TEXT,
                TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
    }
}
