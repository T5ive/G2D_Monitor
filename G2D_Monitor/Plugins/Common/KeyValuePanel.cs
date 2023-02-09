namespace G2D_Monitor.Plugins.Common
{
    internal class KeyValuePanel
    {
        private static readonly Dictionary<Type, int> TOP_ALIGN = new()
        {
            { typeof(TextBox), 3 },
            { typeof(NumericUpDown), 2 },
            { typeof(CheckBox), -2 },
            { typeof(ComboBox), 3 },
            { typeof(PictureBox), 2 }
        };

        private readonly List<KeyValuePair<string, Control>> Controls = new();
        private readonly List<Control> AutoSizeControls = new();
        private FlowLayoutPanel? FlowPanel = null;

        public KeyValuePanel Add<T>(string label, Action<T> initializer) where T : Control, new() 
        {
            var control = new T();
            initializer(control);
            Controls.Add(new(label, control));
            return this;
        }

        public KeyValuePanel AddReadOnlyTextBox(string label, out Action<string> setter, int width = -1)
        {
            var textBox = new TextBox();
            textBox.ReadOnly = true;
            textBox.BorderStyle = BorderStyle.None;
            if (width <= 0) { AutoSizeControls.Add(textBox); textBox.Width = 120; }
            else textBox.Width = width;
            Controls.Add(new(label, textBox));
            setter = s => { if (!textBox.Text.Equals(s)) textBox.Text = s; };
            return this;
        }

        public KeyValuePanel AddEditableTextBox(string label, string text, int width, Action<string> getter)
        {
            var textBox = new TextBox();
            textBox.Text = text;
            if (width <= 0) { AutoSizeControls.Add(textBox); textBox.Width = 120; }
            else textBox.Width = width;
            Controls.Add(new(label, textBox));
            textBox.LostFocus += (_, _) => getter(textBox.Text);
            return this;
        }

        public KeyValuePanel AddNumeric(string label, int value, int min, int max, Action<int> valueChanged)
        {
            var control = new NumericUpDown();
            control.Minimum = min;
            control.Width = 60;
            control.TextAlign = HorizontalAlignment.Center;
            if (max > min) control.Maximum = max;
            if (value >= min && value <= max) control.Value = value;
            Controls.Add(new(label, control));
            control.ValueChanged += (_, _) => valueChanged(decimal.ToInt32(control.Value));
            return this;
        }

        public KeyValuePanel AddCheckBox(string label, bool isChecked, Action<bool> checkedChanged)
        {
            var control = new CheckBox();
            control.Text = string.Empty;
            control.Checked = isChecked;
            Controls.Add(new(label, control));
            control.CheckedChanged += (_, _) => checkedChanged(control.Checked);
            return this;
        }

        public KeyValuePanel AddComboBox(string label, object[] items, int selectedIndex, Action<object, int> selectedItemChanged)
        {
            var control = new ComboBox();
            control.Width = 121;
            control.DropDownStyle = ComboBoxStyle.DropDownList;
            control.Items.AddRange(items);
            control.SelectedIndex = selectedIndex;
            Controls.Add(new(label, control));
            control.SelectedIndexChanged += (_, _) => selectedItemChanged(control.SelectedItem, control.SelectedIndex);
            return this;
        }

        public KeyValuePanel AddColorSelector(string label, Color val, Action<Color> colorChanged)
        {
            var control = new PictureBox();
            control.Width = control.Height = 24;
            control.BackColor = val;
            control.Cursor = Cursors.Hand;
            control.BorderStyle = BorderStyle.FixedSingle;
            control.Click += (_, _) =>
            {
                var diag = new ColorDialog();
                diag.Color = val;
                if (diag.ShowDialog() == DialogResult.OK) colorChanged(control.BackColor = diag.Color);
                diag.Dispose();
            };
            Controls.Add(new(label, control));
            return this;
        }

        public KeyValuePanel Attach(TabPage tab)
        {
            if (FlowPanel != null) return this;
            var panel = new FlowLayoutPanel();
            FlowPanel = panel;
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;
            panel.SuspendLayout();
            panel.Padding = new Padding(12, 8, 0, 0);
            foreach (var item in Controls)
            {
                var label = new Label
                {
                    AutoSize = true,
                    Margin = new Padding(0, 10, 3, 0),
                    Text = item.Key
                };
                panel.Controls.Add(label);
                item.Value.Tag = label;
                panel.Controls.Add(item.Value);
                panel.SetFlowBreak(item.Value, true);
            }
            tab.Controls.Add(panel);
            panel.ResumeLayout(false);
            panel.PerformLayout();
            panel.SizeChanged += SizeChanged;
            return this;
        }

        private void SizeChanged(object? sender, EventArgs e)
        {
            if (sender is FlowLayoutPanel panel)
            {
                foreach (var control in AutoSizeControls)
                {
                    if (control.Tag is Label label)
                        control.Width = Math.Max(20, panel.Width - label.Left - label.Width - control.Margin.Left - 36);
                }
            }
        }

        public void Align()
        {
            if (FlowPanel == null) return;
            
            var maxLeft = 0;
            foreach (var item in Controls)
            {
                JustifyTop(item.Value);
                maxLeft = Math.Max(item.Value.Left, maxLeft);
            }
            maxLeft += 5;
            foreach (var item in Controls) JustifyLeft(item.Value, maxLeft);
            SizeChanged(FlowPanel, EventArgs.Empty);
            FlowPanel = null;
        }

        private static void JustifyTop(Control control)
        {
            if (control.Tag is Label label)
            {
                if (!TOP_ALIGN.TryGetValue(control.GetType(), out var v)) v = 0;
                if (control is TextBox tb && tb.BorderStyle == BorderStyle.None) v = 0;
                v = label.Top - v - control.Top;
                if (v != 0) control.Margin = new(control.Margin.Left, control.Margin.Top + v, control.Margin.Right, control.Margin.Bottom);
            }
        }

        private static void JustifyLeft(Control control, int left)
        {
            var l = control.Left;
            left -= control.Left;
            if (left != 0) control.Margin = new(control.Margin.Left + left, control.Margin.Top, control.Margin.Right, control.Margin.Bottom);
        }
    }
}
