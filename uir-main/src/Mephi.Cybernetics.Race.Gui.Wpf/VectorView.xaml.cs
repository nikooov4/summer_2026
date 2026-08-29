using System.Numerics;
using System.Windows.Controls;

namespace Mephi.Cybernetics.Race.Gui.Wpf
{
    /// <summary>
    /// Interaction logic for VectorView.xaml
    /// </summary>
    public partial class VectorView : UserControl
    {
        public VectorView()
        {
            InitializeComponent();
            var vector = new Vector3(11, 222, 3333777);
            this.DataContext = new VectorWrapper(vector);
          
        }
        
    }
}
