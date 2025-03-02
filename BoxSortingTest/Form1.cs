using System;
using System.Drawing;
using System.Windows.Forms;

namespace BoxSortingTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Vytvoření FlowLayoutPanel pro uspořádání prvků
            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            this.Controls.Add(flowLayoutPanel);

            // Přidání několika Panelů s Label a TextBox
            for (int i = 1; i <= 5; i++)
            {
                var panel = new Panel
                {
                    Size = new Size(320, 30),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.White
                };

                var label = new Label
                {
                    Text = $"Box {i}:",
                    AutoSize = false,
                    Size = new Size(80, 20),
                    Location = new Point(5, 5)
                };

                var textBox = new TextBox
                {
                    Text = $"Test Test n:{i}",
                    Size = new Size(200, 20),
                    Location = new Point(90, 5)
                };

                var dragLabel = new Label
                {
                    Text = "≡",
                    AutoSize = false,
                    Size = new Size(20, 20),
                    Location = new Point(300, 5),
                    Cursor = Cursors.Hand // Změna kurzoru na ruku
                };

                // Přidání Label, TextBox a dragLabel do Panelu
                panel.Controls.Add(label);
                panel.Controls.Add(textBox);
                panel.Controls.Add(dragLabel);

                // Povolení drag-and-drop pro dragLabel
                dragLabel.MouseDown += DragLabel_MouseDown;
                dragLabel.MouseMove += DragLabel_MouseMove;
                dragLabel.MouseUp += DragLabel_MouseUp;

                // Přidání Panelu do FlowLayoutPanel
                flowLayoutPanel.Controls.Add(panel);
            }

            // Vytvoření indikátoru pro vložení
            _insertIndicator = new Label
            {
                Text = "-----",
                AutoSize = false,
                Size = new Size(300, 10),
                BackColor = Color.Red,
                Visible = false // Skrytí indikátoru na začátku
            };
            flowLayoutPanel.Controls.Add(_insertIndicator);
        }

        private bool isDragging = false;
        private Point dragStartPoint;
        private Control draggedControl;
        private int draggedIndex;
        private Label _insertIndicator; // Indikátor pro vložení

        private void DragLabel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                draggedControl = ((Label)sender).Parent as Panel; // Chytáme celý Panel
                dragStartPoint = e.Location;
                draggedIndex = ((FlowLayoutPanel)draggedControl.Parent).Controls.IndexOf(draggedControl);
                draggedControl.BringToFront();
                draggedControl.BackColor = Color.LightBlue; // Zvýraznění při tažení

                // Dočasné zakázání automatického přeuspořádávání
                ((FlowLayoutPanel)draggedControl.Parent).SuspendLayout();
            }
        }

        private void DragLabel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedControl != null)
            {
                Point newLocation = draggedControl.PointToScreen(e.Location);
                newLocation = ((FlowLayoutPanel)draggedControl.Parent).PointToClient(newLocation);

                // Odečtení počátečního bodu pro plynulý pohyb
                newLocation.X -= dragStartPoint.X;
                newLocation.Y -= dragStartPoint.Y;

                // Omezení pohybu, aby nešel mimo FlowLayoutPanel
                newLocation.X = Math.Max(0, Math.Min(newLocation.X, ((FlowLayoutPanel)draggedControl.Parent).Width - draggedControl.Width));
                newLocation.Y = Math.Max(0, Math.Min(newLocation.Y, ((FlowLayoutPanel)draggedControl.Parent).Height - draggedControl.Height));

                draggedControl.Location = newLocation;

                // Zobrazení indikátoru pro vložení
                ShowInsertIndicator(draggedControl);
            }
        }

        private void DragLabel_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedControl != null)
            {
                isDragging = false;
                draggedControl.BackColor = Color.White; // Vrácení barvy

                // Skrytí indikátoru
                _insertIndicator.Visible = false;

                // Vložení Panelu na pozici indikátoru
                InsertPanelAtIndicator(draggedControl);

                // Obnovení automatického přeuspořádávání
                ((FlowLayoutPanel)draggedControl.Parent).ResumeLayout();

                draggedControl = null;
            }
        }

        private void ShowInsertIndicator(Control draggedControl)
        {
            FlowLayoutPanel parent = (FlowLayoutPanel)draggedControl.Parent;
            int newIndex = CalculateNewIndex(draggedControl);

            if (newIndex >= 0 && newIndex < parent.Controls.Count)
            {
                // Nastavení pozice indikátoru
                _insertIndicator.Location = new Point(0, parent.Controls[newIndex].Location.Y - 5);
                _insertIndicator.Visible = true;
            }
        }

        private void InsertPanelAtIndicator(Control draggedControl)
        {
            FlowLayoutPanel parent = (FlowLayoutPanel)draggedControl.Parent;
            int newIndex = CalculateNewIndex(draggedControl);

            if (newIndex != draggedIndex && newIndex >= 0 && newIndex < parent.Controls.Count)
            {
                // Odstranění Panelu z původní pozice
                parent.Controls.Remove(draggedControl);

                // Vložení Panelu na novou pozici
                parent.Controls.Add(draggedControl);
                parent.Controls.SetChildIndex(draggedControl, newIndex-1); // TODO FIX
            }
        }

        private int CalculateNewIndex(Control draggedControl)
        {
            FlowLayoutPanel parent = (FlowLayoutPanel)draggedControl.Parent;
            int midPoint = draggedControl.Location.Y + (draggedControl.Height / 2);

            for (int i = 0; i < parent.Controls.Count; i++)
            {
                if (parent.Controls[i] == draggedControl) continue; // Přeskočení aktuálně taženého Panelu

                if (midPoint < parent.Controls[i].Location.Y + (parent.Controls[i].Height / 2))
                {
                    return i;
                }
            }
            return parent.Controls.Count;
        }
    }
}